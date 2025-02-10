using LiteDB;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.LiteDb;

internal class StopwatchLiteDbRepository : LiteDbRepository<StopwatchModel>, IStopwatchRepository
{
	public StopwatchLiteDbRepository(LiteDatabase database) : base(database, "Stopwatches")
	{
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
