using Xunit;

/// Tests for making a rotated watermark fit the image it sits on.
///
/// A long sentence was being cut off. "Not For Sale. Only for review by manuj chandra
/// sharma" sets to 1760x88 at 72pt, which fits a 1920x1080 image with room to spare --
/// until it is turned 45 degrees, when it needs 1307x1307 and runs off the bottom. The
/// diagonal runs out, not the width, which is why no amount of extra image width helps
/// and why this arithmetic is worth doing rather than guessing from character counts.
public class RotatedFitTests
{
    // ── Media.RotatedSize ────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(180)]
    [InlineData(360)]
    [InlineData(-180)]
    public void NotTurnedMeansNoChange(double degrees)
    {
        (double w, double h) = Media.RotatedSize(1760, 88, degrees);

        Assert.Equal(1760, w, 6);
        Assert.Equal(88, h, 6);
    }

    [Theory]
    [InlineData(90)]
    [InlineData(270)]
    [InlineData(-90)]
    public void AQuarterTurnSwapsTheSides(double degrees)
    {
        (double w, double h) = Media.RotatedSize(1760, 88, degrees);

        Assert.Equal(88, w, 6);
        Assert.Equal(1760, h, 6);
    }

    [Fact]
    public void FortyFiveDegreesIsTheCaseThatBites()
    {
        // The real sentence, at the default rotation. Both sides become (1760+88)/root2,
        // so a label a sixth of the image's height suddenly needs more than all of it.
        (double w, double h) = Media.RotatedSize(1760, 88, 45);

        Assert.Equal(1307, w, 0);
        Assert.Equal(1307, h, 0);
        Assert.True(h > 1080, "the whole problem: taller than a 1080 high image");
    }

    [Fact]
    public void NegativeAndObtuseAnglesMeasureTheSame()
    {
        // The flag takes any angle, and a bounding box does not care about direction.
        (double w1, double h1) = Media.RotatedSize(1760, 88, 45);
        (double w2, double h2) = Media.RotatedSize(1760, 88, -45);
        (double w3, double h3) = Media.RotatedSize(1760, 88, 135);

        Assert.Equal(w1, w2, 6);
        Assert.Equal(h1, h2, 6);
        Assert.Equal(w1, w3, 6);
        Assert.Equal(h1, h3, 6);
    }

    // ── Media.ShrinkToFit ────────────────────────────────────────────────────

    [Fact]
    public void AlreadyInsideMeansLeaveItAlone()
    {
        Assert.Equal(1.0, Media.ShrinkToFit(500, 500, 1920, 1080));
        Assert.Equal(1.0, Media.ShrinkToFit(1920, 1080, 1920, 1080));
    }

    [Fact]
    public void NeverEnlargesToFill()
    {
        // This caps; it does not stretch a small watermark across a big image. That is
        // ScaleForScreen's job, and it has already run by this point.
        Assert.Equal(1.0, Media.ShrinkToFit(100, 100, 4608, 3456));
    }

    [Fact]
    public void ShrinksByWhicheverSideOverflowsMore()
    {
        // 1307x1307 into 1920x1080: the height is what runs out.
        double fit = Media.ShrinkToFit(1307, 1307, 1920, 1080);

        Assert.Equal(1080.0 / 1307, fit, 6);
        Assert.True(1307 * fit <= 1080 + 0.001);
    }

    [Fact]
    public void TheResultActuallyFits()
    {
        // The property worth asserting, across the sizes this program really sees.
        (int W, int H)[] images = [(1920, 1080), (4608, 3456), (650, 564), (6000, 1000)];

        foreach ((int imageW, int imageH) in images)
        {
            (double stampW, double stampH) = Media.RotatedSize(1760, 88, 45);
            double fit = Media.ShrinkToFit(stampW, stampH, imageW, imageH);

            Assert.True(stampW * fit <= imageW + 0.001, $"width on {imageW}x{imageH}");
            Assert.True(stampH * fit <= imageH + 0.001, $"height on {imageW}x{imageH}");
        }
    }

    [Theory]
    [InlineData(0, 100, 1920, 1080)]
    [InlineData(100, 0, 1920, 1080)]
    [InlineData(100, 100, 0, 1080)]
    [InlineData(100, 100, 1920, 0)]
    [InlineData(-1, -1, -1, -1)]
    public void NonsenseChangesNothing(double w, double h, double bw, double bh)
        => Assert.Equal(1.0, Media.ShrinkToFit(w, h, bw, bh));
}
