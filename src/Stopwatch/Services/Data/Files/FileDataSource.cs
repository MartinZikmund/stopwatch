using System.Text.Json;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Files;

public class FileDataSource : IDataSource
{
	private const string FileName = "data.json";
	private string _filePath;

	public IStopwatchRepository Stopwatches => new FileStopwatchRepository(this);

	public IRepository<HistoryEntryModel> HistoryStopwatches => new FileRepository<HistoryEntryModel>("history.json", this, StopwatchJsonContext.Default.ListHistoryEntryModel);

	public FileDataSource()
	{
	}

	public string DataFolderPath { get; private set; }

	public async Task InitializeAsync()
	{
		var dataFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
		DataFolderPath = dataFolder.Path;
	}
}
