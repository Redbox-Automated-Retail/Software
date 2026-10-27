using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PurchaseFormatSelectionPopupViewController : BaseViewController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static void ConfigureRentalFormatSelectionPopupView(RentalFormatSelectionViewParameter parameter)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		FormatSelectionPopupViewModel formatSelectionPopupViewModel = GetViewModel();
		if (formatSelectionPopupViewModel == null)
		{
			formatSelectionPopupViewModel = new FormatSelectionPopupViewModel();
		}
		formatSelectionPopupViewModel.CancelButtonText = Resources.cancel_button.ToUpper();
		formatSelectionPopupViewModel.OnCancelButtonClicked = delegate
		{
			ProcessCancelButtonClick(parameter?.CancelAction);
		};
		if (parameter == null || parameter.RollupProduct == null)
		{
			return;
		}
		TitleRollupProduct rollupProduct = parameter.RollupProduct;
		bool isPurchase = parameter.IsPurchase;
		bool flag = parameter.RedboxPlusBenefitId == 2 && ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.HasAnyTitleRollupTitle(parameter.RollupProduct) == true;
		formatSelectionPopupViewModel.HeaderText = rollupProduct.LongName;
		if (isPurchase)
		{
			formatSelectionPopupViewModel.DescriptionText = Resources.purchase_format_dialog_description;
		}
		else if (flag)
		{
			formatSelectionPopupViewModel.DescriptionText = string.Format(Resources.redbox_plus_already_in_cart_message, parameter?.RollupProduct?.ProductName);
		}
		else
		{
			formatSelectionPopupViewModel.DescriptionText = Resources.rental_format_dialog_description;
		}
		if (parameter.DiscountedPriceCalc != null)
		{
			FormatSelectionPopupViewModel formatSelectionPopupViewModel2 = formatSelectionPopupViewModel;
			formatSelectionPopupViewModel2.DescriptionText = formatSelectionPopupViewModel2.DescriptionText + " " + Resources.purchase_format_dialog_discount_message;
		}
		formatSelectionPopupViewModel.DigitalCodeInfoText = Resources.title_rollup_format_digital_code_info;
		formatSelectionPopupViewModel.OnFormatButtonClicked = delegate(TitleType type)
		{
			ProcessButtonClick(rollupProduct, type, type.GetDisplayName(), isPurchase, parameter.ShowMnp, parameter.RedboxPlusBenefitId);
		};
		formatSelectionPopupViewModel.OnPurchaseButtonClicked = delegate
		{
			ProcessPurchaseButtonClick(rollupProduct);
		};
		formatSelectionPopupViewModel.OnDigitalCodeInfoButtonClicked = delegate
		{
			ProcessDigitalCodeInfoClicked(rollupProduct);
		};
		foreach (ITitleProduct item in rollupProduct.KioskProducts.OrderBy((ITitleProduct p) => (int)((p.TitleType != TitleType.DigitalCode) ? p.TitleType : ((TitleType)999))))
		{
			AddFormatButtonData(formatSelectionPopupViewModel, item, service, isPurchase, parameter.DiscountedPriceCalc, parameter.RedboxPlusBenefitId, viewUserControl);
		}
		if (parameter.ShowPurchaseButton && rollupProduct.IsSellableAndInStock && rollupProduct.PurchasePrice > 0m)
		{
			formatSelectionPopupViewModel.PurchaseButtonText = Resources.format_selection_buy_button_text;
			formatSelectionPopupViewModel.PurchaseButtonLine2Text = GetPurchaseButtonPriceText(rollupProduct);
			formatSelectionPopupViewModel.PurchaseButtonVisibility = Visibility.Visible;
		}
		else
		{
			formatSelectionPopupViewModel.PurchaseButtonVisibility = Visibility.Collapsed;
		}
		if (formatSelectionPopupViewModel.ButtonData.Count + ((formatSelectionPopupViewModel.PurchaseButtonVisibility == Visibility.Visible) ? 1 : 0) == 3)
		{
			formatSelectionPopupViewModel.DialogWidth = 900;
			formatSelectionPopupViewModel.PurchaseButtonMargin = new Thickness(12.0, 24.0, 12.0, 0.0);
		}
		else
		{
			formatSelectionPopupViewModel.DialogWidth = 750;
			formatSelectionPopupViewModel.PurchaseButtonMargin = new Thickness(286.0, -56.0, 12.0, 0.0);
		}
		ConfigureTTS(formatSelectionPopupViewModel);
		viewUserControl.DataContext = formatSelectionPopupViewModel;
	}

	private static void AddFormatButtonData(FormatSelectionPopupViewModel viewModel, ITitleProduct kioskProduct, IPricingService pricingService, bool isPurchase, Func<decimal, decimal> discountedPriceCalc, int? redboxPlusBenefitId, FrameworkElement popupViewUserControl)
	{
		bool showOutOfStock = !(Redbox.Rental.Services.Configuration.Configuration.Instance.OutOfStockTitleDetailsABTestVersion == "B") || kioskProduct.TitleFamily != TitleFamily.Movies || isPurchase;
		bool showPricing = redboxPlusBenefitId != 2;
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(kioskProduct, isPurchase, new TitleRollupOverlayButtonTextValues(), showOutOfStock, discountedPriceCalc, showPricing, redboxPlusBenefitId);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			if (kioskProduct.TitleType == TitleType.DigitalCode && rollupFormatButtonProperties.IsEnabled)
			{
				rollupFormatButtonProperties.ButtonText += "*";
			}
			Style style = WPFFunctions.GetStyle(popupViewUserControl, rollupFormatButtonProperties.StyleName);
			viewModel.ButtonData.Add(new FormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, style, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled));
		}
	}

	private static FormatSelectionPopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as FormatSelectionPopupViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(FormatSelectionPopupViewModel formatSelectionPopupViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			formatSelectionPopupViewModel.OnGetSpeechControl = () => PurchaseFormatSelectionPopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static bool IsTitleTypeAvailable(TitleType titleType)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleType, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products != null)
		{
			return products.Count > 0;
		}
		return false;
	}

	private static void ProcessButtonClick(TitleRollupProduct rollupProduct, TitleType type, string analysisText, bool isPurchase, bool showMnp, int? redboxPlusBenefitId)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupAnalysisText", analysisText);
		OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController " + analysisText, popOnly: false);
		if (rollupProduct == null)
		{
			return;
		}
		ITitleProduct titleProduct = rollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == type);
		if (titleProduct == null)
		{
			return;
		}
		titleProduct.TivoQueryId = titleProduct.TivoQueryId ?? rollupProduct.TivoQueryId;
		if (titleProduct.InStock | isPurchase)
		{
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if (showMnp && (titleProduct.HasMultiNightPrice || (titleProduct.IsSellableAndInStock && (titleProduct.IsMovie || titleProduct.IsGame))) && !isPurchase && !flag)
			{
				BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct, "purchase_popup_view");
				return;
			}
			BrowseFunctions.SetRedboxPlusFreeOneNightRentalOrAddProductToCart(new AddProductToCartParameters
			{
				TitleProduct = titleProduct,
				IsPurchase = isPurchase,
				PricingRecord = null,
				ItemSourceType = ItemSourceType.Browse,
				RedboxPlusBenefitId = redboxPlusBenefitId
			});
		}
		else
		{
			ShowOutOfStockOptionsDialog(rollupProduct, type, showMnp, redboxPlusBenefitId);
		}
	}

	private static string GetPurchaseButtonPriceText(TitleRollupProduct rollupProduct)
	{
		List<ITitleProduct> list = rollupProduct.KioskProducts.Where((ITitleProduct tp) => tp.IsSellableAndInStock && tp.PurchasePrice > 0m).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		decimal price = list.Min((ITitleProduct tp) => tp.PurchasePrice);
		bool flag = list.Count == 1 || list.All((ITitleProduct tp) => tp.PurchasePrice == price);
		return string.Format("${0:F2}{1}", price, flag ? "" : "+");
	}

	private static void ProcessPurchaseButtonClick(TitleRollupProduct rollupProduct)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupPurchase");
		OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController Purchase button", popOnly: false);
		BrowseFunctions.ShowPurchasePopupView(rollupProduct, isPurchase: true);
	}

	private static void ShowOutOfStockOptionsDialog(ITitleProduct kioskProduct, TitleType titleType, bool showMnp, int? redboxPlusBenefitId)
	{
		bool flag = kioskProduct is TitleRollupProduct;
		ITitleProduct selectedProduct = (flag ? ((TitleRollupProduct)kioskProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == titleType) : kioskProduct);
		OptionSelectionDialogInfo optionSelectionDialogInfo = new OptionSelectionDialogInfo
		{
			ViewAnalyticsName = "message_popup_view",
			ViewAnalyticsContext = "OutOfStockDialog",
			HeaderText = kioskProduct.ProductName,
			CancelButtonText = Resources.cancel_button
		};
		optionSelectionDialogInfo.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupOutOfStockCancel");
			ServiceLocator.Instance.GetService<IViewService>()?.GoBack("OutOfStockDialogCancel", popOnly: false);
		}, (Func<bool>)null);
		optionSelectionDialogInfo.DescriptionText = Resources.out_of_stock_movie_message;
		ObservableCollection<ButtonData> observableCollection = new ObservableCollection<ButtonData>();
		if (flag && ((ITitleRollupProduct)kioskProduct).KioskProducts.Any((ITitleProduct p) => p.InStock))
		{
			optionSelectionDialogInfo.DescriptionText = string.Format(Resources.out_of_stock_format_message, titleType.GetDisplayDescription());
			foreach (ITitleProduct inStockFormat in ((ITitleRollupProduct)kioskProduct).KioskProducts.Where((ITitleProduct p) => p.InStock && p.TitleType != TitleType.DigitalCode))
			{
				IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(inStockFormat.ProductId);
				new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues();
				string displayDescription = inStockFormat.TitleType.GetDisplayDescription();
				ButtonData item = new ButtonData((pricingRecord != null) ? (displayDescription + " $" + pricingRecord.InitialNight.ToString("0.00")) : displayDescription, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupOutOfStockAddFormat");
					ProcessButtonClick((TitleRollupProduct)kioskProduct, inStockFormat.TitleType, inStockFormat.TitleType.GetDisplayName(), isPurchase: false, showMnp, redboxPlusBenefitId);
				}, (Func<bool>)null));
				observableCollection.Add(item);
			}
		}
		observableCollection.Add(new ButtonData(Resources.related_movies, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupOutOfStockRelatedMovies");
			BrowseFunctions.ShowRelatedProductsDialog(selectedProduct);
		}, (Func<bool>)null)));
		optionSelectionDialogInfo.ButtonData = observableCollection;
		OrchestrationService.ViewShow("option_selection_dialog_view", optionSelectionDialogInfo);
	}

	private static void ShowReserveOnlineMessage()
	{
		MessagePopupViewController.ShowReserveOnlineRentDialog(delegate
		{
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogTimeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogBack", popOnly: false);
			BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupReserveOnlineRentBack");
		}, delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupReserveOnlineRentGotit");
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
		});
	}

	private static void ProcessCancelButtonClick(Action cancelAction)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
		}
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupCancel");
		OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController cancel", popOnly: false);
		cancelAction?.Invoke();
	}

	private static void ProcessDigitalCodeInfoClicked(TitleRollupProduct titleRollupProduct)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupDigitalCodeInformation");
		if (titleRollupProduct != null)
		{
			ITitleProduct product = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode);
			OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController Digital code Info button", popOnly: false);
			MessagePopupViewController.ShowMessagePopupViewDigitalCode($"{product.PurchasePrice:N2}", delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupCancelWhatDigitalCode");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info back button", popOnly: false);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("PurchaseFormatSelectionPopupBuyWhatDigitalCode");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info buy button", popOnly: false);
				BrowseFunctions.AddProductToCart(product, isPurchase: true);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info time out", popOnly: false);
			});
		}
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("medium", 15000);
		(service?.CreateTimer("RentalFormatSelectionPopupViewTimer", timeOut, timeOut, delegate
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
			}
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("RentalFormatSelectionViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("RentalFormatSelectionPopupViewTimer"))?.Stop();
		service?.RemoveTimer("RentalFormatSelectionPopupViewTimer");
	}

	public static void LeaveRentalFormatSelectionPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}
}
