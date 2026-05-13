namespace FrontendTests.DataObjects;

public class DocumentStatsDataObjectTests
{
    [Fact]
    public void DocumentStats_ShouldRequireAllProperties()
    {
        var act = new DocumentStats
        {
            WordCount = "10",
            CharacterCount = "50",
            FleschKincaidGradeLevel = "3.2",
            FleschKincaidBreakdown = "Primary School"
        };

        act.WordCount.Should().Be("10");
        act.CharacterCount.Should().Be("50");
        act.FleschKincaidGradeLevel.Should().Be("3.2");
        act.FleschKincaidBreakdown.Should().Be("Primary School");
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