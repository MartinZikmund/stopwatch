using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch.Models;
using Stopwatch.Services.Data;

namespace Stopwatch.Services;
internal class HistoryService : IHistoryService
{
	private readonly IDataSource _dataSource;

	public HistoryService(IDataSource dataSource)
	{
		_dataSource = dataSource;
	}

	public HistoryStopwatchModel[] GetAll() =>
		_dataSource.HistoryStopwatches
			.GetAll()
			.OrderByDescending(s => s.InitialStartTime)
			.ToArray();

	public void Delete(HistoryStopwatchModel historyStopwatch)
	{
		_dataSource.HistoryStopwatches.Delete(historyStopwatch);
	}

	public void Save(StopwatchModel stopwatch)
	{
		if (stopwatch.InitialStartTime is not { } startTime)
		{
			throw new InvalidOperationException("Only already started stopwatches can be saved to history.");
		}

		var lapsCopy = stopwatch.Laps.Select(lap => new LapModel() { TotalTime = lap.TotalTime, Note = lap.Note }).ToArray();

		var historyModel = new HistoryStopwatchModel(
			stopwatch.Icon,
			stopwatch.Name,
			startTime,
			stopwatch.PausedElapsedTime,
			lapsCopy,
			stopwatch.BackgroundImageUri,
			stopwatch.BackgroundImageOpacity,
			stopwatch.BackgroundColor);

		_dataSource.HistoryStopwatches.Add(historyModel);
	}
}
