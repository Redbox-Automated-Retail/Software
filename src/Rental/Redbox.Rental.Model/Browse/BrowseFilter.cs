using System.Windows;

namespace Redbox.Rental.Model.Browse;

public class BrowseFilter : DependencyObject
{
	public static readonly DependencyProperty IsButtonEnabledProperty = DependencyProperty.Register("IsButtonEnabled", typeof(bool), typeof(BrowseFilter), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BrowseFilter), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ButtonVisibilityProperty = DependencyProperty.Register("ButtonVisibility", typeof(Visibility), typeof(BrowseFilter), new FrameworkPropertyMetadata(Visibility.Visible));

	private bool _isSelected;

	public string Name { get; set; }

	public int Height { get; set; }

	public int Width { get; set; }

	public object Parameter { get; set; }

	public bool IsVisible
	{
		get
		{
			return ButtonVisibility == Visibility.Visible;
		}
		set
		{
			ButtonVisibility = ((!value) ? Visibility.Collapsed : Visibility.Visible);
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

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			if (_isSelected == IsButtonEnabled)
			{
				IsButtonEnabled = !_isSelected;
			}
		}
	}

	public bool IsDeselectable { get; set; }

	public bool IsButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsButtonEnabledProperty, value);
			});
		}
	}

	public Visibility ButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ButtonVisibilityProperty, value);
			});
		}
	}

	public BrowseFilterType ButtonType { get; set; }
}
