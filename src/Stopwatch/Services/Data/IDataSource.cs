using Stopwatch.Model;

namespace Stopwatch.Services.Data;

public interface IDataSource
{
	StopwatchModel[] GetAll();

	StopwatchModel Get(int id);

	void Add(StopwatchModel stopwatch);

	void Update(StopwatchModel stopwatch);
}
