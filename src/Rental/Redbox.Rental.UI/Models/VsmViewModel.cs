using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models;

public class VsmViewModel : BaseModel<VsmViewModel>
{
	public static readonly DependencyProperty ClosingLocationGridVisibilityProperty = DependencyProperty.Register("ClosingLocationGridVisibility", typeof(Visibility), typeof(VsmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ClosingLocationBannerVisibilityProperty = DependencyProperty.Register("ClosingLocationBannerVisibility", typeof(Visibility), typeof(VsmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ClosingLocationAddressVisibilityProperty = DependencyProperty.Register("ClosingLocationAddressVisibility", typeof(Visibility), typeof(VsmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ClosingLocationCityVisibilityProperty = DependencyProperty.Register("ClosingLocationCityVisibility", typeof(Visibility), typeof(VsmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ClosingLocationNoteVisibilityProperty = DependencyProperty.Register("ClosingLocationNoteVisibility", typeof(Visibility), typeof(VsmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public BitmapImage AdImage { get; set; }

	public IAdImpression AdImpression { get; set; }

	public string TitleText { get; set; }

	public float HeaderHeight { get; set; }

	public int MinimumDisplayTime { get; set; }

	public string ClosingLocationDateText { get; set; }

	public string ClosingLocationText { get; set; }

	public string ClosingLocationBannerText { get; set; }

	public string ClosingLocationAddressText { get; set; }

	public string ClosingLocationCityText { get; set; }

	public Visibility ClosingLocationGridVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ClosingLocationGridVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosingLocationGridVisibilityProperty, value);
			});
		}
	}

	public Visibility ClosingLocationBannerVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ClosingLocationBannerVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosingLocationBannerVisibilityProperty, value);
			});
		}
	}

	public Visibility ClosingLocationAddressVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ClosingLocationAddressVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosingLocationAddressVisibilityProperty, value);
			});
		}
	}

	public Visibility ClosingLocationCityVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ClosingLocationCityVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosingLocationCityVisibilityProperty, value);
			});
		}
	}

	public Visibility ClosingLocationNoteVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ClosingLocationNoteVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClosingLocationNoteVisibilityProperty, value);
			});
		}
	}
}
