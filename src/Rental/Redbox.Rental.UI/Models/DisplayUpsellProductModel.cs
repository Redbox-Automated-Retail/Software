using System.Windows;

namespace Redbox.Rental.UI.Models;

public class DisplayUpsellProductModel : DisplayProductModel
{
	public static readonly DependencyProperty CheckboxVisibilityProperty = DependencyProperty.Register("CheckboxVisibility", typeof(Visibility), typeof(DisplayUpsellProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public Visibility CheckboxBorderVisibility { get; set; }

	public string PriceText { get; set; }

	public Visibility CheckboxVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CheckboxVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckboxVisibilityProperty, value);
			});
		}
	}
}
