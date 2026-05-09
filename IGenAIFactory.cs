using GenAIAgent.Models;

namespace GenAIAgent;

public interface IGenAIFactory
{
    Task CreateAgent();
    Task CreateAgent_V1();
    Task CreateAgent_V2();
    Task CreateAgentWorkFlow_V1();
    void CreateAgentMLNET(FeelingData feelingData);
}
