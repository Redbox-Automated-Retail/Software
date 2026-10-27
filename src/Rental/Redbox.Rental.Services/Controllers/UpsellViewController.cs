using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class UpsellViewController : BaseLoyaltyController
{
	private class Constants
	{
		public static string UpsellIdleTimerName = "upsell_view_idle_timer";

		public static int UpsellIdleTimerDuration = 15000;
	}

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	public static void ConfigureUpsellView(TitleType upsellFromTitleType)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			UpsellViewModel upsellViewModel = new UpsellViewModel
			{
				UpsellFromTitleType = upsellFromTitleType
			};
			ConfigureTextAndVisibility(upsellViewModel);
			ConfigureCommands(upsellViewModel);
			ConfigureUpsellProductDisplay(upsellViewModel);
			ConfigureTTS(upsellViewModel);
			viewUserControl.DataContext = upsellViewModel;
			CommandManager.InvalidateRequerySuggested();
			ConfigureUpsellViewIdleTimer(upsellViewModel);
		}
	}

	public static void CleanupUpsellView()
	{
		OrchestrationService.RemoveIdleTimer(Constants.UpsellIdleTimerName);
	}

	private static void ConfigureUpsellViewIdleTimer(UpsellViewModel upsellViewModel)
	{
		IIdleTimer obj = IdleTimerService?.CreateIdleTimer(Constants.UpsellIdleTimerName);
		obj?.Initialize(Constants.UpsellIdleTimerDuration, delegate
		{
			OrchestrationService.StopIdleTimer(Constants.UpsellIdleTimerName);
			LogHelper.Instance.Log("upsell view: idle timer expired");
			UpsellViewIdleTimerExpired(upsellViewModel);
		});
		obj?.Start();
	}

	private static void ConfigureTTS(UpsellViewModel upsellViewModel)
	{
	}

	private static void ConfigureUpsellProductDisplay(UpsellViewModel upsellViewModel)
	{
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.UpsellInfo.AddMissingUpsellItems(upsellViewModel.UpsellFromTitleType);
		List<IKioskProduct> list = new List<IKioskProduct>();
		List<DisplayUpsellProductModel> list2 = new List<DisplayUpsellProductModel>();
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
		List<IUpsellItem> list3 = new List<IUpsellItem>();
		list3.AddRange(BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.Items.Where((IUpsellItem x) => x.OfferResponse == UpsellItemOfferResponse.none));
		foreach (IUpsellItem eachUpsellItem in list3)
		{
			if (eachUpsellItem.OfferResponse != UpsellItemOfferResponse.none)
			{
				continue;
			}
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == eachUpsellItem.OriginalProductId);
			if (rentalShoppingCartTitleItem == null)
			{
				continue;
			}
			IKioskProduct kioskProduct = null;
			service2.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
			if (kioskProduct != null)
			{
				DisplayUpsellProductModel displayUpsellProductModel = new DisplayUpsellProductModel
				{
					Name = kioskProduct.ProductName,
					Data = eachUpsellItem,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
				};
				displayUpsellProductModel.ConfigureForUpsell();
				IKioskProduct kioskProduct2 = null;
				service2.GetProduct(eachUpsellItem.UpsellProductId, excludeTitleRollup: true, out kioskProduct2);
				if (kioskProduct2 != null)
				{
					list.Add(kioskProduct2);
				}
				ITitleProduct titleProduct = kioskProduct2 as ITitleProduct;
				IList<IPricingRecord> list4 = service?.GetProductPricingRecords(titleProduct?.ProductId ?? 0);
				IPricingRecord pricingRecord = (((list4?.Count ?? 0) > 0) ? list4[0] : null);
				displayUpsellProductModel.PriceText = ((pricingRecord != null) ? $"{pricingRecord.InitialNight:c2}" : null);
				BannerType bannerType = BannerType.None;
				bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
				bool hideBanner = false;
				BrowseFunctions.ConfigureBanner(displayUpsellProductModel, titleProduct, hideBanner, bannerType, BaseViewController.GetViewUserControl());
				displayUpsellProductModel.CheckboxBorderVisibility = ((list3.Count == 1) ? Visibility.Collapsed : Visibility.Visible);
				displayUpsellProductModel.CheckboxVisibility = ((list3.Count != 1) ? Visibility.Collapsed : Visibility.Visible);
				list2.Add(displayUpsellProductModel);
			}
		}
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown = list;
		}
		upsellViewModel.DisplayProductModels = list2;
	}

	private static void ConfigureCommands(UpsellViewModel upsellViewModel)
	{
		upsellViewModel.OnBackButtonClicked = delegate
		{
			ProcessOnBackButtonClicked(upsellViewModel);
		};
		upsellViewModel.OnNoThanksButtonClicked = delegate
		{
			ProcessOnNoThanksButtonClicked(upsellViewModel);
		};
		upsellViewModel.OnUpgradeButtonClicked = delegate
		{
			ProcessOnUpgradeButtonClicked(upsellViewModel);
		};
		upsellViewModel.OnCheckBoxClick = delegate(DisplayUpsellProductModel displayUpsellProductModel)
		{
			displayUpsellProductModel.CheckboxVisibility = ((displayUpsellProductModel.CheckboxVisibility != Visibility.Collapsed) ? Visibility.Collapsed : Visibility.Visible);
			BaseViewController.ProcessWPFHit();
		};
		upsellViewModel.OnCanExecuteUpgradeButton = () => upsellViewModel.DisplayProductModels.Any((DisplayUpsellProductModel x) => x.CheckboxVisibility == Visibility.Visible);
	}

	private static void ProcessOnUpgradeButtonClicked(UpsellViewModel upsellViewModel)
	{
		string eventSubType = ((upsellViewModel.UpsellFromTitleType == TitleType.Bluray) ? "4k_upsell_upgrade_button" : "blu_ray_upsell_upgrade_button");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("UpsellUpgrade", eventSubType);
		if (upsellViewModel?.DisplayProductModels != null)
		{
			foreach (DisplayUpsellProductModel displayProductModel in upsellViewModel.DisplayProductModels)
			{
				if (displayProductModel.Data is IUpsellItem { OfferResponse: UpsellItemOfferResponse.none } upsellItem)
				{
					if (displayProductModel.CheckboxVisibility == Visibility.Visible)
					{
						BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.SetUpsellOffer(upsellItem.OriginalProductId, UpsellItemOfferResponse.accepted);
					}
					else
					{
						BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.SetUpsellOffer(upsellItem.OriginalProductId, UpsellItemOfferResponse.rejected);
					}
				}
			}
		}
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.RejectUndecidedUpsellItems();
		OrchestrationService.ViewGoBackSafe("upsell_view upgrade button click", popOnly: false);
		ShoppingCartViewFunctions.ViewShoppingCart();
	}

	private static void ProcessOnBackButtonClicked(UpsellViewModel upsellViewModel)
	{
		string eventSubType = ((upsellViewModel.UpsellFromTitleType == TitleType.Bluray) ? "4k_upsell_cancel_button" : "blu_ray_upsell_back_button");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("UpsellBack", eventSubType);
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.RemoveUndecidedUpsellItems();
		OrchestrationService.ViewGoBackSafe("upsell_view", popOnly: false);
	}

	private static void ProcessOnNoThanksButtonClicked(UpsellViewModel upsellViewModel)
	{
		string eventSubType = ((upsellViewModel.UpsellFromTitleType == TitleType.Bluray) ? "4K_upsell_no_thanks_button" : "blu_ray_upsell_no_thanks_button");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("UpsellNoThanks", eventSubType);
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.RejectUndecidedUpsellItems();
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.UpsellInfo.SetDoNotShowAgain(upsellViewModel.UpsellFromTitleType, value: true);
		OrchestrationService.ViewGoBackSafe("upsell_view no thanks button click", popOnly: false);
		ShoppingCartViewFunctions.ViewShoppingCart();
	}

	private static void UpsellViewIdleTimerExpired(UpsellViewModel upsellViewModel)
	{
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.UpsellInfo.RejectUndecidedUpsellItems();
		BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.UpsellInfo.SetDoNotShowAgain(upsellViewModel.UpsellFromTitleType, value: true);
		OrchestrationService.ViewGoBackSafe("upsell_view timer expired", popOnly: false);
		ShoppingCartViewFunctions.ViewShoppingCart();
	}

	private static void ConfigureTextAndVisibility(UpsellViewModel upsellViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		string name = $"upsell_from_{upsellViewModel.UpsellFromTitleType}_title";
		upsellViewModel.TitleText = Resources.ResourceManager.GetString(name);
		string name2 = $"upsell_from_{upsellViewModel.UpsellFromTitleType}_message";
		upsellViewModel.MessageText = Resources.ResourceManager.GetString(name2);
		string name3 = $"upsell_from_{upsellViewModel.UpsellFromTitleType}_message2";
		upsellViewModel.Message2Text = Resources.ResourceManager.GetString(name3);
		upsellViewModel.BackButtonText = Resources.back_button;
		upsellViewModel.NoThanksButtonText = Resources.no_thanks_button;
		upsellViewModel.UpgradeButtonText = Resources.upgrade;
	}
}
