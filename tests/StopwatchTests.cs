using Stopwatch;
using Xunit;

namespace Stopwatch.Tests;

public class StopwatchTests
{
    /// <summary>A controllable clock so tests are deterministic.</summary>
    private sealed class FakeClock
    {
        public long NowMs;
        public long Read() => NowMs;
    }

    private static (StopwatchService svc, FakeClock clock) NewService(long start = 1_000_000)
    {
        var clock = new FakeClock { NowMs = start };
        return (new StopwatchService(clock.Read), clock);
    }

    [Fact]
    public void Start_produces_running_zeroed_stopwatch()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();

        Assert.True(sw.IsRunning);
        Assert.Equal(0, sw.AccumulatedMs);
        Assert.Equal(clock.NowMs, sw.AnchorEpochMs);
        Assert.Equal(0, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Elapsed_grows_with_the_clock_while_running()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();

        clock.NowMs += 2_500;
        Assert.Equal(2_500, svc.GetElapsedMilliseconds(sw));

        clock.NowMs += 500;
        Assert.Equal(3_000, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Pause_banks_elapsed_and_freezes_it()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();

        clock.NowMs += 4_000;
        sw = svc.Pause(sw);

        Assert.False(sw.IsRunning);
        Assert.Equal(4_000, sw.AccumulatedMs);

        // Time keeps moving, but a paused stopwatch must not.
        clock.NowMs += 10_000;
        Assert.Equal(4_000, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Resume_continues_from_banked_time()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();
        clock.NowMs += 4_000;
        sw = svc.Pause(sw);

        clock.NowMs += 10_000;   // paused gap, ignored
        sw = svc.Resume(sw);
        Assert.True(sw.IsRunning);

        clock.NowMs += 1_000;    // counts again
        Assert.Equal(5_000, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Pause_then_Pause_is_idempotent()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();
        clock.NowMs += 1_000;
        sw = svc.Pause(sw);
        var again = svc.Pause(sw);

        Assert.Equal(sw.AccumulatedMs, again.AccumulatedMs);
        Assert.False(again.IsRunning);
    }

    [Fact]
    public void Resume_on_running_is_a_noop()
    {
        var (svc, _) = NewService();
        var sw = svc.Start();
        var again = svc.Resume(sw);
        Assert.Equal(sw.AnchorEpochMs, again.AnchorEpochMs);
        Assert.True(again.IsRunning);
    }

    [Fact]
    public void Reset_returns_clean_stopwatch()
    {
        var (svc, _) = NewService();
        var sw = svc.Reset();
        Assert.False(sw.IsRunning);
        Assert.Equal(0, sw.AccumulatedMs);
        Assert.Equal(0, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Clock_skew_into_the_future_does_not_go_negative()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();
        clock.NowMs -= 5_000; // anchor is "in the future" relative to now
        Assert.Equal(0, svc.GetElapsedMilliseconds(sw));
    }

    [Fact]
    public void Read_fills_every_derived_field()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();
        clock.NowMs += 83_456; // 1m 23.456s

        var r = svc.Read(sw);
        Assert.Equal(83_456, r.ElapsedMs);
        Assert.Equal(83.456m, r.ElapsedSeconds);
        Assert.Equal("00:01:23.456", r.Formatted);
        Assert.True(r.IsRunning);
    }

    [Fact]
    public void Laps_compute_split_and_per_lap_duration()
    {
        var (svc, clock) = NewService();
        var sw = svc.Start();
        var laps = new List<Lap>();

        clock.NowMs += 1_200;
        var l1 = svc.RecordLap(sw, laps); laps.Add(l1);
        clock.NowMs += 1_800;
        var l2 = svc.RecordLap(sw, laps); laps.Add(l2);
        clock.NowMs += 1_000;
        var l3 = svc.RecordLap(sw, laps); laps.Add(l3);

        Assert.Equal(new[] { 1, 2, 3 }, laps.Select(l => l.Number));
        Assert.Equal(new long[] { 1_200, 3_000, 4_000 }, laps.Select(l => l.SplitMs));
        Assert.Equal(new long[] { 1_200, 1_800, 1_000 }, laps.Select(l => l.LapMs));
        Assert.Equal("00:00:01.800", l2.LapFormatted);
        Assert.Equal("00:00:03.000", l2.SplitFormatted);
    }

    [Theory]
    [InlineData(0, true, "00:00:00.000")]
    [InlineData(0, false, "00:00:00")]
    [InlineData(999, true, "00:00:00.999")]
    [InlineData(61_000, false, "00:01:01")]
    [InlineData(3_661_000, false, "01:01:01")]
    [InlineData(360_000_000, false, "100:00:00")]
    [InlineData(-500, true, "00:00:00.000")]
    public void FormatDuration_handles_edges(long ms, bool millis, string expected)
        => Assert.Equal(expected, new StopwatchService(() => 0).FormatDuration(ms, millis));
}
