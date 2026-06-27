# ODC Stopwatch

A server-clock stopwatch external library for OutSystems Developer Cloud (ODC):
start, pause, resume, reset, read elapsed time, record laps, format durations.

Built with the [ODC External Libraries SDK](https://www.nuget.org/packages/OutSystems.ExternalLibraries.SDK),
targeting .NET 8.

## Design

ODC actions are stateless and may run on different server instances, so the library keeps no
state. Each action returns a `Stopwatch` value the app persists (screen/client variable,
session, or DB) and passes back. Elapsed time is computed from the server's UTC clock on every
read — `accumulated + (now − anchor)` while running, `accumulated` while paused — so it reads
the same on every device and survives reloads. Negative deltas from node clock skew clamp to zero.

## API

| Action | Inputs | Output |
|---|---|---|
| `Start` | — | `Stopwatch` |
| `Pause` | `Stopwatch` | `Stopwatch` (idempotent) |
| `Resume` | `Stopwatch` | `Stopwatch` (idempotent) |
| `Reset` | — | `Stopwatch` |
| `Read` | `Stopwatch` | `StopwatchReading` |
| `GetElapsedMilliseconds` | `Stopwatch` | `Long Integer` |
| `RecordLap` | `Stopwatch`, `List<Lap>` | `Lap` |
| `FormatDuration` | `Long Integer`, `Boolean` | `Text` |

Structures: `Stopwatch` (`AnchorEpochMs`, `AccumulatedMs`, `IsRunning`),
`StopwatchReading` (`ElapsedMs`, `ElapsedSeconds`, `Formatted`, `IsRunning`),
`Lap` (`Number`, `SplitMs`, `LapMs`, `LapFormatted`, `SplitFormatted`).

## Build & package

Requires the .NET 8 SDK (or newer targeting net8.0).

```bash
./pack.sh          # build + test + write dist/Stopwatch_Library.zip
```

The ZIP holds only `Stopwatch_Library.dll` and `Stopwatch_Library.deps.json`; the SDK assembly is
compile-time only and supplied by ODC at runtime.

## Upload

In the ODC Portal: **External logic → Upload external logic**, select `dist/Stopwatch_Library.zip`.
Then add the **Stopwatch_Library** dependency in ODC Studio and use its actions.

## Ticking timer

1. Local variable `MyStopwatch` of type `Stopwatch`.
2. Start button: `MyStopwatch = Stopwatch_Library.Start()`. Pause/Resume similar.
3. A timer (e.g. every 250 ms) calls `Stopwatch_Library.Read(MyStopwatch)` and binds `.Formatted` to a label.
4. Laps: keep a `List<Lap>`, append `Stopwatch_Library.RecordLap(MyStopwatch, laps)`.

## Layout

```
src/    IStopwatchService.cs (OSInterface), StopwatchService.cs, StopwatchEngine.cs,
        TimeFormat.cs, Structures.cs, Stopwatch.csproj
tests/  StopwatchTests.cs (17 tests, fake clock)
pack.sh
```
