using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ApplicationLoadView")]
public partial class ApplicationLoadViewUserControl : UserControl, IComponentConnector
{
	public ApplicationLoadViewUserControl()
	{
		InitializeComponent();
		Spinner.IsAnimated = true;
	}
}
