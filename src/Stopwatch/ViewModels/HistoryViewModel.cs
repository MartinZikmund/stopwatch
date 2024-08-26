using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch.Services.Navigation;

namespace Stopwatch.ViewModels;

internal class HistoryViewModel : PageViewModel
{
	public HistoryViewModel(INavigationService navigationService) : base(navigationService)
	{
	}


}
