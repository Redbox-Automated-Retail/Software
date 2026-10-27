using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "BulletListPopupView")]
public partial class BulletListPopupViewUserControl : UserControl, IComponentConnector
{
	public BulletListPopupViewUserControl()
	{
		InitializeComponent();
	}
}
