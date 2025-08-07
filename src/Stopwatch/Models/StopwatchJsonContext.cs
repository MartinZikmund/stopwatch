using System.Text.Json.Serialization;

namespace Stopwatch.Models;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(StopwatchExportModel))]
[JsonSerializable(typeof(LapExportModel))]
public partial class StopwatchJsonContext : JsonSerializerContext
{
    // The source generator will provide the Default property
}
