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

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "JoinTextClubPromoView")]
public partial class JoinedTextClubViewControl : ViewUserControl, IComponentConnector
{
	private SimpleModel Model { get; set; }

	public JoinedTextClubViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "join_text_club_promo_view";
	}

	protected override void DataContextHasChanged()
	{
		if (base.DataContext != null && base.DataContext is SimpleModel)
		{
			Model = base.DataContext as SimpleModel;
		}
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.TitleText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.MessageText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = "To continue press ENTER"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Function = "TTSRepeatSpeechPart",
			Action = delegate
			{
				ViewUserControl.InvokeRoutedCommand(Model.ContinueButtonCommand);
			}
		});
		speechPart.Loop = true;
	}
}
