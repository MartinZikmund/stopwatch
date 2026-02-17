#if __IOS__ || __ANDROID__
using System.Text.Json;
using SQLite;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Sqlite;

internal class SqliteHistoryRepository : IRepository<HistoryEntryModel>
{
	private readonly SQLiteConnection _connection;

	public SqliteHistoryRepository(SQLiteConnection connection)
	{
		_connection = connection;
	}

	public HistoryEntryModel[] GetAll()
	{
		return _connection.Table<HistoryEntryEntity>()
			.Select(ToModel)
			.ToArray();
	}

	public HistoryEntryModel? Get(int id)
	{
		var entity = _connection.Find<HistoryEntryEntity>(id);
		return entity is null ? null : ToModel(entity);
	}

	public void Add(HistoryEntryModel item)
	{
		var entity = ToEntity(item);
		_connection.Insert(entity);
		item.Id = entity.Id;
	}

	public void Update(HistoryEntryModel item)
	{
		var entity = ToEntity(item);
		_connection.Update(entity);
	}

	public void Delete(int id)
	{
		_connection.Delete<HistoryEntryEntity>(id);
	}

	public void DeleteAll()
	{
		_connection.DeleteAll<HistoryEntryEntity>();
	}

	private static HistoryEntryEntity ToEntity(HistoryEntryModel model) => new()
	{
		Id = model.Id,
		Icon = model.Icon,
		Name = model.Name,
		InitialStartTime = model.InitialStartTime?.ToString("O"),
		PausedElapsedTimeTicks = model.PausedElapsedTime.Ticks,
		LapsJson = JsonSerializer.Serialize(model.Laps, StopwatchJsonContext.Default.LapModelArray),
		BackgroundImageUri = model.BackgroundImageUri,
		BackgroundImageOpacity = model.BackgroundImageOpacity,
		BackgroundColor = model.BackgroundColor,
	};

	private static HistoryEntryModel ToModel(HistoryEntryEntity entity) => new()
	{
		Id = entity.Id,
		Icon = entity.Icon,
		Name = entity.Name,
		InitialStartTime = entity.InitialStartTime is not null
			? DateTimeOffset.Parse(entity.InitialStartTime)
			: null,
		PausedElapsedTime = TimeSpan.FromTicks(entity.PausedElapsedTimeTicks),
		Laps = entity.LapsJson is not null
			? JsonSerializer.Deserialize(entity.LapsJson, StopwatchJsonContext.Default.LapModelArray) ?? []
			: [],
		BackgroundImageUri = entity.BackgroundImageUri,
		BackgroundImageOpacity = entity.BackgroundImageOpacity,
		BackgroundColor = entity.BackgroundColor ?? "#00000000",
	};
}
#endif
