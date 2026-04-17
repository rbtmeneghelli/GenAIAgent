using GenAIAgent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

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
    /* Respondendo igual ao chatGPT */
    //await foreach (var token in agent.RunStreamingAsync(prompt ?? ))
    //{
    //    Console.WriteLine(token);
    //}

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
