using GenAIAgent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

IGenAIFactory genAIFactory = new GenAIFactory();
await genAIFactory.CreateAgent();
await genAIFactory.CreateAgent_V1(config);
await genAIFactory.CreateAgent_V2(config);
