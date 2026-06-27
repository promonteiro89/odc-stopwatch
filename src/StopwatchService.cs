namespace Stopwatch;

public sealed class StopwatchService : IStopwatchService
{
    private readonly Func<long> _now;

    public StopwatchService() : this(() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) { }

    internal StopwatchService(Func<long> now) => _now = now; // test seam

    public Stopwatch Start() => StopwatchEngine.Start(_now());

    public Stopwatch Pause(Stopwatch stopwatch) => StopwatchEngine.Pause(stopwatch, _now());

    public Stopwatch Resume(Stopwatch stopwatch) => StopwatchEngine.Resume(stopwatch, _now());

    public Stopwatch Reset() => StopwatchEngine.Reset();

    public StopwatchReading Read(Stopwatch stopwatch) => StopwatchEngine.Read(stopwatch, _now());

    public long GetElapsedMilliseconds(Stopwatch stopwatch) => StopwatchEngine.ElapsedMs(stopwatch, _now());

    public Lap RecordLap(Stopwatch stopwatch, IEnumerable<Lap> existingLaps)
    {
        long split = StopwatchEngine.ElapsedMs(stopwatch, _now());

        int number = 0;
        long previousSplit = 0;
        if (existingLaps is not null)
        {
            foreach (Lap lap in existingLaps)
            {
                number++;
                previousSplit = lap.SplitMs;
            }
        }

        return StopwatchEngine.BuildLap(split, number + 1, previousSplit);
    }

    public string FormatDuration(long milliseconds, bool includeMilliseconds)
        => TimeFormat.Format(milliseconds, includeMilliseconds);
}
