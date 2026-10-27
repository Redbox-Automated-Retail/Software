using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ComingSoonView")]
public partial class ComingSoonViewUserControl : UserControl, IComponentConnector
{
	public string ComingSoonText
	{
		get
		{
			return TextBlockComingSoon.Text;
		}
		set
		{
			TextBlockComingSoon.Text = value;
		}
	}

	public string RentalsAreOnTheirWayText
	{
		get
		{
			return TextBlockRentalsComingSoon.Text;
		}
		set
		{
			TextBlockRentalsComingSoon.Text = value;
		}
	}

	public string ComingSoonTextStyle
	{
		set
		{
			SetStyle(TextBlockComingSoon, value);
		}
	}

	public string RentalsAreOnTheirWayStyle
	{
		set
		{
			SetStyle(TextBlockRentalsComingSoon, value);
		}
	}

	public ComingSoonViewUserControl()
	{
		InitializeComponent();
	}

	private void SetStyle(FrameworkElement frameworkElement, string styleName)
	{
		object obj = FindResource(styleName);
		if (obj != null && frameworkElement != null && obj is Style style)
		{
			frameworkElement.Style = style;
		}
	}

	public void AddComingSoonTitle(IComingSoonTitle comingSoonTitle)
	{
		ComingSoonVendUserControl element = new ComingSoonVendUserControl
		{
			DataContext = comingSoonTitle,
			Margin = new Thickness(52.5, 0.0, 52.5, 0.0)
		};
		ComingSoonPanel.Children.Add(element);
	}
}
