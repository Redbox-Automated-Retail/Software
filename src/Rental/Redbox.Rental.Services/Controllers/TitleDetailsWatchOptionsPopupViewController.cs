using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class TitleDetailsWatchOptionsPopupViewController : BaseViewController
{
	public static void ConfigureTitleDetailsWatchOptionsPopupView(TitleDetailsWatchOptionsPopupViewParameter titleDetailsWatchOptionsPopupViewParameter)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (titleDetailsWatchOptionsPopupViewParameter != null && titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel != null && viewUserControl != null && titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.Data is ITitleProduct titleProduct)
		{
			TitleDetailsWatchOptionsPopupViewModel titleDetailsWatchOptionsPopupViewModel = new TitleDetailsWatchOptionsPopupViewModel();
			titleDetailsWatchOptionsPopupViewModel.DisplayProductModel = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel;
			titleDetailsWatchOptionsPopupViewModel.RatingBorderThickness = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.RatingBorderThickness;
			titleDetailsWatchOptionsPopupViewModel.RatingVisibility = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.RatingVisibility;
			titleDetailsWatchOptionsPopupViewModel.Rating = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.Rating;
			titleDetailsWatchOptionsPopupViewModel.RunningTimeBorderThickness = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.RunningTimeBorderThickness;
			titleDetailsWatchOptionsPopupViewModel.RunningTimeVisibility = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.RunningTimeVisibility;
			titleDetailsWatchOptionsPopupViewModel.RunningTime = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.RunningTime;
			titleDetailsWatchOptionsPopupViewModel.ReleaseYearBorderThickness = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.ReleaseYearBorderThickness;
			titleDetailsWatchOptionsPopupViewModel.ReleaseYearVisibility = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.ReleaseYearVisibility;
			titleDetailsWatchOptionsPopupViewModel.ReleaseYear = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.ReleaseYear;
			titleDetailsWatchOptionsPopupViewModel.CCSupported = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.CCSupported;
			titleDetailsWatchOptionsPopupViewModel.Header = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.Header;
			titleDetailsWatchOptionsPopupViewModel.Description = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.Description;
			titleDetailsWatchOptionsPopupViewModel.Starring = titleDetailsWatchOptionsPopupViewParameter.DisplayProductModel.Starring;
			string productWebPageUrl = titleProduct.ProductWebPageUrl;
			string url = ((!string.IsNullOrEmpty(productWebPageUrl)) ? (productWebPageUrl + "?icamp=qrkiosk") : "https://www.redbox.com/?icamp=qrkiosk");
			titleDetailsWatchOptionsPopupViewModel.QRCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(url, returnDefaultOnError: true);
			List<DisplayProductMoreLikeThisModel> list = BrowseFunctions.CreateMoreLikeThis(titleProduct, DisplayProductMoreLikeThisModel_OnMoreLikeThisSelected).Take(8).ToList();
			bool flag = list.Count > 0;
			titleDetailsWatchOptionsPopupViewModel.BorderHeight = (flag ? 752 : 585);
			titleDetailsWatchOptionsPopupViewModel.MoreLikeThisVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
			titleDetailsWatchOptionsPopupViewModel.DisplayProductMoreLikeThisModels = list;
			titleDetailsWatchOptionsPopupViewModel.OnWatchOptionsSeeFullDetailsButtonClicked = (Action)Delegate.Combine(titleDetailsWatchOptionsPopupViewModel.OnWatchOptionsSeeFullDetailsButtonClicked, new Action(TitleDetailsWatchOptionsPopupViewModel_OnSeeFullDetailsButtonClicked));
			titleDetailsWatchOptionsPopupViewModel.OnWatchOptionsCloseButtonClicked = (Action)Delegate.Combine(titleDetailsWatchOptionsPopupViewModel.OnWatchOptionsCloseButtonClicked, new Action(TitleDetailsWatchOptionsPopupViewModel_OnCloseButtonClicked));
			viewUserControl.DataContext = titleDetailsWatchOptionsPopupViewModel;
		}
	}

	private static void DisplayProductMoreLikeThisModel_OnMoreLikeThisSelected(IBrowseItemModel browseItemModel)
	{
		if (browseItemModel?.Data is IKioskProduct kioskProduct && kioskProduct is ITitleProduct titleProduct)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsWatchOptionsPopupMoreLikeThis")?.AddData(ProductData.ToProductData(titleProduct));
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
			OrchestrationService.ViewGoBackSafe("TitleDetailsWatchOptionsPopupViewController MoreLikeThis", popOnly: false);
			OrchestrationService.ViewShow("product_detail_view");
		}
	}

	private static void TitleDetailsWatchOptionsPopupViewModel_OnSeeFullDetailsButtonClicked()
	{
		DisplayProductModel displayProductModel = BaseViewController.GetViewModel<TitleDetailsWatchOptionsPopupViewModel>()?.DisplayProductModel;
		if (displayProductModel != null)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsWatchOptionsPopupSeeFullDetails");
			OrchestrationService.ViewShow("title_details_full_details_popup_view", new TitleDetailsFullDetailsPopupViewInfo
			{
				DisplayProductModel = displayProductModel
			});
		}
	}

	private static void TitleDetailsWatchOptionsPopupViewModel_OnCloseButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsWatchOptionsPopupClose");
		OrchestrationService.ViewGoBackSafe("TitleDetailsWatchOptionsPopupViewController Close", popOnly: false);
	}
}
