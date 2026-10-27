using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class VendViewFunctions
{
	[KernelFunction(Name = "kernel.vend_view_show_animation")]
	public static void ShowAnimation(string stringVendViewAnimationType)
	{
		VendViewController.ShowAnimation(stringVendViewAnimationType);
	}

	[KernelFunction(Name = "kernel.vend_view_get_animation")]
	public static string GetAnimation()
	{
		return VendViewController.GetAnimation();
	}
}
