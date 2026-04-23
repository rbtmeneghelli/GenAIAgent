using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace GenAIAgent;

public class GenAIFactory : IGenAIFactory
{
    public async Task CreateAgent(IConfiguration config)
    {
        var openAPIKey = config["OpenAi:ApiKey"];

        var agent = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que pode fornecer informações sobre o clima.""", tools:
                    [
                        AIFunctionFactory.Create(WeatherTool.GetWeather)
                    ]);

        var agent2 = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que irá receber um texto em idioma português e ira traduzir o texto para o idioma em inglês""");

        var session = await agent.CreateSessionAsync();

        Console.WriteLine("Faça uma pergunta:");
        var prompt = Console.ReadLine();

        /* Respondendo igual ao chatGPT */
        await foreach (var token in agent.RunStreamingAsync(prompt ?? string.Empty, session))
        {
            Console.WriteLine(token);
        }
    }

    public async Task CreateAgent_V1(IConfiguration config)
    {
        var openAPIKey = config["OpenAi:ApiKey"];

        var agent = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que pode fornecer informações sobre o clima.""", tools:
                    [
                        AIFunctionFactory.Create(WeatherTool.GetWeather)
                    ]);

        var agent2 = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que irá receber um texto em idioma português e ira traduzir o texto para o idioma em inglês""");

        var session = await agent.CreateSessionAsync();
        var session2 = await agent2.CreateSessionAsync();

        while (true)
        {
            Console.WriteLine("Faça uma pergunta:");
            var prompt = Console.ReadLine();
            var result = agent.RunAsync(prompt ?? string.Empty, session);

            Console.WriteLine("Resposta em português:");
            Console.WriteLine(result.Result.Text);

            var translatedResult = agent2.RunAsync(result.Result.Text, session2);
            Console.WriteLine("Resposta em inglês:");
            Console.WriteLine(translatedResult.Result.Text);

            Console.WriteLine("-------------------------------------------");
        }
    }

    public async Task CreateAgent_V2(IConfiguration config)
    {
        var openAPIKey = config["OpenAi:ApiKey"];
        var agent = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        ChatOptions = new ChatOptions
                        {
                            Instructions = """Você é um agente especialista em responder gentilmente as pessoas""",
                        },
                        Name = "PoliteAgent",
                        ChatHistoryProvider = new LocalFileChatHistoryProvider("PATH_XXX")
                    });

        var session = await agent.CreateSessionAsync();
        Console.WriteLine(await agent.RunAsync("Olá, meu nome é GENAI", session));
        Console.WriteLine(await agent.RunAsync("Qual é o meu nome", session));
    }

    public async Task CreateAgentWorkFlow_V1(IConfiguration config)
    {
        var openAPIKey = config["OpenAi:ApiKey"];

        var agentRedator = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        ChatOptions = new ChatOptions
                        {
                            Instructions = """
                            Você é um redator técnico especializado em .NET e C#.
                            Receba o tópico fornecido e escreva um rascunho de artigo técnico
                            com introdução, desenvolvimento e conclusão.
                            Seja direto e preciso.
                            """,
                        },
                        Name = "RedatorAgent"
                    });

        var agentRevisor = new OpenAIClient(openAPIKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        ChatOptions = new ChatOptions
                        {
                            Instructions = """
                            Você é um revisor de conteúdo técnico.
                            Receba o rascunho e melhore a clareza, corrija imprecisões técnicas
                            e garanta que o texto está adequado para desenvolvedores .NET.
                            Retorne o texto revisado e melhorado.
                            """,
                        },
                        Name = "RevisorAgent"
                    });

        var agentSeo = new OpenAIClient(openAPIKey)
            .GetChatClient("gpt-4o-mini")
            .AsAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    Instructions = """
                            Você é um especialista em SEO para conteúdo técnico.
                            Receba o artigo revisado e gere:
                            1. Um título otimizado para SEO
                            2. Uma meta description de até 160 caracteres começando com "Neste artigo"
                            3. 5 tags relevantes
                            """,
                },
                Name = "SEO"
            });

        var workflow = new WorkflowBuilder(agentRedator)
            .AddEdge(agentRedator, agentRevisor)
            .AddEdge(agentRevisor, agentSeo)
            .Build();

        await using var run = await InProcessExecution.RunStreamingAsync(
            workflow,
            new ChatMessage(ChatRole.User, "Escreva um artigo sobre as novidades do C# 10")
        );

        // TurnToken (Trabalhando com mensagem em memoria entre agentes)
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        await foreach (var evt in run.WatchStreamAsync())
            if (evt is AgentResponseUpdateEvent update)
                Console.WriteLine($"[{update.ExecutorId}]: {update.Data}");

    }
}

file class LocalFileChatHistoryProvider : ChatHistoryProvider
{
    private readonly ProviderSessionState<State> _sessionState;
    private readonly string _file;

    public LocalFileChatHistoryProvider(string file)
    {
        _file = file;
        _sessionState = new ProviderSessionState<State>(
            stateInitializer: _ => LoadFromFile(),
            stateKey: GetType().Name
        );
    }

    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    => new(_sessionState.GetOrInitializeState(context.Session).Messages);

    protected override ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
        state.Messages.AddRange(allNewMessages);

        _sessionState.SaveState(context.Session, state);
        SaveToFile(state);

        return default;
    }

    private State LoadFromFile()
    {
        if (!File.Exists(_file))
            return new State();


        var json = File.ReadAllText(_file);
        var state = JsonSerializer.Deserialize<State>(json);
        return state ?? new State();
    }

    private void SaveToFile(State state)
    {
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_file, json);
    }
}

file class State
{
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];
}