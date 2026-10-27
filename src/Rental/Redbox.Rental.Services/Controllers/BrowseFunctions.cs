using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class BrowseFunctions
{
	public class Constants
	{
		public class IdleTimers
		{
			public const string IdleTimerName = "idle_timer";

			public const string KeyboardIdleTimerName = "keyboard_idle_timer";

			public const string ShoppingCartUpdateADAIdleTimerName = "shopping_cart_update_ada_idle_timer";

			public const string OptInViewTimer = "optin_view_timer";

			public const string ReservationNotFoundTimer = "reservation_not_found_timer";
		}

		public static string ComingSoon = "ComingSoon";
	}

	private static bool _purchaseWarningShown;

	private static bool _adaPurchaseWarningShown;

	private static bool _digitalCodeWarningShown;

	private static bool _subPlatformWarningShown;

	private static bool _multiDiscWarningShown;

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	public static bool IsPurchase => (Redbox.Rental.Services.Session.Session.Instance?.BrowseState.BrowseMode ?? BrowseViewFilter.All) == BrowseViewFilter.ForSale;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static bool IsRedboxPlusBrowseMode
	{
		get
		{
			IBrowseState browseState = BrowseState;
			if (browseState == null)
			{
				return false;
			}
			return browseState.BrowseMode == BrowseViewFilter.RedboxPlus;
		}
	}

	public static bool IsShoppingCartFull => RentalShoppingCartService?.CurrentRentalShoppingCart?.IsFull == true;

	public static bool CanAddProductToCart
	{
		get
		{
			if (!IsRedboxPlusBrowseMode)
			{
				return !IsShoppingCartFull;
			}
			return RedboxPlusFunctions.CanAddRedboxPlusFreeOneNightRental;
		}
	}

	public static void SetRedboxPlusFreeOneNightRentalOrAddProductToCart(AddProductToCartParameters addProductToCartParameters)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null || addProductToCartParameters?.TitleProduct == null)
		{
			return;
		}
		if (addProductToCartParameters.RedboxPlusBenefitId == 2 && rentalShoppingCart != null && rentalShoppingCart.HasTitleItem(addProductToCartParameters.TitleProduct.ProductId))
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart?.GetTitleItemsByCriteria(new RentalShoppingCartFilterCriteria
			{
				IncludeProducts = new List<long> { addProductToCartParameters.TitleProduct.ProductId },
				IncludeFlashDeals = true,
				IncludeLoyaltyRedeemedItems = true,
				IncludeMultiNightPricedItems = true,
				IncludeDiscountRestrictionItems = true
			})?.FirstOrDefault();
			if (rentalShoppingCartTitleItem != null)
			{
				RedboxPlusFunctions.ValidateAndSetFreeOneNightRental(rentalShoppingCartTitleItem, showConfirmationMessage: false, "browse_view");
			}
		}
		else
		{
			AddProductToCart(addProductToCartParameters);
		}
	}

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase)
	{
		AddProductToCart(new AddProductToCartParameters
		{
			TitleProduct = titleProduct,
			IsPurchase = isPurchase,
			PricingRecord = null,
			ItemSourceType = ItemSourceType.Browse,
			RedboxPlusBenefitId = null
		});
	}

	public static void AddProductToCart(ITitleProduct titleProduct, bool isPurchase, IPricingRecord pricingRecord)
	{
		AddProductToCart(new AddProductToCartParameters
		{
			TitleProduct = titleProduct,
			IsPurchase = isPurchase,
			PricingRecord = pricingRecord,
			ItemSourceType = ItemSourceType.Browse,
			RedboxPlusBenefitId = null
		});
	}

	public static void AddProductToCart(AddProductToCartParameters addProductToCartParameters)
	{
		if (addProductToCartParameters?.TitleProduct != null)
		{
			if (addProductToCartParameters.IsPurchase)
			{
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsBuy")?.AddData(ProductData.ToProductData(addProductToCartParameters.TitleProduct));
			}
			else
			{
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsShoppingCartAddItem")?.AddData(ProductData.ToProductData(addProductToCartParameters.TitleProduct));
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView = service?.Peek();
			_purchaseWarningShown = false;
			_adaPurchaseWarningShown = false;
			_digitalCodeWarningShown = false;
			_subPlatformWarningShown = false;
			_multiDiscWarningShown = false;
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = true;
			}
			AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
		}
	}

	public static void ShowCartFullMessage()
	{
		Action continueAction = delegate
		{
			BrowseState.ShoppingCartFullMessagesShown = true;
			BrowseState.IsShoppingCartFullMessagesOpen = false;
			ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("MessagePopupViewController.ShowMessageCartFull", popOnly: false);
		};
		MessagePopupViewController.ShowMessageCartFull(delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			BrowseState.IsShoppingCartFullMessagesOpen = true;
			service?.GoBackSafe("MessagePopupViewController.ShowMessageCartFull", popOnly: false);
			BrowseState.IsShoppingCartFullMessagesOpen = false;
			service.Push("idle_timeout_message_view");
			service.Show();
		}, continueAction);
	}

	public static void ShowBrowseViewRedboxPlus(string contextMessage)
	{
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.RedboxPlus;
		if (ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrameByName("browse_view", out var _) != null)
		{
			OrchestrationService.UnwindToSafe("browse_view", discard: true, contextMessage);
		}
		else
		{
			OrchestrationService.ViewShow("browse_view");
		}
	}

	public static void ConfigureProductFlag(DisplayProductModel displayProductModel, ProductFlags productFlags, bool hideFlag, FrameworkElement frameworkElementToGetStyleFrom, ITitleProduct titleProduct, DisplayProductModelConstants displayProductModelConstants)
	{
		ProductFlag productFlag = ProductFlagCollection.Instance.FirstOrDefault((ProductFlag x) => x.Flag == productFlags);
		if (displayProductModel == null)
		{
			return;
		}
		displayProductModel.FlagVisibility = ((hideFlag || productFlag == null || productFlag.Flag == ProductFlags.None) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.FlagStyle = WPFFunctions.GetStyle(frameworkElementToGetStyleFrom, productFlag?.FlagStyleName);
		if (displayProductModelConstants?.OutOfStockTitleDetailsABTestVersion == "B" && productFlag != null && productFlag.Flag == ProductFlags.OutOfStock && titleProduct.TitleFamily == TitleFamily.Movies)
		{
			displayProductModel.FlagVisibility = Visibility.Collapsed;
			displayProductModel.FlagStyle = null;
		}
		if (productFlag != null && productFlag.Flag == ProductFlags.FlashDeal)
		{
			IPricingRecord pricingRecord = null;
			decimal num = ((!(titleProduct is TitleRollupProduct titleRollupProduct)) ? titleProduct?.RentalPriceRecords?.FirstOrDefault() : (titleRollupProduct.KioskProducts?.Where((ITitleProduct kp) => kp.HasDeal).FirstOrDefault())?.RentalPriceRecords?.FirstOrDefault())?.InitialNight ?? 0m;
			string flagText = ((num < 1m) ? string.Format(Resources.flag_flash_deal_price_cents, num * 100m) : string.Format(Resources.flag_flash_deal_price_dollar, num));
			displayProductModel.FlagText = flagText;
		}
		else
		{
			displayProductModel.FlagText = ((!string.IsNullOrEmpty(productFlag?.TextKeyName)) ? Resources.ResourceManager.GetString(productFlag?.TextKeyName) : null);
		}
		if (displayProductModel.FlagVisibility == Visibility.Visible)
		{
			displayProductModel.FlagMargin = new Thickness(0.0, 31.0, 0.0, 0.0);
		}
	}

	public static ProductFlags GetProductFlag(ITitleProduct titleProduct)
	{
		ProductFlags result = ProductFlags.None;
		if (titleProduct != null)
		{
			ServiceLocator.Instance.GetService<IConfiguration>();
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.EnableSonySweepstakes && titleProduct.Studio.ToLower() == "sony")
			{
				result = ProductFlags.SonySweepstakes;
			}
			else if (titleProduct.HasDeal && BrowseState.BrowseMode != BrowseViewFilter.ForSale && BrowseState.BrowseSelectedSort != BrowseSelectedSort.DigitalCode)
			{
				result = ProductFlags.FlashDeal;
			}
			else if (titleProduct.Genres.Contains(1002L))
			{
				result = ProductFlags.AwardWinner;
			}
			else if (titleProduct.Genres.Contains(1104L))
			{
				result = ProductFlags.AwardNominee;
			}
			else
			{
				double totalDays = (DateTime.Now - titleProduct.MerchandiseDate).TotalDays;
				bool flag = ServiceLocator.Instance.GetService<IDataService>()?.GetIsProductThinned(titleProduct.ProductId) ?? false;
				if (titleProduct.Genres.Contains(1025L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games)
				{
					result = ProductFlags.BackAgain;
				}
				else if (titleProduct.Genres.Contains(1095L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Movies)
				{
					result = ProductFlags.Classics;
				}
				else if (totalDays <= 28.0)
				{
					result = ProductFlags.New;
				}
				else if ((totalDays >= 105.0 && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games) & flag)
				{
					result = ProductFlags.LastChance;
				}
			}
		}
		return result;
	}

	public static BannerType GetProductBannerType(ITitleProduct titleProduct, bool showDvdBanners = false)
	{
		BannerType result = BannerType.None;
		if (titleProduct != null)
		{
			if (titleProduct.TitleType == TitleType.Bluray)
			{
				result = BannerType.Bluray;
			}
			else if (titleProduct.TitleType == TitleType._4KUHD)
			{
				result = BannerType._4KUHD;
			}
			else if (showDvdBanners && titleProduct.TitleType == TitleType.DVD && !(titleProduct is TitleRollupProduct))
			{
				result = BannerType.DVD;
			}
			else if (titleProduct.TitleType == TitleType.DigitalCode)
			{
				result = BannerType.DigitalCode;
			}
		}
		return result;
	}

	public static string GetProductDisplayName(ITitleProduct titleProduct)
	{
		string result = null;
		if (titleProduct != null && titleProduct.ProductName != null)
		{
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string text = value?.GetName(Resources.ResourceManager);
			List<string> list = new List<string>
			{
				text,
				"(" + text + ")",
				text.Replace(" ", null),
				"(" + text.Replace(" ", null) + ")"
			};
			string text2 = value?.GetAlternateNames(Resources.ResourceManager);
			if (!string.IsNullOrEmpty(text2))
			{
				List<string> list2 = text2?.Split(',')?.ToList();
				if (list2 != null)
				{
					foreach (string item in list2)
					{
						list.Add(item);
						list.Add("(" + item + ")");
					}
				}
			}
			result = RemoveEndingText(titleProduct.ProductName, list);
		}
		return result;
	}

	private static string RemoveEndingText(string source, List<string> searchForAndRemoveEndingTextList)
	{
		string text = source?.Trim();
		if (!string.IsNullOrEmpty(text) && searchForAndRemoveEndingTextList != null)
		{
			foreach (string searchForAndRemoveEndingText in searchForAndRemoveEndingTextList)
			{
				if (!string.IsNullOrEmpty(searchForAndRemoveEndingText) && text.ToUpper().EndsWith(searchForAndRemoveEndingText.ToUpper()) && text.Length > searchForAndRemoveEndingText.Length)
				{
					text = text.Substring(0, text.Length - searchForAndRemoveEndingText.Length)?.Trim();
				}
			}
		}
		return text;
	}

	public static void ConfigureBanner(DisplayProductModel displayProductModel, ITitleProduct titleProduct, bool hideBanner, BannerType bannerType, FrameworkElement frameworkElementToGetStyleFrom)
	{
		displayProductModel.BannerVisibility = ((hideBanner || bannerType == BannerType.None) ? Visibility.Collapsed : Visibility.Visible);
		if (bannerType == BannerType.None)
		{
			return;
		}
		Banner banner = (displayProductModel.Banner = BannerCollection.Instance.Find((Banner x) => x.BannerType == bannerType));
		if (banner.BannerType == BannerType.ComingSoon)
		{
			if (titleProduct != null)
			{
				displayProductModel.BannerText = string.Format("{1} {0:M/d}", titleProduct.MerchandiseDate, Resources.ResourceManager.GetString(banner.TextResourceName));
			}
		}
		else
		{
			displayProductModel.BannerText = Resources.ResourceManager.GetString(banner.TextResourceName);
		}
		displayProductModel.BannerBackgroundBrush = WPFFunctions.GetBrush(frameworkElementToGetStyleFrom, banner.BannerStyleName);
	}

	private static void AddProductToCartIfUserApprovesWarnings(AddProductToCartParameters addProductToCartParameters)
	{
		if (addProductToCartParameters == null)
		{
			return;
		}
		LogHelper.Instance.Log($"Starting AddProductToCartIfUserApprovesWarnings for product id {addProductToCartParameters?.TitleProduct?.ProductId}, purchase: {addProductToCartParameters?.IsPurchase}");
		if (!CheckForADAPurchaseWarning(addProductToCartParameters) || !CheckForMatureRatingWarning(addProductToCartParameters) || !CheckFor4kWarning(addProductToCartParameters) || !CheckForSubPlatformWarning(addProductToCartParameters) || !CheckForPurchaseWarning(addProductToCartParameters) || !CheckForDigitalCodeWarning(addProductToCartParameters) || !CheckForAdditionalDiscWarning(addProductToCartParameters))
		{
			return;
		}
		if (addProductToCartParameters.IsRedboxPlusFreeOneNightRental)
		{
			RemoveExistingRedboxPlusFreeOneNightRentalItem();
		}
		ISession instance = Redbox.Rental.Services.Session.Session.Instance;
		if (instance == null || !instance.HasReceivedUserInteractionThatStartsTimedSession)
		{
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.AddProductToCart));
		}
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = RentalShoppingCartService?.CreateRentalShoppingCartItemFromTitleProduct(addProductToCartParameters.TitleProduct);
		if (rentalShoppingCartTitleItem != null)
		{
			rentalShoppingCartTitleItem.SourceType = addProductToCartParameters.ItemSourceType;
			rentalShoppingCartTitleItem.Action = (addProductToCartParameters.IsPurchase ? RentalShoppingCartItemAction.purchase : RentalShoppingCartItemAction.rental);
			rentalShoppingCartTitleItem.Quantity = 1m;
			rentalShoppingCartTitleItem.Status = LineItemStatus.Pending;
			rentalShoppingCartTitleItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(addProductToCartParameters.TitleProduct);
			rentalShoppingCartTitleItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(addProductToCartParameters.TitleProduct);
			rentalShoppingCartTitleItem.PersonalizationTitleTags = PersonalizationFunctions.GetPersonalizationTitleTags(addProductToCartParameters.TitleProduct);
			if (addProductToCartParameters.IsRedboxPlusFreeOneNightRental && RedboxPlusFunctions.IsFreeOneNightRentalAvailable)
			{
				RedboxPlusFunctions.ValidateAndSetFreeOneNightRental(rentalShoppingCartTitleItem, showConfirmationMessage: false, null);
			}
			RentalShoppingCartService?.CurrentRentalShoppingCart?.AddTitleItem(rentalShoppingCartTitleItem, addProductToCartParameters.PricingRecord);
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
			}
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IBrowseState browseState = BrowseState;
		if (browseState != null && browseState.ProductAddedFromCarousel)
		{
			OrchestrationService.ViewShow("browse_view", GetBrowseViewparameters());
			BrowseState.ProductAddedFromCarousel = false;
		}
		bool flag = service?.Stack.Any((IViewFrameInstance vf) => vf.ViewFrame?.ViewName == "recommended_titles_popup_view") ?? false;
		if (BrowseState?.ItemAddedFromView == "recommended_titles_popup_view" || ((BrowseState?.ItemAddedFromView == "product_detail_view") & flag))
		{
			service?.UnwindToSafe("browse_view", discard: true, "recommended_titles_popup_view");
		}
		else if (BrowseState?.ItemAddedFromView == "product_detail_view")
		{
			service?.GoBack("Item Added", popOnly: false);
		}
		if (BrowseState?.ItemAddedFromView == "multi_night_pricing_popup_view")
		{
			service?.UnwindToSafe("browse_view", discard: true, "multi_night_pricing_popup_view");
		}
		if (BrowseState?.ItemAddedFromView == "option_selection_dialog_view")
		{
			service?.UnwindToSafe("browse_view", discard: true, "option_selection_dialog_view");
		}
		IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
		if (rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull)
		{
			ShowCartFullMessage();
		}
	}

	private static void RemoveExistingRedboxPlusFreeOneNightRentalItem()
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x?.IsRedboxPlusFreeOneNightRental ?? false);
			if (rentalShoppingCartTitleItem != null)
			{
				LogHelper.Instance.Log($"Removing existing Redbox+ free one night rental item product id: {rentalShoppingCartTitleItem.ProductId}");
				rentalShoppingCart.RemoveItem(rentalShoppingCartTitleItem, userRemoved: true);
			}
		}
	}

	public static BrowseViewParameters GetBrowseViewparameters()
	{
		BrowseViewParameters browseViewParameters = new BrowseViewParameters();
		if (ApplicationState.Instance.IsADAAccessible)
		{
			browseViewParameters.ViewAnalyticsName = "browse_view_ada_accessible";
		}
		else
		{
			browseViewParameters.ViewAnalyticsName = "browse_view";
		}
		return browseViewParameters;
	}

	public static void ShowRelatedProductsDialog(ITitleProduct selectedProduct)
	{
		IRecommendedTitles recommendedTitles = RecommendedTitlesPopupViewController.GenerateRecommendedTitles(selectedProduct);
		bool flag = false;
		if (recommendedTitles != null && recommendedTitles.Count > 0)
		{
			int numberOfTitlesToShow = 6;
			List<IBrowseItemModel> list = RecommendedTitlesPopupViewController.CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
			if (list != null && list.Count > 0)
			{
				flag = true;
				RecommendedTitlesPopupInfo recommendedTitlesPopupInfo = new RecommendedTitlesPopupInfo();
				recommendedTitlesPopupInfo.RecommendedTitles = recommendedTitles;
				recommendedTitlesPopupInfo.NumberOfTitlesToShow = numberOfTitlesToShow;
				recommendedTitlesPopupInfo.ShownFromView = "option_selection_dialog_view";
				OrchestrationService.ViewShow("recommended_titles_popup_view", recommendedTitlesPopupInfo);
			}
		}
		if (!flag)
		{
			Action action = delegate
			{
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsRelatedMoviesGotit");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBack("RelatedMoviesGotIt", popOnly: false);
			};
			MessagePopupViewController.ShowRelatedTitlesNotFoundMessage(action, action);
		}
	}

	private static bool CheckForAdditionalDiscWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool flag = true;
		if (!_multiDiscWarningShown && !addProductToCartParameters.IsPurchase && (addProductToCartParameters.TitleProduct.RequiredAdditionalTitles?.Count ?? 0) > 0)
		{
			foreach (long requiredAdditionalTitle in addProductToCartParameters.TitleProduct.RequiredAdditionalTitles)
			{
				if (!RentalShoppingCartService.CurrentRentalShoppingCart.HasTitleItem(requiredAdditionalTitle))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				string title = string.Format(Resources.multi_disc_warning_title, addProductToCartParameters.TitleProduct.RequiredAdditionalTitles.Count + 1);
				string empty = string.Empty;
				IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
				IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
				IPricingRecord oneDayProductPricingRecord = service.GetOneDayProductPricingRecord(addProductToCartParameters.TitleProduct.ProductId);
				decimal initialNight = oneDayProductPricingRecord.InitialNight;
				foreach (long requiredAdditionalTitle2 in addProductToCartParameters.TitleProduct.RequiredAdditionalTitles)
				{
					oneDayProductPricingRecord = service.GetOneDayProductPricingRecord(requiredAdditionalTitle2);
					if (oneDayProductPricingRecord != null)
					{
						initialNight += oneDayProductPricingRecord.InitialNight;
					}
				}
				IKioskProduct kioskProduct = null;
				if (addProductToCartParameters.TitleProduct.RequiredAdditionalTitles.Count > 1)
				{
					string text = "";
					for (int i = 0; i < addProductToCartParameters.TitleProduct.RequiredAdditionalTitles.Count - 1; i++)
					{
						kioskProduct = null;
						service2.GetProduct(addProductToCartParameters.TitleProduct.RequiredAdditionalTitles[i], excludeTitleRollup: true, out kioskProduct);
						if (kioskProduct != null)
						{
							if (i > 0)
							{
								text += ",";
							}
							text = text + " " + kioskProduct.ProductName;
						}
					}
					kioskProduct = null;
					service2.GetProduct(addProductToCartParameters.TitleProduct.RequiredAdditionalTitles[addProductToCartParameters.TitleProduct.RequiredAdditionalTitles.Count - 1], excludeTitleRollup: true, out kioskProduct);
					string arg = kioskProduct?.ProductName;
					empty = string.Format(Resources.multi_disc_warning_multiple_disc_message, text, arg, initialNight.ToString("0.00"));
				}
				else
				{
					kioskProduct = null;
					service2.GetProduct(addProductToCartParameters.TitleProduct.RequiredAdditionalTitles[addProductToCartParameters.TitleProduct.RequiredAdditionalTitles.Count - 1], excludeTitleRollup: true, out kioskProduct);
					string arg2 = kioskProduct?.ProductName;
					empty = string.Format(Resources.multi_disc_warning_2_disc_message, arg2, initialNight.ToString("0.00"));
				}
				_multiDiscWarningShown = true;
				MessagePopupViewController.ShowMultiDiscAlert(title, empty, delegate
				{
					if (Redbox.Rental.Services.Session.Session.Instance != null)
					{
						Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
					}
					ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
				}, delegate
				{
					BrowseState._4kWarningShown = true;
					AnalyticsService?.AddButtonPressEvent("BrowseFunctionsOkay");
					ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("multi_disc_message_view:continue", BrowseState.ProductAddedFromCarousel);
					AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
				});
			}
		}
		return flag;
	}

	private static bool CheckForADAPurchaseWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (ApplicationState.Instance.IsADAAccessible && !_adaPurchaseWarningShown && addProductToCartParameters.IsPurchase)
		{
			result = false;
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(addProductToCartParameters.TitleProduct.TitleType, out value);
			string arg = $"{value?.GetName(Resources.ResourceManager)} {((addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Games) ? Resources.game : null)}".Trim();
			string arg2 = $"{addProductToCartParameters.TitleProduct.PurchasePrice:n2}";
			string value2 = string.Format(Resources.ada_purchase_price_message, arg, Resources.currency_symbol, arg2);
			string abe_purchase_price_message = Resources.abe_purchase_price_message;
			string value3 = null;
			if (!string.IsNullOrEmpty(abe_purchase_price_message))
			{
				value3 = (ApplicationState.Instance.IsABEOn ? string.Format(abe_purchase_price_message, arg, Resources.currency_symbol, arg2) : null);
			}
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["ADAPurchaseWarning:Header"] = value2;
				service["ABEPurchaseMessage:Header"] = value3;
			}
			MessagePopupViewController.ShowADAPurchaseWarning(delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("ada_purchase_message_view:hide()", popOnly: false);
			}, delegate
			{
				_adaPurchaseWarningShown = true;
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("ada_purchase_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
			});
		}
		return result;
	}

	private static bool CheckFor4kWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (addProductToCartParameters.TitleProduct.TitleType == TitleType._4KUHD && !BrowseState._4kWarningShown)
		{
			result = false;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["MatureWarning:Header"] = Resources.four_k_alert_title;
				service["Bluray:Message"] = Resources.four_k_alert;
			}
			MessagePopupViewController.Show4KAlert(delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsCancel");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("4k_message_view:hide()", popOnly: false);
			}, delegate
			{
				BrowseState._4kWarningShown = true;
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsGotit");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("4k_message_view:continue", BrowseState.ProductAddedFromCarousel);
				AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
			});
		}
		return result;
	}

	private static bool CheckForSubPlatformWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (!_subPlatformWarningShown)
		{
			ITitleProduct titleProduct = addProductToCartParameters.TitleProduct;
			if (titleProduct != null && titleProduct.SubPlatformId.HasValue)
			{
				ITitleProduct titleProduct2 = addProductToCartParameters.TitleProduct;
				if (titleProduct2 != null)
				{
					_ = titleProduct2.ProductTypeId;
					if (true)
					{
						IDataService service = ServiceLocator.Instance.GetService<IDataService>();
						SubPlatform subPlatform = service?.GetSubPlatforms()?.FirstOrDefault((SubPlatform x) => x.SubPlatformId == addProductToCartParameters.TitleProduct.SubPlatformId);
						ProductType productType = service?.GetProductTypes()?.FirstOrDefault((ProductType x) => x.ProductTypeId == addProductToCartParameters.TitleProduct.ProductTypeId);
						if (subPlatform != null)
						{
							if (productType != null)
							{
								string arg = subPlatform.SubPlatformName.Replace("Compatible", string.Empty).Replace("Required", string.Empty).Replace("XboxOne", string.Empty)
									.Replace("PS4 ", string.Empty)
									.Replace("Move", "Playstation Move")
									.Replace("Eye Camera", "Playstation Eye Camera");
								string arg2 = $"\n{arg}{Resources.subplatform_for} {productType?.ProductTypeName}";
								result = false;
								IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
								if (service2 != null)
								{
									service2["Subplatform:Header"] = Resources.subplatform_warning_title;
									service2["Subplatform:Message"] = string.Format(Resources.subplatform_warning, arg2);
								}
								MessagePopupViewController.ShowSubPlatformWarning(delegate
								{
									if (Redbox.Rental.Services.Session.Session.Instance != null)
									{
										Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
									}
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
								}, delegate
								{
									if (Redbox.Rental.Services.Session.Session.Instance != null)
									{
										Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
									}
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("subplatform_message_view:hide()", popOnly: false);
								}, delegate
								{
									_subPlatformWarningShown = true;
									ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("subplatform_message_view:continue", BrowseState.ProductAddedFromCarousel);
									AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
								});
							}
							else
							{
								LogHelper.Instance.Log("Error: Unable to find product type with id {0}", addProductToCartParameters.TitleProduct.ProductTypeId);
							}
						}
						else
						{
							LogHelper.Instance.Log("Error: Unable to find subPlatorm with id {0}", addProductToCartParameters.TitleProduct.SubPlatformId);
						}
					}
				}
			}
		}
		return result;
	}

	private static bool CheckForPurchaseWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (!_purchaseWarningShown && addProductToCartParameters.IsPurchase && addProductToCartParameters.TitleProduct.ProductId != 1139 && addProductToCartParameters.TitleProduct.ProductTypeId != 17)
		{
			result = false;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				service["PurchaseWarning:Header"] = Resources.products_used;
				service["PurchaseWarning:Message"] = Resources.purchase_warning;
			}
			MessagePopupViewController.ShowPurchaseWarning(delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("purchase_message_view:hide()", popOnly: false);
			}, delegate
			{
				_purchaseWarningShown = true;
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("purchase_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
			});
		}
		return result;
	}

	private static bool CheckForDigitalCodeWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (!_digitalCodeWarningShown && addProductToCartParameters.TitleProduct.ProductTypeId == 17)
		{
			result = false;
			MessagePopupViewController.ShowDigitalCodeAlert(delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsCancelDigitalCodeIncart");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert back button", popOnly: false);
			}, delegate
			{
				_digitalCodeWarningShown = true;
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsOkDigitalCodeIncart");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert ok button", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
			}, delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code alert time out", popOnly: false);
			});
		}
		return result;
	}

	private static bool CheckForMatureRatingWarning(AddProductToCartParameters addProductToCartParameters)
	{
		bool result = true;
		if (addProductToCartParameters.TitleProduct != null && (addProductToCartParameters.TitleProduct.RatingId == 8 || addProductToCartParameters.TitleProduct.RatingId == 18 || addProductToCartParameters.TitleProduct.RatingId == 20 || addProductToCartParameters.TitleProduct.RatingId == 21) && ((!BrowseState.MovieMatureWarningShown && addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Movies) || (!BrowseState.MatureGameWarningShown && addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Games)))
		{
			result = false;
			List<string> list = new List<string>();
			list.Add(addProductToCartParameters.TitleProduct.TitleFamily.ToString());
			list.Add("matureWarning");
			Ratings result2 = Ratings.None;
			Enum.TryParse<Ratings>(addProductToCartParameters.TitleProduct?.RatingId?.ToString() ?? string.Empty, out result2);
			RatingsProperty ratingsProperty = ((result2 != Ratings.None) ? RatingsProperties.Properties[result2] : null);
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (service != null)
			{
				if (addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Movies)
				{
					string mature_movie_warning = Resources.mature_movie_warning;
					mature_movie_warning = string.Format(mature_movie_warning, ratingsProperty?.Name);
					service["MatureWarning:Header"] = string.Format(Resources.esrb_mature_name, ratingsProperty?.Name);
					service["MatureWarning:Message"] = mature_movie_warning;
					service["MatureWarning:TTSMessage"] = mature_movie_warning.Replace("OK", "Enter");
				}
				else if (addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Games)
				{
					string mature_game_warning = Resources.mature_game_warning;
					mature_game_warning = string.Format(mature_game_warning, ratingsProperty?.Name).Replace("M", Resources.esrb_mature);
					service["MatureWarning:Header"] = string.Format(Resources.esrb_mature_game_name, ratingsProperty?.Code);
					service["MatureWarning:Message"] = mature_game_warning;
					service["MatureWarning:TTSMessage"] = mature_game_warning.Replace("OK", "Enter");
				}
			}
			MessagePopupViewController.ShowMatureWarning(delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("rrated_message_view:hide()", popOnly: false);
			}, delegate
			{
				if (addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Movies)
				{
					BrowseState.MovieMatureWarningShown = true;
				}
				else if (addProductToCartParameters.TitleProduct.TitleFamily == TitleFamily.Games)
				{
					BrowseState.MatureGameWarningShown = true;
				}
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("rrated_message_view:continue", popOnly: false);
				AddProductToCartIfUserApprovesWarnings(addProductToCartParameters);
			}, list);
		}
		return result;
	}

	public static BrowseViewFilter GetBrowseMode()
	{
		return GetBrowseMode(BrowseState.TitleType);
	}

	public static BrowseViewFilter GetBrowseMode(TitleType titleType)
	{
		if (!IsRedboxPlusBrowseMode)
		{
			if (titleType != TitleType.DigitalCode)
			{
				return BrowseViewFilter.All;
			}
			return BrowseViewFilter.ForSale;
		}
		return BrowseViewFilter.RedboxPlus;
	}

	public static string GetPurchasePriceText(ITitleProduct titleProduct, Func<decimal, decimal> discountedPriceCalc = null)
	{
		List<ITitleProduct> list = ((titleProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)titleProduct).KioskProducts : new List<ITitleProduct> { titleProduct }).Where((ITitleProduct tp) => tp.IsSellableAndInStock).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		decimal price = list.Min((ITitleProduct tp) => tp.PurchasePrice);
		bool flag = list.Count == 1 || list.All((ITitleProduct tp) => tp.PurchasePrice == price);
		if (discountedPriceCalc != null)
		{
			decimal num = discountedPriceCalc(price);
			if (num > 0m)
			{
				return string.Format("${0:F2}{1}", num, flag ? "" : "+");
			}
			return Resources.offer_info_free;
		}
		return string.Format("${0:F2}{1}", price, flag ? "" : "+");
	}

	public static void PromptToRemoveItem(IBaseRentalShoppingCartItem deleteRentalShoppingCartItem, bool userRemoved)
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null)
		{
			return;
		}
		if (deleteRentalShoppingCartItem is IRentalShoppingCartSubscriptionItem)
		{
			MessagePopupViewController.ShowRemoveSubscriptionWarning(delegate
			{
				OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
			}, delegate
			{
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsCancel");
				OrchestrationService.ViewGoBackSafe("RemoveSubscriptionFromCart", popOnly: false);
			}, delegate
			{
				BrowseState.SubscriptionRemoved = true;
				AnalyticsService?.AddButtonPressEvent("BrowseFunctionsRemove");
				RemoveItemFromCart();
				OrchestrationService.ViewGoBackSafe("RemoveSubscriptionFromCart", popOnly: false);
			});
		}
		else
		{
			RemoveItemFromCart();
		}
		void RemoveItemFromCart()
		{
			rentalShoppingCart.RemoveItem(deleteRentalShoppingCartItem, userRemoved);
		}
	}

	public static void AddMultiNightPricedProductToCart(ITitleProduct titleProduct, string shownFromView)
	{
		string viewAnalyticsName = "multi_night_pricing_popup_view";
		if (titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
		{
			viewAnalyticsName = "multi_night_pricing_with_buy_button_popup_view";
		}
		else if (!titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
		{
			viewAnalyticsName = "non_multi_night_pricing_with_buy_button_popup_view";
		}
		AddMultiNightPricedProductToCart(titleProduct, shownFromView, viewAnalyticsName);
	}

	public static void AddMultiNightPricedProductToCart(ITitleProduct titleProduct, string shownFromView, string viewAnalyticsName)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = true;
		}
		OrchestrationService.ViewShow("multi_night_pricing_popup_view", new MultiNightPricingPopupInfo
		{
			TitleProduct = titleProduct,
			ShownFromView = shownFromView,
			ViewAnalyticsName = viewAnalyticsName
		});
	}

	public static void ShowRentalPopupView(TitleRollupProduct titleRollupProduct, bool isPurchase, Action cancelAction = null, bool showMnp = true, Func<decimal, decimal> discountedPriceCalc = null, bool showPurchaseButton = false)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = true;
		}
		OrchestrationService.SetCurrentUICulture();
		OrchestrationService.ViewShow("rental_popup_view", new RentalFormatSelectionViewParameter(titleRollupProduct, isPurchase, cancelAction, showMnp, discountedPriceCalc, IsRedboxPlusBrowseMode ? new int?(2) : ((int?)null), showPurchaseButton));
	}

	public static void ShowPurchasePopupView(TitleRollupProduct titleRollupProduct, bool isPurchase, Action cancelAction = null, bool showMnp = true, Func<decimal, decimal> discountedPriceCalc = null, bool showPurchaseButton = false)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = true;
		}
		OrchestrationService.SetCurrentUICulture();
		OrchestrationService.ViewShow("purchase_popup_view", new RentalFormatSelectionViewParameter(titleRollupProduct, isPurchase, cancelAction, showMnp, discountedPriceCalc, null, showPurchaseButton));
	}

	public static void ShowRedboxPlusBrowseWarnings()
	{
		ShowRedboxPlusCartFullMessageIfNeeded();
		ShowRedboxPlusFreeOneNightRentalAlreadyUsedMessageIfNeeded();
	}

	private static void ShowRedboxPlusCartFullMessageIfNeeded()
	{
		if (IsRedboxPlusBrowseMode && RedboxPlusFunctions.IsFreeOneNightRentalAvailable && !CanAddProductToCart)
		{
			Action action = delegate
			{
				OrchestrationService.ViewGoBackSafe("RedboxPlusCartFullMessage", popOnly: false);
			};
			MessagePopupViewController.ShowRedboxPlusCartFullMessage(action, action);
		}
	}

	private static void ShowRedboxPlusFreeOneNightRentalAlreadyUsedMessageIfNeeded()
	{
		if (IsRedboxPlusBrowseMode && !RedboxPlusFunctions.IsFreeOneNightRentalAvailable)
		{
			Action action = delegate
			{
				OrchestrationService.ViewGoBackSafe("RedboxPlusFreeOneNightRentalAlreadyUsedMessage", popOnly: false);
			};
			if (RedboxPlusFunctions.RedboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier1)
			{
				MessagePopupViewController.ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier1Message(action, action);
			}
			else
			{
				MessagePopupViewController.ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier2Message(action, action);
			}
		}
	}

	public static List<DisplayProductMoreLikeThisModel> CreateMoreLikeThis(ITitleProduct titleProduct, Action<IBrowseItemModel> onMoreLikeThisSelected)
	{
		List<DisplayProductMoreLikeThisModel> result = new List<DisplayProductMoreLikeThisModel>();
		long productGroupId = ((titleProduct is ITitleRollupProduct titleRollupProduct) ? titleRollupProduct.ProductGroupId : titleProduct.TitleRollupProductGroupId);
		(ServiceLocator.Instance.GetService<IKioskClientService>()?.ProductGroups?.GetMoreLikeThis(productGroupId))?.ForEach(delegate(long eachMoreLikeThis)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				service.GetTitleRollupProduct(eachMoreLikeThis, out var kioskProduct);
				if (kioskProduct != null)
				{
					ITitleRollupProduct moreLikeThisTitleRollupProduct = kioskProduct as ITitleRollupProduct;
					if (moreLikeThisTitleRollupProduct != null)
					{
						if (BrowseState.BrowseItemModels.Any((IBrowseItemModel item) => item.Data == moreLikeThisTitleRollupProduct) && (moreLikeThisTitleRollupProduct.IsRentableAndInStock || moreLikeThisTitleRollupProduct.IsSellableAndInStock))
						{
							DisplayProductMoreLikeThisModel displayProductMoreLikeThisModel = new DisplayProductMoreLikeThisModel
							{
								Name = moreLikeThisTitleRollupProduct.ProductName,
								Data = moreLikeThisTitleRollupProduct,
								Image = OrchestrationService.GetProductBitmapImage(moreLikeThisTitleRollupProduct)
							};
							displayProductMoreLikeThisModel.OnMoreLikeThisSelected += onMoreLikeThisSelected;
							result.Add(displayProductMoreLikeThisModel);
						}
						else
						{
							moreLikeThisTitleRollupProduct.KioskProducts?.ForEach(delegate(ITitleProduct eachMoreLikeThisKioskProduct)
							{
								if (BrowseState.BrowseItemModels.Any((IBrowseItemModel item) => item.Data == eachMoreLikeThisKioskProduct) && (eachMoreLikeThisKioskProduct.IsRentableAndInStock || eachMoreLikeThisKioskProduct.IsSellableAndInStock))
								{
									DisplayProductMoreLikeThisModel displayProductMoreLikeThisModel2 = new DisplayProductMoreLikeThisModel
									{
										Name = eachMoreLikeThisKioskProduct.ProductName,
										Data = eachMoreLikeThisKioskProduct,
										Image = OrchestrationService.GetProductBitmapImage(eachMoreLikeThisKioskProduct)
									};
									displayProductMoreLikeThisModel2.OnMoreLikeThisSelected += onMoreLikeThisSelected;
									result.Add(displayProductMoreLikeThisModel2);
								}
							});
						}
					}
				}
			}
		});
		return result;
	}
}
