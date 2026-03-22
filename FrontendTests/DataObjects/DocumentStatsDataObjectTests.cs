namespace FrontendTests.DataObjects;
public class DocumentStatsDataObjectTests
{
    [Fact]
    public void DocumentStats_ShouldRequireAllProperties()
    {
        Action act = () => new DocumentStats
        {
            WordCount = "10",
            CharacterCount = "50",
            FleschKincaidGradeLevel = "3.2",
            FleschKincaidBreakdown = "Primary School"
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void DocumentStats_ShouldSerializeAndDeserializeCorrectly()
    {
        var original = new DocumentStats
        {
            WordCount = "10",
            CharacterCount = "50",
            FleschKincaidGradeLevel = "3.2",
            FleschKincaidBreakdown = "Primary School"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DocumentStats>(json);

        deserialized.Should().BeEquivalentTo(original);
    }
}