using System.Globalization;

namespace Stopwatch;

internal static class TimeFormat
{
    // HH:MM:SS[.fff]. Hours grow past 99 when needed; negatives clamp to zero.
    public static string Format(long totalMs, bool includeMilliseconds)
    {
        if (totalMs < 0) totalMs = 0;

        long hours = totalMs / 3_600_000;
        int minutes = (int)(totalMs / 60_000 % 60);
        int seconds = (int)(totalMs / 1000 % 60);
        int millis = (int)(totalMs % 1000);

        string core = string.Create(CultureInfo.InvariantCulture, $"{hours:D2}:{minutes:D2}:{seconds:D2}");
        return includeMilliseconds
            ? string.Create(CultureInfo.InvariantCulture, $"{core}.{millis:D3}")
            : core;
    }
}
