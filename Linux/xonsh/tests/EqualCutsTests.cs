using Xunit;

/// Tests for cutting a video into equal pieces short enough to post.
///
/// WhatsApp, Instagram and Facebook all stop at 60 seconds, so this decides whether a
/// video can be shared at all. Cutting every 59 seconds and leaving the remainder gave
/// a 120 second video pieces of 59, 59 and 2 -- the last of which nobody wants to post.
/// Equal pieces have no remainder to strand.
public class EqualCutsTests
{
    private const int Max = 59;
    private const int Fps = 30;

    /// The length of each piece, in seconds, that a set of cuts implies.
    private static List<double> Lengths(double duration, IReadOnlyList<int> cuts)
    {
        List<double> lengths = [];
        double previous = 0;

        foreach (int cut in cuts)
        {
            lengths.Add((cut / (double)Fps) - previous);
            previous = cut / (double)Fps;
        }

        lengths.Add(duration - previous);

        return lengths;
    }

    [Theory]
    [InlineData(10)]
    [InlineData(58)]
    [InlineData(59)]
    public void ShortEnoughIsLeftAlone(double duration)
        => Assert.Empty(Media.EqualCuts(duration, Max, Fps));

    [Fact]
    public void TheCaseThatPromptedThis()
    {
        // 120 seconds. Filling each piece gives 59, 59 and a 2 second stub; three
        // equal pieces give 40 seconds each.
        IReadOnlyList<int> cuts = Media.EqualCuts(120, Max, Fps);

        Assert.Equal([1200, 2400], cuts);
        Assert.All(Lengths(120, cuts), length => Assert.Equal(40, length, 6));
    }

    [Theory]
    [InlineData(60)]      // just over, and the worst case for a stub
    [InlineData(95)]
    [InlineData(118)]     // exactly two pieces
    [InlineData(119)]     // one second more, so three
    [InlineData(200)]
    [InlineData(3600)]
    [InlineData(59.5)]
    [InlineData(1000.37)]
    public void NoPieceIsEverTooLong(double duration)
    {
        List<double> lengths = Lengths(duration, Media.EqualCuts(duration, Max, Fps));

        // The guarantee the whole thing exists for. A hundredth of a second of slack,
        // for the rounding to a whole frame.
        Assert.All(lengths, length => Assert.True(length <= Max + 0.01,
                                                  $"{length:F3}s piece from {duration}s"));
    }

    [Theory]
    [InlineData(60)]
    [InlineData(95)]
    [InlineData(119)]
    [InlineData(200)]
    [InlineData(1000.37)]
    public void ThePiecesAreTheSameLength(double duration)
    {
        List<double> lengths = Lengths(duration, Media.EqualCuts(duration, Max, Fps));

        // Within a frame of each other: the cuts are rounded to whole frames, so an
        // odd duration cannot divide exactly.
        Assert.True(lengths.Max() - lengths.Min() <= 1.0 / Fps + 0.001,
                    $"spread {lengths.Max() - lengths.Min():F4}s in {duration}s");
    }

    [Theory]
    [InlineData(60, 2)]
    [InlineData(118, 2)]
    [InlineData(119, 3)]
    [InlineData(177, 3)]
    [InlineData(178, 4)]
    public void TheFewestPiecesThatWillDo(double duration, int pieces)
    {
        // One piece more than necessary would make them all shorter for nothing.
        Assert.Equal(pieces - 1, Media.EqualCuts(duration, Max, Fps).Count);
    }

    [Fact]
    public void NothingIsLost()
    {
        // The pieces have to add back up to the video.
        const double Duration = 1000.37;

        Assert.Equal(Duration, Lengths(Duration, Media.EqualCuts(Duration, Max, Fps)).Sum(), 6);
    }

    [Fact]
    public void CutsRiseAndNeverRepeat()
    {
        IReadOnlyList<int> cuts = Media.EqualCuts(600, Max, Fps);

        Assert.Equal(cuts.OrderBy(frame => frame), cuts);
        Assert.Equal(cuts.Distinct().Count(), cuts.Count);
        Assert.All(cuts, frame => Assert.True(frame > 0));
    }

    [Theory]
    [InlineData(0, Max, Fps)]
    [InlineData(-5, Max, Fps)]
    [InlineData(120, 0, Fps)]
    [InlineData(120, -1, Fps)]
    [InlineData(120, Max, 0)]
    public void NonsenseCutsNothing(double duration, int max, int fps)
        => Assert.Empty(Media.EqualCuts(duration, max, fps));
}
