using Anthropic;
using Anthropic.Models.Messages;
using Azure;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using GenAiAgent.AI.MultiAgentApproveCredit;
using GenAiAgent.AI.MultiAgentManualOrchestrator;
using GenAiAgent.Core;
using GenAiAgent.Core.Models;
using GenAIAgent;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.ML;
using ModelContextProtocol.Client;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace GenAiAgent.Infra.Factory;

/// <summary>
/// Quando usar agentes de IA
/// Agentes são especialmente úteis em cenários como:
/// - assistentes corporativos
/// -automação de atendimento
/// -suporte técnico inteligente
/// -consulta de documentos
/// -assistentes de vendas
/// -integração com sistemas empresariais
/// </summary>

public class GenAIFactory : IGenAIFactory
{
    private readonly IChatClient _chatClient;
    private readonly OrcherstratorAgent _OrcherstratorAgent;
    private readonly AI.MultiAgentApproveCredit.PlannerAgent _planner;
    private readonly CreditAgent _credit;
    private readonly ComplianceAgent _compliance;
    private readonly AI.MultiAgentApproveCredit.ReviewerAgent _reviewer;
    private readonly ResponseAgent _response;

    public GenAIFactory(
        IChatClient chatClient,
        OrcherstratorAgent orcherstratorAgent,
        AI.MultiAgentApproveCredit.PlannerAgent planner,
        CreditAgent credit,
        ComplianceAgent compliance,
        AI.MultiAgentApproveCredit.ReviewerAgent reviewer,
        ResponseAgent response)
    {
        _chatClient = chatClient;
        _OrcherstratorAgent = orcherstratorAgent;
        _planner = planner;
        _credit = credit;
        _compliance = compliance;
        _reviewer = reviewer;
        _response = response;
    }

