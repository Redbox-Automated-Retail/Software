using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MaintenanceView")]
public partial class MaintenanceViewUserControl : UserControl, IComponentConnector
{
	public MaintenanceViewUserControl()
	{
		InitializeComponent();
	}
}
