namespace DataManagement.Models;

public class Analysis
{
    public string Summary { get; set; }
    public int RiskScore { get; set; }
    public IEnumerable<string> Warnings { get; set; }
    public IEnumerable<string> Recommendations { get; set; }
}