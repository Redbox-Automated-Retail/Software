using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.HelpView;

public class HelpViewFunctions
{
	[KernelFunction(Name = "kernel.show_full_help_view")]
	internal static void ShowFullHelpView(bool allowFMA, bool restartTimer)
	{
		HelpViewController.ShowFullHelpView(allowFMA, restartTimer);
	}
}
