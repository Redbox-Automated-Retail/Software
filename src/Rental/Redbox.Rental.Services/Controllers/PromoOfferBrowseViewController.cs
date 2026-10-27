using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PromoOfferBrowseViewController : BaseViewController
{
	private static Action<ITitleProduct> _continueAction;

	private static Action _cancelAction;

	private static Action _timeoutAction;

	private static ITitleProduct _selectedTitle;

	private static List<ITitleProduct> _availableProducts;

	private static IRentalShoppingCart _originalCartItems;

	private static string _pageNumberTemplate;

	private static int _numBrowsePages;

	private static Func<decimal, decimal> _promoPriceCalc;

	private static bool _allowMnp;

	private static bool _isPurchase;

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IPromoOfferState PromoOfferState => Redbox.Rental.Services.Session.Session.Instance.PromoOfferState;

	public static void ConfigureView(PromoOfferBrowseInfo parameters)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		PromoOfferBrowseViewModel promoOfferBrowseViewModel = GetViewModelFromView();
		if (promoOfferBrowseViewModel == null)
		{
			promoOfferBrowseViewModel = new PromoOfferBrowseViewModel();
			promoOfferBrowseViewModel.PageNumber = Math.Max(PromoOfferState.BrowsePageNumber, 1);
			promoOfferBrowseViewModel.HeaderText = parameters.OfferTitleText;
			promoOfferBrowseViewModel.MiniCartText = "";
			_availableProducts = parameters.applicableProducts;
			_selectedTitle = PromoOfferState.SelectedProduct;
			_allowMnp = parameters.AllowMnp;
			_isPurchase = parameters.IsPurchase;
			_promoPriceCalc = parameters.DiscountedPriceCalc;
			ConfigureShoppingCartChangeHandler();
			ConfigureAvailableTitlesBrowse(promoOfferBrowseViewModel);
			ConfigureMiniCartControl(promoOfferBrowseViewModel);
			promoOfferBrowseViewModel.CancelButtonText = Resources.skip_offer_button_text;
			promoOfferBrowseViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CancelCommand_Execute(parameters.CancelAction);
			}, (Func<bool>)null);
			promoOfferBrowseViewModel.ContinueButtonText = Resources.continue_button_mixed_case;
			promoOfferBrowseViewModel.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate
			{
				ExecuteContinue();
			}, () => ContinueCanExecute());
			_cancelAction = parameters.CancelAction;
			_continueAction = parameters.ContinueAction;
			_timeoutAction = parameters.TimeoutAction;
			ConfigureTTS(promoOfferBrowseViewModel);
			viewUserControl.DataContext = promoOfferBrowseViewModel;
		}
		PromoOfferState.BrowsePageNumber = promoOfferBrowseViewModel.PageNumber;
		CommandManager.InvalidateRequerySuggested();
	}

	private static void ConfigureShoppingCartChangeHandler()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		_originalCartItems = service.CurrentRentalShoppingCart.ShallowClone();
		service.CurrentRentalShoppingCart.OnChange += CurrentRentalShoppingCart_OnChange;
	}

	private static void CurrentRentalShoppingCart_OnChange(IRentalShoppingCart rentalShoppingCart)
	{
		ITitleProduct titleProduct = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem i) => !_originalCartItems.TitleItems.Any((IRentalShoppingCartTitleItem o) => o.ProductId == i.ProductId)).FirstOrDefault()?.TitleProduct;
		if (titleProduct != null)
		{
			_selectedTitle = titleProduct;
			PromoOfferState.SelectedProduct = titleProduct;
		}
		UpdateBrowseDisplay();
	}

	private static void UpdateBrowseDisplay()
	{
		PromoOfferBrowseViewModel viewModelFromView = GetViewModelFromView();
		ConfigureMiniCartControl(viewModelFromView);
		ConfigureAvailableTitlesBrowse(viewModelFromView);
	}

	internal static void LeaveView()
	{
		StopAndRemoveTimer();
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart.OnChange -= CurrentRentalShoppingCart_OnChange;
		PromoOfferBrowseViewModel viewModelFromView = GetViewModelFromView();
		if (viewModelFromView != null && viewModelFromView.BrowseProductControlModel != null)
		{
			viewModelFromView.BrowseProductControlModel.OnAddBrowseItemModel -= BrowseProductControl_OnAddBrowseItemModel;
			viewModelFromView.BrowseProductControlModel.OnBrowseItemModelSelected -= BrowseProductControl_OnAddBrowseItemModel;
			viewModelFromView.BrowseProductControlModel.OnPageChanged -= BrowseProductControl_OnPageChanged;
		}
	}

	private static void ConfigureAvailableTitlesBrowse(PromoOfferBrowseViewModel viewModel)
	{
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.RowCount = 2;
		browseControlModel.ColumnCount = (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 5 : 6);
		browseControlModel.BrowseItemMargin = (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? new Thickness(0.0, 32.0, 0.0, 10.0) : new Thickness(0.0, 48.0, 0.0, 15.0));
		browseControlModel.ShowBorderItems = true;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		browseControlModel.ShowNoResultsMessage = false;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = (_isPurchase ? BrowseViewFilter.ForSale : BrowseViewFilter.All);
		List<IBrowseItemModel> list = (browseControlModel.BrowseItems = CreateBrowseItemsFromTitleProducts(_availableProducts));
		int count = list.Count;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels = browseControlModel.BrowseItems;
		_pageNumberTemplate = Resources.promo_browse_page_number_template;
		_numBrowsePages = count / 10 + ((count % 10 > 0) ? 1 : 0);
		viewModel.PageNumber = Math.Max(1, PromoOfferState.BrowsePageNumber);
		PromoOfferState.BrowsePageNumber = viewModel.PageNumber;
		viewModel.PageNumberText = string.Format(_pageNumberTemplate, viewModel.PageNumber, _numBrowsePages);
		browseControlModel.SetPageNumber(viewModel.PageNumber, PageChangeSource.None);
		for (int i = 0; i < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; i++)
		{
			if (_isPurchase)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithPriceBrowseUserControl
				{
					Name = $"product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			else
			{
				browseControlModel.BrowseControls.Add(new DisplayProductBrowseUserControl
				{
					Name = $"product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
		}
		browseControlModel.OnNotifyBrowseItemsShown += BrowseProductControl_OnNotifyBrowseItemsShown;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddBrowseItemModel;
		browseControlModel.OnBrowseItemModelSelected += BrowseProductControl_OnAddBrowseItemModel;
		browseControlModel.OnPageChanged += BrowseProductControl_OnPageChanged;
		viewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void BrowseProductControl_OnPageChanged(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
	{
		BaseViewController.ProcessWPFHit();
		PromoOfferState.BrowsePageNumber = newPageNumber;
		PromoOfferBrowseViewModel viewModelFromView = GetViewModelFromView();
		viewModelFromView.PageNumberText = string.Format(_pageNumberTemplate, Math.Max(1, newPageNumber), _numBrowsePages);
		viewModelFromView.PageNumber = newPageNumber;
		if (BaseViewController.IsADAMode)
		{
			viewModelFromView.AdaMiniCartControlModel.SetPageNumber(newPageNumber, PageChangeSource.None);
		}
	}

	private static void BrowseProductControl_OnAddBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		ITitleProduct titleProduct = browseItemModel.Data as ITitleProduct;
		if (GetViewModelFromView() == null)
		{
			return;
		}
		if (_selectedTitle != null)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			PromoOfferState.SelectedProduct = null;
			service.RemoveItemFromRentalShoppingCartByProductId(service.CurrentRentalShoppingCart.ShoppingSessionId, _selectedTitle.ProductId, userRemoved: true);
		}
		switch (titleProduct.KioskProductType)
		{
		case KioskProductType.Title:
		case KioskProductType.MDV:
			AddProductCheckMnpView(titleProduct);
			break;
		case KioskProductType.TitleRollup:
		{
			TitleRollupProduct titleRollupProduct = titleProduct as TitleRollupProduct;
			if (!BrowseFunctions.IsPurchase && titleRollupProduct.KioskProducts.Count == 1)
			{
				AddProductCheckMnpView(titleProduct);
				break;
			}
			BrowseFunctions.ShowRentalPopupView(titleRollupProduct, _isPurchase, delegate
			{
				CancelBrowseSelection();
			}, _allowMnp, _promoPriceCalc);
			break;
		}
		case KioskProductType.None:
			break;
		}
	}

	private static void AddProductCheckMnpView(ITitleProduct titleProduct)
	{
		if (_allowMnp && titleProduct.HasMultiNightPrice && !BrowseFunctions.IsPurchase)
		{
			BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct, "promo_offer_browse_view");
		}
		else
		{
			BrowseFunctions.AddProductToCart(titleProduct, _isPurchase);
		}
	}

	private static void CancelBrowseSelection()
	{
		if (_selectedTitle != null)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			IRentalShoppingCart currentRentalShoppingCart = service.CurrentRentalShoppingCart;
			if (!currentRentalShoppingCart.HasTitleItem(_selectedTitle.ProductId))
			{
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = service.CreateRentalShoppingCartItemFromTitleProduct(_selectedTitle);
				rentalShoppingCartTitleItem.SourceType = ItemSourceType.Browse;
				rentalShoppingCartTitleItem.Action = RentalShoppingCartItemAction.rental;
				rentalShoppingCartTitleItem.Quantity = 1m;
				rentalShoppingCartTitleItem.Status = LineItemStatus.Pending;
				rentalShoppingCartTitleItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(_selectedTitle);
				rentalShoppingCartTitleItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(_selectedTitle);
				rentalShoppingCartTitleItem.PersonalizationTitleTags = PersonalizationFunctions.GetPersonalizationTitleTags(_selectedTitle);
				currentRentalShoppingCart.AddItem(rentalShoppingCartTitleItem);
				UpdateBrowseDisplay();
			}
		}
	}

	private static PromoOfferBrowseViewModel GetViewModelFromView()
	{
		FrameworkElement browseViewUserControl = BaseViewController.GetViewUserControl();
		if (browseViewUserControl != null)
		{
			return browseViewUserControl.Dispatcher.Invoke(() => browseViewUserControl?.DataContext as PromoOfferBrowseViewModel);
		}
		return null;
	}

	private static void ExecuteContinue()
	{
		BaseViewController.ProcessWPFHit();
		if (_selectedTitle != null)
		{
			OrchestrationService.ViewGoBackSafe("PromoOfferBrowse_item_selected", popOnly: false);
			_continueAction(_selectedTitle);
		}
	}

	private static bool ContinueCanExecute()
	{
		return _selectedTitle != null;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromTitleProducts(List<ITitleProduct> products, bool isForMiniCart = false, bool isAdaMiniCart = false)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		if (ServiceLocator.Instance.GetService<IDataService>() != null)
		{
			foreach (ITitleProduct product in products)
			{
				if (product == null)
				{
					continue;
				}
				ITitleRollupProduct titleRollupProduct = ((product.KioskProductType == KioskProductType.TitleRollup) ? (product as ITitleRollupProduct) : null);
				ITitleProduct titleProduct = ((product.KioskProductType == KioskProductType.Title) ? product : null);
				if ((titleRollupProduct == null || !titleRollupProduct.IsRentableAndInStock || titleRollupProduct.IsEmptyCase) && (titleProduct == null || !titleProduct.IsRentableAndInStock || titleProduct.IsEmptyCase))
				{
					continue;
				}
				DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
				bool flag = IsProductSelected(product, displayProductModelConstants);
				DisplayProductModel displayProductModel;
				if (!isForMiniCart && BrowseState.BrowseMode == BrowseViewFilter.ForSale && (BrowseState.TitleFamily == TitleFamily.Movies || BrowseState.TitleFamily == TitleFamily.Games))
				{
					string purchasePriceText = BrowseFunctions.GetPurchasePriceText(product);
					string purchasePriceText2 = BrowseFunctions.GetPurchasePriceText(product, _promoPriceCalc);
					displayProductModel = new DisplayProductWithPriceModel
					{
						Name = product.ProductName,
						Data = product,
						Image = OrchestrationService.GetProductBitmapImage(product),
						LockAspectRatio = true,
						Header = product.ProductName,
						DisplayProductModelConstants = displayProductModelConstants,
						StrikethroughPriceText = purchasePriceText,
						PriceText = purchasePriceText2,
						PriceTypeText = (string.IsNullOrEmpty(purchasePriceText) ? "" : (Resources.browse_view_rent_and_buy_purchase_text + ":"))
					};
				}
				else
				{
					displayProductModel = new DisplayProductModel
					{
						Name = product.ProductName,
						Data = product,
						Image = OrchestrationService.GetProductBitmapImage(product),
						Header = product.ProductName,
						DisplayProductModelConstants = displayProductModelConstants,
						AddButtonStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style")
					};
				}
				if (isForMiniCart)
				{
					if (isAdaMiniCart)
					{
						displayProductModel.ConfigureForADAMiniCart();
						displayProductModel.CanAdd = product.IsAvailable && !flag;
					}
					else
					{
						displayProductModel.ConfigureForMiniCart();
						displayProductModel.MiniCartCancelButtonVisibility = Visibility.Visible;
					}
				}
				else
				{
					displayProductModel.ConfigureForRecommendedTitlesPopup();
					displayProductModel.FlagVisibility = Visibility.Visible;
					if (!product.IsAvailable | flag)
					{
						displayProductModel.ImageOpacity = 0.4;
						displayProductModel.CornerAddButtonVisibility = Visibility.Collapsed;
						displayProductModel.AddButtonPlusSignVisibility = Visibility.Collapsed;
						displayProductModel.CanAdd = false;
					}
					else
					{
						displayProductModel.ImageOpacity = 1.0;
					}
					BannerType productBannerType = BrowseFunctions.GetProductBannerType(titleProduct);
					BrowseFunctions.ConfigureBanner(displayProductModel, product, hideBanner: false, productBannerType, BaseViewController.GetViewUserControl());
				}
				displayProductModel.ADAMiniCartAddButtonVisibility = ((!isAdaMiniCart) ? Visibility.Collapsed : Visibility.Visible);
				displayProductModel.AddButtonPlusSignVisibility = (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Visible);
				displayProductModel.AddButtonTextVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
				displayProductModel.AddButtonStyle = (BaseViewController.IsADAMode ? WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "rubine_white_border_button_style") : WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style"));
				if (!flag && !isAdaMiniCart)
				{
					ProductFlags productFlag = BrowseFunctions.GetProductFlag(product);
					BrowseFunctions.ConfigureProductFlag(displayProductModel, productFlag, hideFlag: false, BaseViewController.GetViewUserControl(), product, displayProductModelConstants);
				}
				displayProductModel.ButtonData.Clear();
				list.Add(displayProductModel);
			}
		}
		return list.ToList();
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

	private static void CancelCommand_Execute(Action cancelAction)
	{
		OrchestrationService.ViewGoBackSafe("PromoOfferBrowse_cancel", popOnly: false);
		OrchestrationService.ViewShow("OfferAbandonConfirmView", new OfferAbandonConfirmInfo
		{
			IsInCart = true,
			ContinueAction = cancelAction
		});
	}

	private static void ConfigureMiniCartControl(PromoOfferBrowseViewModel browseViewModel)
	{
		if (browseViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			BrowseControlModel browseControlModel = new BrowseControlModel();
			browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddBrowseItemModel;
			browseControlModel.BackgroundColor = Colors.Transparent;
			browseControlModel.ShowBorderItems = false;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
			browseControlModel.Margin = (BaseViewController.IsADAMode ? new Thickness(0.0, 0.0, 0.0, 20.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
			browseControlModel.BrowseItemMargin = new Thickness(10.0, 10.0, 10.0, 10.0);
			browseControlModel.RowCount = ((!BaseViewController.IsADAMode) ? 1 : 2);
			browseControlModel.ColumnCount = ((!BaseViewController.IsADAMode) ? 1 : (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 5 : 6));
			int num = ((!BaseViewController.IsADAMode) ? 1 : (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.Show10Titles ? 10 : 12));
			for (int i = 0; i < num; i++)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithCommandsUserControl
				{
					Name = $"minicart_product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			browseControlModel.OnCancelBrowseItemModel += MiniCartControl_OnCancelDisplayProductModel;
			if (Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible)
			{
				browseControlModel.SetPageNumber(browseViewModel.PageNumber, PageChangeSource.None);
				browseControlModel.BrowseItems = CreateBrowseItemsFromTitleProducts(_availableProducts, isForMiniCart: true, isAdaMiniCart: true);
				browseViewModel.AdaMiniCartControlModel = browseControlModel;
			}
			else
			{
				browseControlModel.BrowseItems = CreateBrowseItemsFromTitleProducts(new List<ITitleProduct> { _selectedTitle }, isForMiniCart: true);
				browseViewModel.MiniCartControlModel = browseControlModel;
			}
			if (_selectedTitle != null)
			{
				browseViewModel.MiniCartText = string.Format(Resources.promo_browse_item_added_text, GetFormatText(_selectedTitle));
			}
			else
			{
				browseViewModel.MiniCartText = string.Empty;
			}
			browseViewModel.MiniCartVisibility = ((BaseViewController.IsADAMode || _selectedTitle == null) ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.AdaMiniCartVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.EmptyCartSpacesVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.AddToADACartLabelText = Resources.add_to_cart_ada_label;
			browseViewModel.AddToADACartLabelVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static string GetFormatText(ITitleProduct product)
	{
		string empty = string.Empty;
		if (product.TitleFamily == TitleFamily.Games)
		{
			return Resources.game;
		}
		return product.TitleType switch
		{
			TitleType.Bluray => Resources.blu_ray, 
			TitleType.DVD => Resources.dvd, 
			_ => Resources.disc, 
		};
	}

	private static void MiniCartControl_OnCancelDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (browseItemModel != null)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			service.RemoveItemFromRentalShoppingCartByProductId(service.CurrentRentalShoppingCart.ShoppingSessionId, _selectedTitle.ProductId, userRemoved: true);
			_selectedTitle = null;
			PromoOfferState.SelectedProduct = null;
			PromoOfferBrowseViewModel viewModelFromView = GetViewModelFromView();
			if (viewModelFromView != null)
			{
				ConfigureMiniCartControl(viewModelFromView);
				ConfigureAvailableTitlesBrowse(viewModelFromView);
			}
		}
	}

	private static void BrowseProductControl_OnNotifyBrowseItemsShown(List<IBrowseItemModel> browseItemModels, object parameter)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		List<IBrowseItemModel> list2 = new List<IBrowseItemModel>();
		if (browseItemModels != null)
		{
			foreach (IBrowseItemModel browseItemModel in browseItemModels)
			{
				if (browseItemModel?.Data is IKioskProduct item)
				{
					list.Add(item);
				}
				if (!browseItemModel.IsBorderItem)
				{
					list2.Add(browseItemModel);
				}
			}
		}
		if (!AreProductListsIdentical(list, PromoOfferState?.BrowseProductsShown))
		{
			_ = list?.Count ?? 0;
			_ = 0;
			if (PromoOfferState != null)
			{
				PromoOfferState.BrowseProductsShown = list;
				PromoOfferState.TTSBrowseItemModelsShown = list2;
			}
		}
	}

	private static bool AreProductListsIdentical(List<IKioskProduct> shownKioskProducts, List<IKioskProduct> browseProductsShown)
	{
		bool flag = shownKioskProducts?.Count == browseProductsShown?.Count;
		if (flag)
		{
			for (int i = 0; i < shownKioskProducts.Count; i++)
			{
				flag = shownKioskProducts[i] is ITitleProduct && browseProductsShown[i] is ITitleProduct && (shownKioskProducts[i] as ITitleProduct)?.ProductId == (browseProductsShown[i] as ITitleProduct).ProductId;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private static bool IsProductSelected(ITitleProduct titleProduct, DisplayProductModelConstants displayProductModelConstants)
	{
		bool flag = true;
		if (titleProduct != null)
		{
			if (titleProduct is TitleRollupProduct titleRollupProduct)
			{
				foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
				{
					if (kioskProduct.IsRentable)
					{
						bool flag2 = (!(displayProductModelConstants.OutOfStockTitleDetailsABTestVersion == "B") || kioskProduct.TitleFamily != TitleFamily.Movies) && !kioskProduct.InStock;
						flag = flag && (flag2 || (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.HasTitleItem(kioskProduct.ProductId) ?? true));
					}
				}
			}
			else
			{
				flag = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.HasTitleItem(titleProduct.ProductId) ?? true;
			}
		}
		return flag;
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		int timeOut = ((service2 != null && service2.TTSEnabled) ? 120000 : 25000);
		IIdleTimer obj = service?.CreateIdleTimer("PromoOfferBrowseViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			MessagePopupViewController.ShowOfferTimeoutMessage(delegate
			{
				_timeoutAction();
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("PromoOfferBrowseViewController time out", popOnly: false);
				_cancelAction();
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("PromoOfferBrowseViewController time out", popOnly: false);
			});
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("PromoOfferBrowseViewTimer"))?.Stop();
		service?.RemoveIdleTimer("PromoOfferBrowseViewTimer");
	}

	private static void ConfigureTTS(PromoOfferBrowseViewModel browseViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			browseViewModel.OnGetSpeechControl = () => PromoOfferBrowseViewTextToSpeech.GetSpeechControl(GetViewModelFromView);
		}
	}
}
