using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class PerksIconBanner : UserControl, IComponentConnector
{
	public static readonly DependencyProperty PerksFreeRentalTextProperty = DependencyProperty.Register("PerksFreeRentalText", typeof(string), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PerksDealsTextProperty = DependencyProperty.Register("PerksDealsText", typeof(string), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PerksBdayGiftTextProperty = DependencyProperty.Register("PerksBdayGiftText", typeof(string), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GridMarginProperty = DependencyProperty.Register("GridMargin", typeof(Thickness), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty FreeRentalIconWidthProperty = DependencyProperty.Register("FreeRentalIconWidth", typeof(double), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SpecialDealsIconWidthProperty = DependencyProperty.Register("SpecialDealsIconWidth", typeof(double), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BDayIconWidthProperty = DependencyProperty.Register("BDayIconWidth", typeof(double), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(PerksIconBanner), new FrameworkPropertyMetadata(0.2));

	public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyle", typeof(Style), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

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

	public Thickness GridMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(GridMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GridMarginProperty, value);
			});
		}
	}

	public double FreeRentalIconWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(FreeRentalIconWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FreeRentalIconWidthProperty, value);
			});
		}
	}

	public double SpecialDealsIconWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(SpecialDealsIconWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SpecialDealsIconWidthProperty, value);
			});
		}
	}

	public double BDayIconWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(BDayIconWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BDayIconWidthProperty, value);
			});
		}
	}

	public double BackgroundOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(BackgroundOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundOpacityProperty, value);
			});
		}
	}

	public Style TextStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(TextStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextStyleProperty, value);
			});
		}
	}

	public PerksIconBanner()
	{
		InitializeComponent();
	}
}
