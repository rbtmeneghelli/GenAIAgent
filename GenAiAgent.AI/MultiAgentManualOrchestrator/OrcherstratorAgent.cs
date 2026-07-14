namespace GenAiAgent.AI.MultiAgentManualOrchestrator;

public sealed class OrcherstratorAgent
{
    private readonly PlannerAgent _plannerAgent;
    private readonly DeveloperAgent _developerAgent;
    private readonly ReviewerAgent _reviewerAgent;

    public OrcherstratorAgent(PlannerAgent plannerAgent, DeveloperAgent developerAgent, ReviewerAgent reviewerAgent)
    {
        _plannerAgent = plannerAgent;
        _developerAgent = developerAgent;
        _reviewerAgent = reviewerAgent;
    }

    public async Task ExecuteAsync(string request)
    {
        Console.WriteLine("======== PLANEJADOR ========");

        var plan = await _plannerAgent.ExecuteAsync(request);

        Console.WriteLine(plan);
        Console.WriteLine();

        Console.WriteLine("====== DESENVOLVEDOR ======");

        var code = await _developerAgent.ExecuteAsync(plan);

        Console.WriteLine(code);
        Console.WriteLine();

        Console.WriteLine("========= REVISOR =========");

        var review = await _reviewerAgent.ExecuteAsync(code);

        Console.WriteLine(review);
        Console.WriteLine();
    }
}
