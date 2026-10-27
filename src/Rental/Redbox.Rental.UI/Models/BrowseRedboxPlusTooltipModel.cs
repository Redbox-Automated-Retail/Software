using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class BrowseRedboxPlusTooltipModel : DependencyObject
{
	public Action<BrowseRedboxPlusTooltipModel> OnBrowseRedboxPlusTooltipUserControlClicked;

	public static readonly DependencyProperty BrowseRedboxPlusTooltipUserControlVisibilityProperty = DependencyProperty.Register("BrowseRedboxPlusTooltipUserControlVisibility", typeof(Visibility), typeof(BrowseRedboxPlusTooltipModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BrowseRedboxPlusTooltipUserControlMarginProperty = DependencyProperty.Register("BrowseRedboxPlusTooltipUserControlMargin", typeof(Thickness), typeof(BrowseRedboxPlusTooltipModel), new FrameworkPropertyMetadata(new Thickness(28.0, 0.0, 0.0, 138.0)));

	public string Text { get; set; }

	public Visibility BrowseRedboxPlusTooltipUserControlVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BrowseRedboxPlusTooltipUserControlVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseRedboxPlusTooltipUserControlVisibilityProperty, value);
			});
		}
	}

	public Thickness BrowseRedboxPlusTooltipUserControlMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(BrowseRedboxPlusTooltipUserControlMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseRedboxPlusTooltipUserControlMarginProperty, value);
			});
		}
	}

	public void ProcessOnBrowseRedboxPlusTooltipUserControlClicked(BrowseRedboxPlusTooltipModel browseRedboxPlusTooltipModel)
	{
		OnBrowseRedboxPlusTooltipUserControlClicked?.Invoke(browseRedboxPlusTooltipModel);
	}
}
