using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class DisplayGroupModel : BrowseItemModel
{
	private Visibility _rightAddButtonVisibility;

	public static readonly DependencyProperty WatchOptionsVisibilityProperty = DependencyProperty.Register("WatchOptionsVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty WatchOptionsAZVisibilityProperty = DependencyProperty.Register("WatchOptionsAZVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockVisibilityProperty = DependencyProperty.Register("DualInStockVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockAZVisibilityProperty = DependencyProperty.Register("DualInStockAZVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OutOfStockTextAZVisibilityProperty = DependencyProperty.Register("OutOfStockTextAZVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextKioskTextProperty = DependencyProperty.Register("DualInStockTextKioskText", typeof(string), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DualInStockTextAZKioskTextProperty = DependencyProperty.Register("DualInStockTextAZKioskText", typeof(string), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SeparatorLineStyleProperty = DependencyProperty.Register("SeparatorLineStyle", typeof(Style), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register("TitleStyle", typeof(Style), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelectedStyle", typeof(bool), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SeparatorLineVisibilityProperty = DependencyProperty.Register("SeparatorLineVisibility", typeof(Visibility), typeof(DisplayGroupModel), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public Visibility SeparatorLineVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SeparatorLineVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SeparatorLineVisibilityProperty, value);
			});
		}
	}

	public Style SeparatorLineStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(SeparatorLineStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SeparatorLineStyleProperty, value);
			});
		}
	}

	public Style TitleStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(TitleStyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleStyleProperty, value);
			});
		}
	}

	public bool IsSelected
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsSelectedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsSelectedProperty, value);
				TitleStyle = (value ? SelectedTitleStyle : UnselectedTitleStyle);
			});
		}
	}

	public bool LockAspectRatio { get; set; }

	public double ImageBorderCornerRadius { get; set; }

	public Thickness ImageBorderThickness { get; set; }

	public Color ImageBorderColor { get; set; }

	public Visibility DisplayFlagVisibility { get; set; }

	public Style FlagStyle { get; set; }

	public string FlagText { get; set; }

	public Visibility ImageBackgroundVisibility { get; set; }

	public BitmapImage Image { get; set; }

	public double DisplayImageOpacity { get; set; }

	public Visibility BannerVisibility { get; set; }

	public Brush BannerBackgroundBrush { get; set; }

	public string BannerText { get; set; }

	public double AddButtonOpacity { get; set; }

	public Visibility AddButtonVisibility { get; set; }

	public Visibility RightAddButtonVisibility
	{
		get
		{
			return Visibility.Hidden;
		}
		set
		{
			_rightAddButtonVisibility = value;
		}
	}

	public string AddButtonLabelText { get; set; }

	public Visibility DisplayCornerAddButtonVisibility { get; set; }

	public Visibility CancelButtonVisibility { get; set; }

	public string ADAMiniCartAddButtonText { get; set; }

	public Visibility ADAMiniCartAddButtonVisibility { get; set; }

	public Style AddButtonStyle { get; set; }

	public Visibility AddButtonPlusSignVisibility { get; set; }

	public string CornerAddButtonText { get; set; }

	public Visibility AddButtonTextVisibility { get; set; }

	public Style ComingSoonButtonStyle { get; set; }

	public Visibility ComingSoonButtonVisibility { get; set; }

	public Visibility ComingSoonButtonCheckMarkVisibility { get; set; }

	public string ComingSoonButtonText { get; set; }

	public Brush BackgroundBrush { get; set; }

	public Visibility WatchOptionsVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(WatchOptionsVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(WatchOptionsVisibilityProperty, value);
			});
		}
	}

	public Visibility WatchOptionsAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(WatchOptionsAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(WatchOptionsAZVisibilityProperty, value);
			});
		}
	}

	public Visibility DualInStockVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DualInStockVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockVisibilityProperty, value);
			});
		}
	}

	public Visibility DualInStockAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DualInStockAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockAZVisibilityProperty, value);
			});
		}
	}

	public Visibility OutOfStockTextAZVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(OutOfStockTextAZVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OutOfStockTextAZVisibilityProperty, value);
			});
		}
	}

	public string DualInStockTextKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextKioskTextProperty, value);
			});
		}
	}

	public string DualInStockTextAZKioskText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DualInStockTextAZKioskTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DualInStockTextAZKioskTextProperty, value);
			});
		}
	}

	public Style SelectedTitleStyle { get; set; }

	public Style UnselectedTitleStyle { get; set; }

	protected override void SetIsBottomRow(bool value)
	{
		base.SetIsBottomRow(value);
		SeparatorLineVisibility = (base.IsBottomRow ? Visibility.Collapsed : Visibility.Visible);
	}
}
