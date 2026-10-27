using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.FMA.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ShowPlanogramView")]
public partial class ShowPlanogramViewUserControl : UserControl, IComponentConnector
{
	public ShowPlanogramViewUserControl()
	{
		InitializeComponent();
	}
}
