namespace Stopwatch.Models;

public record StopwatchExportModel(
    string Name,
    DateTimeOffset? InitialStartTime,
    DateTimeOffset? LastStartTime,
    TimeSpan PausedElapsedTime,
    LapExportModel[] Laps
);
