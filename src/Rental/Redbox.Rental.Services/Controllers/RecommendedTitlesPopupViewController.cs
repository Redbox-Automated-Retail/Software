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
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class RecommendedTitlesPopupViewController : BaseViewController
{
	private static string _shownFromView;

	public static void ConfigureRecommendedTitlesPopupView(RecommendedTitlesPopupInfo recommendedTitlesPopupInfo)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			_shownFromView = recommendedTitlesPopupInfo.ShownFromView;
			RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel = new RecommendedTitlesPopupViewModel();
			int valueOrDefault = (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance).GetValueOrDefault();
			ISession instance = Redbox.Rental.Services.Session.Session.Instance;
			bool flag = instance != null && instance.PersonalizationSession?.StoredPromoCodes?.Count > 0;
			bool displayUpdatedPerksPoints = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.DisplayUpdatedPerksPoints;
			int num = ((!displayUpdatedPerksPoints) ? 1 : 0);
			int num2 = (displayUpdatedPerksPoints ? 1550 : 1000);
			int num3 = (displayUpdatedPerksPoints ? 2000 : 1500);
			bool flag2 = (valueOrDefault >= num) | flag;
			recommendedTitlesPopupViewModel.PerksInfoBorderVisibility = ((!flag2) ? Visibility.Collapsed : Visibility.Visible);
			recommendedTitlesPopupViewModel.RecommendationsBorderCornerRadius = (flag2 ? new CornerRadius(0.0, 0.0, 16.0, 16.0) : new CornerRadius(16.0, 16.0, 16.0, 16.0));
			recommendedTitlesPopupViewModel.PerksHeadingText = Resources.recommended_titles_perks_heading_text;
			recommendedTitlesPopupViewModel.PerksMessageText = string.Format(Resources.recommended_titles_perks_message_text_points_1_to_999_no_promo, valueOrDefault);
			recommendedTitlesPopupViewModel.HowPointsWorkButtonText = Resources.recommended_titles_how_points_work_button_text;
			recommendedTitlesPopupViewModel.OnHowPointsWorkButtonClicked = (Action)Delegate.Combine(recommendedTitlesPopupViewModel.OnHowPointsWorkButtonClicked, new Action(RecommendedTitlesPopupViewModel_OnHowPointsWorkButtonClicked));
			recommendedTitlesPopupViewModel.HowPointsWorkButtonVisibility = Visibility.Visible;
			if (flag2 && valueOrDefault >= num2 && valueOrDefault <= num3 - 1 && !flag)
			{
				recommendedTitlesPopupViewModel.PerksMessageText = string.Format(Resources.recommended_titles_perks_message_text_points_1000_to_1499_no_promo, num3 - valueOrDefault);
			}
			if (flag2 && valueOrDefault >= num3 && !flag)
			{
				recommendedTitlesPopupViewModel.PerksMessageText = Resources.recommended_titles_perks_message_text_points_1500_no_promo;
			}
			if ((flag2 && valueOrDefault < num3) & flag)
			{
				recommendedTitlesPopupViewModel.PerksMessageText = Resources.recommended_titles_perks_message_text_points_0_to_1499_yes_promo;
				recommendedTitlesPopupViewModel.HowPointsWorkButtonVisibility = Visibility.Collapsed;
			}
			if ((flag2 && valueOrDefault >= num3) & flag)
			{
				recommendedTitlesPopupViewModel.PerksMessageText = Resources.recommended_titles_perks_message_text_points_1500_yes_promo;
			}
			recommendedTitlesPopupViewModel.HeaderText = ((recommendedTitlesPopupInfo.ShownFromView == "option_selection_dialog_view") ? Resources.related_movies_popup_header : Resources.recommended_titles_popup_header);
			ConfigureRecommendedTitlesBrowse(recommendedTitlesPopupViewModel, recommendedTitlesPopupInfo.RecommendedTitles, recommendedTitlesPopupInfo.NumberOfTitlesToShow);
			bool valueOrDefault2 = ServiceLocator.Instance.GetService<IViewService>()?.Stack?.Any((IViewFrameInstance vfi) => vfi?.ViewFrame?.ViewName == "shopping_cart_view") == true;
			recommendedTitlesPopupViewModel.PickupButtonText = Resources.recommended_titles_pickup_button_text;
			recommendedTitlesPopupViewModel.PickupButtonVisibility = (valueOrDefault2 ? Visibility.Collapsed : Visibility.Visible);
			recommendedTitlesPopupViewModel.OnPickupButtonClicked = (Action)Delegate.Combine(recommendedTitlesPopupViewModel.OnPickupButtonClicked, new Action(RecommendedTitlesPopupViewModel_OnPickupButtonClicked));
			recommendedTitlesPopupViewModel.CancelButtonText = Resources.close_button.ToUpper();
			recommendedTitlesPopupViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(recommendedTitlesPopupViewModel.OnCancelButtonClicked, new Action(RecommendedTitlesPopupViewModel_OnCancelButtonClicked));
			ConfigureTTS(recommendedTitlesPopupViewModel);
			viewUserControl.DataContext = recommendedTitlesPopupViewModel;
		}
	}

	public static void LeaveRecommendedTitlesPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	public static IRecommendedTitles UnrollRecommendedTitles(IRecommendedTitles recommendedTitles)
	{
		List<IRecommendedTitleId> list = new List<IRecommendedTitleId>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			foreach (IRecommendedTitleId recommendedTitle in recommendedTitles)
			{
				if (recommendedTitle.IdType == RecommendedTitlesIdType.TitleID)
				{
					list.Add(recommendedTitle);
				}
				else
				{
					if (recommendedTitle.IdType != RecommendedTitlesIdType.ProductGroupID)
					{
						continue;
					}
					service.GetTitleRollupProduct(recommendedTitle.Id, out var kioskProduct);
					if (kioskProduct == null)
					{
						continue;
					}
					ITitleRollupProduct titleRollupProduct = ((kioskProduct.KioskProductType == KioskProductType.TitleRollup) ? (kioskProduct as ITitleRollupProduct) : null);
					if (titleRollupProduct == null)
					{
						continue;
					}
					foreach (ITitleProduct item in titleRollupProduct.KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
					{
						list.Add(new RecommendedTitleId
						{
							Id = item.ProductId,
							IdType = RecommendedTitlesIdType.TitleID
						});
					}
				}
			}
		}
		return new RecommendedTitles(list)
		{
			OfferCode = recommendedTitles.OfferCode
		};
	}

	public static IRecommendedTitles GenerateRecommendedTitles(ITitleProduct titleProduct)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service == null)
		{
			return null;
		}
		ErrorList products = service.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleProduct.TitleType, BrowseSort.SortDate, null, null, null, null, isADAView: true, null, null, out var products2);
		if (products.Count > 0)
		{
			products.ToLogHelper();
			return null;
		}
		if (products2 == null)
		{
			return null;
		}
		List<IKioskProduct> list = (from kp in products2
			where kp is ITitleProduct && (((ITitleProduct)kp).Genres.Intersect(titleProduct.Genres).Count() > 0 || ((ITitleProduct)kp).Genres.Contains(1093L)) && ((ITitleProduct)kp).IsRentableAndInStock
			orderby !((ITitleProduct)kp).Genres.Contains(1093L) descending
			select kp).ToList();
		List<IRecommendedTitleId> list2 = new List<IRecommendedTitleId>();
		foreach (IKioskProduct item in list)
		{
			list2.Add(new RecommendedTitleId
			{
				Id = ((ITitleProduct)item).ProductId,
				IdType = RecommendedTitlesIdType.TitleID
			});
		}
		return new RecommendedTitles(list2);
	}

	public static List<IBrowseItemModel> CreateBrowseItemsFromRecommendedTitles(IRecommendedTitles recommendedTitles, int numberOfTitlesToShow)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			foreach (IRecommendedTitleId recommendedTitle in recommendedTitles)
			{
				IKioskProduct kioskProduct = null;
				if (recommendedTitle.IdType == RecommendedTitlesIdType.TitleID)
				{
					service.GetProduct(recommendedTitle.Id, excludeTitleRollup: true, out kioskProduct);
				}
				else if (recommendedTitle.IdType == RecommendedTitlesIdType.ProductGroupID)
				{
					service.GetTitleRollupProduct(recommendedTitle.Id, out kioskProduct);
				}
				if (kioskProduct == null)
				{
					continue;
				}
				ITitleRollupProduct titleRollupProduct = ((kioskProduct.KioskProductType == KioskProductType.TitleRollup) ? (kioskProduct as ITitleRollupProduct) : null);
				ITitleProduct titleProduct = ((kioskProduct.KioskProductType == KioskProductType.Title) ? (kioskProduct as ITitleProduct) : null);
				if ((titleRollupProduct == null || !titleRollupProduct.IsRentableAndInStock || titleRollupProduct.IsEmptyCase) && (titleProduct == null || !titleProduct.IsRentableAndInStock || titleProduct.IsEmptyCase))
				{
					continue;
				}
				if (kioskProduct is ITitleProduct)
				{
					((ITitleProduct)kioskProduct).TivoQueryId = recommendedTitles.TivoQueryId;
				}
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					Header = kioskProduct.ProductName,
					DisplayProductModelConstants = GetDisplayProductModelConstants(),
					AddButtonStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style")
				};
				if (recommendedTitle.IsWishList)
				{
					displayProductModel.FlagStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "rubine_flag_style");
					displayProductModel.FlagText = Resources.flag_wish_list;
				}
				displayProductModel.ConfigureForRecommendedTitlesPopup();
				displayProductModel.ButtonData.Clear();
				if (titleRollupProduct != null)
				{
					foreach (ITitleProduct item in titleRollupProduct.KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
					{
						AddFormatButtonData(displayProductModel, item);
					}
				}
				else
				{
					AddFormatButtonData(displayProductModel, titleProduct);
				}
				list.Add(displayProductModel);
			}
		}
		return list.Take(numberOfTitlesToShow).ToList();
	}

	private static RecommendedTitlesPopupViewModel GetRecommendedTitlesPopupViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as RecommendedTitlesPopupViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			recommendedTitlesPopupViewModel.OnGetSpeechControl = () => RecommendedTitlesPopupViewTextToSpeech.GetSpeechControl(GetRecommendedTitlesPopupViewModel);
		}
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("long", 40000);
		IIdleTimer obj = service?.CreateIdleTimer("RecommendedTitlesPopupViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("RecommendedTitlesPopupViewController time out", popOnly: false);
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("RecommendedTitlesPopupViewTimer"))?.Stop();
		service?.RemoveIdleTimer("RecommendedTitlesPopupViewTimer");
	}

	private static void ConfigureRecommendedTitlesBrowse(RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel, IRecommendedTitles recommendedTitles, int numberOfTitlesToShow)
	{
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.RowCount = 1;
		browseControlModel.ColumnCount = 4;
		browseControlModel.BrowseItemMargin = new Thickness(0.0, 0.0, 0.0, 0.0);
		browseControlModel.ShowBorderItems = true;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		browseControlModel.ShowNoResultsMessage = false;
		browseControlModel.BrowseItems = CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels = browseControlModel.BrowseItems;
		for (int i = 0; i < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; i++)
		{
			browseControlModel.BrowseControls.Add(new DisplayProductBrowseUserControl
			{
				Name = $"product_{i + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		browseControlModel.OnBrowseItemModelSelected += BrowseProductControl_OnBrowseItemModelSelected;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddBrowseItemModel;
		recommendedTitlesPopupViewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void BrowseProductControl_OnBrowseItemModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel.Data is IKioskProduct { KioskProductType: var kioskProductType } kioskProduct && (kioskProductType == KioskProductType.Title || kioskProductType == KioskProductType.TitleRollup))
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesTitleDetails")?.AddData(ProductData.ToProductData(titleProduct));
			}
			if (titleProduct != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductDetailShownFromView = service?.Peek();
				OrchestrationService.ViewShow("product_detail_view");
			}
		}
	}

	private static void BrowseProductControl_OnAddBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct) || GetRecommendedTitlesPopupViewModel() == null)
		{
			return;
		}
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.TitleRollup:
		{
			TitleRollupProduct titleRollupProduct = kioskProduct as TitleRollupProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				ITitleProduct titleProduct2 = kioskProduct as ITitleProduct;
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesAddTitle")?.AddData(ProductData.ToProductData(titleProduct2));
			}
			else
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedAddItem");
			}
			BrowseFunctions.ShowRentalPopupView(titleRollupProduct, isPurchase: false);
			break;
		}
		case KioskProductType.Title:
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			if (_shownFromView == "option_selection_dialog_view")
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedMoviesAddTitle")?.AddData(ProductData.ToProductData(titleProduct));
			}
			else
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedAddItem");
			}
			if (titleProduct.HasMultiNightPrice)
			{
				BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct, "recommended_titles_popup_view", null);
			}
			else
			{
				BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false);
			}
			break;
		}
		}
	}

	private static void RecommendedTitlesPopupViewModel_OnHowPointsWorkButtonClicked()
	{
		if (_shownFromView == "option_selection_dialog_view")
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedHowPointsWork");
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedHowPointsWork");
		}
		OrchestrationService.ViewShow("perks_signup_info_popup_view");
	}

	private static void RecommendedTitlesPopupViewModel_OnPickupButtonClicked()
	{
		if (_shownFromView == "option_selection_dialog_view")
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedPickup");
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedPickup");
		}
		Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOut();
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult = null;
		}
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ClearAllLoyaltyPointsRecords();
		ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("recommended_titles_popup_view");
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.IsInReservationFlow = true;
		}
		OrchestrationService.ExecuteScript("pickup_reserved_product");
	}

	private static void RecommendedTitlesPopupViewModel_OnCancelButtonClicked()
	{
		if (_shownFromView == "option_selection_dialog_view")
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRelatedNoThanks");
			OrchestrationService.UnwindToSafe("browse_view", discard: true, "recommended_titles_popup_view");
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendedTitlesPopupRecommendedNoThanks");
			OrchestrationService.ViewGoBackSafe("RecommendedTitlesPopupViewController cancel", popOnly: false);
		}
	}

	private static DisplayProductModelConstants GetDisplayProductModelConstants()
	{
		return new DisplayProductModelConstants
		{
			StarringLabel = Resources.starring_label,
			GenresLabel = Resources.title_details_genres_text,
			OneNightPriceLabel = Resources.one_night_price_label,
			OneNightReturnTimeLabel = Resources.return_time_label,
			MultiNightPriceLabel = Resources.multi_night_price_label,
			MultiNightReturnTimeLabel = Resources.return_time_label,
			AdImage = null,
			AdHeight = 0,
			MainHeight = 628,
			BuyText = Resources.title_detail_buy,
			TitleDetailFormatButtonTextValues = new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues(),
			Add = Resources.add,
			ComingSoonLabel = Resources.coming_label,
			OutOfStockLine1Text = Resources.out_of_stock_line1,
			OutOfStockLine2Text = Resources.out_of_stock_line2,
			OutOfStockLine3Text = Resources.out_of_stock_line3,
			OutOfStockLine4Text = Resources.out_of_stock_line4,
			Movies = Resources.Movies,
			Games = Resources.Games,
			Movie = Resources.Movie.ToLower(),
			Game = Resources.game.ToLower(),
			FontMontserrat16Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_montserrat_extrabold_16"),
			FontMontserrat20Style = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_montserrat_medium_20"),
			PriceFor = Resources.price_for,
			Night = Resources.night,
			PricingService = ServiceLocator.Instance.GetService<IPricingService>(),
			RentalShoppingCartService = BaseViewController.RentalShoppingCartService,
			IsInCartBrowse = false,
			EnableRedboxPlus = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption,
			KioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>(),
			ShowWatchOptions = false,
			ShowDualInStock = false
		};
	}

	private static void AddFormatButtonData(DisplayProductModel browseItemModel, ITitleProduct product)
	{
		DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(product, isPurchase: false, displayProductModelConstants.TitleDetailFormatButtonTextValues);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			TilteDetailRentalFormatButtonData tilteDetailRentalFormatButtonData = new TilteDetailRentalFormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled);
			if (!product.IsMovie)
			{
				tilteDetailRentalFormatButtonData.Line1Text = displayProductModelConstants.Add;
			}
			browseItemModel.ButtonData.Add(tilteDetailRentalFormatButtonData);
		}
	}
}
