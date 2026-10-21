using Xunit;

/// Tests for Media.ParseDuration, which reads a duration a person typed.
///
/// The unit is mandatory. Callers work in different units -- media-split-equal in
/// seconds, media-length-move in minutes -- so a bare number would mean whatever that
/// program happens to use, which whoever typed it has no reason to know.
public class DurationTests
{
    [Theory]
    [InlineData("90s", 90)] [InlineData("39m", 2340)] [InlineData("1h", 3600)]
    [InlineData("5m", 300)] [InlineData("7.5m", 450)] [InlineData("1.5h", 5400)]
    [InlineData("600s", 600)] [InlineData("0.5h", 1800)]
    public void ReadsANumberAndAUnit(string text, int expected)
        => Assert.Equal(expected, Media.ParseDuration(text));

    [Theory]
    [InlineData("5M", 300)] [InlineData("1H", 3600)] [InlineData("90S", 90)]
    public void TheUnitIsCaseInsensitive(string text, int expected)
        => Assert.Equal(expected, Media.ParseDuration(text));

    [Theory]
    [InlineData(" 10m ", 600)] [InlineData("2 m", 120)] [InlineData("1 h", 3600)]
    public void SpacingIsForgiven(string text, int expected)
        => Assert.Equal(expected, Media.ParseDuration(text));

    [Theory]
    // A bare number is the whole point of the exercise: "1" from someone thinking in
    // minutes would silently mean one second wherever seconds are the caller's unit.
    [InlineData("600")] [InlineData("1")] [InlineData("0.5")] [InlineData("39")]
    public void ABareNumberIsRefused(string text)
        => Assert.Null(Media.ParseDuration(text));

    [Theory]
    [InlineData("")] [InlineData("   ")] [InlineData("abc")] [InlineData("m5")]
    [InlineData("5d")] [InlineData("5mm")] [InlineData("5m30s")] [InlineData("-5m")]
    [InlineData("1h30m")] [InlineData("five minutes")] [InlineData("5 m s")]
    public void NonsenseIsRefused(string text)
        => Assert.Null(Media.ParseDuration(text));

    [Theory]
    // Zero is not a duration, however it is spelled.
    [InlineData("0s")] [InlineData("0m")] [InlineData("0h")] [InlineData("0.0h")]
    public void ZeroIsRefused(string text)
        => Assert.Null(Media.ParseDuration(text));

    [Fact]
    public void RoundsToWholeSeconds()
    {
        // 0.51 of a minute is 30.6 seconds; the caller wants a whole number.
        Assert.Equal(31, Media.ParseDuration("0.51m"));
        Assert.Equal(1, Media.ParseDuration("0.6s"));
    }

    [Fact]
    public void AValueTooSmallToRoundUpToASecondIsRefused()
    {
        // Rather than silently becoming zero and being treated as "no limit".
        Assert.Null(Media.ParseDuration("0.4s"));
    }
}
