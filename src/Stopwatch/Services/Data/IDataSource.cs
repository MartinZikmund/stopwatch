using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public interface IDataSource
{
	Task InitializeAsync();

	IStopwatchRepository Stopwatches { get; }

	IRepository<HistoryStopwatchModel> HistoryStopwatches { get; }
}
