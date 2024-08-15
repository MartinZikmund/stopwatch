using Stopwatch.Models;

namespace Stopwatch.Services.Data;

public class DataFileLayout
{
	public List<StopwatchModel> Stopwatches { get; set; } = new();
}
