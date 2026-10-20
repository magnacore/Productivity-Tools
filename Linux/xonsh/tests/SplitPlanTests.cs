using Xunit;

/// Tests for Media.PlanSplit, the arithmetic that decides where a recording is cut.
///
/// Two modes. Equal makes every part the same length with nothing left over. Fixed
/// makes every part exactly the requested length and keeps the remainder as a final
/// part, unless that remainder is too short to be worth having.
public class SplitPlanTests
{
    private const int Floor = 30;   // the --min-part default, used by fixed mode only

    /// Equal mode is bounded by the threshold, so the tests need one. Twice the part
    /// length is what media-split-equal-tui uses for a custom length, and what the
    /// 5 minute preset has always used.
    private static SplitPlan Plan(double duration, int split, bool equal, int threshold = 0)
    {
        SplitPlan? plan = Media.PlanSplit(duration, split, threshold == 0 ? split * 2 : threshold,
                                          Floor, equal);
        Assert.NotNull(plan);
        return plan;
    }

    // ── what both modes must never do ────────────────────────────────────────

    [Theory]
    // length, requested part
    [InlineData(3100, 300)] [InlineData(901, 300)] [InlineData(1199, 300)]
    [InlineData(1000, 300)] [InlineData(4500, 3600)] [InlineData(8999, 3600)]
    [InlineData(1800, 300)] [InlineData(599, 300)] [InlineData(301, 300)]
    [InlineData(7200, 450)] [InlineData(60, 300)] [InlineData(3599, 3600)]
    public void EveryPlanAccountsForTheWholeFile(double duration, int split)
    {
        foreach (bool equal in new[] { true, false })
        {
            SplitPlan plan = Plan(duration, split, equal);

            // Losing or inventing material is the one unforgivable outcome. Rounding to
            // whole seconds is all the slack allowed, once per part.
            Assert.InRange(plan.PartLengths.Sum(), duration - plan.PartCount, duration + plan.PartCount);
            Assert.All(plan.PartLengths, length => Assert.True(length > 0));
        }
    }

    [Theory]
    // length, requested part, threshold -- every case a file the caller would
    // actually hand over, meaning longer than the threshold.
    [InlineData(3100, 300, 600)] [InlineData(1199, 300, 600)] [InlineData(4500, 3600, 3600)]
    [InlineData(8999, 3600, 7200)] [InlineData(2399, 300, 600)] [InlineData(1349, 450, 600)]
    public void EqualNeverProducesAPartAsLongAsTheThreshold(double duration, int split, int threshold)
    {
        // The whole bound. The threshold already states how long a piece may be before
        // it is worth cutting up, so a part that stays under it is by the caller's own
        // definition a reasonable thing to sit down with.
        SplitPlan plan = Plan(duration, split, equal: true, threshold);

        Assert.All(plan.PartLengths, length => Assert.True(length < threshold,
            $"part of {length}s reaches the {threshold}s threshold"));
    }

    [Theory]
    [InlineData(3100, 300, 600)] [InlineData(1199, 300, 600)] [InlineData(4500, 3600, 3600)]
    [InlineData(601, 450, 600)] [InlineData(8999, 3600, 7200)]
    public void EqualNeverHandsBackAFileWholeWhenItIsAboveTheThreshold(
        double duration, int split, int threshold)
    {
        // This is what used to need a separate guard. Anything reaching the planner is
        // longer than the threshold, so duration/threshold is at least one and the count
        // is at least two: 1h15m asked for in 1h parts cannot come back unsplit.
        SplitPlan plan = Plan(duration, split, equal: true, threshold);

        Assert.True(plan.PartCount >= 2, $"came back as {plan.PartCount} part(s)");
    }

    [Theory]
    [InlineData(3100, 300)] [InlineData(1199, 300)] [InlineData(4500, 3600)]
    [InlineData(901, 300)] [InlineData(2399, 300)]
    public void EqualLeavesNoShortTail(double duration, int split)
    {
        SplitPlan plan = Plan(duration, split, equal: true);

        // Every part the same length bar rounding. The interval is a whole number of
        // seconds, so the last part absorbs the rounding error of all the others and can
        // be up to one second shorter per part: 901s over three parts is 301, 301, 299.
        // That is the full extent of the unevenness "equal" permits.
        Assert.InRange(plan.PartLengths.Max() - plan.PartLengths.Min(), 0, plan.PartCount - 1);
    }

