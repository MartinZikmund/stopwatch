using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.SQLite;

internal class SQLiteDataSource : IDataSource
{
	private readonly ILogger<SQLiteDataSource> _logger;
	private readonly StopwatchDbContext _dbContext;
	private bool _isInitialized;

	public SQLiteDataSource(ILogger<SQLiteDataSource> logger, StopwatchDbContext dbContext)
	{
		_logger = logger;
		_dbContext = dbContext;
	}

	public IStopwatchRepository Stopwatches { get; private set; } = null!;

	public IRepository<HistoryEntryModel> HistoryStopwatches { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		if (_isInitialized)
		{
			return;
		}

		_isInitialized = true;
		await _dbContext.Database.EnsureCreatedAsync();

		Stopwatches = new SQLiteStopwatchRepository(_dbContext);
		HistoryStopwatches = new SQLiteRepository<HistoryEntryModel>(_dbContext);
	}
}