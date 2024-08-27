using Stopwatch.Models;

namespace Stopwatch.Services;
public interface IHistoryService
{
	HistoryEntryModel[] GetAll();

	void Delete(HistoryEntryModel historyStopwatch);

	void Save(StopwatchModel stopwatch);

	void Clear();
}
