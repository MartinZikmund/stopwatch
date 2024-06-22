using Stopwatch.Model;

namespace Stopwatch.Services.Data;

public class DataFileLayout
{
	public List<StopwatchModel> Stopwatches { get; set; } = new();
}
