using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReturnVisitPromoOfferView")]
public partial class ReturnVisitPromoOfferViewUserControl : ViewUserControl, IComponentConnector
{
	private ReturnVisitPromoOfferViewModel ReturnVisitPromoOfferViewModel => base.DataContext as ReturnVisitPromoOfferViewModel;

	public ReturnVisitPromoOfferViewUserControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
	}

	private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ReturnVisitPromoOfferViewModel?.ProcessOnCloseButtonClicked();
	}

	private void UseNowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ReturnVisitPromoOfferViewModel?.ProcessOnUseNowButtonClicked();
	}
}
