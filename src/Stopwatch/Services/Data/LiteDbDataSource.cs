using System.Diagnostics;
using LiteDB;
using Stopwatch.Models;

namespace Stopwatch.Services.Data;
internal class LiteDbDataSource : IDataSource
{
	private LiteDatabase _liteDatabase;

	public LiteDbDataSource()
	{
	}

	public async Task InitializeAsync()
	{
		var dataFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
		var dbPath = Path.Combine(dataFolder.Path, "stopwatch.db");
		_liteDatabase = new LiteDatabase(dbPath);
	}

	public void Add(StopwatchModel stopwatch)
	{
		var collection = _liteDatabase.GetCollection<StopwatchModel>();
		var id = collection.Insert(stopwatch);
		stopwatch.Id = id.AsInt32;
	}

	public StopwatchModel Get(int id)
	{
		var collection = _liteDatabase.GetCollection<StopwatchModel>();
		return collection.FindById(id);
	}
	public StopwatchModel[] GetAll()
	{
		var collection = _liteDatabase.GetCollection<StopwatchModel>();
		return collection.FindAll().ToArray();
	}

	public void Update(StopwatchModel stopwatch)
	{
		var collection = _liteDatabase.GetCollection<StopwatchModel>();
		collection.Update(stopwatch);
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
