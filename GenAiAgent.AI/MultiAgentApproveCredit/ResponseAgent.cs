namespace GenAiAgent.AI.MultiAgentApproveCredit;

public class ResponseAgent
{
    public ChatClientAgent Create(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            instructions: """
        Gere uma resposta final
        para o usuário.
        """);
    }
}
