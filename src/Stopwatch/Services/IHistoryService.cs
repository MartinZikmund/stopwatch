using Stopwatch.Models;

namespace Stopwatch.Services;
public interface IHistoryService
{
	HistoryStopwatchModel[] GetAll();

	void Delete(HistoryStopwatchModel historyStopwatch);

	void Save(StopwatchModel stopwatch);
}
