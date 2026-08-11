using GenAiAgent.AI.MultiAgentApproveCredit.Tools;

namespace GenAiAgent.AI.MultiAgentApproveCredit;

public class ComplianceAgent
{
    public ComplianceAgent() { }

    public ChatClientAgent Create(IChatClient chatClient)
    {
        var AITools = new List<AITool>
        {
            AIFunctionFactory.Create(
                ComplianceTool.CheckCompliance,
                name: "check_compliance",
                description: "Verifica se o cliente está em conformidade com as políticas internas.")
        };

        return new ChatClientAgent(
            chatClient,
            instructions: """ Verifique políticas internas e regras de compliance. """,
            tools: AITools
        );
    }
}