    // ── equal ────────────────────────────────────────────────────────────────

    [Fact]
    public void EqualSpreadsASmallRemainderAcrossTheParts()
    {
        // 51m40 at 5m: ten parts share a 1m40 remainder, ten seconds each.
        SplitPlan plan = Plan(3100, 300, equal: true);

        Assert.Equal(10, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.InRange(length, 310, 310));
    }

    [Fact]
    public void EqualStretchesThePartsWhileTheThresholdAllowsIt()
    {
        // 19m59 at 5m with a 10m threshold: three parts of 6m40. Longer than asked for,
        // but well under the threshold, so it is a part worth having rather than a
        // fourth cut nobody wanted.
        SplitPlan plan = Plan(1199, 300, equal: true, threshold: 600);

        Assert.Equal(3, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.InRange(length, 399, 400));
    }

    [Fact]
    public void EqualCutsAnotherPartWhenStretchingWouldReachTheThreshold()
    {
        // 22m29 at 7m30 with a 10m threshold: two parts would be 11m14, over the
        // threshold, so three of 7m30 instead.
        SplitPlan plan = Plan(1349, 450, equal: true, threshold: 600);

        Assert.Equal(3, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.InRange(length, 449, 450));
    }

    [Fact]
    public void EqualSplitsTheHourCaseRatherThanReturningItWhole()
    {
        // The case that prompted all of this: 1h15m asked for in 1h parts, with an hour
        // threshold so the file is one the caller would actually pass on.
        SplitPlan plan = Plan(4500, 3600, equal: true, threshold: 3600);

        Assert.Equal(2, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.Equal(2250, length));
    }

    [Fact]
    public void EqualLeavesAnExactMultipleAlone()
    {
        SplitPlan plan = Plan(1800, 300, equal: true);

        Assert.Equal(6, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.Equal(300, length));
    }

    [Theory]
    // ffprobe never reports a whole number. A nominally 310 second file measures
    // 310.008, and dividing that by a part count and rounding up used to cost every
    // part a second it should not have had, leaving the last one far short: ten parts
    // came out as nine of 32s and one of 22s instead of ten of 31s.
    [InlineData(310.008, 30)] [InlineData(120.008, 30)] [InlineData(3100.004, 300)]
    [InlineData(1800.992, 300)] [InlineData(901.5, 300)]
    public void EqualSurvivesAFractionalDuration(double duration, int split)
    {
        SplitPlan plan = Plan(duration, split, equal: true);

        // No part may be shorter than the others by more than the rounding slack.
        Assert.InRange(plan.PartLengths.Max() - plan.PartLengths.Min(), 0, plan.PartCount - 1);
        Assert.InRange(plan.PartLengths.Sum(), duration - plan.PartCount, duration + plan.PartCount);
    }

    [Fact]
    public void EqualDividesAMeasuredFileIntoThePartsItShould()
    {
        // The exact case from the failing run: 310.008s in 30s parts.
        SplitPlan plan = Plan(310.008, 30, equal: true);

        Assert.Equal(10, plan.PartCount);
        Assert.All(plan.PartLengths, length => Assert.InRange(length, 31, 31));
    }

    [Theory]
    // A measured duration is never whole. ffprobe calls a nominally 60 second file
    // 60.008, and equal mode must leave no remainder -- so without this it would cut a
    // file that is already exactly one part long into two halves.
    [InlineData(60.008, 60)] [InlineData(300.004, 300)] [InlineData(3600.992, 3600)]
    [InlineData(900.008, 300)] [InlineData(1200.05, 300)]
    public void EqualTreatsANegligibleRemainderAsNone(double duration, int split)
    {
        SplitPlan plan = Plan(duration, split, equal: true, threshold: split * 2);

        // The count is the parts that fit, with the fraction carried by the last one --
        // not an extra part for the fraction itself.
        Assert.Equal((int)(duration / split), plan.PartCount);
        Assert.Equal((int)Math.Round(duration), plan.PartLengths.Sum());
    }

