using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class AvodLeadGenerationOfferViewController : BaseViewController
{
	private static string _emailAddress;

	private static OptInConfirmationStatus _optInConfirmationStatus;

	private static Action _continueAction;

	public static void ConfigureAvodLeadGenerationOfferView(AvodLeadGenerationOfferViewParameters avodLeadGenerationOfferViewParameters)
	{
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			AvodLeadGenerationOfferViewModel avodLeadGenerationOfferViewModel = new AvodLeadGenerationOfferViewModel();
			avodLeadGenerationOfferViewModel.GridBackgroundImageSource = (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableAvodLeadGenerationOfferWithBoxArt ? "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/avod-lead-generation-offer-background-with-box-art.png" : "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/avod-lead-generation-offer-background-without-box-art.png");
			_emailAddress = avodLeadGenerationOfferViewParameters?.EmailAddress;
			_optInConfirmationStatus = avodLeadGenerationOfferViewParameters?.OptInConfirmationStatus ?? OptInConfirmationStatus.DoNotShow;
			avodLeadGenerationOfferViewModel.EmailText = ((!string.IsNullOrEmpty(_emailAddress)) ? string.Format(Resources.avod_lead_generation_has_email_text, _emailAddress) : Resources.avod_lead_generation_no_email_text);
			avodLeadGenerationOfferViewModel.EmailMessageTextVisibility = ((_optInConfirmationStatus == OptInConfirmationStatus.SubscribedConfirmed) ? Visibility.Collapsed : Visibility.Visible);
			avodLeadGenerationOfferViewModel.NoButtonText = ((_optInConfirmationStatus == OptInConfirmationStatus.SubscribedConfirmed) ? Resources.not_now_button.ToUpper() : Resources.no_thanks_button.ToUpper());
			avodLeadGenerationOfferViewModel.QRCodeStackPanelVisibility = ((!string.IsNullOrEmpty(_emailAddress)) ? Visibility.Collapsed : Visibility.Visible);
			_continueAction = avodLeadGenerationOfferViewParameters?.ContinueAction;
			avodLeadGenerationOfferViewModel.OnNoButtonClicked = (Action)Delegate.Combine(avodLeadGenerationOfferViewModel.OnNoButtonClicked, new Action(AvodLeadGenerationOfferViewModel_OnNoButtonClicked));
			avodLeadGenerationOfferViewModel.OnEmailButtonClicked = (Action)Delegate.Combine(avodLeadGenerationOfferViewModel.OnEmailButtonClicked, new Action(AvodLeadGenerationOfferViewModel_OnEmailButtonClicked));
			avodLeadGenerationOfferViewModel.OnQRCodeButtonClicked = (Action)Delegate.Combine(avodLeadGenerationOfferViewModel.OnQRCodeButtonClicked, new Action(AvodLeadGenerationOfferViewModel_OnQRCodeButtonClicked));
			viewUserControl.DataContext = avodLeadGenerationOfferViewModel;
		}
	}

	public static void LeaveAvodLeadGenerationOfferView()
	{
		StopAndRemoveTimer();
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = 40000;
		(service?.CreateTimer("AvodLeadGenerationOfferViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			_continueAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("AvodLeadGenerationOfferViewTimer"))?.Stop();
		service?.RemoveTimer("AvodLeadGenerationOfferViewTimer");
	}

	private static void ShowPleaseWaitViewAndSendEmailConfirm()
	{
		if (!string.IsNullOrEmpty(_emailAddress))
		{
			IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service != null && service2 != null)
			{
				OrchestrationService.ViewShow("please_wait_view");
				SendEmailConfirmCallback completeCallback = delegate(IEmailConfirmResult emailConfirmResult)
				{
					ProcessEmailConfirmResult(emailConfirmResult);
				};
				service.SendEmailConfirm(service2.KioskId, _emailAddress, null, completeCallback);
			}
		}
		else
		{
			_continueAction?.Invoke();
		}
	}

	private static void ProcessEmailConfirmResult(IEmailConfirmResult result)
	{
		if (result != null)
		{
			_optInConfirmationStatus = (OptInConfirmationStatus)result.ConfirmationStatus;
			SendKioskOptInsAndShowLeadGenerationOfferConfirmationView();
		}
		else
		{
			_continueAction?.Invoke();
		}
	}

	private static void SendKioskOptInsAndShowLeadGenerationOfferConfirmationView()
	{
		if (!string.IsNullOrEmpty(_emailAddress))
		{
			SignupFunctions.SendKioskOptInForMarketingCampaignMessages(KioskOptInItemId.Yes, (ConfirmationStatus)_optInConfirmationStatus, _emailAddress);
			IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service != null && service2 != null)
			{
				IOptInItem optInItem = service.NewOptInItem();
				optInItem.Item_Type = KioskOptIntItemType.Non_Item_Specific;
				optInItem.ItemId = 0L;
				optInItem.OptInMethodData = _emailAddress;
				optInItem.OptInMethodType = OptInMethodType.EmailAddress;
				optInItem.TemplateName = "AvodLeadGenerationOfferOptInTemplate";
				optInItem.CampaignName = "AvodLeadGenerationOfferOptInCampaign";
				List<IOptInItem> optInItems = new List<IOptInItem> { optInItem };
				service.KioskOptIn(service2.KioskId, optInItems, null);
			}
			OrchestrationService.ViewShow("lead_generation_offer_confirmation_view", new LeadGenerationOfferConfirmationViewParameters
			{
				ContinueAction = _continueAction
			});
		}
		else
		{
			_continueAction?.Invoke();
		}
	}

	private static void AvodLeadGenerationOfferViewModel_OnNoButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("AvodLeadGenerationOfferNoButton");
		_continueAction?.Invoke();
	}

	private static void AvodLeadGenerationOfferViewModel_OnEmailButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("AvodLeadGenerationOfferEmailButton");
		if (!string.IsNullOrEmpty(_emailAddress))
		{
			SendKioskOptInsAndShowLeadGenerationOfferConfirmationView();
			return;
		}
		KeyboardInfo parameter = new KeyboardInfo
		{
			KeyboardMode = KeyboardMode.LEAD_GENERATION_SIGN_UP,
			MaxLength = 64,
			KeyboardText = null,
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ErrorMessage = string.Empty,
			KeepKeyboardTextOnInitialErrorMessage = true,
			CancelAction = delegate
			{
				_continueAction?.Invoke();
			},
			ContinueAction = delegate(string emailAddress)
			{
				_emailAddress = emailAddress;
				ShowPleaseWaitViewAndSendEmailConfirm();
			}
		};
		OrchestrationService.ViewShow("keyboard_lead_generation_signup_view", parameter);
	}

	private static void AvodLeadGenerationOfferViewModel_OnQRCodeButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("AvodLeadGenerationOfferQRCodeButton");
		BitmapImage qrCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.avod_lead_generation_qr_code_image_url, returnDefaultOnError: true);
		Action action = delegate
		{
			OrchestrationService.ViewGoBackSafe("QR Code DONE Button", popOnly: false);
		};
		MessagePopupViewController.ShowQRCodeDialog(qrCodeImage, Resources.avod_lead_generation_qr_code_display_url, action, action);
	}
}
