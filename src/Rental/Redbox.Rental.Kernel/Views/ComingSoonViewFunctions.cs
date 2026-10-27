using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class ComingSoonViewFunctions
{
	[KernelFunction(Name = "kernel.has_coming_soon_titles")]
	internal static bool HasComingSoonTitles()
	{
		return ComingSoonViewController.HasComingSoonTitles();
	}
}
