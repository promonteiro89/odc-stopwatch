using OutSystems.ExternalLibraries.SDK;

namespace Stopwatch;

// Stateless: every action returns a Stopwatch value the app persists and passes back.
// Elapsed time is measured from the server clock, never the client.
[OSInterface(
    Name = "Stopwatch_Library",
    Description = "Stopwatch: start, pause, resume, reset, read elapsed time, record laps, and format durations.",
    IconResourceName = "AppIcon.png")]
public interface IStopwatchService
{
    [OSAction(ReturnName = "stopwatch", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "A new running stopwatch.",
        IconResourceName = "ActionIcon.png",
        Description = "Starts a new stopwatch.")]
    Stopwatch Start();

    [OSAction(ReturnName = "result", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "The paused stopwatch.",
        IconResourceName = "ActionIcon.png",
        Description = "Pauses the stopwatch. No-op if already paused.")]
    Stopwatch Pause(
        [OSParameter(Description = "Stopwatch to pause.")] Stopwatch stopwatch);

    [OSAction(ReturnName = "result", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "The running stopwatch.",
        IconResourceName = "ActionIcon.png",
        Description = "Resumes the stopwatch. No-op if already running.")]
    Stopwatch Resume(
        [OSParameter(Description = "Stopwatch to resume.")] Stopwatch stopwatch);

    [OSAction(ReturnName = "stopwatch", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "A zeroed, non-running stopwatch.",
        IconResourceName = "ActionIcon.png",
        Description = "Returns a cleared stopwatch.")]
    Stopwatch Reset();

    [OSAction(ReturnName = "reading", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "Elapsed ms, seconds, formatted string, and running flag.",
        IconResourceName = "ActionIcon.png",
        Description = "Reads the current elapsed time.")]
    StopwatchReading Read(
        [OSParameter(Description = "Stopwatch to read.")] Stopwatch stopwatch);

    [OSAction(ReturnName = "elapsedMs", ReturnType = OSDataType.LongInteger,
        ReturnDescription = "Elapsed milliseconds.",
        IconResourceName = "ActionIcon.png",
        Description = "Returns the elapsed milliseconds.")]
    long GetElapsedMilliseconds(
        [OSParameter(Description = "Stopwatch to measure.")] Stopwatch stopwatch);

    [OSAction(ReturnName = "lap", ReturnType = OSDataType.InferredFromDotNetType,
        ReturnDescription = "The recorded lap.",
        IconResourceName = "ActionIcon.png",
        Description = "Records a lap. Pass the laps so far so this lap's duration can be computed; append the result.")]
    Lap RecordLap(
        [OSParameter(Description = "Stopwatch to split.")] Stopwatch stopwatch,
        [OSParameter(Description = "Laps recorded so far, in order. Empty for the first lap.")] IEnumerable<Lap> existingLaps);

    [OSAction(ReturnName = "formatted", ReturnType = OSDataType.Text,
        ReturnDescription = "Duration as HH:MM:SS[.fff].",
        IconResourceName = "ActionIcon.png",
        Description = "Formats a millisecond duration.")]
    string FormatDuration(
        [OSParameter(DataType = OSDataType.LongInteger, Description = "Duration in milliseconds.")] long milliseconds,
        [OSParameter(Description = "Include the .fff fraction.")] bool includeMilliseconds);
}
