using System.Reflection;
using MZikmund.Toolkit.WinUI.Infrastructure;
using MZikmund.Toolkit.WinUI.Services;
using Stopwatch.Dialogs;
using Stopwatch.Services.Dialogs;
using Stopwatch.Services.Localization;

namespace Stopwatch.Services;

public class DialogService : IDialogService
{
	private readonly Dictionary<string, Type> _dialogs = new();
	private readonly IDialogCoordinator _dialogCoordinator;
	private readonly IXamlRootProvider _xamlRootProvider;

	public DialogService(IDialogCoordinator dialogCoordinator, IXamlRootProvider xamlRootProvider)
	{
		_dialogCoordinator = dialogCoordinator ?? throw new ArgumentNullException(nameof(dialogCoordinator));
		_xamlRootProvider = xamlRootProvider ?? throw new ArgumentNullException(nameof(xamlRootProvider));
	}

	public async Task<ContentDialogResult> ShowAsync(object viewModel)
	{
		var viewModelType = viewModel.GetType();
		if (!viewModelType.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("ViewModel name must end with 'ViewModel' by convention.");
		}

		var viewModelName = viewModelType.Name;
		var dialogTypeName = viewModelName.Substring(0, viewModelName.Length - "ViewModel".Length);
		if (!_dialogs.TryGetValue(dialogTypeName, out var dialogType))
		{
			throw new InvalidOperationException($"Dialog for {viewModelType} not found");
		}
		var dialog = (ContentDialog?)Activator.CreateInstance(dialogType);
		if (dialog is null)
		{
			throw new InvalidOperationException($"Instance of {dialogType} could not be created");
		}
		dialog.DataContext = viewModel;
		dialog.XamlRoot = _xamlRootProvider.XamlRoot;
		return await _dialogCoordinator.ShowAsync(dialog);
	}

	public void RegisterDialogsFromAssembly(Assembly sourceAssembly)
	{
		_dialogs.Add(typeof(ColorPickerDialog).Name, typeof(ColorPickerDialog));
		_dialogs.Add(typeof(ProOnlyFeatureDialog).Name, typeof(ProOnlyFeatureDialog));
	}

	public async Task<ContentDialogResult> ShowAsync(string title, string content)
	{
		var dialog = new ContentDialog()
		{
			Title = title,
			Content = content,
			PrimaryButtonText = Localizer.Instance.GetString("Ok"),
			XamlRoot = _xamlRootProvider.XamlRoot
		};

		return await _dialogCoordinator.ShowAsync(dialog);
	}

	public async Task<ContentDialogResult> ShowAsync(ContentDialog contentDialog)
	{
		if (contentDialog.XamlRoot is null)
		{
			contentDialog.XamlRoot = _xamlRootProvider.XamlRoot;
		}
		return await _dialogCoordinator.ShowAsync(contentDialog);
	}
}
