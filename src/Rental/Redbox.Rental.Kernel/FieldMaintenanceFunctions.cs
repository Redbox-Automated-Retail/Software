using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Planogram;

namespace Redbox.Rental.Kernel;

public static class FieldMaintenanceFunctions
{
	[KernelFunction(Name = "kernel.fieldmaintenancegetcurrentsafetytip")]
	internal static string GetCurrentSafetyTip()
	{
		return ServiceLocator.Instance.GetService<IFieldMaintenanceService>()?.GetCurrentSafetyTip();
	}

	[KernelFunction(Name = "kernel.planogramsexist")]
	internal static bool PlanogramsExist()
	{
		List<Planogram> list = ServiceLocator.Instance.GetService<IKioskClientService>()?.Planogram?.GetActivePlanograms();
		if (list != null)
		{
			return list.Count > 0;
		}
		return false;
	}
}
