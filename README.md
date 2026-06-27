# Stopwatch_Library

A precise, server-clock stopwatch for **OutSystems Developer Cloud (ODC)**, delivered as an
[external library](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_custom_code/).
Start, pause, resume, reset, read elapsed time, record laps, and format durations — all as
server actions.

[![Release](https://img.shields.io/github/v/release/promonteiro89/odc-stopwatch?sort=semver)](https://github.com/promonteiro89/odc-stopwatch/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Why a server-clock stopwatch

ODC external-library actions are **stateless** — no object survives between two calls, and
consecutive calls may run on different server instances. A stopwatch you "keep" on the server
would be unreliable, and `System.Diagnostics.Stopwatch` (process-local, non-serializable)
can't span calls at all.

So this library holds **no state of its own**. Every action returns a `Stopwatch` value that
**you persist** (in a client/screen variable, session, or database row) and pass back in.
Elapsed time is recomputed from the server clock on each read:

```
elapsed = AccumulatedMs + (IsRunning ? now − AnchorEpochMs : 0)
```

The result: the same stopwatch reads identically on every device, survives page reloads, and
self-corrects a missed UI refresh — with millisecond precision and no clock-skew surprises
(a negative delta is clamped to zero).

---

## Installation

1. Build the upload package (see [Building](#building)) or download
   `Stopwatch_Library.zip` from the [latest release](https://github.com/promonteiro89/odc-stopwatch/releases).
2. In the **ODC Portal**: **External logic → Upload external logic**, select the ZIP.
3. In **ODC Studio**, add the **Stopwatch_Library** dependency and drag its actions into your
   logic flows.

---

## API

### Actions

| Action | Inputs | Output | Description |
|---|---|---|---|
| `Start` | — | `stopwatch` | A new, running stopwatch. |
| `Pause` | `stopwatch` | `result` | Freeze; resumable. No-op if already paused. |
| `Resume` | `stopwatch` | `result` | Continue from where it was paused. No-op if running. |
| `Reset` | — | `stopwatch` | A zeroed, non-running stopwatch. |
| `Read` | `stopwatch` | `reading` | Snapshot: ms, seconds, formatted string, running flag. |
| `GetElapsedMilliseconds` | `stopwatch` | `elapsedMs` | Elapsed time as a number. |
| `RecordLap` | `stopwatch`, `existingLaps` | `lap` | A lap with its split total and per-lap duration. |
| `FormatDuration` | `milliseconds`, `includeMilliseconds` | `formatted` | Format any duration as `HH:MM:SS[.fff]`. |

### Structures

**`Stopwatch`** — the state you persist between calls.

| Field | Type | Notes |
|---|---|---|
| `AnchorEpochMs` | Long Integer | Epoch ms when the running segment started (managed by the actions). |
| `AccumulatedMs` | Long Integer | Elapsed ms banked from paused segments (managed by the actions). |
| `IsRunning` | Boolean | Whether it is currently counting. |

**`StopwatchReading`** — a computed snapshot.

| Field | Type | Notes |
|---|---|---|
| `ElapsedMs` | Long Integer | Total elapsed milliseconds. |
| `ElapsedSeconds` | Decimal | Total elapsed seconds, millisecond precision. |
| `Formatted` | Text | `HH:MM:SS.fff` (hours grow past 99 when needed). |
| `IsRunning` | Boolean | Whether it was running at the reading. |

**`Lap`** — a recorded split.

| Field | Type | Notes |
|---|---|---|
| `Number` | Integer | 1-based lap position. |
| `SplitMs` | Long Integer | Total elapsed ms at the lap. |
| `LapMs` | Long Integer | Ms since the previous lap. |
| `LapFormatted` | Text | `LapMs` as `HH:MM:SS.fff`. |
| `SplitFormatted` | Text | `SplitMs` as `HH:MM:SS.fff`. |

---

## Usage: a ticking timer

1. Add a local/screen variable `MyStopwatch` of type `Stopwatch`.
2. **Start** button → `MyStopwatch = Stopwatch_Library.Start()`.
3. **Pause** → `MyStopwatch = Stopwatch_Library.Pause(MyStopwatch)`;
   **Resume** → `MyStopwatch = Stopwatch_Library.Resume(MyStopwatch)`.
4. Refresh on a timer (e.g. every 100–250 ms): call `Stopwatch_Library.Read(MyStopwatch)` and
   bind `.Formatted` to a label.
5. **Laps**: keep a `List<Lap>`; on a **Lap** button append
   `Stopwatch_Library.RecordLap(MyStopwatch, laps)`.

Because elapsed time is recomputed from the server clock on each `Read`, the display stays
correct even if a refresh is missed, the page is reopened, or the value is reloaded from the
database.

---

## Building

Requires the **.NET 8 SDK** (or newer, targeting `net8.0`).

```bash
# Compile
dotnet build Stopwatch.csproj -c Release

# Package for ODC upload (DLL + deps.json)
cd bin/Release/net8.0
zip Stopwatch_Library.zip Stopwatch_Library.dll Stopwatch_Library.deps.json
```

The OutSystems SDK assembly is a compile-time contract supplied by ODC at runtime, so it is
intentionally **not** bundled in the ZIP.

---

## Project layout

```
IStopwatchService.cs   # [OSInterface] — the exposed action surface
StopwatchService.cs    # implementation (stateless; injectable clock)
StopwatchEngine.cs     # pure, clock-free logic
TimeFormat.cs          # HH:MM:SS.fff formatting
Structures.cs          # [OSStructure] Stopwatch / StopwatchReading / Lap
Resources/             # embedded library and action icons
Stopwatch.csproj       # net8.0, OutSystems External Libraries SDK 1.5.0
```

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Ricardo Monteiro
