using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models;

public class PerksThanksViewModel : BaseModel<PerksThanksViewModel>
{
	public static readonly DependencyProperty DisplayImageProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("DisplayImage", TYPES.BITMAPIMAGE);

	public static readonly DependencyProperty TitleTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("TitleText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty MessageTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("MessageText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CommentTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("CommentText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty FooterTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("FooterText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty PerksIconsVisibilityProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("PerksIconsVisibility", TYPES.VISIBILITY, Visibility.Collapsed);

	public static readonly DependencyProperty PerksFreeRentalTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("PerksFreeRentalText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty PerksDealsTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("PerksDealsText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty PerksBdayGiftTextProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("PerksBdayGiftText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty TitleMarginProperty = BaseModel<PerksThanksViewModel>.CreateDependencyProperty("TitleMargin", typeof(Thickness), new Thickness(0.0, 36.0, 0.0, 36.0));

	public Action<AnalyticsTypes> AnalyticsAction { get; set; }

	public Action ContinueAction { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public BitmapImage DisplayImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(DisplayImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayImageProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string MessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MessageTextProperty, value);
			});
		}
	}

	public string CommentText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CommentTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CommentTextProperty, value);
			});
		}
	}

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

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}

	public Visibility PerksIconsVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PerksIconsVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksIconsVisibilityProperty, value);
			});
		}
	}

	public string PerksFreeRentalText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksFreeRentalTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksFreeRentalTextProperty, value);
			});
		}
	}

	public string PerksDealsText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksDealsTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksDealsTextProperty, value);
			});
		}
	}

	public string PerksBdayGiftText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksBdayGiftTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksBdayGiftTextProperty, value);
			});
		}
	}

	public Thickness TitleMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(TitleMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleMarginProperty, value);
			});
		}
	}
}
