namespace FrontendTests.Services;

public class DocumentStatsServiceTests
{
    private readonly DocumentStatsService _service = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetDocumentStats_ShouldReturnZeroStats_WhenInputIsWhitespace(string input)
    {
        var result = _service.GetDocumentStats(input);

        result.WordCount.Should().Be("0");
        result.CharacterCount.Should().Be("0");
        result.FleschKincaidGradeLevel.Should().Be("0");
        result.FleschKincaidBreakdown.Should().Be("N/A");
    }


    [Fact]
    public void GetDocumentStats_ShouldCountWordsSentencesAndCharacters()
    {
        const string text = "Hello world. This is a test.";

        var result = _service.GetDocumentStats(text);

        result.WordCount.Should().Be("6");
        result.CharacterCount.Should().Be(text.Length.ToString());
    }


    [Fact]
    public void GetDocumentStats_ShouldProduceHigherGradeForComplexText()
    {
        const string simple = "The cat sat on the mat.";
        const string complex = "The philosophical implications of quantum entanglement challenge classical intuition.";

        var simpleStats = _service.GetDocumentStats(simple);
        var complexStats = _service.GetDocumentStats(complex);

        double.Parse(complexStats.FleschKincaidGradeLevel!)
            .Should().BeGreaterThan(double.Parse(simpleStats.FleschKincaidGradeLevel!));
    }

    [Theory]
    [InlineData("cat", 1)]
    [InlineData("reading", 2)]
    [InlineData("beautiful", 3)]
    [InlineData("queue", 1)]
    public void CountSyllables_ShouldReturnExpectedValues(string word, int expected)
    {
        var method = typeof(DocumentStatsService)
            .GetMethod("CountSyllables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = (int)(method?.Invoke(null, [word]) ?? throw new InvalidOperationException());

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(2.5, "Primary School")]
    [InlineData(5.0, "Primary KS2")]
    [InlineData(8.0, "Secondary School")]
    [InlineData(11.0, "Secondary School")]
    [InlineData(15.0, "University")]
    public void GetReadabilityDescription_ShouldReturnCorrectCategory(double fkgl, string expected)
    {
        var result = DocumentStatsService.GetReadabilityDescription(fkgl);

        result.Should().Contain(expected);
    }
}