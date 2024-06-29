using Uno.Disposables;
using Windows.System.Display;

namespace Stopwatch.Services;

internal class DisplayRequestManager : IDisplayRequestManager
{
	private int _activeRequestCount = 0;
	private int _generation = 0;

	private readonly DisplayRequest _displayRequest = new();

	public IDisposable RequestActive()
	{
		var currentGeneration = _generation;
		_displayRequest.RequestActive();
		return Disposable.Create(() =>
		{
			// Ensure we did not Clear in-between.
			if (_generation == currentGeneration)
			{
				_activeRequestCount--;
				_displayRequest.RequestRelease();
			}
		});
	}

	public void Clear()
	{
		while (_activeRequestCount > 0)
		{
			_displayRequest.RequestRelease();
			_activeRequestCount--;
		}
		_generation++;
	}
}
