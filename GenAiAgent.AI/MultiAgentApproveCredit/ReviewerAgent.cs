namespace GenAiAgent.AI.MultiAgentApproveCredit;

public class ReviewerAgent
{
    public ChatClientAgent Create(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            instructions: """
        Revise todas as respostas
        recebidas dos demais agentes.
        """);
    }
}
