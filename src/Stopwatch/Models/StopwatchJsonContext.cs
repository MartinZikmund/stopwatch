using Stopwatch.Services.Data;

namespace Stopwatch.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(StopwatchExportModel))]
[JsonSerializable(typeof(LapExportModel))]
[JsonSerializable(typeof(DataFileLayout))]
[JsonSerializable(typeof(StopwatchModel))]
[JsonSerializable(typeof(HistoryEntryModel[]))]
[JsonSerializable(typeof(List<HistoryEntryModel>))]
[JsonSerializable(typeof(StopwatchModel[]))]
[JsonSerializable(typeof(List<StopwatchModel>))]
public partial class StopwatchJsonContext : JsonSerializerContext
{
    // The source generator will provide the Default property
}
