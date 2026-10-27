using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class BrowseMessagePopupModel : DependencyObject
{
	public Action OnBrowseMessagePopupInfoClicked;

	public Action OnBrowseMessagePopupCloseClicked;

	public static readonly DependencyProperty BrowseMessagePopupUserControlVisibilityProperty = DependencyProperty.Register("BrowseMessagePopupUserControlVisibility", typeof(Visibility), typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public string Text1 { get; set; }

	public string Text2 { get; set; }

	public string InfoButtonText { get; set; }

	public Visibility BrowseMessagePopupUserControlVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BrowseMessagePopupUserControlVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseMessagePopupUserControlVisibilityProperty, value);
			});
		}
	}

	public void ProcessOnBrowseMessagePopupInfoClicked()
	{
		OnBrowseMessagePopupInfoClicked?.Invoke();
	}

	public void ProcessOnBrowseMessagePopupCloseClicked()
	{
		OnBrowseMessagePopupCloseClicked?.Invoke();
	}
}
