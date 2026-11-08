using Xunit;

/// Tests for the watermark's size-against-the-screen arithmetic.
///
/// image-watermark used to pass -pointsize a fixed number, which is absolute pixels,
/// so the watermark shrank as the image grew. Measured with the defaults: 72pt was
/// 14.7% of an 800x600 image's height and 1.5% of an 8000x6000 one -- on a 1080p
/// screen, 158px of text against 16px, from the same command.
///
/// This is the arithmetic that fixes it, kept pure and tested because it decides what
/// comes out, the same reason PlanSplit and MirrorPath live in utilities.cs.
public class ScaleForScreenTests
{
    private const int Screen = 1080;

    [Theory]
    // Exactly the reference screen: left alone.
    [InlineData(1920, 1080, 1.0)]
    // Height-limited, which is the ordinary photo case.
    [InlineData(3000, 2000, 2000.0 / 1080)]
    [InlineData(6000, 4000, 4000.0 / 1080)]
    [InlineData(8000, 6000, 6000.0 / 1080)]
    // Width-limited. A panorama is not tall, but it is still shown smaller than life,
    // so the text has to grow -- this is the case a height-only rule would miss.
    [InlineData(6000, 1000, 6000.0 / 1920)]
    public void ScalesByWhicheverDimensionLimitsTheFit(int w, int h, double expected)
        => Assert.Equal(expected, Media.ScaleForScreen(w, h, Screen), 6);

    [Theory]
    // Smaller than the screen in both dimensions: never shrink. Asked for explicitly
    // -- shrinking would assume the viewer scales small images up to fill the window,
    // which is the viewer's business.
    [InlineData(800, 600)]
    [InlineData(640, 480)]
    [InlineData(1920, 1079)]
    [InlineData(1, 1)]
    public void NeverScalesBelowOne(int w, int h)
        => Assert.Equal(1.0, Media.ScaleForScreen(w, h, Screen));

    [Theory]
    // Unreadable dimensions, from an image magick could not identify. Changing
    // nothing is the right answer to not knowing.
    [InlineData(0, 0)]
    [InlineData(0, 1080)]
    [InlineData(1920, 0)]
    [InlineData(-1, -1)]
    public void UnknownSizeChangesNothing(int w, int h)
        => Assert.Equal(1.0, Media.ScaleForScreen(w, h, Screen));

    [Fact]
    public void AnUnusableScreenHeightChangesNothing()
    {
        Assert.Equal(1.0, Media.ScaleForScreen(6000, 4000, 0));
        Assert.Equal(1.0, Media.ScaleForScreen(6000, 4000, -1080));
    }

    [Fact]
    public void ARetinaScreenAsksForLessEnlargement()
    {
        // Targeting a 4K screen means a 6000x4000 image is nearer life size there, so
        // it needs proportionally less enlargement than for a 1080p one.
        double atHd = Media.ScaleForScreen(6000, 4000, 1080);
        double at4K = Media.ScaleForScreen(6000, 4000, 2160);

        Assert.True(at4K < atHd);
        Assert.Equal(atHd / 2, at4K, 6);
    }

    [Fact]
    public void TheApparentSizeComesOutConstant()
    {
        // The property the whole thing exists for: a font size multiplied by this
        // scale, then shown at whatever fraction fits a 1080p screen, looks the same
        // whatever the image's dimensions.
        const double FontSize = 72;

        double[] apparent =
        [
            .. new (int W, int H)[] { (1920, 1080), (3000, 2000), (6000, 4000), (8000, 6000), (6000, 1000) }
                .Select(size =>
                {
                    double scale = Media.ScaleForScreen(size.W, size.H, Screen);
                    double fit = Math.Min(1920.0 / size.W, 1080.0 / size.H);

                    return FontSize * scale * fit;
                }),
        ];

        foreach (double size in apparent) Assert.Equal(FontSize, size, 6);
    }
}
