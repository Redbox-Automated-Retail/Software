using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public static class VsmFunctions
{
	[KernelFunction(Name = "kernel.is_vsm_ad_available")]
	internal static bool IsVsmAdAvailable()
	{
		return VsmViewController.IsVsmAdAvailable();
	}
}
