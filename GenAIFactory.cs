using GenAIAgent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace GenAIAgent;

internal static class GenAIFactory : IGenAIFactory
{
    public static async Task CreateAgent()
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

        /* Respondendo igual ao chatGPT */
        await foreach (var token in agent.RunStreamingAsync(prompt ?? string.Empty, session))
        {
            Console.WriteLine(token);
        }
    }

    public static async Task CreateAgent_V1(IConfiguration config)
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

    public static async Task CreateAgent_V2(IConfiguration config)
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

        var session = agent.CreateSessionAsync();
        Console.WriteLine(agent.RunAsync("Olá, meu nome é GENAI", session));
        Console.WriteLine(agent.RunAsync("Qual é o meu nome", session));
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

    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(Invoking context, CancellationToken cancellationToken = new CancellationToken())
    => new(_sessionState.GetOrInitializeState(context.Session).Messages);

    protected override ValueTask StoreChatHistoryAsync(Invoking context, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = new CancellationToken())
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        var allNewMessages = context.RequestMesages.Concat(context.ResponseMessages ?? []);
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
        state ?? new State();
    }

    private void SaveToFile(State state)
    {
        var json = JsonSerializer.Serialize(state, new JsonSerializeOptions
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