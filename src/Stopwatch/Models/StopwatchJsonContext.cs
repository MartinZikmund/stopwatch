using Stopwatch.Services.Data;
using Stopwatch.Services.Store;

namespace Stopwatch.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(StopwatchExportModel))]
[JsonSerializable(typeof(LapExportModel))]
[JsonSerializable(typeof(DataFileLayout))]
[JsonSerializable(typeof(StopwatchModel))]
[JsonSerializable(typeof(HistoryEntryModel[]))]
[JsonSerializable(typeof(List<HistoryEntryModel>))]
[JsonSerializable(typeof(StopwatchModel[]))]
[JsonSerializable(typeof(LapModel[]))]
[JsonSerializable(typeof(List<StopwatchModel>))]
[JsonSerializable(typeof(RevenueCatOptions))]
public partial class StopwatchJsonContext : JsonSerializerContext
{
	// The source generator will provide the Default property
}
