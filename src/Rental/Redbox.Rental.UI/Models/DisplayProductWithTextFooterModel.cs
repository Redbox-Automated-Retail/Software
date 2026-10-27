using System.Windows;

namespace Redbox.Rental.UI.Models;

public class DisplayProductWithTextFooterModel : DisplayProductModel
{
	public static readonly DependencyProperty RecommendationOnPickupCartCancelButtonVisibilityProperty = DependencyProperty.Register("RecommendationOnPickupCartCancelButtonVisibility", typeof(Visibility), typeof(DisplayProductWithTextFooterModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty FooterTextStyleProperty = DependencyProperty.Register("FooterTextStyle", typeof(Style), typeof(DisplayProductWithTextFooterModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FooterTextProperty = DependencyProperty.Register("FooterText", typeof(string), typeof(DisplayProductWithTextFooterModel), new FrameworkPropertyMetadata(null));

	public string FooterText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FooterTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FooterTextProperty, value);
			});
		}
	}

	public Visibility RecommendationOnPickupCartCancelButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RecommendationOnPickupCartCancelButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RecommendationOnPickupCartCancelButtonVisibilityProperty, value);
			});
		}
	}

	public Style FooterTextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(FooterTextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FooterTextStyleProperty, value);
			});
		}
	}
}
