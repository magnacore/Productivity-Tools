using Xunit;

/// Tests for which extensions the suite treats as video.
///
/// This has gone wrong twice, both times silently and both times in the same shape:
/// a folder of perfectly ordinary video files was walked, found not to be video, and
/// copied through untouched instead of being converted. First with .rmvb, which the
/// system database simply did not know; then with .mts, which it knew about and got
/// wrong -- Debian maps it to model/vnd.mts, a CAD format, and because the system
/// database was applied on top of the suite's own table, declaring .mts here was not
/// enough to fix it.
///
/// Neither failure announced itself. The conversion reported success, the output
/// folder filled up, and only the file sizes gave it away. Hence a test: it costs
/// nothing and it fails loudly the next time the precedence or the table changes.
public class MimeTests
{
    [Theory]
    // Camcorder formats. AVCHD is what a Sony or Panasonic writes; .tod is JVC's.
    [InlineData("holiday.mts")]
    [InlineData("holiday.m2ts")]
    [InlineData("holiday.tod")]
    // Off a DVD.
    [InlineData("VTS_01_1.vob")]
    [InlineData("title.m2v")]
    // Downloads.
    [InlineData("lecture.rmvb")]
    [InlineData("clip.ogv")]
    [InlineData("clip.divx")]
    [InlineData("phone.3gp")]
    public void RecognisesVideoExtensions(string file) => Assert.True(Fs.IsVideo(file));

    [Theory]
    // The ordinary ones, which must not regress while the awkward ones are added.
    [InlineData("a.mp4")]
    [InlineData("a.mkv")]
    [InlineData("a.webm")]
    [InlineData("a.mov")]
    [InlineData("a.avi")]
    public void StillRecognisesTheUsualOnes(string file) => Assert.True(Fs.IsVideo(file));

    [Theory]
    // .mod is left out on purpose: it is the Amiga tracker format as well as a JVC
    // camcorder one, and treating a music file as video would hand it to ffmpeg as
    // something it is not.
    [InlineData("song.mod")]
    [InlineData("notes.txt")]
    [InlineData("photo.jpg")]
    [InlineData("music.mp3")]
    [InlineData("book.epub")]
    public void DoesNotClaimNonVideo(string file) => Assert.False(Fs.IsVideo(file));

    [Fact]
    public void SuiteTableBeatsTheSystemDatabase()
    {
        // The specific regression: /etc/mime.types on Debian says model/vnd.mts, and
        // the suite's own entry has to win or the whole corpus is copied, not
        // converted. Asserted through the mime string rather than IsVideo so the
        // failure names the cause.
        Assert.Equal("video", Fs.Mime("camera.mts"));
    }
}
