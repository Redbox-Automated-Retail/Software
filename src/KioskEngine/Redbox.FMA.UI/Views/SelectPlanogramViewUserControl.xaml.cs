using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.FMA.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SelectPlanogramView")]
public partial class SelectPlanogramViewUserControl : UserControl, IComponentConnector
{
	public SelectPlanogramViewUserControl()
	{
		InitializeComponent();
	}
}
