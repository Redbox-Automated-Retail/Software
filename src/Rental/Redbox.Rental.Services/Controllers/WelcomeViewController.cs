using System.Windows;
using System.Windows.Input;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class WelcomeViewController
{
	public static void ConfigureView()
	{
		FrameworkElement userControl = GetUserControl();
		if (userControl != null)
		{
			userControl.DataContext = GetModel();
		}
		CommandManager.InvalidateRequerySuggested();
	}

	private static FrameworkElement GetUserControl()
	{
		return ActorFunctions.GetActor("TitleDetailUserControl")?.WPFFrameworkElement;
	}

	private static object GetModel()
	{
		WelcomeViewModel welcomeViewModel = new WelcomeViewModel();
		welcomeViewModel.NavigateHelp += OnNavigateHelp;
		welcomeViewModel.NavigateStart += OnNavigateStart;
		return welcomeViewModel;
	}

	private static void OnNavigateHelp()
	{
		OrchestrationService.ViewShow("help_view");
	}

	private static void OnNavigateStart(bool isSpanish, bool isAda)
	{
		if (isSpanish)
		{
			OrchestrationService.SetCulture(OrchestrationService.GetOtherCulture());
		}
		if (isAda)
		{
			ApplicationState.Instance.IsADAAccessible = true;
		}
		OrchestrationService.ViewShow("start_view");
	}
}
