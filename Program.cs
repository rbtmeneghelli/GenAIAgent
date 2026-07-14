using GenAiAgent.AI;
using GenAiAgent.Core;
using GenAiAgent.Core.Constants;
using GenAiAgent.Core.Models;
using GenAiAgent.Infra;
using GenAiAgent.Infra.Factory;
using GenAIAgent.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

bool runProgram = true;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddAgents();

builder.Services.AddHostedService<NewsletterWorker>();

builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

Configuration.OpenAi.ApiKey = builder.Configuration["OpenAi:ApiKey"] ?? throw new Exception("OpenAI API Key not found in configuration");

var environment = app.Services.GetRequiredService<IHostEnvironment>();
Configuration.RootPath = environment.ContentRootPath;

do
{
    Console.Clear();

    Console.WriteLine("Escolha uma das opções disponiveis de 0 a 5, abaixo: ");
    Console.WriteLine("0 - Sair do programa");
    Console.WriteLine("1 - Chamar código de agente padrão");
    Console.WriteLine("2 - Chamar código de agente V1");
    Console.WriteLine("3 - Chamar código de agente V2");
    Console.WriteLine("4 - Chamar código de agente WorkFlow V1");
    Console.WriteLine("5 - Chamar um serviço de aprendizado de maquina");
    Console.WriteLine("6 - Chamar um código de agente do azure AI foundry");
    Console.WriteLine("7 - Chamar um código de IA generativa da Anthropic similar ao (ChatGpt, Github Copilot, DeepSeek, Gemini e etc...)");
    Console.WriteLine("8 - Chamar um código para simular um ChatClient da Anthropic");
    Console.WriteLine("9 - Chamar um código de multiagentes com orquestração manual");

    string? userChoice = Console.ReadLine();

    int.TryParse(userChoice, out int choice);

    if (choice < 0 || choice > 9)
    {
        ConsoleAppExtension.ShowConsoleMessage(FixConstant.WRONG_CHOICE);
        continue;
    }

    IGenAIFactory genAIFactory = app.Services.GetRequiredService<IGenAIFactory>();

    switch (choice)
    {
        case 0:
            runProgram = false;
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.EXIT_CHOICE);
            break;
        case 1:
            await genAIFactory.CreateAgent();
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 2:
            await genAIFactory.CreateAgent_V1();
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 3:
            await genAIFactory.CreateAgent_V2();
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 4:
            await genAIFactory.CreateAgentWorkFlow_V1();
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 5:
            genAIFactory.CreateAgentMLNET(new FeelingData { Text = "Isso é excelente!" });
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 6:
            var azureAgent = await genAIFactory.CreateAzureAgent();
            Console.WriteLine("Digite a pergunta para o agente Azure AI Foundry");
            var azureFoundryAsk = Console.ReadLine();
            var azureFoundryResult = await genAIFactory.UseAzureAgent(azureAgent, azureFoundryAsk ?? "Explique .NET 8");
            Console.WriteLine($"A resposta da sua pergunta é: {azureFoundryResult}");
            ConsoleAppExtension.ShowConsoleMessage(azureFoundryResult);
            break;
        case 7:
            Console.WriteLine("Digite uma pergunta para a AI generativa da anthropic");
            var anthropicAsk = Console.ReadLine();
            await genAIFactory.UseAnthropicFromGenerateAI(anthropicAsk);
            break;
        case 8:
            await genAIFactory.UseAnthropicMCP();
            break;
        case 9:
            await genAIFactory.CreateAndUseMultiAgent(""" Criar uma API ASP.NET Core para cadastro de clientes utilizando EF Core e o SQLite Use EnsureCreated() para criar o banco """);
            break;
    }
} while (runProgram);

file static class ConsoleAppExtension
{
    public static void ShowConsoleMessage(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
        Console.ReadKey();
    }
}