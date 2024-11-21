#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MZikmund.Toolkit.WinUI.Services;

namespace Stopwatch.Services.Store;
internal class FakeStoreService : IStoreService
{
	private readonly IPreferences _preferences;
	private readonly IDialogService _dialogService;
	private string? _price;

	public FakeStoreService(IPreferences preferences, IDialogService dialogService)
	{
		_preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
		_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
	}

	public async Task<string?> GetPriceAsync()
	{
		if (_price is null)
		{
			await Task.Delay(1000);
			_price = "0.99 $";
		}
		return _price;
	}

	public async Task<bool> HasProAsync()
	{
		await Task.Delay(100);
		if (_preferences.TryGet<bool>("FakeStoreService_HasPro", out var hasPro))
		{
			return hasPro;
		}

		return false;
	}

	public async Task<bool> TryPurchaseProAsync()
	{
		var dialog = new ContentDialog
		{
			Title = "Purchase Pro",
			Content = "Do you want to purchase Pro?",
			PrimaryButtonText = "Yes",
			CloseButtonText = "No"
		};
		var result = await _dialogService.ShowAsync(dialog);
		if (result == ContentDialogResult.Primary)
		{
			_preferences.Set("FakeStoreService_HasPro", true);
			return true;
		}
		return false;
	}
}
#endif
