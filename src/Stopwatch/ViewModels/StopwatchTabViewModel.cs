using Stopwatch.Models;

namespace Stopwatch.ViewModels;

public class StopwatchTabViewModel
{
	private readonly StopwatchModel _model;

	public StopwatchTabViewModel(StopwatchModel model)
	{
		_model = model;
	}

	public string Name => _model.Name;

	public bool IsRunning => _model.LastStartTime is not null;
}
