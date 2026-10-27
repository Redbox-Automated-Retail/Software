using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Redbox.Rental.UI.Controls;

public partial class ChevronUserControl : UserControl, IComponentConnector
{
	public enum Direction
	{
		Left,
		Right
	}

	private static DependencyProperty ChevronDirectionProperty = DependencyProperty.Register("ChevronDirection", typeof(Direction), typeof(ChevronUserControl), new PropertyMetadata(Direction.Right, ChevronDirectionChanged));

	private static DependencyProperty ChevronColorProperty = DependencyProperty.Register("ChevronColor", typeof(Brush), typeof(ChevronUserControl), new PropertyMetadata(Brushes.Black));

	public Direction ChevronDirection
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Direction)GetValue(ChevronDirectionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ChevronDirectionProperty, value);
			});
		}
	}

	public Brush ChevronColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(ChevronColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ChevronColorProperty, value);
			});
		}
	}

	public ChevronUserControl()
	{
		ChevronColor = Brushes.White;
		InitializeComponent();
		RightArrow.Visibility = Visibility.Visible;
		LeftArrow.Visibility = Visibility.Collapsed;
	}

	private static void ChevronDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ChevronUserControl chevronUserControl = d as ChevronUserControl;
		if (d != null)
		{
			if ((Direction)e.NewValue == Direction.Right)
			{
				chevronUserControl.RightArrow.Visibility = Visibility.Visible;
				chevronUserControl.LeftArrow.Visibility = Visibility.Collapsed;
			}
			else
			{
				chevronUserControl.LeftArrow.Visibility = Visibility.Visible;
				chevronUserControl.RightArrow.Visibility = Visibility.Collapsed;
			}
		}
	}
}
