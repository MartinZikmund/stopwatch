using Stopwatch.Models;

namespace Stopwatch.Services;

public interface IHistoryService
{
	HistoryEntryModel[] GetAll();

	void Delete(HistoryEntryModel historyStopwatch);

	bool CanSave(StopwatchModel stopwatch);

	void Save(StopwatchModel stopwatch);

	void Clear();
}
