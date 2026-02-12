#if __IOS__ || __ANDROID__
using SQLite;

namespace Stopwatch.Services.Data.Sqlite;

[Table("HistoryEntries")]
internal class HistoryEntryEntity
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }

	public string? Icon { get; set; }

	public string? Name { get; set; }

	public string? InitialStartTime { get; set; }

	public long PausedElapsedTimeTicks { get; set; }

	public string? LapsJson { get; set; }

	public string? BackgroundImageUri { get; set; }

	public double BackgroundImageOpacity { get; set; } = 0.8;

	public string? BackgroundColor { get; set; }
}
#endif
