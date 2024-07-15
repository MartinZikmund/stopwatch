using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Stopwatch.Services.Navigation;

namespace Stopwatch.Core.Services;

public class ImagePickerService : IImagePickerService
{
	private const int BufferSize = 1024;
	private const string BackgroundsFolderName = "UserBackgrounds";
	private readonly IWindowShellProvider _windowShellProvider;

	public ImagePickerService(IWindowShellProvider windowShellProvider)
	{
		_windowShellProvider = windowShellProvider;
	}

	public async Task<Uri?> PickAsync()
	{
		//pick image
		var filePicker = new FileOpenPicker();
		filePicker.FileTypeFilter.Add(".jpg");
		filePicker.FileTypeFilter.Add(".jpeg");
		filePicker.FileTypeFilter.Add(".png");
		filePicker.ViewMode = PickerViewMode.Thumbnail;
		filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

		var handle = WinRT.Interop.WindowNative.GetWindowHandle(_windowShellProvider.Window);
		WinRT.Interop.InitializeWithWindow.Initialize(filePicker, handle);

		StorageFile file = await filePicker.PickSingleFileAsync();
		if (file == null)
		{
			return null;
		}
		else
		{
			Guid backgroundId = Guid.NewGuid();
			var targetFileName = backgroundId + file.FileType;
			var userBackgroundsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(BackgroundsFolderName, CreationCollisionOption.OpenIfExists);
			var imageFile = await userBackgroundsFolder.CreateFileAsync(backgroundId + file.FileType);
			await file.CopyAsync(userBackgroundsFolder, targetFileName, NameCollisionOption.ReplaceExisting);

			return new Uri("ms-appdata:///local/" + BackgroundsFolderName + "/" + targetFileName, UriKind.Absolute);
		}
	}
}
