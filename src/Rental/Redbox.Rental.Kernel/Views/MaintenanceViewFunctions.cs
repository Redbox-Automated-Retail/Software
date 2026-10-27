using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.Rental.Kernel.Views;

public static class MaintenanceViewFunctions
{
	[KernelFunction(Name = "kernel.add_maintenance_mode_reason")]
	public static void AddMaintenanceModeReason(string source, string errorCode)
	{
		Enum.TryParse<MaintenanceModeSource>(source, out var result);
		if (result == MaintenanceModeSource.None)
		{
			LogHelper.Instance.Log("Error in MaintenanceViewFunctions.AddMaintenanceModeReason.  Invalid source: " + source);
		}
		ServiceLocator.Instance.GetService<IApplicationState>()?.AddMaintenanceModeReason(result, errorCode);
	}

	[KernelFunction(Name = "kernel.is_in_maintenance_mode")]
	public static bool IsInMaintenanceMode()
	{
		return ServiceLocator.Instance.GetService<IApplicationState>()?.IsInMaintenanceMode ?? false;
	}

	[KernelFunction(Name = "kernel.clear_hal_maintenance_mode")]
	public static void ClearHalMaintenanceMode()
	{
		ClearMaintenanceMode(MaintenanceModeSource.HAL);
	}

	[KernelFunction(Name = "kernel.clear_inventory_sync_maintenance_mode")]
	public static void ClearInventorySyncMaintenanceMode()
	{
		ClearMaintenanceMode(MaintenanceModeSource.InventorySync);
	}

	[KernelFunction(Name = "kernel.update_minutes_in_maintenance_mode")]
	public static void UpdateMinutesInMaintenanceMode(string value)
	{
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if (service != null && service.IsInMaintenanceMode && service.HasMinutesInMaintenanceModeSource)
		{
			ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueStatistic("MinutesInMaintenance", value);
		}
	}

	private static void ClearMaintenanceMode(MaintenanceModeSource source)
	{
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if (service != null && service.IsInMaintenanceMode)
		{
			service.RemoveMaintenanceModeReason(source);
		}
	}
}
