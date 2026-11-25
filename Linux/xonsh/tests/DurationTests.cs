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
    [InlineData("5d")] [InlineData("5mm")] [InlineData("-5m")]
    [InlineData("five minutes")] [InlineData("5 m s")]
    public void NonsenseIsRefused(string text)
        => Assert.Null(Media.ParseDuration(text));

    // ── compound durations ───────────────────────────────────────────────────
    //
    // "5m30s" and "1h30m" were refused until compound forms were allowed, and these
    // two cases were in NonsenseIsRefused above. The failure when that changed is the
    // reason they were worth having: the tests said plainly which behaviour was being
    // altered, rather than leaving it to be noticed later.

    [Theory]
    [InlineData("1h25m", 5100)]
    [InlineData("1h 25m", 5100)]
    [InlineData("5m30s", 330)]
    [InlineData("1h30m", 5400)]
    [InlineData("2h30m15s", 9015)]
    [InlineData("1h0m1s", 3601)]
    [InlineData("1H25M", 5100)]
    public void PartsAddUp(string text, int expected)
        => Assert.Equal(expected, Media.ParseDuration(text));

    [Fact]
    public void AFractionInACompoundStillCounts()
    {
        // Odd to type, but unambiguous: an hour and a half, then half an hour.
        Assert.Equal(7200, Media.ParseDuration("1.5h30m"));
    }

    [Theory]
    // Out of order, and repeated. Both are far likelier to be a slip than an
    // intention, and the dialogs simply ask again -- so a refusal costs one retype
    // where a guess costs a wrong split.
    [InlineData("25m1h")]
    [InlineData("30s5m")]
    [InlineData("1h1h")]
    [InlineData("5m5m")]
    [InlineData("1h30m2h")]
    public void OutOfOrderOrRepeatedIsRefused(string text)
        => Assert.Null(Media.ParseDuration(text));

    [Theory]
    // The property the mandatory unit exists for, which compounds must not weaken: a
    // number with no unit of its own means whatever the program downstream happens to
    // use, and "1" meaning a second where a minute was meant makes thousands of files.
    [InlineData("1h25")]
    [InlineData("25 1h")]
    [InlineData("1h 25 m 30")]
    public void EveryPartNeedsItsOwnUnit(string text)
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
