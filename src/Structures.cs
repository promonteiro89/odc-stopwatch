using OutSystems.ExternalLibraries.SDK;

namespace Stopwatch;

[OSStructure(Description = "Stopwatch state. Store it and pass it back to every action.")]
public struct Stopwatch
{
    [OSStructureField(DataType = OSDataType.LongInteger, IsMandatory = true, DefaultValue = "0",
        Description = "Epoch ms when the running segment started. Managed by the actions.")]
    public long AnchorEpochMs;

    [OSStructureField(DataType = OSDataType.LongInteger, IsMandatory = true, DefaultValue = "0",
        Description = "Elapsed ms banked from paused segments. Managed by the actions.")]
    public long AccumulatedMs;

    [OSStructureField(DataType = OSDataType.Boolean, IsMandatory = true, DefaultValue = "false",
        Description = "True while counting.")]
    public bool IsRunning;
}

[OSStructure(Description = "A computed snapshot of elapsed time.")]
public struct StopwatchReading
{
    [OSStructureField(DataType = OSDataType.LongInteger, Description = "Elapsed milliseconds.")]
    public long ElapsedMs;

    [OSStructureField(DataType = OSDataType.Decimal, Description = "Elapsed seconds, ms precision.")]
    public decimal ElapsedSeconds;

    [OSStructureField(DataType = OSDataType.Text, Description = "Elapsed time as HH:MM:SS.fff.")]
    public string Formatted;

    [OSStructureField(DataType = OSDataType.Boolean, Description = "Whether the stopwatch was running.")]
    public bool IsRunning;
}

[OSStructure(Description = "A recorded lap.")]
public struct Lap
{
    [OSStructureField(DataType = OSDataType.Integer, Description = "1-based lap position.")]
    public int Number;

    [OSStructureField(DataType = OSDataType.LongInteger, Description = "Total elapsed ms at this lap.")]
    public long SplitMs;

    [OSStructureField(DataType = OSDataType.LongInteger, Description = "Ms since the previous lap.")]
    public long LapMs;

    [OSStructureField(DataType = OSDataType.Text, Description = "LapMs as HH:MM:SS.fff.")]
    public string LapFormatted;

    [OSStructureField(DataType = OSDataType.Text, Description = "SplitMs as HH:MM:SS.fff.")]
    public string SplitFormatted;
}
