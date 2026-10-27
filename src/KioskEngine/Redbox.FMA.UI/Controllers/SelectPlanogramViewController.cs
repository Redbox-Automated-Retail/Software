using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.FMA.Models;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Planogram;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.Services;
using Redbox.Rental.UI;

namespace Redbox.FMA.Controllers;

public class SelectPlanogramViewController : BaseViewController
{
	public static void ConfigureView()
	{
		IKioskClientServicePlanogram kioskClientServicePlanogram = ServiceLocator.Instance.GetService<IKioskClientService>()?.Planogram;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		SelectPlanogramViewModel selectPlanogramViewModel = GetViewModel();
		if (selectPlanogramViewModel == null)
		{
			selectPlanogramViewModel = new SelectPlanogramViewModel
			{
				CloseCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					CloseButtonClickedAction();
				}, (Func<bool>)null),
				SelectCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(Planogram p)
				{
					SelectPlanogram(p);
				}),
				ForceSyncCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					ForceSyncPlanograms();
				}, (Func<bool>)null)
			};
			List<Planogram> list = kioskClientServicePlanogram?.GetActivePlanograms();
			if (list != null)
			{
				selectPlanogramViewModel.Planograms = list.OrderBy((Planogram p) => p.Name);
			}
		}
		viewUserControl.DataContext = selectPlanogramViewModel;
	}

	private static void SelectPlanogram(Planogram p)
	{
		ServiceLocator.Instance.GetService<IKioskClientService>()?.Planogram?.SaveSelectedPlanogram(p);
		OrchestrationService.ViewGoBackSafe("select_planogram_view", popOnly: false);
	}

	private static void CloseButtonClickedAction()
	{
		OrchestrationService.ViewGoBackSafe("select_planogram_view", popOnly: false);
	}

	private static SelectPlanogramViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as SelectPlanogramViewModel;
		}
		return null;
	}

	private static void ForceSyncPlanograms()
	{
		PopupMessageViewParameters popupMessageViewParameters = new PopupMessageViewParameters
		{
			ContinueAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("sync message", popOnly: false);
			}
		};
		if (ServiceLocator.Instance.GetService<IKioskClientService>()?.Planogram?.SyncPlanograms(forceRefresh: true) ?? false)
		{
			popupMessageViewParameters.MessageText = "Planogram file download in progress. Please allow a few minutes for the download to complete.";
		}
		else
		{
			popupMessageViewParameters.MessageText = "Unable to sync planograms with server. Please ensure the kiosk is on line.";
		}
		OrchestrationService.ViewShow("fma_popup_message_view", popupMessageViewParameters);
	}
}
