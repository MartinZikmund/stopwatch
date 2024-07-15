namespace Stopwatch.Core.Services;

public interface IImagePickerService
{
	Task<Uri?> PickAsync();
}
