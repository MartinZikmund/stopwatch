#if __IOS__ || __ANDROID__
using System.Text.Json;
using SQLite;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Sqlite;

internal class SqliteStopwatchRepository : IStopwatchRepository
{
	private readonly SQLiteConnection _connection;

	public SqliteStopwatchRepository(SQLiteConnection connection)
	{
		_connection = connection;
	}

	public StopwatchModel GetOrCreateFirst()
	{
		if (GetAll() is { Length: > 0 } array)
		{
			return array[0];
		}

		var stopwatch = new StopwatchModel();
		Add(stopwatch);
		return stopwatch;
	}

	public StopwatchModel[] GetAll()
	{
		return _connection.Table<StopwatchEntity>()
			.Select(ToModel)
			.ToArray();
	}

	public StopwatchModel? Get(int id)
	{
		var entity = _connection.Find<StopwatchEntity>(id);
		return entity is null ? null : ToModel(entity);
	}

	public void Add(StopwatchModel item)
	{
		var entity = ToEntity(item);
		_connection.Insert(entity);
		item.Id = entity.Id;
	}

	public void Update(StopwatchModel item)
	{
		var entity = ToEntity(item);
		_connection.Update(entity);
	}

	public void Delete(int id)
	{
		_connection.Delete<StopwatchEntity>(id);
	}

	public void DeleteAll()
	{
		_connection.DeleteAll<StopwatchEntity>();
	}

	private static StopwatchEntity ToEntity(StopwatchModel model) => new()
	{
		Id = model.Id,
		Icon = model.Icon,
		Name = model.Name,
		InitialStartTime = model.InitialStartTime?.ToString("O"),
		LastStartTime = model.LastStartTime?.ToString("O"),
		PausedElapsedTimeTicks = model.PausedElapsedTime.Ticks,
		LapsJson = JsonSerializer.Serialize(model.Laps, StopwatchJsonContext.Default.LapModelArray),
		Theme = (int)model.Theme,
		BackgroundImageUri = model.BackgroundImageUri,
		BackgroundImageOpacity = model.BackgroundImageOpacity,
		BackgroundColor = model.BackgroundColor,
	};

	private static StopwatchModel ToModel(StopwatchEntity entity) => new()
	{
		Id = entity.Id,
		Icon = entity.Icon,
		Name = entity.Name,
		InitialStartTime = entity.InitialStartTime is not null
			? DateTimeOffset.Parse(entity.InitialStartTime)
			: null,
		LastStartTime = entity.LastStartTime is not null
			? DateTimeOffset.Parse(entity.LastStartTime)
			: null,
		PausedElapsedTime = TimeSpan.FromTicks(entity.PausedElapsedTimeTicks),
		Laps = entity.LapsJson is not null
			? JsonSerializer.Deserialize(entity.LapsJson, StopwatchJsonContext.Default.LapModelArray) ?? []
			: [],
		Theme = (ElementTheme)entity.Theme,
		BackgroundImageUri = entity.BackgroundImageUri,
		BackgroundImageOpacity = entity.BackgroundImageOpacity,
		BackgroundColor = entity.BackgroundColor ?? "#00000000",
	};
}
#endif
