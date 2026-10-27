using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusLeadGenerationOfferViewController : BaseViewController
{
	private static string _emailAddress;

	private static OptInConfirmationStatus _optInConfirmationStatus;

	private static Action _continueAction;

	public static void ConfigureRedboxPlusLeadGenerationOfferView(RedboxPlusLeadGenerationOfferViewParameters redboxPlusLeadGenerationOfferViewParameters)
	{
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			RedboxPlusLeadGenerationOfferViewModel redboxPlusLeadGenerationOfferViewModel = new RedboxPlusLeadGenerationOfferViewModel();
			PopulateBoxArt(redboxPlusLeadGenerationOfferViewModel);
			_emailAddress = redboxPlusLeadGenerationOfferViewParameters?.EmailAddress;
			_optInConfirmationStatus = redboxPlusLeadGenerationOfferViewParameters?.OptInConfirmationStatus ?? OptInConfirmationStatus.DoNotShow;
			redboxPlusLeadGenerationOfferViewModel.EmailText = ((!string.IsNullOrEmpty(_emailAddress)) ? string.Format(Resources.redbox_plus_lead_generation_has_email_text, _emailAddress) : Resources.redbox_plus_lead_generation_no_email_text);
			redboxPlusLeadGenerationOfferViewModel.EmailMessageTextVisibility = ((_optInConfirmationStatus == OptInConfirmationStatus.SubscribedConfirmed) ? Visibility.Collapsed : Visibility.Visible);
			redboxPlusLeadGenerationOfferViewModel.NoButtonText = ((_optInConfirmationStatus == OptInConfirmationStatus.SubscribedConfirmed) ? Resources.not_now_button.ToUpper() : Resources.no_thanks_button.ToUpper());
			redboxPlusLeadGenerationOfferViewModel.QRCodeStackPanelVisibility = ((!string.IsNullOrEmpty(_emailAddress)) ? Visibility.Collapsed : Visibility.Visible);
			_continueAction = redboxPlusLeadGenerationOfferViewParameters?.ContinueAction;
			redboxPlusLeadGenerationOfferViewModel.OnNoButtonClicked = (Action)Delegate.Combine(redboxPlusLeadGenerationOfferViewModel.OnNoButtonClicked, new Action(RedboxPlusLeadGenerationOfferViewModel_OnNoButtonClicked));
			redboxPlusLeadGenerationOfferViewModel.OnEmailButtonClicked = (Action)Delegate.Combine(redboxPlusLeadGenerationOfferViewModel.OnEmailButtonClicked, new Action(RedboxPlusLeadGenerationOfferViewModel_OnEmailButtonClicked));
			redboxPlusLeadGenerationOfferViewModel.OnQRCodeButtonClicked = (Action)Delegate.Combine(redboxPlusLeadGenerationOfferViewModel.OnQRCodeButtonClicked, new Action(RedboxPlusLeadGenerationOfferViewModel_OnQRCodeButtonClicked));
			viewUserControl.DataContext = redboxPlusLeadGenerationOfferViewModel;
		}
	}

	public static void LeaveRedboxPlusLeadGenerationOfferView()
	{
		StopAndRemoveTimer();
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = 40000;
		(service?.CreateTimer("RedboxPlusLeadGenerationOfferViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			_continueAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("RedboxPlusLeadGenerationOfferViewTimer"))?.Stop();
		service?.RemoveTimer("RedboxPlusLeadGenerationOfferViewTimer");
	}

	private static void PopulateBoxArt(RedboxPlusLeadGenerationOfferViewModel redboxPlusLeadGenerationOfferViewModel)
	{
		List<IKioskProduct> products = null;
		ServiceLocator.Instance.GetService<IDataService>()?.GetProducts(BrowseViewFilter.RedboxPlus, TitleFamily.Movies, TitleType.All, BrowseSort.SortDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products == null)
		{
			return;
		}
		int num = 1;
		foreach (IKioskProduct item in products.Take(9))
		{
			redboxPlusLeadGenerationOfferViewModel?.GetType()?.GetProperty($"BoxArt{num++}")?.SetValue(redboxPlusLeadGenerationOfferViewModel, OrchestrationService.GetProductBitmapImage(item));
		}
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
				optInItem.TemplateName = "RedboxPlusLeadGenerationOfferOptInTemplate";
				optInItem.CampaignName = "RedboxPlusLeadGenerationOfferOptInCampaign";
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

	private static void RedboxPlusLeadGenerationOfferViewModel_OnNoButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusLeadGenerationOfferNoButton");
		_continueAction?.Invoke();
	}

	private static void RedboxPlusLeadGenerationOfferViewModel_OnEmailButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusLeadGenerationOfferEmailButton");
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

	private static void RedboxPlusLeadGenerationOfferViewModel_OnQRCodeButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusLeadGenerationOfferQRCodeButton");
		BitmapImage qrCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.redbox_plus_lead_generation_qr_code_image_url, returnDefaultOnError: true);
		Action action = delegate
		{
			OrchestrationService.ViewGoBackSafe("QR Code DONE Button", popOnly: false);
		};
		MessagePopupViewController.ShowQRCodeDialog(qrCodeImage, Resources.redbox_plus_lead_generation_qr_code_display_url, action, action);
	}
}
