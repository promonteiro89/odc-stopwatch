# Stopwatch for ODC

[![Platform](https://img.shields.io/badge/Platform-OutSystems_ODC-red.svg)](https://www.outsystems.com/odc/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen.svg)](#)

A lightweight .NET 8.0 External Logic component for OutSystems Developer Cloud (ODC) that provides a precise, **server-clock stopwatch** — start, pause, resume, reset, read elapsed time, record laps, and format durations — with zero configuration.

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Action Reference](#action-reference)
- [Data Structures](#data-structures)
- [Project Structure](#project-structure)
- [Build and Deployment](#build-and-deployment)
- [Notes and Best Practices](#notes-and-best-practices)
- [Contributing](#contributing)
- [License](#license)

---

## Architecture

```
odc-stopwatch/
├── Stopwatch.csproj        # Project definition
├── IStopwatchService.cs    # ODC External Logic interface
├── StopwatchService.cs     # Implementation
├── StopwatchEngine.cs      # Pure, clock-free timing logic
├── TimeFormat.cs           # HH:MM:SS.fff formatting
├── Structures.cs           # Strongly-typed ODC structures
└── Resources/              # Embedded branded icons
```

The library is a **stateless calculator**. ODC External Logic actions hold no state between calls and may run on different server instances, so a live timer object cannot survive across calls. Instead, every action returns a `Stopwatch` value that the app persists and passes back; elapsed time is recomputed from the server clock on each read:

```
elapsed = AccumulatedMs + (IsRunning ? now − AnchorEpochMs : 0)
```

### Key Architectural Decisions
- **Stateless execution:** state travels with the data, not the server. Each action reads the clock on demand and returns a new `Stopwatch` value — thread-safe in high-concurrency ODC environments.
- **Server clock is the source of truth:** time is measured from the server (UTC epoch milliseconds), never the client, so a stopwatch reads identically on every device and survives page reloads. Clock skew that would yield a negative delta is clamped to zero.
- **Not `System.Diagnostics.Stopwatch`:** that type is process-local and non-serializable, so it cannot span stateless calls; an epoch-anchored value model is used instead.
- **Resource embedding:** branded library and action icons are embedded directly into the assembly for an integrated experience in ODC Studio.

---

## Prerequisites

- [OutSystems Developer Cloud (ODC)](https://www.outsystems.com/odc/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

No third-party packages — the library depends only on the OutSystems External Libraries SDK (compile-time) and the .NET base class library.

---

## Quick Start

```bash
# Build
dotnet build Stopwatch.csproj -c Release

# Publish for ODC
dotnet publish Stopwatch.csproj -c Release -f net8.0 --no-self-contained
```

After publishing, zip the contents of the `publish/` folder (**excluding** `OutSystems.ExternalLibraries.SDK.dll`) and upload it to the ODC Portal under **External Logic**.

---

## Action Reference

Persist the `Stopwatch` value returned by each action (in a client/screen variable, session, or database row) and pass it back into the next call.

#### `Start`
Starts a brand-new stopwatch, anchored to the current server time.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `stopwatch` | `Stopwatch` | A new, running stopwatch |

#### `Pause`
Freezes a running stopwatch. No-op if already paused.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `stopwatch` | `Stopwatch` | The stopwatch to pause |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `result` | `Stopwatch` | The paused stopwatch (resumable) |

#### `Resume`
Continues a paused stopwatch from where it stopped. No-op if already running.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `stopwatch` | `Stopwatch` | The stopwatch to resume |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `result` | `Stopwatch` | The running stopwatch |

#### `Reset`
Returns a cleared stopwatch (zero elapsed, not running).

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `stopwatch` | `Stopwatch` | A zeroed, non-running stopwatch |

#### `Read`
Reads the current elapsed time at the present server instant.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `stopwatch` | `Stopwatch` | The stopwatch to read |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `reading` | `StopwatchReading` | Elapsed ms, seconds, formatted string, and running flag |

#### `GetElapsedMilliseconds`
Returns only the elapsed milliseconds — lighter than `Read` when a number is all you need.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `stopwatch` | `Stopwatch` | The stopwatch to measure |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `elapsedMs` | `Long Integer` | Total elapsed milliseconds |

#### `RecordLap`
Records a lap. Pass the laps recorded so far so this lap's own duration can be computed; append the result to that list.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `stopwatch` | `Stopwatch` | The stopwatch to take a split from |
| `existingLaps` | `List<Lap>` | All laps recorded so far, in order (empty for the first lap) |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `lap` | `Lap` | The new lap: number, split total, and per-lap duration |

#### `FormatDuration`
Formats any millisecond duration as a clock string.

**Inputs:**
| Input | Type | Description |
|-------|------|-------------|
| `milliseconds` | `Long Integer` | Duration in milliseconds |
| `includeMilliseconds` | `Boolean` | Include the `.fff` fraction |

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `formatted` | `Text` | The duration as `HH:MM:SS` or `HH:MM:SS.fff` |

---

## Data Structures

### `Stopwatch`
The serializable state you persist between calls.
- `AnchorEpochMs`: Long Integer — epoch ms when the running segment started (managed by the actions)
- `AccumulatedMs`: Long Integer — elapsed ms banked from paused segments (managed by the actions)
- `IsRunning`: Boolean — whether it is currently counting

### `StopwatchReading`
A computed snapshot of elapsed time.
- `ElapsedMs`: Long Integer
- `ElapsedSeconds`: Decimal — millisecond precision
- `Formatted`: Text — `HH:MM:SS.fff` (hours grow past 99 when needed)
- `IsRunning`: Boolean

### `Lap`
A recorded split.
- `Number`: Integer — 1-based lap position
- `SplitMs`: Long Integer — total elapsed ms at the lap
- `LapMs`: Long Integer — ms since the previous lap
- `LapFormatted`: Text — `LapMs` as `HH:MM:SS.fff`
- `SplitFormatted`: Text — `SplitMs` as `HH:MM:SS.fff`

---

## Project Structure

```
odc-stopwatch/
├── Stopwatch.csproj        # net8.0, OutSystems External Libraries SDK 1.5.0
├── IStopwatchService.cs    # OSInterface & OSAction definitions
├── StopwatchService.cs     # Implementation (stateless; injectable clock)
├── StopwatchEngine.cs      # Pure, clock-free timing logic
├── TimeFormat.cs           # HH:MM:SS.fff formatting
├── Structures.cs           # OSStructure: Stopwatch / StopwatchReading / Lap
└── Resources/              # Branding assets
    ├── AppIcon.png         # Library icon
    └── ActionIcon.png      # Action-level icon
```

---

## Build and Deployment

1. **Publish:** Run `dotnet publish` as shown in Quick Start.
2. **Clean:** Delete `OutSystems.ExternalLibraries.SDK.dll` from the `publish/` directory if present (ODC supplies it at runtime).
3. **Zip:** Compress the remaining files into a flat structure (no subfolders).
4. **Deploy:** Upload to ODC Portal > External Logic, then add the **Stopwatch_Library** dependency in ODC Studio.

---

## Notes and Best Practices

- **Persist the value:** store the `Stopwatch` returned by each action and pass it back in. The component keeps no state of its own.
- **Live display:** for a ticking timer, store the `Stopwatch`, run a UI timer/refresh, and call `Read` on each tick — bind `.Formatted` to a label. Because elapsed time is recomputed from the server clock, the display stays correct even if a refresh is missed or the page is reopened.
- **Pause vs. Reset:** `Pause` freezes and is resumable; `Reset` returns a zeroed stopwatch.
- **Precision:** timing is millisecond-resolution, far finer than any human-driven UI stopwatch needs.

---

## Contributing

Contributions, bug reports, and feature requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on development setup, code conventions, and how to submit a pull request.

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
