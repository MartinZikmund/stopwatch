using LiteDB;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.LiteDb;

internal class LiteDbDataSource : IDataSource
{
	private bool _isInitialized;
	private LiteDatabase? _liteDatabase;

	public LiteDbDataSource()
	{
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
		var dataFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
		var dbPath = Path.Combine(dataFolder.Path, "stopwatch.db");
		_liteDatabase = new LiteDatabase(dbPath);

		Stopwatches = new StopwatchLiteDbRepository(_liteDatabase);
		HistoryStopwatches = new LiteDbRepository<HistoryEntryModel>(_liteDatabase);
	}
}
