using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReturnView")]
public partial class ReturnViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ReturnViewModel Model => base.DataContext as ReturnViewModel;

	public ReturnViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		ISpeechControl result = new SpeechControl
		{
			Name = "tts_return_view"
		};
		AddSpeechPartInstructions(result);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private void AddSpeechPartInstructions(ISpeechControl result)
	{
		if (Model.TouchlessBannerVisibility == Visibility.Visible)
		{
			ISpeechPart noTouchPart = new SpeechPart
			{
				Sequence = 0,
				Name = "ZeroTouch",
				Loop = false,
				StartPause = 500,
				EndPause = 1000
			};
			noTouchPart.Refresh = delegate
			{
				noTouchPart.Clear();
				noTouchPart.Texts.Add(new TextPart
				{
					Text = "If you were trying to do Touch-free pickup"
				});
				noTouchPart.Texts.Add(new TextPart
				{
					Text = "then " + Model.TouchlessBannerMessageText
				});
			};
			result.SpeechParts.Add(noTouchPart);
		}
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Instructions",
			Loop = true,
			StartPause = 0,
			EndPause = 10000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Please insert your disc in the return slot located on the right side of the kiosk."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Insert the disc by holding it in your hand with the opening tab facing the return slot."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The raised plastic pieces will be in the back."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The Kiosk will return to the welcome screen momentarily."
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
	}
}
