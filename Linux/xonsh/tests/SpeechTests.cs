using System.Text;
using Xunit;

/// Tests for the text that gets read aloud.
///
/// Shared by text-to-speech and text-to-speech-local, and never covered before it was
/// shared: it had been private to one program, where no test could reach it. Both the
/// cleaning and the splitting are audible. Markup read out loud is heard as "hash",
/// "star", "underscore"; a chunk boundary is a hard stop and a fresh breath, so where
/// it falls decides whether a sentence is broken in half.
public class SpeechTests
{
    // ── Speech.Clean ─────────────────────────────────────────────────────────

    [Fact]
    public void TheLanguageIsNamedNotSpelled()
        => Assert.Equal("CSharp is fine", Speech.Clean("C# is fine"));

    [Theory]
    [InlineData("# Heading", " Heading")]
    [InlineData("**bold**", "bold")]
    [InlineData("`code`", "code")]
    [InlineData("▪ bullet", " bullet")]
    public void DecorationIsDropped(string given, string expected)
        => Assert.Equal(expected, Speech.Clean(given));

    [Fact]
    public void UnderscoresBecomeWordBreaks()
        => Assert.Equal("max chunk bytes", Speech.Clean("max_chunk_bytes"));

    [Fact]
    public void CopyrightFootersGo()
        => Assert.Equal("Before  After",
                        Speech.Clean("Before © 2026 Someone. All rights reserved. After"));

    [Fact]
    public void RulesDrawnOutOfPunctuationGo()
    {
        Assert.Equal("Above\nBelow", Speech.Clean("Above\n=====\nBelow").Replace("\n\n", "\n"));
        Assert.DoesNotContain("---", Speech.Clean("Above\n---------\nBelow"), StringComparison.Ordinal);
    }

    [Fact]
    public void OrdinaryProseIsLeftAlone()
    {
        const string Text = "The quick brown fox jumps over the lazy dog.";

        Assert.Equal(Text, Speech.Clean(Text));
    }

    // ── Speech.Chunk ─────────────────────────────────────────────────────────

    [Fact]
    public void ShortTextIsOneChunk()
        => Assert.Single(Speech.Chunk("One short sentence.", 4000));

    [Fact]
    public void NothingIsLost()
    {
        // The words have to survive, whatever the boundaries end up being. Spacing at
        // the joins may differ, so this compares the words rather than the characters.
        string text = string.Join(" ", Enumerable.Range(1, 400).Select(n => $"Sentence number {n}."));

        List<string> chunks = Speech.Chunk(text, 200);

        Assert.Equal(
            text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries),
            string.Concat(chunks).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void NoChunkExceedsTheLimit()
    {
        string text = string.Join(" ", Enumerable.Range(1, 300).Select(n => $"This is sentence {n}."));

        Assert.All(Speech.Chunk(text, 300),
                   chunk => Assert.True(Encoding.UTF8.GetByteCount(chunk) <= 300,
                                        $"{Encoding.UTF8.GetByteCount(chunk)} bytes"));
    }

    [Fact]
    public void CutsFallAtSentenceEnds()
    {
        // The reason this is not a plain character count. A cut inside a clause is
        // heard as a full stop that was never written.
        List<string> chunks = Speech.Chunk(
            "First sentence here. Second sentence here. Third sentence here.", 25);

        Assert.All(chunks, chunk => Assert.True(
            chunk.TrimEnd().EndsWith('.'), $"chunk ended mid-clause: \"{chunk}\""));
    }

    [Fact]
    public void NoChunkIsNothingButWhitespace()
    {
        // A silent chunk is a request that synthesises nothing, and an extra seam in
        // the joined audio. Trailing whitespace used to make one whenever the chunk
        // before it filled up exactly.
        List<string> chunks = Speech.Chunk("Something to say.          \n\n   ", 20);

        Assert.All(chunks, chunk => Assert.NotEqual(string.Empty, chunk.Trim()));
    }

    [Fact]
    public void AWordLongerThanAWholeChunkIsStillHandled()
    {
        // A URL with no spaces in it, longer than one request allows. It has to be cut
        // somewhere, and the only requirement is that it is cut rather than refused.
        string url = "https://example.com/" + new string('a', 500);

        List<string> chunks = Speech.Chunk(url, 100);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, chunk => Assert.True(Encoding.UTF8.GetByteCount(chunk) <= 100));
        Assert.Equal(url, string.Concat(chunks));
    }

    [Fact]
    public void AstralCharactersAreNotCutInHalf()
    {
        // Cutting between the halves of a surrogate pair produces a lone surrogate,
        // which encodes as replacement bytes -- so the text would arrive corrupted
        // rather than merely split.
        string text = string.Concat(Enumerable.Repeat("\U0001F600", 200));

        List<string> chunks = Speech.Chunk(text, 40);

        Assert.Equal(text, string.Concat(chunks));
        Assert.All(chunks, chunk => Assert.DoesNotContain('�', chunk));
        Assert.All(chunks, chunk => Assert.True(Encoding.UTF8.GetByteCount(chunk) <= 40));
    }

    [Fact]
    public void MultibyteTextIsMeasuredInBytesNotCharacters()
    {
        // The limit applies to the encoded request, and one character is not one byte.
        string text = string.Concat(Enumerable.Repeat("é", 300));

        Assert.All(Speech.Chunk(text, 100),
                   chunk => Assert.True(Encoding.UTF8.GetByteCount(chunk) <= 100));
    }
}
