using Microsoft.Extensions.Configuration;

namespace GenAIAgent;

public interface IGenAIFactory
{
    Task CreateAgent(IConfiguration config);
    Task CreateAgent_V1(IConfiguration config);
    Task CreateAgent_V2(IConfiguration config);
    Task CreateAgentWorkFlow_V1(IConfiguration config);
}
