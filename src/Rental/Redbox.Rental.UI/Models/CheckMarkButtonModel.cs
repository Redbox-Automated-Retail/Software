using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class CheckMarkButtonModel : DependencyObject
{
	private bool _isSelected;

	private bool _showDownArrow;

	private bool _showArrow;

	private string _smallText;

	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	private static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(8.0, 0.0, 8.0, 0.0)));

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(null));

	private static readonly DependencyProperty SmallTextVisibilityProperty = DependencyProperty.Register("SmallTextVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty TextMaxWidthProperty = DependencyProperty.Register("TextMaxWidth", typeof(int), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(220));

	private static readonly DependencyProperty TextPaddingProperty = DependencyProperty.Register("TextPadding", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(24.0, 0.0, 24.0, 0.0)));

	private static readonly DependencyProperty DownArrowImageVisibilityProperty = DependencyProperty.Register("DownArrowImageVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	private static readonly DependencyProperty ArrowImageVisibilityProperty = DependencyProperty.Register("ArrowImageVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty CheckMarkImageMarginProperty = DependencyProperty.Register("CheckMarkImageMargin", typeof(Thickness), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(new Thickness(-35.0, -35.0, 0.0, 0.0)));

	private static readonly DependencyProperty CheckMarkImageVisibilityProperty = DependencyProperty.Register("CheckMarkImageVisibility", typeof(Visibility), typeof(CheckMarkButtonModel), new FrameworkPropertyMetadata(Visibility.Hidden));

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			Style = (_isSelected ? SelectedStyle : UnselectedStyle);
			CheckMarkImageVisibility = ((!_isSelected) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	public bool ShowDownArrow
	{
		get
		{
			return _showDownArrow;
		}
		set
		{
			_showDownArrow = value;
			TextPadding = (_showDownArrow ? new Thickness(7.0, 0.0, 24.0, 0.0) : new Thickness(24.0, 0.0, 24.0, 0.0));
			DownArrowImageVisibility = ((!_showDownArrow) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	public bool ShowArrow
	{
		get
		{
			return _showArrow;
		}
		set
		{
			_showArrow = value;
			TextPadding = (_showArrow ? new Thickness(24.0, 0.0, 4.0, 0.0) : new Thickness(24.0, 0.0, 24.0, 0.0));
			ArrowImageVisibility = ((!_showArrow) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	public string SmallText
	{
		get
		{
			return _smallText;
		}
		set
		{
			_smallText = value;
			SmallTextVisibility = (string.IsNullOrEmpty(_smallText) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private Style UnselectedStyle { get; set; }

	private Style SelectedStyle { get; set; }

	public Visibility Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(VisibilityProperty, value);
			});
		}
	}

	private Style Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StyleProperty, value);
			});
		}
	}

	public Thickness Margin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(MarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MarginProperty, value);
			});
		}
	}

	public string Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextProperty, value);
			});
		}
	}

	private Visibility SmallTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SmallTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SmallTextVisibilityProperty, value);
			});
		}
	}

	public int TextMaxWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(TextMaxWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextMaxWidthProperty, value);
			});
		}
	}

	private Thickness TextPadding
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(TextPaddingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TextPaddingProperty, value);
			});
		}
	}

	private Visibility DownArrowImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DownArrowImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DownArrowImageVisibilityProperty, value);
			});
		}
	}

	private Visibility ArrowImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ArrowImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ArrowImageVisibilityProperty, value);
			});
		}
	}

	public Thickness CheckMarkImageMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(CheckMarkImageMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckMarkImageMarginProperty, value);
			});
		}
	}

	private Visibility CheckMarkImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CheckMarkImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckMarkImageVisibilityProperty, value);
			});
		}
	}

	public event Action<CheckMarkButtonModel> OnClicked;

	public CheckMarkButtonModel(Style unselectedStyle, Style selectedStyle)
	{
		UnselectedStyle = unselectedStyle;
		SelectedStyle = selectedStyle;
	}

	public void ProcessOnClicked(CheckMarkButtonModel checkMarkButtonModel)
	{
		OnClicked?.Invoke(checkMarkButtonModel);
	}
}
