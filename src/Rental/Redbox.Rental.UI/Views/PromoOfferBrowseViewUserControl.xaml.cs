using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromoOfferBrowseView")]
public partial class PromoOfferBrowseViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	public PromoOfferBrowseViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return (base.DataContext as PromoOfferBrowseViewModel)?.ProcessGetSpeechControl();
	}
}
