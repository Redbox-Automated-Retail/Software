using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<HelpViewModel> getHelpViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_help_document_view";
		AddSpeechPartInstructions(result, getHelpViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<HelpViewModel> getHelpViewModelFunction)
	{
		HelpViewModel viewModel = getHelpViewModelFunction();
		if (viewModel != null)
		{
			ISpeechPart speechPart = new SpeechPart
			{
				Name = "Instructions",
				Sequence = 0,
				KeySequenceDelay = 0,
				Loop = true,
				StartPause = 250,
				EndPause = 5000
			};
			speechPart.Refresh = delegate
			{
				speechPart.Clear();
				speechPart.Texts.Add(new TextPart
				{
					Text = "The title of this help document is " + viewModel.HelpDocumentInfoList[viewModel.CurrentHelpDocument].AbeTitle
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the tab key for document content."
				});
				AddKeyPressActions(viewModel, speechPart, "1");
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the minus key to go to the previous page."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the plus key to go to the next page."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the cancel key to go back to the previous screen."
				});
			};
			speechControl.SpeechParts.Add(speechPart);
			ISpeechPart speechPart2 = new SpeechPart
			{
				Name = "Instructions",
				Sequence = 1,
				KeySequenceDelay = 0,
				Loop = true,
				StartPause = 250,
				EndPause = 5000
			};
			speechPart2.Refresh = delegate
			{
				speechPart2.Clear();
				string text = $"Page {viewModel.CurrentHelpDocumentPage} of {viewModel.HelpDocumentPageCount} in document {viewModel.HelpDocumentInfoList[viewModel.CurrentHelpDocument].AbeTitle} is as follows:";
				speechPart2.Texts.Add(new TextPart
				{
					Text = text
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = viewModel.CurrentPageText
				});
				AddKeyPressActions(viewModel, speechPart2, "0");
				speechPart2.Texts.Add(new TextPart
				{
					Text = "Press the minus key to go to the previous page."
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = "Press the plus key to go to the next page."
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = "Press the tab key for document options."
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = "Press the cancel key to go back to the previous screen."
				});
			};
			speechControl.SpeechParts.Add(speechPart2);
		}
	}

	private static void AddKeyPressActions(HelpViewModel viewModel, ISpeechPart speechPart, string tabKeySpeechPart)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = delegate
			{
				viewModel.ProcessGoBack();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "0",
			Action = delegate
			{
				viewModel.ProcessGoBack();
			}
		});
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "TAB",
			Function = "TTSSkipToSpeechPart",
			Value = tabKeySpeechPart
		});
		int num = 1;
		foreach (HelpButtonData button in viewModel.ButtonList)
		{
			if (button.DocumentToShow != viewModel.CurrentHelpDocument)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = num.ToString(),
					Function = "TTSSkipToSpeechPart",
					Value = "1",
					Action = delegate
					{
						viewModel.ProcessHelpDocumentButtonPress(button.DocumentToShow);
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press " + num + " to change the document to " + viewModel.HelpDocumentInfoList[button.DocumentToShow].AbeTitle + "."
				});
			}
			num++;
		}
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "MINUS",
			Function = "TTSSkipToSpeechPart",
			Value = "1",
			Action = delegate
			{
				if (viewModel.CurrentHelpDocumentPage > 1)
				{
					viewModel.CurrentHelpDocumentPage--;
				}
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "PLUS",
			Function = "TTSSkipToSpeechPart",
			Value = "1",
			Action = delegate
			{
				if (viewModel.CurrentHelpDocumentPage < viewModel.HelpDocumentPageCount)
				{
					viewModel.CurrentHelpDocumentPage++;
				}
			}
		});
	}
}
