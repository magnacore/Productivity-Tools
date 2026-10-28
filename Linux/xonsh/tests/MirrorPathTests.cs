using Xunit;

/// Tests for the path mapping behind video-convert.
///
/// These decide where every converted file lands. A mistake here either overwrites
/// something or scatters output outside the folder the caller named, so the mapping is
/// pure and tested rather than inlined into the walk.
public class MirrorPathTests
{
    // ── Fs.MirrorPath ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("in", "out", "in/clip.mp4", "out/clip.mp4")]
    [InlineData("in", "out", "in/A/clip.mp4", "out/A/clip.mp4")]
    [InlineData("in", "out", "in/A/B/C/clip.mp4", "out/A/B/C/clip.mp4")]
    public void SubDirectoriesArePreserved(string inRoot, string outRoot, string path, string expected)
        => Assert.Equal(Path.GetFullPath(expected), Fs.MirrorPath(inRoot, outRoot, path));

    [Theory]
    // A trailing separator, a redundant "." and a doubled separator all name the same
    // directory, and must all give the same answer.
    [InlineData("in/", "out/")]
    [InlineData("./in", "./out")]
    [InlineData("in/.", "out/.")]
    public void RootSpellingDoesNotChangeTheAnswer(string inRoot, string outRoot)
        => Assert.Equal(Path.GetFullPath("out/A/clip.mp4"),
                        Fs.MirrorPath(inRoot, outRoot, "in/A/clip.mp4"));

    [Fact]
    public void TheResultIsAlwaysUnderTheOutputRoot()
    {
        // The property that matters: nothing escapes the folder the caller named.
        foreach (string path in new[] { "in/a.mp4", "in/A/b.mp4", "in/A/B/c.mp4" })
            Assert.True(Fs.IsInside(Fs.MirrorPath("in", "out", path), Path.GetFullPath("out")));
    }

    [Theory]
    [InlineData("in", "out", "elsewhere/clip.mp4")]
    [InlineData("in", "out", "/tmp/clip.mp4")]
    [InlineData("in/A", "out", "in/clip.mp4")]
    public void AFileOutsideTheInputRootIsRefused(string inRoot, string outRoot, string path)
        => Assert.Throws<ArgumentException>(() => Fs.MirrorPath(inRoot, outRoot, path));

    [Fact]
    public void NamesWithSpacesAndHashesSurvive()
    {
        // The collection is full of both, and "#" is the tag marker.
        Assert.Equal(Path.GetFullPath("out/A B/Andrew Mayne - Hypercards #archive.mkv"),
                     Fs.MirrorPath("in", "out", "in/A B/Andrew Mayne - Hypercards #archive.mkv"));
    }

    // ── Fs.IsInside ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("/a/b", "/a", true)]
    [InlineData("/a/b/c", "/a", true)]
    [InlineData("/a", "/a", true)]          // the same directory counts
    [InlineData("/a/", "/a", true)]         // trailing separator is noise
    [InlineData("/a", "/a/b", false)]       // a parent is not inside its child
    [InlineData("/ab", "/a", false)]        // the separator matters: /ab is not in /a
    [InlineData("/a/bc", "/a/b", false)]
    [InlineData("/x", "/a", false)]
    public void ContainmentIsDecidedOnWholePathSegments(string inner, string outer, bool expected)
        => Assert.Equal(expected, Fs.IsInside(inner, outer));

    [Fact]
    public void TheDefaultOutputFolderIsNotInsideTheInput()
    {
        // The guard that stops the walk eating its own output: the default sits beside
        // the input, never within it.
        Assert.False(Fs.IsInside("/media/demo_folder_converted_2026-10-27", "/media/demo_folder"));
        Assert.True(Fs.IsInside("/media/demo_folder/out", "/media/demo_folder"));
    }

    // ── Media.ConvertedName ──────────────────────────────────────────────────

    [Theory]
    [InlineData("clip.mp4", "archive", ".mkv", "clip #archive.mkv")]
    [InlineData("clip.mp4", "fast", ".mkv", "clip #fast.mkv")]
    [InlineData("clip.mp4", "whatsapp", ".mp4", "clip #whatsapp.mp4")]
    [InlineData("lecture.rmvb", "archive", ".mkv", "lecture #archive.mkv")]
    public void TheTagSaysWhichSettingsProducedTheFile(
        string source, string tag, string extension, string expected)
        => Assert.Equal(expected, Media.ConvertedName(source, tag, extension));

    [Theory]
    // Only the final extension is replaced, and a dotted stem keeps its dots.
    [InlineData("Andrew Mayne - Hypercards.avi", "archive #archive.mkv")]
    [InlineData("season.01.episode.02.mkv", "archive season.01.episode.02 #archive.mkv")]
    [InlineData("no-extension", "archive no-extension #archive.mkv")]
    public void AwkwardNamesAreHandled(string source, string _)
    {
        string result = Media.ConvertedName(source, "archive", ".mkv");

        Assert.EndsWith(" #archive.mkv", result);
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void AnyDirectoryPartIsDropped()
    {
        // The caller decides where the file goes; this only names it.
        Assert.Equal("clip #fast.mkv", Media.ConvertedName("in/A/B/clip.mp4", "fast", ".mkv"));
    }
}
