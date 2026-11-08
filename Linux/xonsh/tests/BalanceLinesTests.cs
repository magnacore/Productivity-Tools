using Xunit;

/// Tests for folding a watermark onto several lines.
///
/// A rotated stamp costs the sum of its sides: at 45 degrees a label of w by h needs
/// (w + h) / root 2 in both directions. So one long line is the worst possible shape.
/// The sentence that prompted this set to 1760x88, wanting 1307x1307 on an image only
/// 1080 tall -- one clipped copy. The same words over four lines measure about 394x528,
/// rotate into 652x652, and tile three across at the same point size.
///
/// The rule this must never break: words stay whole. ImageMagick's own caption: wraps
/// on width alone and rendered DRAFT as "DR" above "AFT", which is why the line breaks
/// are chosen here instead.
public class BalanceLinesTests
{
    [Theory]
    [InlineData("DRAFT", 1)]
    [InlineData("DRAFT", 2)]
    [InlineData("DRAFT", 9)]
    public void ASingleWordIsNeverSplit(string text, int lines)
        => Assert.Equal("DRAFT", Media.BalanceLines(text, lines));

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-3)]
    public void OneLineOrFewerLeavesTheTextAsItWas(int lines)
        => Assert.Equal("Not For Sale", Media.BalanceLines("Not For Sale", lines));

    [Fact]
    public void MoreLinesThanWordsGivesOneWordEach()
    {
        string wrapped = Media.BalanceLines("Not For Sale", 10);

        Assert.Equal(["Not", "For", "Sale"], wrapped.Split('\n'));
    }

    [Fact]
    public void EveryWordSurvivesInOrder()
    {
        const string Text = "Not For Sale. Only for review by manuj chandra sharma";

        string wrapped = Media.BalanceLines(Text, 4);

        // The words, and only the words, in the order they were given.
        Assert.Equal(Text.Split(' '), wrapped.Replace('\n', ' ').Split(' '));
    }

    [Fact]
    public void NoLineIsEmpty()
    {
        string wrapped = Media.BalanceLines("Not For Sale. Only for review by manuj chandra sharma", 4);

        Assert.All(wrapped.Split('\n'), line => Assert.NotEqual(string.Empty, line.Trim()));
    }

    [Fact]
    public void AsksForNoMoreLinesThanItWasGiven()
    {
        for (int lines = 1; lines <= 6; lines++)
        {
            string wrapped = Media.BalanceLines("one two three four five six seven eight", lines);

            Assert.True(wrapped.Split('\n').Length <= lines);
        }
    }

    [Fact]
    public void TheLinesComeOutSimilarLengths()
    {
        // Similar lengths are the point: they make the label squarest, and a square
        // label has the smallest rotated footprint for a given amount of text.
        string[] got = Media.BalanceLines(
            "Not For Sale. Only for review by manuj chandra sharma", 4).Split('\n');

        int shortest = got.Min(line => line.Length);
        int longest = got.Max(line => line.Length);

        Assert.Equal(4, got.Length);
        Assert.True(longest - shortest <= 10, $"spread of {longest - shortest}: {string.Join(" | ", got)}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTextIsHandedBackUnchanged(string text)
        => Assert.Equal(text, Media.BalanceLines(text, 3));

    [Fact]
    public void RunsOfWhitespaceCollapse()
    {
        // Split on whitespace means tabs and double spaces do not become empty words.
        string wrapped = Media.BalanceLines("Not   For\tSale", 2);

        Assert.DoesNotContain("  ", wrapped, System.StringComparison.Ordinal);
        Assert.Equal(["Not", "For", "Sale"], wrapped.Replace('\n', ' ').Split(' '));
    }

    [Fact]
    public void TheWrappedShapeIsActuallySmallerWhenRotated()
    {
        // The property the whole thing exists for, using the measured label sizes from
        // the real sentence at 72pt: one line 1760x88, four lines 394x528.
        (double oneW, double oneH) = Media.RotatedSize(1760, 88, 45);
        (double fourW, double fourH) = Media.RotatedSize(394, 528, 45);

        Assert.True(fourH < oneH / 1.9, $"{fourH:F0} should be far under {oneH:F0}");
        Assert.True(fourH < 1080, "and must fit inside a 1080 high image");
        Assert.True(oneH > 1080, "where one line did not");
        Assert.True(fourW < oneW);
    }
}
