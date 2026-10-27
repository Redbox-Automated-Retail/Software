using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReservationDetailsView")]
public partial class ReservationDetailsViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ReservationDetailsViewModel Model => base.DataContext as ReservationDetailsViewModel;

	public ReservationDetailsViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}

	private void OkayButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOkayButtonClick();
		HandleWPFHit();
	}
}
