using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public interface IDataSource
{
	Task InitializeAsync();

	IStopwatchRepository Stopwatches { get; }

	IRepository<HistoryEntryModel> HistoryStopwatches { get; }
}
