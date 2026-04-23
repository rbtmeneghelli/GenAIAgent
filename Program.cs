using GenAIAgent;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

IGenAIFactory genAIFactory = new GenAIFactory();
await genAIFactory.CreateAgent(config);
await genAIFactory.CreateAgent_V1(config);
await genAIFactory.CreateAgent_V2(config);
await genAIFactory.CreateAgentWorkFlow_V1(config);