    public async Task CreateAgent()
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que pode fornecer informações sobre o clima.""", tools:
                    [
                        AIFunctionFactory.Create(WeatherTool.GetWeather)
                    ]);

        var agent2 = new OpenAIClient(Configuration.OpenAi.ApiKey)
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

    public async Task CreateAgent_V1()
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que pode fornecer informações sobre o clima.""", tools:
                    [
                        AIFunctionFactory.Create(WeatherTool.GetWeather)
                    ]);

        var agent2 = new OpenAIClient(Configuration.OpenAi.ApiKey)
                    .GetChatClient("gpt-4o-mini")
                    .AsAIAgent("""Você é um assistente que irá receber um texto em idioma português e ira traduzir o texto para o idioma em inglês""");

        var session = await agent.CreateSessionAsync();
        var session2 = await agent2.CreateSessionAsync();

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

    public async Task CreateAgent_V2()
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
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

    public async Task CreateAgentWorkFlow_V1()
    {
        var agentRedator = new OpenAIClient(Configuration.OpenAi.ApiKey)
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

        var agentRevisor = new OpenAIClient(Configuration.OpenAi.ApiKey)
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

        var agentSeo = new OpenAIClient(Configuration.OpenAi.ApiKey)
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

    public void CreateAgentMLNET(FeelingData feelingData)
    {
        Console.WriteLine("Iniciando o processo de treinamento do modelo de análise de sentimento...\n");
        var context = new MLContext();

        var data = new List<FeelingData>
        {
            new FeelingData { Text = "Isso é muito bom", Label = true },
            new FeelingData { Text = "Gostei bastante", Label = true },
            new FeelingData { Text = "O livro é excelente", Label = true },
            new FeelingData { Text = "Isso é ruim", Label = false },
            new FeelingData { Text = "A experiência foi Horrível", Label = false }
        };

        // Convert to IDataView
        var dataView = context.Data.LoadFromEnumerable(data);

        // Pipeline
        var pipeline = context.Transforms.Text
                       .FeaturizeText("Features", nameof(FeelingData.Text))
                       .Append(context.BinaryClassification.Trainers.SdcaLogisticRegression());

        // Training
        var model = pipeline.Fit(dataView);

        var predictor = context.Model.CreatePredictionEngine<FeelingData, FeelingPrediction>(model);

        Console.WriteLine($"Texto : {feelingData.Text}\n");

        var resultado = predictor.Predict(feelingData);

        Console.WriteLine($"Predição: {resultado.PredictedLabel}");
        Console.WriteLine($"Probabilidade: {resultado.Probability}");

        Console.ReadLine();
    }

    public async Task<ChatClient> CreateAzureAgent()
    {
        // 1. Criar client do projeto
        var endpoint = new Uri("<endpoint>");
        var credential = new DefaultAzureCredential();

        var projectClient = new AIProjectClient(endpoint, credential);

        // 2. Obter conexão com OpenAI dentro do Azure AI Foundry
        var connection = projectClient.GetConnection(typeof(AzureOpenAIClient).FullName!);

        if (!connection.TryGetLocatorAsUri(out var uri))
            throw new Exception("Invalid connection");

        // 3. Criar client de inferência
        var openAiClient = new AzureOpenAIClient(uri, credential);

        // 4. Criar client de chat
        return openAiClient.GetChatClient("gpt-4o-mini");
    }

    public async Task<string> UseAzureAgent(ChatClient chatClient, string ask)
    {
        // 5. Executar chat
        var response = chatClient.CompleteChat(ask);
        var answer = response.Value.Content[0].Text;
        return answer;
    }

    /// <summary>
    /// Documentação oficial >> https://platform.claude.com/docs/en/api/sdks/csharp
    /// </summary>
    /// <returns></returns>
    public async Task UseAnthropicFromGenerateAI(string contentMessage, bool autoconfig = true, bool applyStream = true)
    {
        AnthropicClient client = autoconfig ?
                                 new() : // Configure the client using environment variables (ANTHROPIC_API_KEY (string), ANTHROPIC_AUTH_TOKEN (string) and ANTHROPIC_BASE_URL (string))
                                 new() { ApiKey = "my-anthropic-api-key" }; // Manually configuration

        MessageCreateParams parameters = new()
        {
            MaxTokens = 1024,
            Messages =
            [
                new()
        {
            Role = Role.User,
            Content = contentMessage,
        },
            ],
            Model = Model.ClaudeOpus4_7,
        };

        if (applyStream)
        {
            await foreach (var message in client.Messages.CreateStreaming(parameters))
            {
                Console.WriteLine(message);
            }
        }
        else
        {
            var message = await client.WithOptions(options =>
                          options with
                          {
                              BaseUrl = "https://example.com",
                              Timeout = TimeSpan.FromSeconds(42),
                          }
                          )
                          .Messages.Create(parameters);

            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Documentação oficial >> https://platform.claude.com/docs/en/api/sdks/csharp
    /// </summary>
    /// <returns></returns>
    public async Task UseAnthropicMCP()
    {
        // Configured using the ANTHROPIC_API_KEY, ANTHROPIC_AUTH_TOKEN and ANTHROPIC_BASE_URL environment variables
        AnthropicClient client = new();

        IChatClient chatClient = client.AsIChatClient("claude-opus-4-8")
                                 .AsBuilder()
                                 .UseFunctionInvocation()
                                 .Build();

        // Using McpClient from the MCP C# SDK
        McpClient learningServer = await McpClient.CreateAsync(new ModelContextProtocol.Client.HttpClientTransport(new() { Endpoint = new("https://learn.microsoft.com/api/mcp") }));

        ChatOptions options = new() { Tools = [.. await learningServer.ListToolsAsync()] };

        Console.WriteLine(await chatClient.GetResponseAsync("Tell me about IChatClient", options));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task CreateAndUseMultiAgent(string request)
    {
        await _OrcherstratorAgent.ExecuteAsync(request);
    }

    public async Task CreateMultiAgentToApproveCredit(string prompt)
    {
        var planner = _planner.Create(_chatClient).BindAsExecutor();
        var credit = _credit.Create(_chatClient).BindAsExecutor();
        var compliance = _compliance.Create(_chatClient).BindAsExecutor();
        var reviewer = _reviewer.Create(_chatClient).BindAsExecutor();
        var response = _response.Create(_chatClient).BindAsExecutor();

        var workflow = new WorkflowBuilder(planner)
            .AddFanOutEdge(
                planner,
                new[]
                {
                credit,
                compliance
                })
            .AddFanInBarrierEdge(
                new[]
                {
                credit,
                compliance
                },
                reviewer)
            .AddEdge(reviewer, response)
            .Build();

        var result = await InProcessExecution.RunAsync(workflow,prompt);

        foreach (var evt in result.NewEvents)
        {
            if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine(output.Data);
            }
        }
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