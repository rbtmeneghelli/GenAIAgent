namespace GenAiAgent.Core.Models;

public class FeelingPrediction
{
    public bool PredictedLabel { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}
