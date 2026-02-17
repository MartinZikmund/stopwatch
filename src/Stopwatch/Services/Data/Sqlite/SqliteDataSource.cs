#if __IOS__ || __ANDROID__
using SQLite;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Sqlite;

internal class SqliteDataSource : IDataSource
{
	private bool _isInitialized;
	private SQLiteConnection? _connection;

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
		var dbPath = Path.Combine(dataFolder.Path, "stopwatch.sqlite");
		_connection = new SQLiteConnection(dbPath);

		_connection.CreateTable<StopwatchEntity>();
		_connection.CreateTable<HistoryEntryEntity>();

		Stopwatches = new SqliteStopwatchRepository(_connection);
		HistoryStopwatches = new SqliteHistoryRepository(_connection);
	}
}
#endif
