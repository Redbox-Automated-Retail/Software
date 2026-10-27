using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ControlName = "StartView", ThemeName = "Prototype")]
public partial class StartViewPrototypeUserControl : UserControl, IComponentConnector
{
	public StartViewPrototypeUserControl()
	{
		InitializeComponent();
	}
}
