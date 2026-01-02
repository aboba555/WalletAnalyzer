namespace BusinessLogic.DTO.OpenAI;

public class GptResponse
{
    public string Summary { get; set; }
    public int RiskScore { get; set; }
    public List<string> Warnings { get; set; }
    public List<string> Recommendations { get; set; }
}