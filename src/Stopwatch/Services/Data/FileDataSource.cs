using System.Text.Json;
using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public class FileDataSource : IDataSource
{
	private const string FileName = "data.json";
	private StorageFile _dataFile;
	private string _filePath;

	public IStopwatchRepository Stopwatches => throw new NotImplementedException();

	public IRepository<HistoryEntryModel> HistoryStopwatches => throw new NotImplementedException();

	public FileDataSource()
	{
	}

	public async Task InitializeAsync()
	{
		var dataFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
		_filePath = Path.Combine(dataFolder.Path, FileName);
		if (!File.Exists(_filePath))
		{
			SaveData(new DataFileLayout());
		}
	}

	public void Add(StopwatchModel stopwatch)
	{
		var data = ReadData();
		data.Stopwatches.Add(stopwatch);
		stopwatch.Id = data.Stopwatches.Count == 0 ? 1 : data.Stopwatches.Max(s => s.Id) + 1;
		SaveData(data);
	}

	public StopwatchModel? Get(int id)
	{
		var data = ReadData();
		return data.Stopwatches.FirstOrDefault(s => s.Id == id);
	}

	public StopwatchModel[] GetAll()
	{
		var data = ReadData();
		return data.Stopwatches.ToArray();
	}

	public void Update(StopwatchModel stopwatch)
	{
		var data = ReadData();
		var index = data.Stopwatches.FindIndex(s => s.Id == stopwatch.Id);
		if (index == -1)
		{
			return;
		}

		data.Stopwatches[index] = stopwatch;
		SaveData(data);
	}

	private DataFileLayout ReadData()
	{
		var data = File.ReadAllText(_filePath);
		return JsonSerializer.Deserialize<DataFileLayout>(data) ?? new DataFileLayout();
	}

	private void SaveData(DataFileLayout dataFileLayout)
	{
		File.WriteAllText(_filePath, JsonSerializer.Serialize(dataFileLayout));
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
