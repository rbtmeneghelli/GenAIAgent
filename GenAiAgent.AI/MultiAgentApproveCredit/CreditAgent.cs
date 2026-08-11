using GenAiAgent.AI.MultiAgentApproveCredit.Tools;

namespace GenAiAgent.AI.MultiAgentApproveCredit;

public class CreditAgent
{
    public CreditAgent() { }

    public ChatClientAgent Create(IChatClient chatClient)
    {
        var AITools = new List<AITool>
        {
            AIFunctionFactory.Create(
                CreditTool.GetCreditScore,
                name: "check_credit_score",
                description: "Verifica o score de crédito do cliente.")
        };

        return new ChatClientAgent(
            chatClient,
            instructions: """ Consulte o histórico financeiro e o score do cliente. """,
            tools: AITools
        );
    }
}
