using System.Windows;
using System.Windows.Media;

namespace Redbox.Rental.UI.Models;

public class DisplayGroupSelectionItemModel : DependencyObject
{
	private bool _isEnabled;

	private bool _isSelected;

	public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(DisplayGroupSelectionItemModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyleP", typeof(Style), typeof(DisplayGroupSelectionItemModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(DisplayGroupSelectionItemModel), new FrameworkPropertyMetadata(null));

	public string Name
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(NameProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NameProperty, value);
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

	public Brush BackgroundBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(BackgroundBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundBrushProperty, value);
			});
		}
	}

	public Style EnableTextStyle { get; set; }

	public Style DisabledTextStyle { get; set; }

	public Brush SelectedBackgroundBrush { get; set; }

	public Brush UnselectedBackgroundBrusch { get; set; }

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			_isEnabled = value;
			TextStyle = (_isEnabled ? EnableTextStyle : DisabledTextStyle);
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			BackgroundBrush = (_isSelected ? SelectedBackgroundBrush : UnselectedBackgroundBrusch);
		}
	}
}
