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
		Migrate(_liteDatabase);
		Stopwatches = new StopwatchLiteDbRepository(_liteDatabase);
		HistoryStopwatches = new LiteDbRepository<HistoryEntryModel>(_liteDatabase, "History");
	}

	private void Migrate(LiteDatabase db)
	{
		if (db.UserVersion == 0)
		{
			if (db.GetCollectionNames().Contains("StopwatchModel"))
			{
				var legacyCollection = db.GetCollection("StopwatchModel");
				var newCollection = db.GetCollection<StopwatchModel>("Stopwatches");
				foreach (var stopwatch in legacyCollection.FindAll())
				{
					var stopwatchModel = new StopwatchModel();
					stopwatchModel.Id = Guid.NewGuid().ToString();
					stopwatchModel.Name = stopwatch["Name"].AsString;
					stopwatchModel.Icon = stopwatch["Icon"].AsString;
					stopwatchModel.BackgroundColor = stopwatch["BackgroundColor"].AsString;
					stopwatchModel.BackgroundImageUri = stopwatch["BackgroundImageUri"].AsString;
					stopwatchModel.BackgroundImageOpacity = stopwatch["BackgroundImageOpacity"].AsDouble;
					stopwatchModel.InitialStartTime = stopwatch.TryGetValue("InitialStartTime", out var initialStartTimeValue) ? initialStartTimeValue.AsDateTime : null;
					stopwatchModel.LastStartTime = stopwatch.TryGetValue("LastStartTime", out var lastStartTimeValue) ? lastStartTimeValue.AsDateTime : null;
					stopwatchModel.PausedElapsedTime = TimeSpan.FromTicks(stopwatch["PausedElapsedTime"]);
					stopwatchModel.Theme = Enum.TryParse<ElementTheme>(stopwatch["Theme"].AsString, out var theme) ? theme : ElementTheme.Default;
					stopwatchModel.Laps = stopwatch["Laps"].AsArray.Select(lap => new LapModel
					{
						Note = lap["Note"].AsString,
						TotalTime = TimeSpan.FromTicks(lap["TotalTime"])
					}).ToArray();

					newCollection.Insert(stopwatchModel);
				}
			}

			db.UserVersion = 1;
		}
	}
}
