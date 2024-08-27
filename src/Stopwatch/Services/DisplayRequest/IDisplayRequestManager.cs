namespace Stopwatch.Services;

public interface IDisplayRequestManager
{
	IDisposable RequestActive();

	void Clear();
}
