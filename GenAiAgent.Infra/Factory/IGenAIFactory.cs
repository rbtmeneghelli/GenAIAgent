using GenAiAgent.Core.Models;
using OpenAI.Chat;

namespace GenAiAgent.Infra.Factory;

public interface IGenAIFactory
{
    Task CreateAgent();
    Task CreateAgent_V1();
    Task CreateAgent_V2();
    Task CreateAgentWorkFlow_V1();
    void CreateAgentMLNET(FeelingData feelingData);
    Task<ChatClient> CreateAzureAgent();
    Task<string> UseAzureAgent(ChatClient chatClient, string ask);
    Task UseAnthropicFromGenerateAI(string contentMessage, bool autoconfig = true, bool applyStream = true);
    Task UseAnthropicMCP();
    Task CreateAndUseMultiAgent(string request);
}
