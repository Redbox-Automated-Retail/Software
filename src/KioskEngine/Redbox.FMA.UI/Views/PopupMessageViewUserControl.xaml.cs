using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.FMA.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "FmaPopupMessageView")]
public partial class PopupMessageViewUserControl : UserControl, IComponentConnector
{
	public PopupMessageViewUserControl()
	{
		InitializeComponent();
	}
}
