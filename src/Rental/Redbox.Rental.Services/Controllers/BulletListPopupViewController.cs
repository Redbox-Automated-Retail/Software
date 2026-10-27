using System;
using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class BulletListPopupViewController : BaseViewController
{
	public static void ConfigureRedBoxAppInfoView(BulletListPopupParameters parameters)
	{
		BulletListPopupViewModel bulletListPopupViewModel = null;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			bulletListPopupViewModel = BaseViewController.GetViewModel<BulletListPopupViewModel>();
			if (bulletListPopupViewModel == null)
			{
				bulletListPopupViewModel = new BulletListPopupViewModel();
			}
			bulletListPopupViewModel.Title = Resources.app_info_title;
			bulletListPopupViewModel.BulletPoints = new ObservableCollection<string>
			{
				Resources.app_info_bullet_1,
				Resources.app_info_bullet_2,
				Resources.app_info_bullet_3,
				Resources.app_info_bullet_4,
				Resources.app_info_bullet_5
			};
			bulletListPopupViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BulletListPopupReserveOnlineGotit", parameters?.ViewAnalyticsName);
				OrchestrationService.ViewGoBackSafe("Bullet Point popup Back", popOnly: false);
			}, (Func<bool>)null);
			bulletListPopupViewModel.CancelButtonText = Resources.got_it_button;
			viewUserControl.DataContext = bulletListPopupViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	internal static void LeavePerksInfoView()
	{
		BaseViewController.StopIdleTimer();
	}

	internal static void LeaveRedboxAppInfoView()
	{
		BaseViewController.StopIdleTimer();
	}

	public static void ConfigurePerksInfoView(BulletListPopupParameters parameters)
	{
		BulletListPopupViewModel bulletListPopupViewModel = null;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			bulletListPopupViewModel = BaseViewController.GetViewModel<BulletListPopupViewModel>();
			if (bulletListPopupViewModel == null)
			{
				bulletListPopupViewModel = new BulletListPopupViewModel();
			}
			bulletListPopupViewModel.Title = Resources.perks_info_title;
			bulletListPopupViewModel.BulletPoints = new ObservableCollection<string>
			{
				Resources.perks_info_bullet_1,
				Resources.perks_info_bullet_2,
				Resources.perks_info_bullet_3,
				Resources.perks_info_bullet_4,
				Resources.perks_info_bullet_5
			};
			bulletListPopupViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("BulletListPopupRedboxPerksGotit", parameters?.ViewAnalyticsName);
				OrchestrationService.ViewGoBackSafe("Bullet Point popup Back", popOnly: false);
			}, (Func<bool>)null);
			bulletListPopupViewModel.CancelButtonText = Resources.got_it_button;
			viewUserControl.DataContext = bulletListPopupViewModel;
			BaseViewController.StartIdleTimer();
		}
	}
}
