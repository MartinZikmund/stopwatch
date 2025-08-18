using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Stopwatch.Models;

namespace Stopwatch.Services.Data.Files;

internal class FileRepository<T> : IRepository<T> where T : class, IId
{
	private readonly string _filePath;
	private readonly JsonTypeInfo<List<T>> _jsonTypeInfo;

	public FileRepository(string dataFileName, FileDataSource fileDataSource, JsonTypeInfo<List<T>> jsonTypeInfo)
	{
		_filePath = Path.Combine(fileDataSource.DataFolderPath, dataFileName);
		_jsonTypeInfo = jsonTypeInfo;
	}

	public void Add(T item)
	{
		var data = ReadData();
		data.Add(item);
		item.Id = data.Count == 0 ? 1 : data.Max(s => s.Id) + 1;
		SaveData(data);
	}

	public void Delete(int id)
	{
		var data = ReadData();
		var stopwatch = data.FirstOrDefault(s => s.Id == id);
		if (stopwatch != null)
		{
			data.Remove(stopwatch);
			SaveData(data);
		}
	}

	public void DeleteAll()
	{
		var data = ReadData();
		data.Clear();
		SaveData(data);
	}

	public T? Get(int id)
	{
		var data = ReadData();
		return data.FirstOrDefault(s => s.Id == id);
	}

	public T[] GetAll()
	{
		var data = ReadData();
		return data.ToArray();
	}

	public void Update(T stopwatch)
	{
		var data = ReadData();
		var index = data.FindIndex(s => s.Id == stopwatch.Id);
		if (index == -1)
		{
			return;
		}

		data[index] = stopwatch;
		SaveData(data);
	}

	protected List<T> ReadData()
	{
		if (!File.Exists(_filePath))
		{
			return [];
		}

		var data = File.ReadAllText(_filePath);
		return JsonSerializer.Deserialize(data, _jsonTypeInfo) ?? [];
	}

	protected void SaveData(List<T> data)
	{
		File.WriteAllText(_filePath, JsonSerializer.Serialize(data, _jsonTypeInfo));
	}


}
