using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PromoConfirmationViewController : BaseViewController
{
	private const string timerName = "PromoConfirmationViewTimer";

	public static void ConfigurePromoConfirmationView(PromoConfirmationInfo promoInfo)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			PromoConfirmationViewModel promoConfirmationViewModel = GetViewModel();
			if (promoConfirmationViewModel == null)
			{
				promoConfirmationViewModel = new PromoConfirmationViewModel();
			}
			promoConfirmationViewModel.MaskedPromoCode = promoInfo.promoCodeModel.MaskedPromoCode;
			promoConfirmationViewModel.PromoCode = promoInfo.promoCodeModel.PromoCode;
			promoConfirmationViewModel.DescriptionText = promoInfo.promoCodeModel.PromoCodeDescription;
			promoConfirmationViewModel.ApplyButtonText = Resources.confirm_promo_apply_button_text;
			promoConfirmationViewModel.CancelButtonText = Resources.cancel_button;
			promoConfirmationViewModel.LegalText = string.Format(Resources.confirm_promo_legal_text, promoInfo.promoCodeModel.ExpirationDate.ToString("MM/dd/yyyy"));
			promoConfirmationViewModel.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("PromoConfirmationCancel");
				OrchestrationService.ViewGoBackSafe("PromoConfirmCancel", popOnly: false);
				promoInfo.CancelAction?.Invoke();
			}, (Func<bool>)null);
			promoConfirmationViewModel.ApplyCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(string p)
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("PromoConfirmationApplyPromo");
				OrchestrationService.ViewGoBackSafe("PromoConfirmApply", popOnly: false);
				promoInfo.ApplyPromoCodeAction?.Invoke(p);
			}, null);
			viewUserControl.DataContext = promoConfirmationViewModel;
			ConfigureTTS(promoConfirmationViewModel);
		}
	}

	internal static void LeavePromoConfirmationView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void ConfigureTTS(PromoConfirmationViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => PromoConfirmationViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static PromoConfirmationViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as PromoConfirmationViewModel;
		}
		return null;
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("long", 40000);
		(service?.CreateTimer("PromoConfirmationViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("PromoConfirmationViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("PromoConfirmationViewTimer"))?.Stop();
		service?.RemoveTimer("PromoConfirmationViewTimer");
	}
}
