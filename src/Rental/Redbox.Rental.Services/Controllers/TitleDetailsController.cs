using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class TitleDetailsController : BaseViewController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance?.BrowseState;

	private static IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private static bool HasActiveRedboxPlusSubscription => (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession)?.RedboxPlusSession?.HasActiveSubscription == true;

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			TitleDetailModel titleDetailModel = (TitleDetailModel)(viewUserControl.DataContext = GetModel());
			CommandManager.InvalidateRequerySuggested();
			ConfigureTTS();
			BaseViewController.StartIdleTimer();
			StartAdImpression(titleDetailModel.BrowseProductControlModel);
		}
	}

	private static TitleDetailModel GetModel()
	{
		TitleDetailModel titleDetailModel = new TitleDetailModel();
		titleDetailModel.BackgroundImageVisibility = Visibility.Collapsed;
		titleDetailModel.BrowseProductControlModel = ConfigureBrowseControl();
		titleDetailModel.BuyButtonHit += OnBuyButtonHit;
		titleDetailModel.RentProduct += OnRentProduct;
		titleDetailModel.MoreInfoHit += OnMoreInfoHit;
		titleDetailModel.GoBackHit += OnGoBack;
		titleDetailModel.SeeFullDetails += OnSeeFullDetails;
		titleDetailModel.RedboxPlusInfo += OnRedboxPlusInfo;
		titleDetailModel.DualInStockLearnMore += OnDualInStockLearnMore;
		titleDetailModel.BackText = Resources.back_button.ToUpper();
		return titleDetailModel;
	}

	private static void ConfigureTTS()
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		TitleDetailModel titleDetailModel = viewUserControl.DataContext as TitleDetailModel;
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			titleDetailModel.OnGetSpeechControl = () => TitleDetailViewTextToSpeech.GetSpeechControl(GetModelFromView);
		}
	}

	private static TitleDetailModel GetModelFromView()
	{
		return BaseViewController.GetViewUserControl()?.DataContext as TitleDetailModel;
	}

	private static int GetPageNumber(BrowseControlModel browseControlModel)
	{
		int result = -1;
		IKioskProduct selectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		if (browseControlModel?.BrowseItems != null)
		{
			result = browseControlModel.BrowseItems.FindIndex((IBrowseItemModel x) => (!BaseViewController.IsADAMode || !(selectedProduct is TitleRollupProduct)) ? (x.Data == selectedProduct) : TitleRollupUtilities.IsAdaDefaultTitle((ITitleProduct)x.Data, (TitleRollupProduct)selectedProduct)) + 1;
		}
		return result;
	}

	private static BrowseControlModel ConfigureBrowseControl()
	{
		OrchestrationService.SetCurrentUICulture();
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.BackgroundColor = Brushes.Transparent.Color;
		browseProductControl.PageNumberVisibility = Visibility.Collapsed;
		browseProductControl.ShowPaddles = true;
		browseProductControl.ShowNoResultsMessage = false;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		browseProductControl.ShowBorderItems = false;
		browseProductControl.OnAddBrowseItemModel += OnBuyButtonHit;
		browseProductControl.UseFullPageWidth = true;
		browseProductControl.BrowseItemMargin = new Thickness(0.0, 0.0, 0.0, 0.0);
		browseProductControl.RowCount = 1;
		browseProductControl.ColumnCount = 1;
		browseProductControl.BrowseItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels?.Where((IBrowseItemModel x) => x is DisplayProductModel).ToList() ?? new List<IBrowseItemModel>();
		IKioskProduct browseSelectionSelectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		_ = BaseViewController.IsADAMode;
		int pageNumber = GetPageNumber(browseProductControl);
		ConfigurePage(browseProductControl, pageNumber - 1, loadAd: false, loadMoreLikeThis: false);
		ConfigurePage(browseProductControl, pageNumber, loadAd: true, loadMoreLikeThis: true);
		ConfigurePage(browseProductControl, pageNumber + 1, loadAd: false, loadMoreLikeThis: false);
		browseProductControl.SetPageNumber(pageNumber, PageChangeSource.None);
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageSource)
		{
			IAd ad = (BaseViewController.ApplicationState.IsADAAccessible ? null : ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.TitleDetail));
			BrowseState.BrowseSelectionSelectedProduct = browseProductControl.BrowseItems[newPageNumber - 1].Data as IKioskProduct;
			RecordAdImpression(browseProductControl, prevPageNumber);
			if (newPageNumber > prevPageNumber)
			{
				ConfigurePage(browseProductControl, newPageNumber + 1, loadAd: false, loadMoreLikeThis: false);
			}
			else if (newPageNumber < prevPageNumber)
			{
				ConfigurePage(browseProductControl, newPageNumber - 1, loadAd: false, loadMoreLikeThis: false);
			}
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent((prevPageNumber < newPageNumber) ? "TitleDetailsPageNext" : "TitleDetailsPageBack");
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
			ConfigureTTS();
			PopulateModelAdImage(GetDisplayProductModel(browseProductControl, newPageNumber), ad);
			StartAdImpression(browseProductControl, newPageNumber);
		};
		if (browseSelectionSelectedProduct is ITitleProduct)
		{
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddData(ProductData.ToProductData(browseSelectionSelectedProduct as ITitleProduct));
		}
		if (browseProductControl.BrowseItems.FirstOrDefault((IBrowseItemModel x) => x.Data == Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct) is DisplayProductModel displayProductModel)
		{
			displayProductModel.ComingSoonButtonText = Resources.title_detail_coming_soon_button_text;
		}
		browseProductControl.BrowseControls = new List<Control>();
		browseProductControl.BrowseControls.Add(new TitleDetailDisplayProductWithMoreLikeThisUserControl());
		return browseProductControl;
	}

	private static void ConfigurePage(BrowseControlModel browseControlModel, int pageNumber, bool loadAd, bool loadMoreLikeThis)
	{
		if (browseControlModel?.BrowseItems != null && pageNumber >= 1 && pageNumber <= browseControlModel.BrowseItems.Count && browseControlModel.BrowseItems[pageNumber - 1] is DisplayProductModel displayProductModel)
		{
			BrowseViewController.ConfigureDisplayProductModel(displayProductModel.Data as IKioskProduct, displayProductModel, isADAMiniCart: false, displayProductModel.DisplayProductModelConstants);
			displayProductModel?.ConfigureForTitleDetail();
			PopulateTitleDetailProperties(displayProductModel, loadAd, loadMoreLikeThis);
		}
	}

	private static void OnGoBack()
	{
		GoBack("go_back");
	}

	private static void OnBuyButtonHit(IBrowseItemModel displayProductModel, object commandType)
	{
		IKioskProduct kioskProduct = displayProductModel.Data as IKioskProduct;
		if (kioskProduct is TitleRollupProduct { IsEmptyCase: false } titleRollupProduct)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsBuy");
			BrowseFunctions.ShowPurchasePopupView(titleRollupProduct, isPurchase: true);
		}
		else
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsBuy")?.AddData(ProductData.ToProductData(titleProduct));
			BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
		}
	}

	private static void OnMoreInfoHit(IBrowseItemModel model)
	{
		IComingSoonOptinInfo comingSoonOptinInfo = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsSeeRelatedCollection");
		ITitleProduct titleProduct = model?.Data as ITitleProduct;
		if (titleProduct == null)
		{
			return;
		}
		OrchestrationService.ViewShow("keyboard_email_coming_soon_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = comingSoonOptinInfo.EmailAddress,
			CancelAction = delegate
			{
				ViewService?.GoBack("go back", popOnly: false);
			},
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ContinueAction = delegate(string keyboardText)
			{
				if (comingSoonOptinInfo != null && ViewService != null)
				{
					comingSoonOptinInfo.EmailAddress = keyboardText;
					comingSoonOptinInfo.AddProduct(titleProduct.ProductId);
					ViewService.UnwindTo("browse_view", discard: true, "KeyboardView: KeyboardMode.EMAIL_COMING_SOON  - NEXT button");
				}
			}
		});
	}

	public static void OnRentProduct(IBrowseItemModel displayProductModel, object commandType, TitleType titleType, bool fromOosSelection = false)
	{
		if (!(displayProductModel?.Data is ITitleProduct titleProduct))
		{
			return;
		}
		ITitleProduct titleProduct2 = ((titleProduct is TitleRollupProduct) ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == titleType) : titleProduct);
		if (!fromOosSelection)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsRentButton")?.AddData(ProductData.ToProductData(titleProduct2));
		}
		if (titleProduct2.InStock | fromOosSelection)
		{
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if ((titleProduct2.HasMultiNightPrice || (titleProduct2.IsSellableAndInStock && (titleProduct2.IsMovie || titleProduct2.IsGame))) && !flag)
			{
				BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct2, "product_detail_view");
			}
			else
			{
				AddToCart(titleProduct2, isPurchase: false);
			}
		}
		else
		{
			ShowOutOfStockOptionsDialog(displayProductModel, titleProduct, titleType, commandType);
		}
	}

	private static void ShowOutOfStockOptionsDialog(IBrowseItemModel displayProductModel, ITitleProduct kioskProduct, TitleType titleType, object commandType)
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
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockCancel");
			GoBack("OutOfStockDialogCancel");
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
					BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockAddFormat");
					OnRentProduct(displayProductModel, commandType, inStockFormat.TitleType, fromOosSelection: true);
				}, (Func<bool>)null));
				observableCollection.Add(item);
			}
		}
		observableCollection.Add(new ButtonData(Resources.related_movies, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockRelatedMovies");
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
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsReserveOnlineRentBack");
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogBack", popOnly: false);
		}, delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsReserveOnlineRentGotit");
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
		});
	}

	private static void AddToCart(IKioskProduct product, bool isPurchase)
	{
		BrowseFunctions.AddProductToCart((ITitleProduct)product, isPurchase);
	}

	private static void GoBack(string reason, bool popOnly = false)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
		service.GoBack(reason, popOnly: false);
	}

	public static void OnSeeFullDetails(IBrowseItemModel browseItemModel)
	{
		if (browseItemModel is DisplayProductModel displayProductModel)
		{
			OrchestrationService.ViewShow("title_details_full_details_popup_view", new TitleDetailsFullDetailsPopupViewInfo
			{
				DisplayProductModel = displayProductModel
			});
		}
	}

	public static void OnRedboxPlusInfo()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsRedboxPlusInfo");
		OrchestrationService.ViewShow("redbox_plus_info_popup_view");
	}

	public static void OnDualInStockLearnMore()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsDualInStockLearnMore");
		MessagePopupViewController.ShowDualInStockLearnMorePopup(delegate
		{
			OrchestrationService.ViewGoBackSafe("DualInStockLearnMorePopupTimeout", popOnly: false);
		}, delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsDualInStockLearnMoreClose");
			OrchestrationService.ViewGoBackSafe("DualInStockLearnMorePopupClose", popOnly: false);
		});
	}

	public static void PopulateTitleDetailProperties(DisplayProductModel displayProductModel, bool loadAd, bool loadMoreLikeThis)
	{
		if (displayProductModel.TitleDetailPropertiesInitialized)
		{
			return;
		}
		displayProductModel.TitleDetailPropertiesInitialized = true;
		ITitleProduct titleProduct = displayProductModel.Data as ITitleProduct;
		RatingsProperty value = null;
		RatingsProperties.Properties.TryGetValue((Ratings)(titleProduct?.RatingId).GetValueOrDefault(), out value);
		TitleTypeProperty value2 = null;
		string format = null;
		if (titleProduct != null)
		{
			_ = titleProduct.TitleType;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value2);
			format = value2?.GetName(Resources.ResourceManager);
		}
		string ratingLabel = string.Empty;
		string ratingDescription = string.Empty;
		Visibility ratingVisibility = Visibility.Visible;
		if (value != null && !titleProduct.IsEmptyCase)
		{
			if (titleProduct.TitleFamily == TitleFamily.Games)
			{
				ratingLabel = "ESRB Rating:";
				ratingDescription = $"{value.Name} - {value.Description}";
			}
			else if (titleProduct.Genres.Contains(1017L))
			{
				ratingLabel = "TV Rating:";
				ratingDescription = value.Name;
			}
			else
			{
				ratingLabel = "MPAA Rating:";
				ratingDescription = ((value.Code == Ratings.NR.ToString()) ? "Not rated by the MPAA" : value.Name);
			}
		}
		else
		{
			ratingVisibility = Visibility.Collapsed;
		}
		IList<IPricingRecord> list = displayProductModel.DisplayProductModelConstants.PricingService?.GetProductPricingRecords(titleProduct.ProductId);
		bool flag = list != null && list.Count > 1;
		IRentalShoppingCart rentalShoppingCart = displayProductModel.DisplayProductModelConstants.RentalShoppingCartService?.CurrentRentalShoppingCart;
		bool flag2 = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		int num = (int)(((rentalShoppingCart != null && rentalShoppingCart.ServiceFeeEnabled) ? rentalShoppingCart.DefaultServiceFee : 0m) * 100m);
		DateTime dateTime = DateTime.Today.Add(Redbox.Rental.Services.Session.Session.Instance?.RentalReturnTime ?? TimeSpan.FromHours(21.0));
		displayProductModel.Description = titleProduct.ProductDescription;
		displayProductModel.Rating = value?.Name;
		displayProductModel.Format = format;
		displayProductModel.RunningTime = titleProduct?.RunningTime;
		displayProductModel.ReleaseYear = titleProduct?.NationalStreetDate?.Year.ToString();
		displayProductModel.CCSupported = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || titleProduct == null || !titleProduct.ClosedCaptioned) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.Starring = string.Join(", ", titleProduct?.Starring ?? new List<string>());
		displayProductModel.StarringLabel = displayProductModel.DisplayProductModelConstants?.StarringLabel;
		displayProductModel.SeeFullDetailsText = Resources.title_details_see_full_details_text;
		displayProductModel.Genres = GetGenreText(titleProduct?.Genres);
		displayProductModel.GenresLabel = displayProductModel.DisplayProductModelConstants?.GenresLabel;
		displayProductModel.DirectedBy = string.Join(", ", titleProduct?.Directors ?? new List<string>());
		displayProductModel.OneNightPrice = GetPriceText(titleProduct, 0, displayProductModel.DisplayProductModelConstants);
		displayProductModel.OneNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightPriceLabel : null);
		displayProductModel.MultiNightPrice = GetPriceText(titleProduct, 1, displayProductModel.DisplayProductModelConstants);
		displayProductModel.MultiNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightPriceLabel : null);
		displayProductModel.MultiNightPriceVisibility = ((!flag || flag2 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
		displayProductModel.RatingLabel = ratingLabel;
		displayProductModel.RatingDescription = ratingDescription;
		displayProductModel.RatingVisibility = ratingVisibility;
		displayProductModel.OneNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 0, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.OneNightReturnTimeWithText = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? string.Format(Resources.title_details_return_time_text, dateTime.ToString("h:mm tt"), DateTime.Now.AddDays(1.0).ToShortDateString()) : null);
		displayProductModel.OneNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 1, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.MultiNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTimeVisibility = ((!flag || flag2 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.DealText = GetDealText(titleProduct);
		displayProductModel.DealTextVisibility = ((!titleProduct.HasDeal) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.RunningTimeVisibility = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || string.IsNullOrEmpty(titleProduct?.RunningTime)) ? Visibility.Collapsed : Visibility.Visible);
		int releaseYearVisibility;
		if (titleProduct != null && !titleProduct.IsEmptyCase && titleProduct != null)
		{
			DateTime? nationalStreetDate = titleProduct.NationalStreetDate;
			if (nationalStreetDate.HasValue)
			{
				_ = nationalStreetDate.GetValueOrDefault().Year;
				if (true)
				{
					releaseYearVisibility = 0;
					goto IL_0571;
				}
			}
		}
		releaseYearVisibility = 2;
		goto IL_0571;
		IL_0571:
		displayProductModel.ReleaseYearVisibility = (Visibility)releaseYearVisibility;
		displayProductModel.RatingBorderThickness = ((displayProductModel.RunningTimeVisibility == Visibility.Visible || displayProductModel.ReleaseYearVisibility == Visibility.Visible || displayProductModel.CCSupported == Visibility.Visible) ? new Thickness(0.0, 0.0, 1.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
		displayProductModel.RunningTimeBorderThickness = ((displayProductModel.ReleaseYearVisibility == Visibility.Visible || displayProductModel.CCSupported == Visibility.Visible) ? new Thickness(0.0, 0.0, 1.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
		displayProductModel.ReleaseYearBorderThickness = ((displayProductModel.CCSupported == Visibility.Visible) ? new Thickness(0.0, 0.0, 1.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
		displayProductModel.RedboxPlusInfoText = Resources.redbox_plus_info_button_text;
		displayProductModel.RedboxPlusInfoVisibility = ((BaseViewController.ApplicationState.IsADAAccessible || !displayProductModel.DisplayProductModelConstants.EnableRedboxPlus || !HasActiveRedboxPlusSubscription || titleProduct == null || !titleProduct.IsRedboxPlusEligible) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.RedboxPlusInfoADAVisibility = ((!BaseViewController.ApplicationState.IsADAAccessible || !displayProductModel.DisplayProductModelConstants.EnableRedboxPlus || !HasActiveRedboxPlusSubscription || titleProduct == null || !titleProduct.IsRedboxPlusEligible) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.RedboxPlusInfoADAGridHeight = ((displayProductModel.RedboxPlusInfoADAVisibility == Visibility.Visible) ? 38 : 0);
		displayProductModel.MoreLikeThisText = Resources.title_details_more_like_this_text;
		displayProductModel.MainHeight = displayProductModel.DisplayProductModelConstants.MainHeight;
		displayProductModel.NotEmptyCaseVisibility = ((titleProduct != null && titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.BuyText = displayProductModel.DisplayProductModelConstants.BuyText;
		displayProductModel.BuyButtonBuyTextStyle = displayProductModel.DisplayProductModelConstants.FontMontserrat16Style;
		displayProductModel.BuyButtonBuyPriceTextStyle = displayProductModel.DisplayProductModelConstants.FontMontserrat20Style;
		displayProductModel.BuyPriceText = GetBuyPriceText(titleProduct);
		displayProductModel.StarringVisibility = ((titleProduct == null || !titleProduct.IsMovie || titleProduct == null || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ComingSoonLabelText = displayProductModel.DisplayProductModelConstants.ComingSoonLabel + " " + ((titleProduct != null && titleProduct.ReleaseDate.HasValue) ? titleProduct.ReleaseDate.Value.ToString("MM/dd") : string.Empty);
		displayProductModel.OutOfStockLine1Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine1Text;
		displayProductModel.OutOfStockLine2Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine2Text;
		displayProductModel.OutOfStockLine3Text = string.Format(displayProductModel.DisplayProductModelConstants.OutOfStockLine3Text, (titleProduct != null && titleProduct.IsMovie) ? displayProductModel.DisplayProductModelConstants.Movie : displayProductModel.DisplayProductModelConstants.Game);
		displayProductModel.OutOfStockLine4Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine4Text;
		string arg = displayProductModel.DisplayProductModelConstants.KioskClientService?.Kiosk?.KioskLabel ?? "A";
		string arg2 = displayProductModel.DisplayProductModelConstants.KioskClientService?.Kiosk?.DualKiosk?.Label ?? "A";
		displayProductModel.DualInStockTextTitleDetailsOutOfStockKioskText = string.Format(Resources.dual_in_stock_text_title_details_out_of_stock_kiosk_text, arg);
		displayProductModel.DualInStockTextTitleDetailsInStockKioskText = string.Format(Resources.dual_in_stock_text_title_details_in_stock_kiosk_text, arg2);
		IAd ad = ((!loadAd || BaseViewController.ApplicationState.IsADAAccessible) ? null : ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.TitleDetail));
		PopulateModelAdImage(displayProductModel, ad);
		if (loadMoreLikeThis && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily == TitleFamily.Movies && Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort != BrowseSelectedSort.ComingSoon && !BaseViewController.ApplicationState.IsADAAccessible)
		{
			PopulateModelMoreLikeThis(displayProductModel, titleProduct);
		}
	}

	private static void PopulateModelAdImage(DisplayProductModel displayProductModel, IAd ad)
	{
		displayProductModel.AdImage = ad?.Image;
		displayProductModel.AdHeight = ((ad != null) ? 98 : 0);
		displayProductModel.MainHeight = ((ad != null) ? 530 : 628);
		displayProductModel.AdImageVisibility = ((ad == null) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.AdImpression = ((ad != null) ? new AdImpression
		{
			Ad = ad,
			AdLocation = AdLocation.TitleDetail
		} : null);
		displayProductModel.BoxArtGridWidth = ((ad != null) ? 512 : 512);
		displayProductModel.DetailsGridWidth = ((ad != null) ? 512 : 512);
		displayProductModel.DescriptionMaxHeight = ((ad != null) ? 72 : 144);
	}

	private static void PopulateModelMoreLikeThis(DisplayProductModel displayProductModel, ITitleProduct titleProduct)
	{
		List<DisplayProductMoreLikeThisModel> list = BrowseFunctions.CreateMoreLikeThis(titleProduct, DisplayProductMoreLikeThisModel_OnMoreLikeThisSelected).Take(4).ToList();
		displayProductModel.MoreLikeThisVisibility = ((list == null || list.Count <= 1) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.DisplayProductMoreLikeThis1 = ((list != null && list.Count > 0) ? list[0] : null);
		displayProductModel.DisplayProductMoreLikeThis2 = ((list != null && list.Count > 1) ? list[1] : null);
		displayProductModel.DisplayProductMoreLikeThis3 = ((list != null && list.Count > 2) ? list[2] : null);
		displayProductModel.DisplayProductMoreLikeThis4 = ((list != null && list.Count > 3) ? list[3] : null);
	}

	private static void DisplayProductMoreLikeThisModel_OnMoreLikeThisSelected(IBrowseItemModel browseItemModel)
	{
		if (browseItemModel?.Data is IKioskProduct kioskProduct && kioskProduct is ITitleProduct titleProduct)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsMoreLikeThis")?.AddData(ProductData.ToProductData(titleProduct));
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
			OrchestrationService.ViewGoBack("MoreLikeThis", popOnly: false);
			OrchestrationService.ViewShow("product_detail_view");
		}
	}

	private static string GetGenreText(List<long> genres)
	{
		List<string> genreStrings = new List<string>();
		genres?.ForEach(delegate(long x)
		{
			GenresProperties.Properties.TryGetValue((Genres)x, out var value);
			if (value != null)
			{
				genreStrings.Add(value.Description);
			}
		});
		return string.Join(", ", genreStrings);
	}

	private static string GetPriceText(IKioskProduct kioskProduct, int pricingRecordIndex, DisplayProductModelConstants displayProductModelConstants)
	{
		List<string> priceText = new List<string>();
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			IList<IPricingRecord> list = displayProductModelConstants.PricingService?.GetProductPricingRecords(tp.ProductId);
			if (tp.TitleType != TitleType.DigitalCode)
			{
				IPricingRecord pricingRecord = ((list != null && list.Count > pricingRecordIndex) ? list[pricingRecordIndex] : null);
				if (pricingRecord != null)
				{
					string arg = ((TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {displayProductModelConstants.PriceFor} {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
					string arg2 = (((pricingRecord.InitialDays ?? 1) > 1) ? string.Format(" {0} {1} {2}{3}", displayProductModelConstants.PriceFor, pricingRecord.InitialDays ?? 1, displayProductModelConstants.Night, (pricingRecord.InitialDays > 1) ? "s" : string.Empty) : string.Empty);
					priceText.Add($"{pricingRecord.InitialNight:C}{arg}{arg2}");
				}
			}
		});
		return string.Join("\n", priceText);
	}

	private static string GetReturnTimeText(IKioskProduct kioskProduct, int pricingRecordIndex, DisplayProductModelConstants displayProductModelConstants)
	{
		List<string> returnTimeText = new List<string>();
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			if (tp.TitleType != TitleType.DigitalCode)
			{
				IList<IPricingRecord> list = displayProductModelConstants.PricingService?.GetProductPricingRecords(tp.ProductId);
				IPricingRecord pricingRecord = ((list != null && list.Count > pricingRecordIndex) ? list[pricingRecordIndex] : null);
				if (pricingRecord != null)
				{
					DateTime dateTime = DateTime.Today.Add(Redbox.Rental.Services.Session.Session.Instance?.RentalReturnTime ?? TimeSpan.FromHours(21.0));
					DateTime dateTime2 = DateTime.Now.AddDays(pricingRecord.InitialDays ?? 1);
					string text = Resources.ResourceManager.GetString(dateTime2.DayOfWeek.ToString());
					string text2 = ((pricingRecordIndex > 0 && TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {displayProductModelConstants.PriceFor} {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
					returnTimeText.Add(string.Format("{0} {1}, {2}{3}", dateTime.ToString("h:mm tt"), text, dateTime2.ToShortDateString(), text2));
				}
			}
		});
		if (pricingRecordIndex <= 0)
		{
			if (returnTimeText.Count <= 0)
			{
				return string.Empty;
			}
			return returnTimeText[0];
		}
		return string.Join("\n", returnTimeText);
	}

	private static string GetDealText(IKioskProduct kioskProduct)
	{
		List<string> dealText = new List<string>();
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			if (tp.TitleType != TitleType.DigitalCode)
			{
				(ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(tp.ProductId))?.ForEach(delegate(IPricingRecord pr)
				{
					if (pr != null && pr.IsDeal)
					{
						string arg = ((TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
						dealText.Add($"{pr.ExtraNight:C}{arg}");
					}
				});
			}
		});
		return string.Format(Resources.flash_deal_text, string.Join(", ", dealText));
	}

	public static BitmapImage GetSubPlatformBitmapImage(ITitleProduct kioskProduct)
	{
		BitmapImage result = null;
		long? num = kioskProduct?.SubPlatformId;
		if (num.HasValue && num > 0)
		{
			result = Constants.SubPlatformImages.GetSubPlatformImage((Constants.SubPlatforms)num.Value);
		}
		return result;
	}

	private static string GetBuyPriceText(ITitleProduct titleProduct)
	{
		List<ITitleProduct> list = ((titleProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)titleProduct).KioskProducts : new List<ITitleProduct> { titleProduct }).Where((ITitleProduct tp) => tp.IsSellableAndInStock).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		decimal price = list.Min((ITitleProduct tp) => tp.PurchasePrice);
		bool flag = list.Count == 1 || list.All((ITitleProduct tp) => tp.PurchasePrice == price);
		return string.Format("${0:F2}{1}", price, flag ? "" : "+");
	}

	public static void OnLeave(IViewFrameInstance viewFrameInstance)
	{
		RecordAdImpression(BaseViewController.GetViewModel<TitleDetailModel>(viewFrameInstance.Id)?.BrowseProductControlModel);
	}

	private static void RecordAdImpression(BrowseControlModel browseControlModel)
	{
		if (browseControlModel != null)
		{
			RecordAdImpression(browseControlModel, GetPageNumber(browseControlModel));
		}
	}

	private static void RecordAdImpression(BrowseControlModel browseControlModel, int pageNumber)
	{
		BaseViewController.RecordAdImpression(GetDisplayProductModel(browseControlModel, pageNumber)?.AdImpression);
	}

	private static void StartAdImpression(BrowseControlModel browseControlModel)
	{
		if (browseControlModel != null)
		{
			GetDisplayProductModel(browseControlModel, GetPageNumber(browseControlModel))?.AdImpression?.Start();
		}
	}

	private static void StartAdImpression(BrowseControlModel browseControlModel, int pageNumber)
	{
		GetDisplayProductModel(browseControlModel, pageNumber)?.AdImpression?.Start();
	}

	private static DisplayProductModel GetDisplayProductModel(BrowseControlModel browseControlModel, int pageNumber)
	{
		DisplayProductModel result = null;
		if (browseControlModel?.BrowseItems != null && pageNumber > 0 && pageNumber <= browseControlModel.BrowseItems.Count)
		{
			result = browseControlModel.BrowseItems[pageNumber - 1] as DisplayProductModel;
		}
		return result;
	}
}
