namespace Stopwatch.Services.Export;

using Stopwatch.Models;

/// <summary>
/// Service for exporting stopwatch and history data to various formats.
/// </summary>
public interface IExportService
{
	/// <summary>
	/// Exports an active stopwatch to JSON format.
	/// </summary>
	Task<bool> ExportToJsonAsync(StopwatchModel stopwatch, string suggestedFileName);

	/// <summary>
	/// Exports an active stopwatch's laps to CSV format.
	/// </summary>
	Task<bool> ExportToCsvAsync(StopwatchModel stopwatch, string suggestedFileName);

	/// <summary>
	/// Exports an active stopwatch to XML format.
	/// </summary>
	Task<bool> ExportToXmlAsync(StopwatchModel stopwatch, string suggestedFileName);

	/// <summary>
	/// Exports an active stopwatch's laps to Excel format.
	/// </summary>
	Task<bool> ExportToExcelAsync(StopwatchModel stopwatch, string suggestedFileName);

	/// <summary>
	/// Exports a history entry to JSON format.
	/// </summary>
	Task<bool> ExportToJsonAsync(HistoryEntryModel entry, string suggestedFileName);

	/// <summary>
	/// Exports a history entry's laps to CSV format.
	/// </summary>
	Task<bool> ExportToCsvAsync(HistoryEntryModel entry, string suggestedFileName);

	/// <summary>
	/// Exports a history entry to XML format.
	/// </summary>
	Task<bool> ExportToXmlAsync(HistoryEntryModel entry, string suggestedFileName);

	/// <summary>
	/// Exports a history entry's laps to Excel format.
	/// </summary>
	Task<bool> ExportToExcelAsync(HistoryEntryModel entry, string suggestedFileName);

	/// <summary>
	/// Exports all history entries to a single JSON file.
	/// </summary>
	Task<bool> ExportAllHistoryToJsonAsync(HistoryEntryModel[] entries);
}
