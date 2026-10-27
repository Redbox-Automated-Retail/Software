using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RecommendationOnPickupViewController : BaseLoyaltyController
{
	private class Constants
	{
		public static string RecommendationOnPickupIdleTimerName = "recommendation_on_pickup_idle_timer";

		public static int RecommendationOnPickupIdleTimerDuration = 40000;
	}

	private static IReservationDetails _reservationValues;

	private static RentalShoppingCartChanged ShoppingCartChangeEventHandler;

	private static object _lockObject = new object();

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static IRentalShoppingCart RentalShoppingCart => BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			RecommendationOnPickupViewModel recommendationOnPickupViewModel = new RecommendationOnPickupViewModel();
			OrchestrationService.SetCurrentUICulture();
			ConfigureShoppingCart(recommendationOnPickupViewModel, clearCart: false);
			if (string.IsNullOrEmpty(_reservationValues?.ReferenceNumber))
			{
				LogHelper.Instance.Log("No reservation found for mod resv pickup, aborting reservation pickup.");
				OrchestrationService.StartIdleTimer("idle_timer");
				ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
				return;
			}
			ConfigureShoppingCartChangeHandler();
			ConfigureRecommendedTitlesBrowse(recommendationOnPickupViewModel);
			ConfigureTextAndVisibility(recommendationOnPickupViewModel);
			ConfigureCommands(recommendationOnPickupViewModel);
			ConfigureRecommendationOnPickupViewIdleTimer();
			viewUserControl.DataContext = recommendationOnPickupViewModel;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static void ConfigureRecommendationOnPickupViewIdleTimer()
	{
		IIdleTimer obj = IdleTimerService?.CreateIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
		obj?.Initialize(Constants.RecommendationOnPickupIdleTimerDuration, delegate
		{
			OrchestrationService.StopIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
			LogHelper.Instance.Log("recommendation on pickup view: idle timer expired");
			if (ServiceLocator.Instance.GetService<IViewService>()?.Peek() == "recommendation_on_pickup_view")
			{
				MessagePopupViewController.ShowRecommendationOnPickupPrompt(delegate
				{
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt timed out", popOnly: false);
					PickupOnlyReservedTitlesFromTimeoutDialog();
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt - vend only reserved titles", popOnly: false);
					PickupOnlyReservedTitlesFromTimeoutDialog();
				}, delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendationOnPickupKeepBrowsing");
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt - keep browsing", popOnly: false);
				});
			}
		});
		obj?.Start();
	}

	private static void PickupOnlyReservedTitlesFromTimeoutDialog()
	{
		if (_reservationValues.AuthorizeAtPickup)
		{
			foreach (IRentalShoppingCartTitleItem item in RentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem i) => i.SourceType == ItemSourceType.Recommendation))
			{
				RentalShoppingCart.RemoveItem(item, userRemoved: false);
			}
			ReservationFunctions.InvokeAuthorizeOnPickupCallback(_reservationValues?.ReferenceNumber, cartWasModified: false);
		}
		else
		{
			ReservationFunctions.InvokeVendOnlyReservedTitlesCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
		}
	}

	private static void ConfigureRecommendedTitlesBrowse(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		LogHelper.Instance.Log("RecommendationOnPickupController.ConfigureRecommendedTitlesBrowse");
		bool includeBluray = RentalShoppingCart?.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.SourceType == ItemSourceType.Reservation && x.TitleType == TitleType.Bluray) ?? false;
		List<long> excludeProducts = new List<long>();
		_reservationValues?.HistoryTitleIds.ForEach(delegate(int x)
		{
			excludeProducts.Add(x);
		});
		excludeProducts.AddRange(RentalShoppingCart?.TitleItems.Where((IRentalShoppingCartTitleItem x) => x.SourceType == ItemSourceType.Reservation).Select((IRentalShoppingCartTitleItem x) => x.ProductId));
		List<IKioskProduct> list = GenerateRecommendations(includeBluray, excludeProducts);
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown = list;
		}
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.RowCount = 1;
		browseControlModel.ColumnCount = 6;
		browseControlModel.BrowseItemMargin = new Thickness(6.0, 18.0, 6.0, 8.0);
		browseControlModel.ShowBorderItems = false;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		browseControlModel.ShowNoResultsMessage = false;
		browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
		browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(list);
		for (int num = 0; num < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; num++)
		{
			browseControlModel.BrowseControls.Add(new DisplayProductWithTextFooterUserControl
			{
				Name = $"product_{num + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		recommendationOnPickupViewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void ConfigureShoppingCartChangeHandler()
	{
		if (ShoppingCartChangeEventHandler == null)
		{
			ShoppingCartChangeEventHandler = CurrentRentalShoppingCart_OnChange;
			RentalShoppingCart.OnChange += ShoppingCartChangeEventHandler;
		}
	}

	public static void CleanupRecommendationOnPickupView()
	{
		OrchestrationService.RemoveIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
		if (RentalShoppingCart != null && ShoppingCartChangeEventHandler != null)
		{
			try
			{
				RentalShoppingCart.OnChange -= ShoppingCartChangeEventHandler;
			}
			catch
			{
			}
			ShoppingCartChangeEventHandler = null;
		}
	}

	private static RecommendationOnPickupViewModel GetRecommendationOnPickupViewModelFromView()
	{
		FrameworkElement recommendationOnPickupViewUserControl = BaseViewController.GetViewUserControl();
		if (recommendationOnPickupViewUserControl != null)
		{
			return recommendationOnPickupViewUserControl.Dispatcher.Invoke(() => recommendationOnPickupViewUserControl?.DataContext as RecommendationOnPickupViewModel);
		}
		return null;
	}

	private static void CurrentRentalShoppingCart_OnChange(IRentalShoppingCart rentalShoppingCart)
	{
		RecommendationOnPickupViewModel recommendationOnPickupViewModelFromView = GetRecommendationOnPickupViewModelFromView();
		if (recommendationOnPickupViewModelFromView != null && !recommendationOnPickupViewModelFromView.IsViewUpdatingCart)
		{
			ConfigureShoppingCart(recommendationOnPickupViewModelFromView, clearCart: false);
			ConfigureTextAndVisibility(recommendationOnPickupViewModelFromView);
			if (recommendationOnPickupViewModelFromView?.BrowseProductControlModel?.BrowseItems != null)
			{
				UpdateDisplayProductModels(recommendationOnPickupViewModelFromView.BrowseProductControlModel.BrowseItems);
			}
		}
	}

	private static void UpdateDisplayProductModels(List<IBrowseItemModel> displayProductModels)
	{
		foreach (IBrowseItemModel displayProductModel in displayProductModels)
		{
			if (displayProductModel is DisplayProductWithTextFooterModel displayProductWithTextFooterModel)
			{
				ConfigureDisplayProductModel(displayProductWithTextFooterModel.Data as IKioskProduct, displayProductWithTextFooterModel);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		if (kioskProducts != null)
		{
			foreach (IKioskProduct kioskProduct in kioskProducts)
			{
				DisplayProductWithTextFooterModel displayProductWithTextFooterModel = new DisplayProductWithTextFooterModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					LockAspectRatio = true
				};
				ConfigureDisplayProductModel(kioskProduct, displayProductWithTextFooterModel);
				list.Add(displayProductWithTextFooterModel);
			}
		}
		return list;
	}

	private static void ConfigureDisplayProductModel(IKioskProduct eachKioskProduct, DisplayProductWithTextFooterModel browseItemModel)
	{
		browseItemModel.ConfigureForBrowse();
		ITitleProduct titleProduct = eachKioskProduct as ITitleProduct;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
		bool flag = (browseItemModel.CanAdd = rentalShoppingCart != null && !rentalShoppingCart.HasTitleItem(titleProduct.ProductId) && !(RentalShoppingCart?.IsFull ?? true));
		browseItemModel.ImageOpacity = (flag ? 1.0 : 0.4);
		browseItemModel.CornerAddButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style");
		browseItemModel.FooterTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_gray_montserrat_medium_16");
		IRentalShoppingCart rentalShoppingCart2 = RentalShoppingCart;
		if (rentalShoppingCart2 != null && rentalShoppingCart2.HasTitleItem(titleProduct.ProductId))
		{
			browseItemModel.FooterText = Resources.in_cart;
		}
		else
		{
			IPricingRecord pricingRecord = (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(titleProduct?.ProductId ?? 0))?[0];
			browseItemModel.FooterText = ((pricingRecord != null) ? $"{pricingRecord.InitialNight:c2} / {Resources.night}" : null);
		}
		BannerType bannerType = BannerType.None;
		bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
		bool hideBanner = false;
		BrowseFunctions.ConfigureBanner(browseItemModel, titleProduct, hideBanner, bannerType, BaseViewController.GetViewUserControl());
	}

	private static void BrowseProductControl_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (browseItemModel.Data is ITitleProduct titleProduct)
		{
			BrowseFunctions.AddProductToCart(new AddProductToCartParameters
			{
				TitleProduct = titleProduct,
				IsPurchase = false,
				PricingRecord = null,
				ItemSourceType = ItemSourceType.Recommendation,
				RedboxPlusBenefitId = null
			});
		}
	}

	public static List<IKioskProduct> GenerateRecommendations(bool includeBluray, List<long> excludeProducts)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		LogHelper.Instance.Log("Generating recommendation list on top 20");
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<long> list2 = new List<long> { 1139L };
			list2.AddRange(excludeProducts);
			List<IKioskProduct> products = null;
			service.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, 1093L, null, null, list2, isADAView: true, null, null, out products);
			List<IKioskProduct> list3 = new List<IKioskProduct>();
			foreach (IKioskProduct item in products)
			{
				if (item is ITitleProduct { IsAvailable: not false, InStock: not false, SellThruNew: false, DoNotRent: false, KioskProductType: not KioskProductType.MDV } titleProduct && (titleProduct.TitleType == TitleType.DVD || titleProduct.TitleType == TitleType.Bluray) && (includeBluray || titleProduct.TitleType == TitleType.DVD))
				{
					list3.Add(item);
				}
			}
			new List<IKioskProduct>();
			list3.Sort(delegate(IKioskProduct x, IKioskProduct y)
			{
				ITitleProduct titleProduct3 = x as ITitleProduct;
				ITitleProduct titleProduct4 = y as ITitleProduct;
				if (titleProduct3 == null)
				{
					return -1;
				}
				if (titleProduct4 == null)
				{
					return 1;
				}
				int num = titleProduct3.TitleType.CompareTo(titleProduct4.TitleType) * -1;
				if (num == 0)
				{
					num = titleProduct3.MerchandiseDate.CompareTo(titleProduct4.MerchandiseDate);
					if (num == 0)
					{
						num = titleProduct3.ProductId.CompareTo(titleProduct4.ProductId);
					}
				}
				return num;
			});
			list.AddRange(list3.Take(6));
		}
		foreach (IKioskProduct item2 in list)
		{
			ITitleProduct titleProduct2 = item2 as ITitleProduct;
			LogHelper.Instance.Log($"recommendation list: adding {titleProduct2?.ProductId}");
		}
		return list;
	}

	private static void ConfigureCommands(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		recommendationOnPickupViewModel.OnPickupButtonClicked = delegate
		{
			LockHelper.TryLockObject(_lockObject, delegate
			{
				if (!recommendationOnPickupViewModel.HasPickupOrPayActionExecuted)
				{
					recommendationOnPickupViewModel.HasPickupOrPayActionExecuted = true;
					BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendationOnPickupPickup");
					AddSecondaryMdvDiscsBackToShoppingCart(RentalShoppingCart, recommendationOnPickupViewModel);
					if (_reservationValues.AuthorizeAtPickup)
					{
						ReservationFunctions.InvokeAuthorizeOnPickupCallback(_reservationValues?.ReferenceNumber, cartWasModified: false);
					}
					else
					{
						ReservationFunctions.InvokeVendOnlyReservedTitlesCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
					}
				}
				else
				{
					LogHelper.Instance.Log("RecommendationOnPickupViewController.OnPickupButtonClicked Pickup button not pressed because it was disabled.");
				}
			}, "RecommendationOnPickupViewController.OnPickupButtonClicked");
		};
		recommendationOnPickupViewModel.OnPayButtonClicked = delegate
		{
			LockHelper.TryLockObject(_lockObject, delegate
			{
				if (!recommendationOnPickupViewModel.HasPickupOrPayActionExecuted)
				{
					recommendationOnPickupViewModel.HasPickupOrPayActionExecuted = true;
					BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendationOnPickupPay");
					AddSecondaryMdvDiscsBackToShoppingCart(RentalShoppingCart, recommendationOnPickupViewModel);
					if (_reservationValues.AuthorizeAtPickup)
					{
						List<long> list = new List<long>();
						foreach (IRentalShoppingCartTitleItem titleItem in RentalShoppingCart.TitleItems)
						{
							if (titleItem.SourceType == ItemSourceType.Recommendation)
							{
								list.Add(titleItem.ProductId);
							}
						}
						ReservationFunctions.InvokeAuthorizeOnPickupCallback(_reservationValues?.ReferenceNumber, cartWasModified: true);
					}
					else
					{
						ReservationFunctions.InvokeReservationAuthorizeCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
					}
				}
				else
				{
					LogHelper.Instance.Log("RecommendationOnPickupViewController.OnPayButtonClicked Pay button not pressed because it was disabled.");
				}
			}, "RecommendationOnPickupViewController.OnPayButtonClicked");
		};
		recommendationOnPickupViewModel.OnTermsAndPrivacyButtonClicked = delegate
		{
			HelpViewController.ShowTermsAndConditions();
		};
	}

	private static void ConfigureShoppingCart(RecommendationOnPickupViewModel recommendationOnPickupViewModel, bool clearCart)
	{
		if (RentalShoppingCart == null)
		{
			return;
		}
		lock (recommendationOnPickupViewModel)
		{
			try
			{
				recommendationOnPickupViewModel.IsViewUpdatingCart = true;
				PopulateShoppingCartWithReservationItems(RentalShoppingCart, clearCart, recommendationOnPickupViewModel);
				RentalShoppingCart.PickupRecommendationsShown = true;
				ConfigureShoppingCartControl(recommendationOnPickupViewModel);
			}
			finally
			{
				recommendationOnPickupViewModel.IsViewUpdatingCart = false;
			}
		}
	}

	private static void PopulateShoppingCartWithReservationItems(IRentalShoppingCart rentalShoppingCart, bool clearCart, RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		_reservationValues = ReservationFunctions.PopulateShoppingCartWithReservationItems(rentalShoppingCart, null, clearCart);
		if (_reservationValues == null)
		{
			_reservationValues = new ReservationDetails();
			LogHelper.Instance.Log("RecommendationOnPickupViewController.PopulateShoppingCartWithReservationItems reservationInfo is null.", LogEntryType.Info);
		}
		List<IRentalShoppingCartTitleItem> list = rentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem i) => i.MultiDisc && i.DiscNumber > 1).ToList();
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			rentalShoppingCart.RemoveItem(item, userRemoved: false);
		}
		recommendationOnPickupViewModel.SecondaryMdvDiscs = list;
	}

	private static void AddSecondaryMdvDiscsBackToShoppingCart(IRentalShoppingCart rentalShoppingCart, RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		lock (recommendationOnPickupViewModel)
		{
			try
			{
				recommendationOnPickupViewModel.IsViewUpdatingCart = true;
				if (recommendationOnPickupViewModel.SecondaryMdvDiscs == null)
				{
					return;
				}
				foreach (IRentalShoppingCartTitleItem secondaryMdvDisc in recommendationOnPickupViewModel.SecondaryMdvDiscs)
				{
					rentalShoppingCart.AddItem(secondaryMdvDisc);
				}
				recommendationOnPickupViewModel.SecondaryMdvDiscs = null;
			}
			finally
			{
				recommendationOnPickupViewModel.IsViewUpdatingCart = false;
			}
		}
	}

	private static void ConfigureShoppingCartControl(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		if (recommendationOnPickupViewModel != null)
		{
			BrowseControlModel browseControlModel = new BrowseControlModel();
			browseControlModel.BackgroundColor = Colors.Transparent;
			browseControlModel.ShowBorderItems = false;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
			browseControlModel.Margin = new Thickness(0.0, 0.0, 0.0, 8.0);
			browseControlModel.BrowseItemMargin = new Thickness(16.0, 12.0, 16.0, 0.0);
			browseControlModel.RowCount = 1;
			browseControlModel.ColumnCount = 5;
			for (int i = 0; i < RentalShoppingCart.MaximumCapacity; i++)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithTextFooterUserControl
				{
					Name = $"shopping_cart_product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			browseControlModel.OnCancelBrowseItemModel += ShoppingCartControl_OnCancelDisplayProductModel;
			browseControlModel.BrowseItems = CreateBrowseItemsFromCartItems();
			recommendationOnPickupViewModel.ShoppingCartControlModel = browseControlModel;
		}
		else
		{
			LogHelper.Instance.Log("RecommendationOnPickupViewController.ConfigureShoppingCartControl recommendationOnPickupViewModel is null.", LogEntryType.Info);
		}
	}

	private static void ShoppingCartControl_OnCancelDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (browseItemModel == null)
		{
			return;
		}
		IRentalShoppingCartTitleItem rentalShoppingCartItem = browseItemModel.Data as IRentalShoppingCartTitleItem;
		if (RentalShoppingCart != null && rentalShoppingCartItem != null)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = RentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == rentalShoppingCartItem.ProductId);
			if (rentalShoppingCartTitleItem != null)
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("RecommendationOnPickupShoppingCartRemoveItem");
				RentalShoppingCart.RemoveItem(rentalShoppingCartTitleItem, userRemoved: true);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromCartItems()
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(titleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct != null)
				{
					DisplayProductWithTextFooterModel displayProductWithTextFooterModel = new DisplayProductWithTextFooterModel
					{
						Name = kioskProduct.ProductName,
						Data = titleItem,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					displayProductWithTextFooterModel.ConfigureForRecommendationOnPickupCart();
					displayProductWithTextFooterModel.RecommendationOnPickupCartCancelButtonVisibility = ((titleItem.SourceType == ItemSourceType.Reservation) ? Visibility.Collapsed : Visibility.Visible);
					displayProductWithTextFooterModel.FooterTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_white_montserrat_medium_16");
					if (titleItem.SourceType == ItemSourceType.Reservation)
					{
						displayProductWithTextFooterModel.FooterText = Resources.reserved_title;
					}
					else
					{
						displayProductWithTextFooterModel.FooterText = $"{titleItem.Price:c2} / {Resources.night}";
					}
					ITitleProduct titleProduct = kioskProduct as ITitleProduct;
					BannerType bannerType = BannerType.None;
					bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
					bool hideBanner = false;
					BrowseFunctions.ConfigureBanner(displayProductWithTextFooterModel, titleProduct, hideBanner, bannerType, BaseViewController.GetViewUserControl());
					list.Add(displayProductWithTextFooterModel);
				}
				else
				{
					LogHelper.Instance.Log($"RecommendationOnPickupViewController.CreateBrowseItemsFromCartItems productId: {titleItem.ProductId} was not found", LogEntryType.Info);
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("RecommendationOnPickupViewController.CreateBrowseItemsFromCartItems - IDataService or RentalShoppingCart not found", LogEntryType.Info);
		}
		return list;
	}

	private static void ConfigureTextAndVisibility(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		if (recommendationOnPickupViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		recommendationOnPickupViewModel.AddMoreText = Resources.add_more;
		recommendationOnPickupViewModel.TermsAndPrivacyButtonText = Resources.terms_link_text;
		recommendationOnPickupViewModel.ChargedOnceText = Resources.charged_once;
		recommendationOnPickupViewModel.AgreeToTermsText = Resources.by_pressing_pay;
		int num = RentalShoppingCart?.TitleItems.Count((IRentalShoppingCartTitleItem x) => x.SourceType != ItemSourceType.Reservation) ?? 0;
		recommendationOnPickupViewModel.PayButtonText = Resources.pay_button;
		recommendationOnPickupViewModel.PayButtonVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		recommendationOnPickupViewModel.PickupButtonText = Resources.pickup;
		recommendationOnPickupViewModel.PickupButtonVisibility = ((num != 0) ? Visibility.Collapsed : Visibility.Visible);
		int num2 = RentalShoppingCart?.TitleItems.Count((IRentalShoppingCartTitleItem x) => x.TitleFamily == TitleFamily.Movies && x.SourceType == ItemSourceType.Reservation) ?? 0;
		int num3 = RentalShoppingCart?.TitleItems.Count((IRentalShoppingCartTitleItem x) => x.TitleFamily == TitleFamily.Games && x.SourceType == ItemSourceType.Reservation) ?? 0;
		int num4 = RentalShoppingCart?.TitleItems.Count((IRentalShoppingCartTitleItem x) => x.SourceType == ItemSourceType.Reservation) ?? 0;
		string format = Resources.pickup_discs;
		if (num4 == 1)
		{
			if (num2 == 1)
			{
				format = Resources.pickup_movie;
			}
			else if (num3 == 1)
			{
				format = Resources.pickup_game;
			}
		}
		else if (num2 == num4)
		{
			format = Resources.pickup_movies;
		}
		else if (num3 == num4)
		{
			format = Resources.pickup_games;
		}
		recommendationOnPickupViewModel.TitleText = string.Format(format, num4);
		bool flag = (RentalShoppingCart?.ServiceFee?.ActualAmount).GetValueOrDefault() > 0m;
		recommendationOnPickupViewModel.ServiceFeeMessage = (flag ? string.Format(Resources.service_fee_text, $"{(RentalShoppingCart?.ServiceFee.DefaultAmount ?? 0m) * 100m:f0}") : null);
		recommendationOnPickupViewModel.PricingModel.PricingLineMargin = new Thickness(0.0, 2.0, 0.0, 2.0);
		recommendationOnPickupViewModel.PricingModel.ReservedDiscsLabelText = Resources.totals_reserved_discs;
		recommendationOnPickupViewModel.PricingModel.AddedDiscsLabelText = Resources.totals_added_discs;
		recommendationOnPickupViewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
		recommendationOnPickupViewModel.PricingModel.TaxLabelText = Resources.tax_label;
		recommendationOnPickupViewModel.PricingModel.TotalLabelText = Resources.total_label;
		recommendationOnPickupViewModel.PricingModel.DiscountLabelText = Resources.totals_promo_code;
		recommendationOnPickupViewModel.PricingModel.SubtotalLineVisibility = Visibility.Collapsed;
		recommendationOnPickupViewModel.PricingModel.ServiceFeeLineVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
		string text2 = $"{((!((RentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() != 0m)) ? ((decimal?)null) : RentalShoppingCart?.Totals?.DiscountAmountUsed):C}";
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = $"-{text2}";
		}
		recommendationOnPickupViewModel.PricingModel.DiscountValueText = text2;
		recommendationOnPickupViewModel.PricingModel.DiscountLineVisibility = ((!((RentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m)) ? Visibility.Collapsed : Visibility.Visible);
		decimal num5 = RentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem x) => x.SourceType == ItemSourceType.Reservation).Sum((IRentalShoppingCartTitleItem x) => x.Price);
		recommendationOnPickupViewModel.PricingModel.ReservedDiscsValueText = $"{num5:C}";
		decimal num6 = RentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem x) => x.SourceType != ItemSourceType.Reservation).Sum((IRentalShoppingCartTitleItem x) => x.Price);
		recommendationOnPickupViewModel.PricingModel.AddedDiscsValueText = $"{num6:C}";
		recommendationOnPickupViewModel.PricingModel.ServiceFeeValueText = $"{(RentalShoppingCart?.ServiceFee?.ActualAmount).GetValueOrDefault():C}";
		recommendationOnPickupViewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
		recommendationOnPickupViewModel.PricingModel.TaxValueText = $"{RentalShoppingCart?.Totals?.TaxAmount:C}";
		recommendationOnPickupViewModel.PricingModel.TotalValueText = $"{RentalShoppingCart?.Totals?.GrandTotal:f2}";
	}
}
