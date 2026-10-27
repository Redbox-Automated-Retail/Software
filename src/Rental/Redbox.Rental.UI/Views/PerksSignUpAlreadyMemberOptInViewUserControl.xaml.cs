using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksSignUpAlreadyMemberOptInView")]
public partial class PerksSignUpAlreadyMemberOptInViewUserControl : ViewUserControl, IComponentConnector
{
	public PerksSignUpAlreadyMemberOptInViewUserControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
	}
}
