using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SignInThanksView")]
public partial class SignInThanksViewUserControl : ViewUserControl, IComponentConnector
{
	private SignInThanksViewModel Model => base.DataContext as SignInThanksViewModel;

	public SignInThanksViewUserControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "please_wait_view";
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = Model?.MessageText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model?.ThanksText
		});
	}
}
