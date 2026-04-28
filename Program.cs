using GenAIAgent;
using GenAIAgent.Constants;
using GenAIAgent.Models;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
bool runProgram = true;

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

    string? userChoice = Console.ReadLine();

    int.TryParse(userChoice, out int choice);

    if (choice < 0 || choice > 5)
    {
        ConsoleAppExtension.ShowConsoleMessage(FixConstant.WRONG_CHOICE);
        continue;
    }

    IGenAIFactory genAIFactory = new GenAIFactory();

    switch (choice)
    {
        case 0:
            runProgram = false;
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.EXIT_CHOICE);
            break;
        case 1:
            await genAIFactory.CreateAgent(config);
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 2:
            await genAIFactory.CreateAgent_V1(config);
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 3:
            await genAIFactory.CreateAgent_V2(config);
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 4:
            await genAIFactory.CreateAgentWorkFlow_V1(config);
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
        case 5:
            genAIFactory.CreateAgentMLNET(new FeelingData { Text = "Isso é excelente!" });
            ConsoleAppExtension.ShowConsoleMessage(FixConstant.RIGHT_CHOICE);
            break;
    }


}
while (runProgram);

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