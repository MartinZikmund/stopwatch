namespace Stopwatch.Services.Export;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Stopwatch.Models;
using Stopwatch.Services.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

/// <summary>
/// Service for exporting stopwatch and history data to various formats.
/// </summary>
public class ExportService : IExportService
{
	private readonly IWindowShellProvider _windowShellProvider;

	public ExportService(IWindowShellProvider windowShellProvider)
	{
		_windowShellProvider = windowShellProvider;
	}

	public async Task<bool> ExportToJsonAsync(StopwatchModel stopwatch, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".json", "JSON files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var jsonContent = FormatAsJson(stopwatch);
			await FileIO.WriteTextAsync(file, jsonContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToCsvAsync(StopwatchModel stopwatch, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".csv", "CSV files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var csvContent = FormatLapsAsCsv(stopwatch.Laps);
			await FileIO.WriteTextAsync(file, csvContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToXmlAsync(StopwatchModel stopwatch, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".xml", "XML files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var xmlContent = FormatAsXml(stopwatch.Name, stopwatch.InitialStartTime, stopwatch.LastStartTime, stopwatch.PausedElapsedTime, stopwatch.Laps);
			await FileIO.WriteTextAsync(file, xmlContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToExcelAsync(StopwatchModel stopwatch, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".xlsx", "Excel files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			// Excel can open CSV files with .xlsx extension
			var csvContent = FormatLapsAsCsv(stopwatch.Laps);
			await FileIO.WriteTextAsync(file, csvContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToJsonAsync(HistoryEntryModel entry, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".json", "JSON files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var jsonContent = FormatAsJson(entry);
			await FileIO.WriteTextAsync(file, jsonContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToCsvAsync(HistoryEntryModel entry, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".csv", "CSV files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var csvContent = FormatLapsAsCsv(entry.Laps);
			await FileIO.WriteTextAsync(file, csvContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToXmlAsync(HistoryEntryModel entry, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".xml", "XML files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var xmlContent = FormatAsXml(entry.Name, entry.InitialStartTime, null, entry.PausedElapsedTime, entry.Laps);
			await FileIO.WriteTextAsync(file, xmlContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportToExcelAsync(HistoryEntryModel entry, string suggestedFileName)
	{
		try
		{
			var savePicker = CreateSavePicker(".xlsx", "Excel files");
			savePicker.SuggestedFileName = suggestedFileName;

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			// Excel can open CSV files with .xlsx extension
			var csvContent = FormatLapsAsCsv(entry.Laps);
			await FileIO.WriteTextAsync(file, csvContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> ExportAllHistoryToJsonAsync(HistoryEntryModel[] entries)
	{
		try
		{
			var savePicker = CreateSavePicker(".json", "JSON files");
			savePicker.SuggestedFileName = $"history_export_{DateTime.Now:yyyyMMdd_HHmmss}";

			var file = await PickFileAsync(savePicker);
			if (file == null)
			{
				return false;
			}

			var jsonContent = JsonSerializer.Serialize(entries, StopwatchJsonContext.Default.HistoryEntryModelArray);
			await FileIO.WriteTextAsync(file, jsonContent);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private FileSavePicker CreateSavePicker(string extension, string description)
	{
		var savePicker = new FileSavePicker();
		savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		savePicker.FileTypeChoices.Add(description, new List<string> { extension });
		return savePicker;
	}

	private async Task<StorageFile?> PickFileAsync(FileSavePicker savePicker)
	{
		// Get the current window handle from the injected service
		var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_windowShellProvider.Window);
		WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

		return await savePicker.PickSaveFileAsync();
	}

	private string FormatAsJson(StopwatchModel stopwatch)
	{
		var lapExports = new LapExportModel[stopwatch.Laps.Length];
		for (int i = 0; i < stopwatch.Laps.Length; i++)
		{
			var previousTotal = i > 0 ? stopwatch.Laps[i - 1].TotalTime : TimeSpan.Zero;
			var lapTime = stopwatch.Laps[i].TotalTime - previousTotal;
			lapExports[i] = new LapExportModel(lapTime, stopwatch.Laps[i].TotalTime, stopwatch.Laps[i].Note);
		}
		var exportData = new StopwatchExportModel(
			stopwatch.Name,
			stopwatch.InitialStartTime,
			stopwatch.LastStartTime,
			stopwatch.PausedElapsedTime,
			lapExports
		);
		return JsonSerializer.Serialize(exportData, StopwatchJsonContext.Default.StopwatchExportModel);
	}

	private string FormatAsJson(HistoryEntryModel entry)
	{
		var lapExports = new LapExportModel[entry.Laps.Length];
		for (int i = 0; i < entry.Laps.Length; i++)
		{
			var previousTotal = i > 0 ? entry.Laps[i - 1].TotalTime : TimeSpan.Zero;
			var lapTime = entry.Laps[i].TotalTime - previousTotal;
			lapExports[i] = new LapExportModel(lapTime, entry.Laps[i].TotalTime, entry.Laps[i].Note);
		}
		var exportData = new StopwatchExportModel(
			entry.Name,
			entry.InitialStartTime,
			null, // LastStartTime not applicable for history
			entry.PausedElapsedTime,
			lapExports
		);
		return JsonSerializer.Serialize(exportData, StopwatchJsonContext.Default.StopwatchExportModel);
	}

	private string FormatLapsAsCsv(LapModel[] laps)
	{
		var csv = new System.Text.StringBuilder();
		csv.AppendLine("LapTime,TotalTime,Note");

		for (var i = 0; i < laps.Length; i++)
		{
			var lap = laps[i];
			var previousLapTime = i > 0 ? laps[i - 1].TotalTime : TimeSpan.Zero;
			var lapTime = lap.TotalTime - previousLapTime;
			csv.AppendLine($"{lapTime},{lap.TotalTime},{lap.Note}");
		}

		return csv.ToString();
	}

	private string FormatAsXml(string name, DateTimeOffset? initialStartTime, DateTimeOffset? lastStartTime, TimeSpan pausedElapsedTime, LapModel[] laps)
	{
		var xml = new System.Text.StringBuilder();
		xml.AppendLine("<Stopwatch>");
		xml.AppendLine($"<Name>{name}</Name>");
		xml.AppendLine($"<InitialStartTime>{initialStartTime}</InitialStartTime>");
		xml.AppendLine($"<LastStartTime>{lastStartTime}</LastStartTime>");
		xml.AppendLine($"<PausedElapsedTime>{pausedElapsedTime}</PausedElapsedTime>");
		xml.AppendLine($"<Laps>");
		for (var i = 0; i < laps.Length; i++)
		{
			var lap = laps[i];
			var previousLapTime = i > 0 ? laps[i - 1].TotalTime : TimeSpan.Zero;
			xml.AppendLine($"<Lap>");
			xml.AppendLine($"<LapTime>{lap.TotalTime - previousLapTime}</LapTime>");
			xml.AppendLine($"<TotalTime>{lap.TotalTime}</TotalTime>");
			xml.AppendLine($"<Note>{lap.Note}</Note>");
			xml.AppendLine("</Lap>");
		}
		xml.AppendLine("</Laps>");
		xml.AppendLine("</Stopwatch>");

		return xml.ToString();
	}
}
