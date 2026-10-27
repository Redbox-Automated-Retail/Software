using System.Windows;

namespace Redbox.Rental.Model.Browse;

public class BrowseMenuButton : DependencyObject
{
	public static readonly DependencyProperty IsButtonEnabledProperty = DependencyProperty.Register("IsButtonEnabled", typeof(bool), typeof(BrowseMenuButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BrowseMenuButton), new FrameworkPropertyMetadata(null));

	private string _textStyleName = "font_montserrat_extrabold_14_correct";

	private int _y;

	private int _height = 56;

	private bool _isSelected;

	public string Name { get; set; }

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

	public int Width { get; set; }

	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	public int X { get; set; }

	public int Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
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

	public bool FixedPosition { get; set; }

	public string TextStyleName
	{
		get
		{
			return _textStyleName;
		}
		set
		{
			_textStyleName = value;
		}
	}
}
