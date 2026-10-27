using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class EmailOptInViewController : BaseViewController
{
	private static object lockObject = new object();

	public static void ConfigureView(Action continueAction)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			EmailOptInViewModel emailOptInViewModel = new EmailOptInViewModel();
			ConfigureTextAndVisibility(emailOptInViewModel);
			ConfigureCommands(emailOptInViewModel, continueAction);
			viewUserControl.DataContext = emailOptInViewModel;
			ConfigureTimeoutTimer(emailOptInViewModel, continueAction);
			if (OrchestrationService.InvokeIsVendJobCompletedCallback())
			{
				OrchestrationService.ViewGoBackSafe("email_optin_view vend job already completed", popOnly: true);
			}
		}
	}

	private static void ConfigureTimeoutTimer(EmailOptInViewModel emailOptInViewModel, Action continueAction)
	{
		int timeout = OrchestrationService.TTSGetTimeout("short", 10000);
		IIdleTimer idleTimer = OrchestrationService.CreateIdleTimer("optin_view_timer");
		idleTimer.Initialize(timeout, delegate
		{
			LockHelper.TryLockObject(lockObject, delegate
			{
				if (!emailOptInViewModel.HasActionExecuted)
				{
					emailOptInViewModel.HasActionExecuted = true;
					LogHelper.Instance.Log("EmailOptInView timed out.");
					OrchestrationService.ViewGoBackSafe("email_optin_view timeout", popOnly: false);
					continueAction?.Invoke();
				}
			}, "EmailOptInView time out action");
		});
		idleTimer.Start();
	}

	private static void ConfigureCommands(EmailOptInViewModel emailOptInViewModel, Action continueAction)
	{
		emailOptInViewModel.OnNoThanksButtonClicked += delegate
		{
			LockHelper.TryLockObject(lockObject, delegate
			{
				if (!emailOptInViewModel.HasActionExecuted)
				{
					emailOptInViewModel.HasActionExecuted = true;
					BaseViewController.AnalyticsService?.AddButtonPressEvent("EmailOptInRejectOptIn");
					OrchestrationService.ViewGoBackSafe("email_optin_view reject button hit", popOnly: false);
					continueAction?.Invoke();
				}
			}, "EmailOptInView reject opt in action");
		};
		emailOptInViewModel.OnYesButtonClicked += delegate
		{
			LockHelper.TryLockObject(lockObject, delegate
			{
				if (!emailOptInViewModel.HasActionExecuted)
				{
					emailOptInViewModel.HasActionExecuted = true;
					BaseViewController.AnalyticsService?.AddButtonPressEvent("EmailOptInAcceptOptIn");
					SendKioskOptinForEmailCampaign();
					OrchestrationService.ViewGoBackSafe("email_optin_view accept button hit", popOnly: false);
					continueAction?.Invoke();
				}
			}, "EmailOptInView accept opt in action");
		};
	}

	public static void SendKioskOptinForEmailCampaign()
	{
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
		IOptInItem optInItem = service.NewOptInItem();
		optInItem.Item_Type = KioskOptIntItemType.Non_Item_Specific;
		optInItem.ItemId = 0L;
		optInItem.OptInMethodData = Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email;
		optInItem.OptInMethodType = OptInMethodType.EmailAddress;
		service.KioskOptIn(optInItems: new List<IOptInItem> { optInItem }, kioskId: service2.KioskId, callback: null);
	}

	private static void ConfigureTextAndVisibility(EmailOptInViewModel emailOptInViewModel)
	{
		BaseViewController.SetCurrentUICulture();
		emailOptInViewModel.YesButtonText = Resources.yes_button;
		emailOptInViewModel.NoThanksButtonText = Resources.no_thanks_button;
		emailOptInViewModel.MessageText = Resources.email_optin_offer_message;
	}

	public static void LeaveView()
	{
		OrchestrationService.StopIdleTimer("optin_view_timer");
		OrchestrationService.RemoveIdleTimer("optin_view_timer");
	}
}
