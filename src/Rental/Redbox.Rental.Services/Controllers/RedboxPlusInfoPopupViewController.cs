using System;
using System.Windows;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusInfoPopupViewController : BaseViewController
{
	public static void ConfigureRedboxPlusInfoPopupView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			RedboxPlusInfoPopupViewModel redboxPlusInfoPopupViewModel = new RedboxPlusInfoPopupViewModel();
			redboxPlusInfoPopupViewModel.Header = Resources.redbox_plus_info_header;
			redboxPlusInfoPopupViewModel.DescriptionLine1 = Resources.redbox_plus_info_description_line1;
			redboxPlusInfoPopupViewModel.DescriptionLine2 = Resources.redbox_plus_info_description_line2;
			redboxPlusInfoPopupViewModel.DescriptionLine3 = Resources.redbox_plus_info_description_line3;
			redboxPlusInfoPopupViewModel.Note = Resources.redbox_plus_info_note;
			redboxPlusInfoPopupViewModel.CloseButtonText = Resources.redbox_plus_info_close_button_text;
			redboxPlusInfoPopupViewModel.OnCloseButtonClicked = (Action)Delegate.Combine(redboxPlusInfoPopupViewModel.OnCloseButtonClicked, new Action(RedboxPlusInfoPopupViewModel_OnCloseButtonClicked));
			viewUserControl.DataContext = redboxPlusInfoPopupViewModel;
		}
	}

	private static void RedboxPlusInfoPopupViewModel_OnCloseButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusInfoPopupClose");
		OrchestrationService.ViewGoBackSafe("RedboxPlusInfoPopupViewController close", popOnly: false);
	}
}
