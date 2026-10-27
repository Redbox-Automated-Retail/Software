using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models;

public class PleaseWaitViewModel : DependencyObject
{
	public static readonly DependencyProperty AdImageSourceProperty = DependencyProperty.Register("AdImageSource", typeof(BitmapImage), typeof(PleaseWaitViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdImageVisibilityProperty = DependencyProperty.Register("AdImageVisibility", typeof(Visibility), typeof(PleaseWaitViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public IAdImpression AdImpression { get; set; }

	public BitmapImage AdImageSource
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(AdImageSourceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageSourceProperty, value);
			});
		}
	}

	public Visibility AdImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageVisibilityProperty, value);
			});
		}
	}
}
