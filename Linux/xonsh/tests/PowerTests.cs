using Xunit;

/// Tests for deciding whether the machine is on mains.
///
/// video-convert pauses its encodes on battery, so this answer decides whether hours
/// of work run or wait. The two failure directions are not equal: pausing a desktop
/// that has no battery would stop it converting at all and look like a hang, while
/// carrying on when the answer is unknown merely does what the program did before any
/// of this existed. Both of those defaults are asserted below.
public class PowerTests : IDisposable
{
    private readonly string _root =
        Directory.CreateTempSubdirectory("power-tests-").FullName;

    private string Supply(string name, string type, string? online = null,
                          string? capacity = null)
    {
        string path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);

        File.WriteAllText(Path.Combine(path, "type"), type + "\n");

        if (online is not null) File.WriteAllText(Path.Combine(path, "online"), online + "\n");
        if (capacity is not null) File.WriteAllText(Path.Combine(path, "capacity"), capacity + "\n");

        return path;
    }

    [Fact]
    public void PluggedIn()
    {
        Supply("AC", "Mains", online: "1");
        Supply("BAT0", "Battery", capacity: "80");

        Assert.True(Power.OnMains(_root));
    }

    [Fact]
    public void OnBattery()
    {
        Supply("AC", "Mains", online: "0");
        Supply("BAT0", "Battery", capacity: "80");

        Assert.False(Power.OnMains(_root));
    }

    [Fact]
    public void ADesktopIsAlwaysOnMains()
    {
        // No mains supply listed at all. Nothing should ever pause on a machine with
        // no battery to flatten, so the absence of evidence has to read as "plugged
        // in" rather than "on battery".
        Assert.True(Power.OnMains(_root));
    }

    [Fact]
    public void ABatteryWithNoMainsEntryStillCounts()
    {
        // Some machines list the battery but no AC adapter. Without a mains entry
        // there is nothing to say the cable is out, and guessing it is would stop the
        // conversion for good.
        Supply("BAT0", "Battery", capacity: "55");

        Assert.True(Power.OnMains(_root));
    }

    [Fact]
    public void AnUnreadableTreeMeansCarryOn()
        => Assert.True(Power.OnMains(Path.Combine(_root, "does-not-exist")));

    [Fact]
    public void AMainsSupplyThatWillNotSayMeansCarryOn()
    {
        // Only a supply that positively reports 0 counts as unplugged. An entry with
        // no readable online file says nothing, and pausing on nothing would stall a
        // conversion that never resumes -- a worse mistake than running on battery,
        // which is merely what this program did before.
        Supply("AC", "Mains");

        Assert.True(Power.OnMains(_root));
    }

    [Fact]
    public void OneSilentAdapterDoesNotMaskAnUnpluggedOne()
    {
        Supply("ADP0", "Mains");
        Supply("ACAD", "Mains", online: "0");

        Assert.False(Power.OnMains(_root));
    }

    [Fact]
    public void AnySupplyOnlineIsEnough()
    {
        // A laptop can list more than one: a barrel adapter and a USB-C one, with
        // only whichever is in use reading 1.
        Supply("ADP0", "Mains", online: "0");
        Supply("ACAD", "Mains", online: "1");

        Assert.True(Power.OnMains(_root));
    }

    [Fact]
    public void BatteryEntriesDoNotAnswerTheQuestion()
    {
        // A battery's own "online" is about the battery being present, not about the
        // cable, so only Mains entries are consulted.
        Supply("AC", "Mains", online: "0");
        Supply("BAT0", "Battery", online: "1", capacity: "80");

        Assert.False(Power.OnMains(_root));
    }

    [Fact]
    public void TrailingWhitespaceDoesNotConfuseIt()
    {
        string path = Supply("AC", "Mains");
        File.WriteAllText(Path.Combine(path, "online"), "1\n\n");

        Assert.True(Power.OnMains(_root));
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); }
        catch (Exception) { /* a leftover temp directory is not worth failing a test */ }

        GC.SuppressFinalize(this);
    }
}
