using System;

namespace Stopwatch.Models;

public record LapExportModel(
	TimeSpan LapTime,
	TimeSpan TotalTime,
	string Note
);
