using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MultiNightPricingPopupViewController : BaseViewController
{
	public static void ConfigureMultiNightPricingPopupView(MultiNightPricingPopupInfo multiNightPricingPopupInfo)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		IAnalyticsService analyticsService = ServiceLocator.Instance.GetService<IAnalyticsService>();
		analyticsService?.AddData(ProductData.ToProductData(multiNightPricingPopupInfo.TitleProduct));
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = new MultiNightPricingPopupViewModel();
			multiNightPricingPopupViewModel.TitleProduct = multiNightPricingPopupInfo.TitleProduct;
			bool showBuyButton = ShowBuyButton(multiNightPricingPopupInfo.ShownFromView, multiNightPricingPopupInfo.TitleProduct);
			multiNightPricingPopupViewModel.HeaderText = multiNightPricingPopupInfo.TitleProduct.ProductName;
			multiNightPricingPopupViewModel.DescriptionText = Resources.multi_night_pricing_popup_description_text;
			IList<IPricingRecord> list = (multiNightPricingPopupViewModel.PricingRecords = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(multiNightPricingPopupInfo.TitleProduct.ProductId));
			bool hasOneNightPrice = list != null && list.Count > 0;
			bool hasMultiNightPrice = list != null && list.Count > 1;
			multiNightPricingPopupViewModel.OneNightButtonVisibility = ((!hasOneNightPrice) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.OneNightButtonText1 = (hasOneNightPrice ? string.Format(Resources.multi_night_pricing_popup_button_text, list[0].InitialDays ?? 1, string.Empty) : string.Empty);
			multiNightPricingPopupViewModel.OneNightButtonText2 = (hasOneNightPrice ? $"{list[0].InitialNight:C}" : string.Empty);
			MultiNightPopupData multiNightPopupData = new MultiNightPopupData
			{
				ProductId = multiNightPricingPopupInfo.TitleProduct.ProductId
			};
			if (hasOneNightPrice)
			{
				multiNightPopupData.InitialNightPrice = list[0].InitialNight;
			}
			multiNightPricingPopupViewModel.MultiNightButtonVisibility = ((!hasMultiNightPrice) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.MultiNightButtonText1 = (hasMultiNightPrice ? string.Format(Resources.multi_night_pricing_popup_button_text, list[1].InitialDays ?? 1, "S") : string.Empty);
			multiNightPricingPopupViewModel.MultiNightButtonText2 = (hasMultiNightPrice ? $"{list[1].InitialNight:C}" : string.Empty);
			if (hasMultiNightPrice)
			{
				multiNightPopupData.HasMultiNightPrice = true;
				multiNightPopupData.MultiNightPrice = list[1].InitialNight;
				multiNightPopupData.MultipNightInitialDays = list[1].InitialDays;
			}
			multiNightPricingPopupViewModel.BuyButtonVisibility = ((!showBuyButton) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.BuyButtonText1 = (showBuyButton ? Resources.multi_night_pricing_popup_buy_button_text : string.Empty);
			multiNightPricingPopupViewModel.BuyButtonText2 = (showBuyButton ? $"${multiNightPricingPopupInfo.TitleProduct.PurchasePrice:F2}" : string.Empty);
			if (showBuyButton)
			{
				multiNightPopupData.ShowBuyButton = true;
				multiNightPopupData.PurchasePrice = multiNightPricingPopupInfo.TitleProduct.PurchasePrice;
			}
			analyticsService?.AddData(multiNightPopupData);
			multiNightPricingPopupViewModel.CancelButtonText = Resources.multi_night_pricing_popup_cancel_button_text.ToUpper();
			multiNightPricingPopupViewModel.DisclaimerText = ((!hasMultiNightPrice & showBuyButton) ? GetDisclaimerText(list[0], showBuyButton, hasMultiNightPrice) : (hasMultiNightPrice ? GetDisclaimerText(list[1], showBuyButton, hasMultiNightPrice) : string.Empty));
			multiNightPricingPopupViewModel.OnOneNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnOneNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnOneNightButtonClicked));
			multiNightPricingPopupViewModel.OnMultiNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnMultiNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnMultiNightButtonClicked));
			multiNightPricingPopupViewModel.OnBuyButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnBuyButtonClicked, new Action(MultiNightPricingPopupViewModel_OnBuyButtonClicked));
			multiNightPricingPopupViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnCancelButtonClicked, new Action(MultiNightPricingPopupViewModel_OnCancelButtonClicked));
			ConfigureTTS(multiNightPricingPopupViewModel);
			viewUserControl.DataContext = multiNightPricingPopupViewModel;
		}
	}

	public static void LeaveMultiNightPricingPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static MultiNightPricingPopupViewModel GetMultiNightPricingPopupViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MultiNightPricingPopupViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(MultiNightPricingPopupViewModel multiNightPricingPopupViewModel)
	{
		ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (textToSpeechService != null && textToSpeechService.TTSEnabled)
		{
			multiNightPricingPopupViewModel.OnGetSpeechControl = () => MultiNightPricingPopupViewTextToSpeech.GetSpeechControl(GetMultiNightPricingPopupViewModel);
		}
	}

	private static void CreateAndStartTimer()
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("medium", 15000);
		(timerService?.CreateTimer("MultiNightPricingPopupViewTimer", timeOut, timeOut, delegate
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
			}
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		(timerService?.GetTimer("MultiNightPricingPopupViewTimer"))?.Stop();
		timerService?.RemoveTimer("MultiNightPricingPopupViewTimer");
	}

	private static bool ShowBuyButton(string shownFromView, ITitleProduct titleProduct)
	{
		switch (shownFromView)
		{
		case "start_view":
		case "browse_view":
		case "product_detail_view":
		case "purchase_popup_view":
			if (titleProduct.IsSellableAndInStock)
			{
				if (!titleProduct.IsMovie)
				{
					return titleProduct.IsGame;
				}
				return true;
			}
			break;
		}
		return false;
	}

	private static string GetDisclaimerText(IPricingRecord pricingRecord, bool showBuyButton, bool hasMultiNight)
	{
		if (showBuyButton & hasMultiNight)
		{
			return Resources.multi_night_pricing_popup_disclaimer_b_text;
		}
		if (showBuyButton && !hasMultiNight)
		{
			return string.Format(Resources.multi_night_pricing_popup_disclaimer_c_text, pricingRecord.ExtraNight);
		}
		return string.Format(Resources.multi_night_pricing_popup_disclaimer_text, pricingRecord.ExtraNight);
	}

	private static void MultiNightPricingPopupViewModel_OnOneNightButtonClicked()
	{
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 0)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("MultiNightPopupOneNightRental")?.AddData(RentalPricingData.ToPricingData(multiNightPricingPopupViewModel.TitleProduct.ProductId, multiNightPricingPopupViewModel.PricingRecords[0]));
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController oneNightRental", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[0]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnMultiNightButtonClicked()
	{
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 1)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("MultiNightPopupMultiNightRental")?.AddData(RentalPricingData.ToPricingData(multiNightPricingPopupViewModel.TitleProduct.ProductId, multiNightPricingPopupViewModel.PricingRecords[1]));
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController multiNightRental", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[1]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnBuyButtonClicked()
	{
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("MultiNightPopupPurchase")?.AddData(PurchasePricingData.ToPricingData(multiNightPricingPopupViewModel.TitleProduct.ProductId, multiNightPricingPopupViewModel.PricingRecords[0]));
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController buy", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: true);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnCancelButtonClicked()
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
		}
		BaseViewController.AnalyticsService?.AddButtonPressEvent("MultiNightPopupCancel");
		OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController cancel", popOnly: false);
	}
}
