namespace GenAiAgent.AI.MultiAgentApproveCredit;

public class PlannerAgent
{
    public ChatClientAgent Create(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            instructions: """
        Analise a solicitação e determine
        quais verificações serão necessárias.
        """);
    }
}
