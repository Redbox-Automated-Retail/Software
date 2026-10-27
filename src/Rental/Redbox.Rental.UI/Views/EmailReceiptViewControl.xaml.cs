using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "EmailReceiptView")]
public partial class EmailReceiptViewControl : ViewUserControl, IComponentConnector
{
	public EmailReceiptViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "email_receipt_view";
	}

	protected override void DataContextHasChanged()
	{
		bool flag = GetModel<SimpleModel>()?.BannerImage != null;
		AdImage.Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		SimpleModel model = GetModel<SimpleModel>();
		speechPart.Texts.Add(new TextPart
		{
			Text = model?.TitleText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = model?.MessageText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = model.CommentText
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
				ViewUserControl.InvokeRoutedCommand(GetModel<SimpleModel>()?.ContinueButtonCommand);
			}
		});
		speechPart.Loop = true;
	}
}
