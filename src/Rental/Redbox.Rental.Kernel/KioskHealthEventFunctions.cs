using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Kernel;

public static class KioskHealthEventFunctions
{
	[KernelFunction(Name = "kernel.notify_arcus_reset")]
	internal static void NotifyArcusReset(string resetType)
	{
		ServiceLocator.Instance.GetService<IArcusHealth>().EventOccurred(resetType);
	}

	[KernelFunction(Name = "kernel.is_arcus_reset_enabled")]
	internal static bool IsArcusResetEnabled()
	{
		return Configuration.Instance.Current.KioskHealth.ArcusActive;
	}
}
