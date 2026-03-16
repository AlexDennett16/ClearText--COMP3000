namespace ClearText.DataObjects;

public class DocumentStats
{
    public required string WordCount { get; set; }
    public required string CharacterCount { get; set; }
    public required string? FleschKincaidGradeLevel { get; set; }
    public required string FleschKincaidBreakdown { get; set; }
}