using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MaintenanceViewController : BaseViewController
{
	private const string timerName = "minutes-in-maintenance";

	private const int timerInterval = 600000;

	public static void ConfigureView(string errorCode)
	{
		BaseViewController.StopIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			MaintenanceViewModel maintenanceViewModel = GetViewModel();
			if (maintenanceViewModel == null)
			{
				maintenanceViewModel = new MaintenanceViewModel();
			}
			maintenanceViewModel.Title = Resources.maintenance_view_title;
			maintenanceViewModel.TextLine1 = Resources.maintenance_view_line1;
			maintenanceViewModel.TextLine2 = Resources.maintenance_view_line2;
			maintenanceViewModel.TextLine3 = Resources.maintenance_view_line3;
			maintenanceViewModel.Footnote = Resources.maintenance_view_footnote;
			Store store = null;
			try
			{
				store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("Unable to retrieve store from data service.", e);
			}
			string text = "";
			string text2 = "";
			if (store == null)
			{
				text = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "ID", "Not Configured");
				text2 = "Error";
			}
			else
			{
				text = store.StoreId.ToString();
				text2 = store.Zip.Substring(0, Math.Min(store.Zip.Length, 5));
			}
			maintenanceViewModel.StoreNumberText = string.Format(Resources.maintenance_view_kiosk_string, text);
			maintenanceViewModel.ZipCodeText = string.Format(Resources.maintenance_view_zip_string, text2);
			maintenanceViewModel.ErrorCodeText = string.Format(Resources.maintenance_view_error_string, errorCode);
			maintenanceViewModel.BackgroundImage = GetBackgroundImageFromErrorCode(errorCode);
			ServiceLocator.Instance.GetService<IMacroService>()["MaintenanceView:ErrorCode"] = errorCode;
			if (maintenanceViewModel.MaintenanceTimer == null)
			{
				maintenanceViewModel.MaintenanceTimer = CreateMaintenanceTimer(maintenanceViewModel);
			}
			if (!maintenanceViewModel.MaintenanceTimer.Enabled)
			{
				maintenanceViewModel.MaintenanceTimer.Start();
				ServiceLocator.Instance.GetService<IHardwareService>()?.NotifyMaintModeState(goingIntoMM: true);
			}
			viewUserControl.DataContext = maintenanceViewModel;
		}
	}

	private static BitmapImage GetBackgroundImageFromErrorCode(string errorCode)
	{
		switch (errorCode)
		{
		case "SOFT-FILE01":
		case "SOFT-FILE02":
		case "0032RSVB":
			return Redbox.Rental.UI.Views.Constants.MaintenanceViewBackgroundImages.GreenBackground;
		case "0016SYNC":
			return Redbox.Rental.UI.Views.Constants.MaintenanceViewBackgroundImages.OrangeBackground;
		default:
			return Redbox.Rental.UI.Views.Constants.MaintenanceViewBackgroundImages.RedBackground;
		}
	}

	public static void LeaveMaintenanceView()
	{
		ServiceLocator.Instance.GetService<ITimerService>().GetTimer("minutes-in-maintenance").Stop();
		OrchestrationService.SendHealthServicesMinutesInMaintenanceModeStatistics(0);
		ServiceLocator.Instance.GetService<IHardwareService>()?.NotifyMaintModeState(goingIntoMM: false);
		RefreshHardwareState();
	}

	private static MaintenanceViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MaintenanceViewModel;
		}
		return null;
	}

	private static ITimer CreateMaintenanceTimer(MaintenanceViewModel model)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		model.MinutesInMaintenance = 0;
		Random random = new Random();
		int value = 600000 + random.Next(100, 1000);
		return service.CreateTimer("minutes-in-maintenance", value, 600000, delegate
		{
			UpdateMaintenanceStatistics(model);
		});
	}

	private static void UpdateMaintenanceStatistics(MaintenanceViewModel model)
	{
		if (model != null)
		{
			model.MinutesInMaintenance += 10;
			OrchestrationService.SendHealthServicesMinutesInMaintenanceModeStatistics(model.MinutesInMaintenance);
		}
	}

	private static void RefreshHardwareState()
	{
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		if (service == null)
		{
			LogHelper.Instance.Log("Hardware State was not found.");
		}
		else if (service.IsInitialized)
		{
			service.Refresh();
		}
		else
		{
			service.Initialize();
		}
	}
}
