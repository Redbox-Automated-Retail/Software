using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class OfferInfoUserControl : UserControl, IComponentConnector
{
	public OfferInfoUserControl()
	{
		InitializeComponent();
		TransparentContinueButton.IsAnimated = false;
	}
}
