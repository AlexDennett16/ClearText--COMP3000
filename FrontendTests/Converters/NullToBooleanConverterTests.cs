namespace FrontendTests.Converters;

public class NullToBoolConverterTests
{
    private readonly NullToBoolConverter _converter = new();

    [Theory]
    [InlineData(null!, false)]
    [InlineData("", true)]
    [InlineData("hello", true)]
    [InlineData(0, true)]
    public void Convert_ShouldReturnTrue_WhenValueIsNotNull(object? input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), null, null);

        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotSupportedException()
    {
        var act = () => _converter.ConvertBack(true, typeof(object), null, null);

        act.Should().Throw<NotSupportedException>();
    }
}