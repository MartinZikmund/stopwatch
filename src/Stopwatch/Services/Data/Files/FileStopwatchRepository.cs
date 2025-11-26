using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Files;

internal class FileStopwatchRepository : FileRepository<StopwatchModel>, IStopwatchRepository
{
	private readonly FileDataSource _dataSource;

	public FileStopwatchRepository(FileDataSource dataSource) : base("stopwatches.json", dataSource, StopwatchJsonContext.Default.ListStopwatchModel)
	{
		_dataSource = dataSource;
	}

	public StopwatchModel GetOrCreateFirst()
	{
		StopwatchModel stopwatch;
		if (GetAll() is { Length: > 0 } array)
		{
			stopwatch = array[0];
		}
		else
		{
			stopwatch = new StopwatchModel();
			Add(stopwatch);
		}

		return stopwatch;
	}
}
