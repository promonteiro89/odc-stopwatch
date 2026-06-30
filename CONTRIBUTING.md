# Contributing

Thanks for your interest in improving Stopwatch_Library.

## Prerequisites

- .NET 10 SDK (targeting `net10.0`)

## Build

```bash
dotnet build Stopwatch.csproj -c Release
```

## Project conventions

- The public surface is the `[OSInterface]` in `IStopwatchService.cs`. Keep action and
  parameter names camelCase, and ensure each action's output name does not collide with an
  input name (ODC otherwise appends a numeric suffix).
- Keep the timing logic pure and clock-free in `StopwatchEngine.cs` — "now" is passed in as
  epoch milliseconds. `StopwatchService.cs` is the only place that reads the real clock, via
  an injectable seam.
- Structures live in `Structures.cs` with explicit `OSDataType` on every field.
- Comments are terse and explain *why*, not *what*. No filler.

## Pull requests

1. Fork and create a feature branch off `main`.
2. Make focused commits with clear messages.
3. Confirm `dotnet build Stopwatch.csproj -c Release` is clean (no warnings).
4. Open a PR against `main` describing the change and its rationale.

## Reporting issues

Open a GitHub issue with steps to reproduce, expected vs. actual behaviour, and the ODC /
runtime context where relevant.
