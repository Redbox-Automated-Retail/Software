using System;
using System.Windows;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class TitleDetailsFullDetailsPopupViewController : BaseViewController
{
	public static void ConfigureTitleDetailsFullDetailsPopupView(TitleDetailsFullDetailsPopupViewInfo titleDetailsFullDetailsPopupViewInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			TitleDetailsFullDetailsPopupViewModel titleDetailsFullDetailsPopupViewModel = new TitleDetailsFullDetailsPopupViewModel();
			titleDetailsFullDetailsPopupViewModel.Header = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Header;
			titleDetailsFullDetailsPopupViewModel.Description = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Description;
			titleDetailsFullDetailsPopupViewModel.StarringLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.StarringLabel;
			titleDetailsFullDetailsPopupViewModel.Starring = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Starring;
			titleDetailsFullDetailsPopupViewModel.StarringVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.StarringVisibility;
			titleDetailsFullDetailsPopupViewModel.DirectorsLabel = Resources.title_details_directors_text;
			titleDetailsFullDetailsPopupViewModel.Directors = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DirectedBy;
			titleDetailsFullDetailsPopupViewModel.DirectorsVisibility = (string.IsNullOrEmpty(titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DirectedBy) ? Visibility.Collapsed : Visibility.Visible);
			titleDetailsFullDetailsPopupViewModel.RatingLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.RatingLabel;
			titleDetailsFullDetailsPopupViewModel.Rating = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.RatingDescription;
			titleDetailsFullDetailsPopupViewModel.RatingVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.RatingVisibility;
			titleDetailsFullDetailsPopupViewModel.ClosedCaptionedText = ((titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.CCSupported == Visibility.Visible) ? Resources.title_details_cc_text : "");
			titleDetailsFullDetailsPopupViewModel.OneNightPriceLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightPriceLabel;
			titleDetailsFullDetailsPopupViewModel.OneNightPrices = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightPrice.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.OneNightReturnTimeLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightReturnTimeLabel;
			titleDetailsFullDetailsPopupViewModel.OneNightReturnTimes = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightReturnTime.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightPriceLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightPriceLabel;
			titleDetailsFullDetailsPopupViewModel.MultiNightPrices = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightPrice.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightPricesVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightPriceVisibility;
			titleDetailsFullDetailsPopupViewModel.MultiNightReturnTimeLabel = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightReturnTimeLabel;
			titleDetailsFullDetailsPopupViewModel.MultiNightReturnTimes = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightReturnTime.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightReturnTimesVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightReturnTimeVisibility;
			titleDetailsFullDetailsPopupViewModel.ServiceFeeMessage = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.ServiceFeeText;
			titleDetailsFullDetailsPopupViewModel.ServiceFeeMessageVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.ServiceFeeTextVisibility;
			titleDetailsFullDetailsPopupViewModel.DealMessage = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DealText;
			titleDetailsFullDetailsPopupViewModel.DealMessageVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DealTextVisibility;
			titleDetailsFullDetailsPopupViewModel.CloseButtonText = Resources.close_button.ToUpper();
			titleDetailsFullDetailsPopupViewModel.OnFullDetailsCloseButtonClicked = (Action)Delegate.Combine(titleDetailsFullDetailsPopupViewModel.OnFullDetailsCloseButtonClicked, new Action(TitleDetailsFullDetailsPopupViewModel_OnCloseButtonClicked));
			viewUserControl.DataContext = titleDetailsFullDetailsPopupViewModel;
		}
	}

	private static void TitleDetailsFullDetailsPopupViewModel_OnCloseButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsFullDetailsPopupClose");
		OrchestrationService.ViewGoBackSafe("TitleDetailsFullDetailsPopupViewController close", popOnly: false);
	}
}
