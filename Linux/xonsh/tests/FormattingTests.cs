using System.Numerics;
using Xunit;

// Media duration, as media-length prints it.
public class HmsTests
{
    [Theory]
    [InlineData(0, "0:00:00")]
    [InlineData(59, "0:00:59")]
    [InlineData(60, "0:01:00")]
    [InlineData(3600, "1:00:00")]
    [InlineData(36000, "10:00:00")]
    [InlineData(-90, "-0:01:30")]
    public void FormatsAsHoursMinutesSeconds(double seconds, string expected)
        => Assert.Equal(expected, Media.Hms(seconds));

    [Fact]
    public void RoundsRatherThanTruncating()
        => Assert.Equal("0:00:02", Media.Hms(1.6));
}

// Spelled-out numbers, used by password-generate to describe a keyspace. Must take
// BigInteger, because the seed-phrase figures run past 2^130.
public class ToWordsTests
{
    [Theory]
    [InlineData(0, "zero")]
    [InlineData(7, "seven")]
    [InlineData(21, "twenty-one")]
    [InlineData(100, "one hundred")]
    [InlineData(1000, "one thousand")]
    public void SpellsSmallNumbers(long n, string expected)
        => Assert.Equal(expected, Num.ToWords(n));

    [Fact]
    public void JoinsTheFinalGroupWithAnd()
    {
        // The rule that was wrong in an early port: a trailing group below one
        // hundred is joined with " and ".
        Assert.Equal("one million, two thousand and sixty-nine", Num.ToWords(1_002_069));
    }

    [Fact]
    public void HandlesNumbersFarPastLong()
    {
        // 2^130 is 1.361e39, so the leading group is 10^39 — duodecillion.
        var huge = BigInteger.Pow(2, 130);
        var words = Num.ToWords(huge);
        Assert.StartsWith("one duodecillion", words);
        Assert.DoesNotContain("  ", words);
    }
}

// Progress-bar descriptions are padded to one width so the bars do not slide left
// and right as filenames change length.
public class FitDescriptionTests
{
    [Fact]
    public void PadsEveryDescriptionToTheSameWidth()
    {
        var shortOne = Ui.FitDescription("a.mkv:");
        var longOne = Ui.FitDescription("Animal Rights [2nd Edition] and then some more.mkv:");
        Assert.Equal(shortOne.Length, longOne.Length);
    }

    [Fact]
    public void CutsFromTheMiddleSoTheTailSurvives()
    {
        // Two titles differing only at the end must stay distinguishable.
        var rights = Ui.FitDescription("Splitting No-Nonsense Guide to Animal Rights.pdf:");
        var welfare = Ui.FitDescription("Splitting No-Nonsense Guide to Animal Welfare.pdf:");
        Assert.NotEqual(rights, welfare);
        Assert.Contains("…", rights);
    }

    [Fact]
    public void LeavesShortTextIntactApartFromPadding()
        => Assert.Equal("short:", Ui.FitDescription("short:").TrimEnd());
}

public class TitleCaseTests
{
    [Theory]
    [InlineData("the great escape", "The Great Escape")]
    [InlineData("hyphen-separated_words", "Hyphen Separated Words")]
    [InlineData("ALL CAPS", "All Caps")]
    public void CapitalisesEachWord(string input, string expected)
        => Assert.Equal(expected, Fs.TitleCase(input));
}
