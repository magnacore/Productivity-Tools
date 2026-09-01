using Xunit;

// Filename handling. Every converter in the suite builds its output name through
// these, so a mistake here renames or overwrites the wrong thing across the board.
public class SplitExtTests
{
    [Theory]
    [InlineData("lecture.mkv", "lecture", ".mkv")]
    [InlineData("archive.tar.gz", "archive.tar", ".gz")]   // last dot wins
    [InlineData("README", "README", "")]                   // no extension at all
    [InlineData(".bashrc", ".bashrc", "")]                 // a dotfile is not an extension
    [InlineData("..hidden", "..hidden", "")]
    [InlineData("Animal Rights [2nd Edition].epub", "Animal Rights [2nd Edition]", ".epub")]
    public void SplitsOnTheLastDot(string file, string name, string ext)
    {
        var (actualName, actualExt) = Fs.SplitExt(file);
        Assert.Equal(name, actualName);
        Assert.Equal(ext, actualExt);
    }

    [Fact]
    public void KeepsTheDirectoryOnTheNamePart()
    {
        // Regression: an earlier version slugified the whole path, which moved files
        // out of their folder.
        var (name, ext) = Fs.SplitExt(Path.Combine("sub dir", "Track 01.mp3"));
        Assert.Equal(Path.Combine("sub dir", "Track 01"), name);
        Assert.Equal(".mp3", ext);
    }
}

public class SlugTests
{
    [Theory]
    [InlineData("Some File!.TXT", "some-filetxt")]
    [InlineData("  spaced   out  ", "spaced-out")]
    [InlineData("a/b\\c", "abc")]                     // separators are not word characters
    [InlineData("--leading--and--trailing--", "leading-and-trailing")]
    public void ReducesToASafeName(string input, string expected)
        => Assert.Equal(expected, Fs.Slug(input));

    [Fact]
    public void StripsDots_WhichIsWhyCallersSplitTheExtensionOffFirst()
    {
        // Deliberate, and faithful to the original: "remove characters that aren't
        // alphanumerics, underscores, or hyphens" — this is Django's slugify, not
        // get_valid_filename, and dots are not in that set.
        //
        // It is safe only because RenameToValid slugifies the stem alone and puts the
        // extension back afterwards. Anyone tempted to make Slug keep dots should
        // change the caller instead.
        Assert.Equal("archivetar", Fs.Slug("archive.tar"));
    }

    [Fact]
    public void KeepsTheBaseLetterOfAnAccentedCharacter()
    {
        // Decomposing first is what makes this work. With InvariantGlobalization on,
        // Normalize is a no-op and this collapses to "ncode-nme" — the reason the
        // test project sets InvariantGlobalization=false.
        Assert.Equal("unicode-name", Fs.Slug("Ünïcode Näme"));
    }

    [Fact]
    public void HandlesNullAndEmpty()
    {
        Assert.Equal(string.Empty, Fs.Slug(null!));
        Assert.Equal(string.Empty, Fs.Slug(string.Empty));
    }
}

public class FindUrlsTests
{
    [Fact]
    public void PullsEveryUrlOutOfSurroundingText()
    {
        var urls = Fs.FindUrls("see https://example.com/a and http://b.example.org/x?y=1 too");
        Assert.Equal(2, urls.Count);
        Assert.Contains("https://example.com/a", urls);
    }

    [Fact]
    public void FindsNothingInTextWithoutUrls()
        => Assert.Empty(Fs.FindUrls("no links here at all"));
}