    [Theory]
    // length, requested part, bound
    [InlineData(90.008, 30, 60)] [InlineData(900.008, 300, 600)] [InlineData(60.008, 60, 120)]
    [InlineData(310.008, 30, 60)] [InlineData(5040.008, 3600, 7200)] [InlineData(1000.008, 300, 600)]
    [InlineData(3100.004, 300, 600)] [InlineData(1199.5, 300, 600)]
    public void NoPlanEverProducesAFragment(double duration, int split, int bound)
    {
        // The reason this matters: handing mkvmerge a repeating interval makes it write
        // whatever is left at the end as a part of its own, however small. A 90.008
        // second file cut at 30 seconds came back as three parts and a fourth of six
        // milliseconds, numbered and percentage tagged like any other. Nothing was lost,
        // but a six millisecond file is not a part.
        foreach (bool equal in new[] { true, false })
        {
            SplitPlan plan = Media.PlanSplit(duration, split, bound, Floor, equal)!;

            Assert.All(plan.PartLengths, length => Assert.True(length >= 1,
                $"{(equal ? "equal" : "fixed")} produced a part of {length}s"));

            // And the whole file is still accounted for.
            Assert.InRange(plan.PartLengths.Sum(), duration - plan.PartCount, duration + plan.PartCount);
        }
    }

    // ── fixed ────────────────────────────────────────────────────────────────

    [Fact]
    public void FixedKeepsARemainderThatIsWorthHaving()
    {
        // 16m40 at 5m: three exact parts and a 1m40 tail, which is long enough to keep.
        SplitPlan plan = Plan(1000, 300, equal: false);

        Assert.Equal(new[] { 300, 300, 300, 100 }, plan.PartLengths);
        Assert.Equal("duration:300s", plan.SplitArgument);
    }

    [Fact]
    public void FixedLetsTheLastPartSwallowARemainderTooShortToKeep()
    {
        // 15m01 at 5m would otherwise leave a one second part.
        SplitPlan plan = Plan(901, 300, equal: false);

        Assert.Equal(new[] { 300, 300, 301 }, plan.PartLengths);

        // One interval cannot say "exact except the last", so the cuts are explicit.
        Assert.Equal("timecodes:300s,600s", plan.SplitArgument);
    }

    [Fact]
    public void FixedAbsorbsIntoAWholeFileWhenThereIsOnlyOnePart()
    {
        // 5m10 at 5m: a ten second tail, and no earlier part to hand it to.
        SplitPlan plan = Plan(310, 300, equal: false);

        Assert.Equal(1, plan.PartCount);
        Assert.Equal(310, plan.PartLengths[0]);
    }

    [Fact]
    public void FixedKeepsARemainderExactlyAtTheFloor()
    {
        // The boundary: 30s is long enough to stand alone, 29s is not.
        Assert.Equal(4, Plan(930, 300, equal: false).PartCount);
        Assert.Equal(3, Plan(929, 300, equal: false).PartCount);
    }

    [Fact]
    public void FixedSplitsTheHourCaseIntoAnHourAndTheRemainder()
    {
        SplitPlan plan = Plan(4500, 3600, equal: false);

        Assert.Equal(new[] { 3600, 900 }, plan.PartLengths);
    }

    // ── edges ────────────────────────────────────────────────────────────────

    [Fact]
    public void AFileShorterThanOnePartIsLeftWhole()
    {
        foreach (bool equal in new[] { true, false })
        {
            SplitPlan plan = Plan(200, 300, equal);

            Assert.Equal(1, plan.PartCount);
            Assert.Equal(200, plan.PartLengths[0]);

            // Still split, so the single part carries the -001 the caller matches on.
            Assert.StartsWith("duration:", plan.SplitArgument);
        }
    }

    [Theory]
    [InlineData(0, 300)] [InlineData(-5, 300)] [InlineData(600, 0)] [InlineData(600, -1)]
    public void NonsenseIsRefusedRatherThanGuessed(double duration, int split)
    {
        // A duration of zero is what Media.Duration returns when ffprobe fails, and the
        // caller must be able to tell that apart from a successful plan.
        Assert.Null(Media.PlanSplit(duration, split, split * 2, Floor, equal: true));
        Assert.Null(Media.PlanSplit(duration, split, split * 2, Floor, equal: false));
    }

    [Fact]
    public void TheFloorIsConfigurable()
    {
        // With a 5 second floor, a 10 second tail is worth keeping.
        SplitPlan keep = Media.PlanSplit(910, 300, 600, 5, equal: false)!;
        Assert.Equal(4, keep.PartCount);

        // With a 120 second floor it is not.
        SplitPlan absorb = Media.PlanSplit(910, 300, 600, 120, equal: false)!;
        Assert.Equal(3, absorb.PartCount);
    }
}
