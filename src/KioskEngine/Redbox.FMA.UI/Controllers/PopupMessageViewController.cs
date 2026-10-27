using System.Windows;
using Redbox.FMA.Models;
using Redbox.Rental.UI;

namespace Redbox.FMA.Controllers;

public class PopupMessageViewController : BaseViewController
{
	public static void ConfigureView(PopupMessageViewParameters parameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		PopupMessageViewModel popupMessageViewModel = GetViewModel();
		if (popupMessageViewModel == null)
		{
			popupMessageViewModel = new PopupMessageViewModel
			{
				MessageText = parameters.MessageText,
				ContinueCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), parameters.ContinueAction)
			};
		}
		viewUserControl.DataContext = popupMessageViewModel;
	}

	private static PopupMessageViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as PopupMessageViewModel;
		}
		return null;
	}
}
