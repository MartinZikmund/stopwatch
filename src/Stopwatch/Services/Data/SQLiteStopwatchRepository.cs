using Microsoft.EntityFrameworkCore;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.SQLite;

internal class SQLiteStopwatchRepository : IStopwatchRepository
{
	private readonly StopwatchDbContext _dbContext;

	public SQLiteStopwatchRepository(StopwatchDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public StopwatchModel GetOrCreateFirst()
	{
		var stopwatch = _dbContext.Stopwatches.FirstOrDefault();
		if (stopwatch == null)
		{
			stopwatch = new StopwatchModel();
			_dbContext.Stopwatches.Add(stopwatch);
			_dbContext.SaveChanges();
		}
		return stopwatch;
	}

	public StopwatchModel[] GetAll()
	{
		return _dbContext.Stopwatches.ToArray();
	}

	public StopwatchModel? Get(int id)
	{
		return _dbContext.Stopwatches.Find(id);
	}

	public void Add(StopwatchModel item)
	{
		_dbContext.Stopwatches.Add(item);
		_dbContext.SaveChanges();
	}

	public void Update(StopwatchModel item)
	{
		_dbContext.Stopwatches.Update(item);
		_dbContext.SaveChanges();
	}

	public void Delete(int id)
	{
		var item = _dbContext.Stopwatches.Find(id);
		if (item != null)
		{
			_dbContext.Stopwatches.Remove(item);
			_dbContext.SaveChanges();
		}
	}

	public void DeleteAll()
	{
		_dbContext.Stopwatches.RemoveRange(_dbContext.Stopwatches);
		_dbContext.SaveChanges();
	}
}