using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

public class Flag : UserControl
{
	public static readonly DependencyProperty FlagColorProperty = Dependency<Flag>.CreateDependencyProperty("FlagColor", typeof(Color), Colors.Black);

	public static readonly DependencyProperty FlagGradientColorProperty = Dependency<Flag>.CreateDependencyProperty("FlagGradientColor", typeof(Color), Colors.Black);

	public static readonly DependencyProperty FlagShadowColorProperty = Dependency<Flag>.CreateDependencyProperty("FlagShadowColor", typeof(Color), Colors.Black);

	public Color FlagColor
	{
		get
		{
			return (Color)GetValue(FlagColorProperty);
		}
		set
		{
			SetValue(FlagColorProperty, value);
		}
	}

	public Color FlagGradientColor
	{
		get
		{
			return (Color)GetValue(FlagGradientColorProperty);
		}
		set
		{
			SetValue(FlagGradientColorProperty, value);
		}
	}

	public Color FlagShadowColor
	{
		get
		{
			return (Color)GetValue(FlagShadowColorProperty);
		}
		set
		{
			SetValue(FlagShadowColorProperty, value);
		}
	}
}
