namespace Stopwatch;

// Pure stopwatch logic. "Now" is passed in (epoch ms) so it's deterministic and testable.
internal static class StopwatchEngine
{
    public static Stopwatch Start(long nowMs) => new()
    {
        AnchorEpochMs = nowMs,
        AccumulatedMs = 0,
        IsRunning = true,
    };

    public static Stopwatch Reset() => new();

    public static long ElapsedMs(in Stopwatch sw, long nowMs)
    {
        long elapsed = sw.AccumulatedMs;
        if (sw.IsRunning)
        {
            long delta = nowMs - sw.AnchorEpochMs; // clamp negative skew between nodes
            if (delta > 0) elapsed += delta;
        }
        return elapsed < 0 ? 0 : elapsed;
    }

    public static Stopwatch Pause(in Stopwatch sw, long nowMs)
    {
        if (!sw.IsRunning) return sw;
        return new Stopwatch
        {
            AnchorEpochMs = 0,
            AccumulatedMs = ElapsedMs(sw, nowMs),
            IsRunning = false,
        };
    }

    public static Stopwatch Resume(in Stopwatch sw, long nowMs)
    {
        if (sw.IsRunning) return sw;
        return new Stopwatch
        {
            AnchorEpochMs = nowMs,
            AccumulatedMs = sw.AccumulatedMs,
            IsRunning = true,
        };
    }

    public static StopwatchReading Read(in Stopwatch sw, long nowMs)
    {
        long ms = ElapsedMs(sw, nowMs);
        return new StopwatchReading
        {
            ElapsedMs = ms,
            ElapsedSeconds = decimal.Round(ms / 1000m, 3),
            Formatted = TimeFormat.Format(ms, includeMilliseconds: true),
            IsRunning = sw.IsRunning,
        };
    }

    public static Lap BuildLap(long splitMs, int number, long previousSplitMs)
    {
        long lapMs = splitMs - previousSplitMs;
        if (lapMs < 0) lapMs = 0;
        return new Lap
        {
            Number = number,
            SplitMs = splitMs,
            LapMs = lapMs,
            LapFormatted = TimeFormat.Format(lapMs, includeMilliseconds: true),
            SplitFormatted = TimeFormat.Format(splitMs, includeMilliseconds: true),
        };
    }
}
