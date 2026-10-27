using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class ZipCodeViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForZipCode = "CheckForZipCode";

		public const string HasNoDigitWithInstructions = "HasNoDigitWithInstructions";

		public const string HasDigitWithInstructions = "HasDigitWithInstructions";

		public const string HasDigitWithoutInstructions = "HasDigitWithoutInstructions";
	}

	public static ISpeechControl GetSpeechControl(Func<ZipCodeViewModel> getZipCodeViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_zip_code_view";
		AddSpeechPartCheckForZipCode(result, getZipCodeViewModelFunction);
		AddSpeechPartHasNoDigitWithInstructions(result, getZipCodeViewModelFunction);
		AddSpeechPartHasDigitWithInstructions(result, getZipCodeViewModelFunction);
		AddSpeechPartHasDigitWithoutInstructions(result, getZipCodeViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForZipCode(ISpeechControl speechControl, Func<ZipCodeViewModel> getZipCodeViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "CheckForZipCode",
			Sequence = 0,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ZipCodeViewModel zipCodeViewModel = getZipCodeViewModelFunction();
			if (zipCodeViewModel != null)
			{
				string text = ((zipCodeViewModel.DisplayText.Length > 0) ? "HasDigitWithInstructions" : "HasNoDigitWithInstructions");
				speechPart.RegularExpression = new RegularExpression
				{
					Success = text,
					Failure = text
				};
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasNoDigitWithInstructions(ISpeechControl speechControl, Func<ZipCodeViewModel> getZipCodeViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasNoDigitWithInstructions",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ZipCodeViewModel zipCodeViewModel = getZipCodeViewModelFunction();
			if (zipCodeViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Please enter your zip code with the help of the keypad."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The zip code associated with your credit card is needed for verification purposes only."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The keypad is located below the screen."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The numbers are arranged in a standard telephone keypad pattern."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasDigitWithInstructions(ISpeechControl speechControl, Func<ZipCodeViewModel> getZipCodeViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasDigitWithInstructions",
			Sequence = 2,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ZipCodeViewModel zipCodeViewModel = getZipCodeViewModelFunction();
			if (zipCodeViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						if (zipCodeViewModel.IsNextButtonEnabled)
						{
							zipCodeViewModel.ProcessOnNextButtonClicked(zipCodeViewModel.DisplayText);
						}
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "ENTER"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnBackButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "BACK"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CLEAR",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "CLEAR ALL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				if (zipCodeViewModel.DisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {zipCodeViewModel.TTSDisplayText} as your zip code."
					});
				}
				if (zipCodeViewModel.IsNextButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "If this is correct, press the enter key."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing clear clears all the digits from the zip code field."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To remove a single digit, press the minus key, which acts as the backspace key."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasDigitWithoutInstructions(ISpeechControl speechControl, Func<ZipCodeViewModel> getZipCodeViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasDigitWithoutInstructions",
			Sequence = 3,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ZipCodeViewModel zipCodeViewModel = getZipCodeViewModelFunction();
			if (zipCodeViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						if (zipCodeViewModel.IsNextButtonEnabled)
						{
							zipCodeViewModel.ProcessOnNextButtonClicked(zipCodeViewModel.DisplayText);
						}
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithInstructions",
					Text = "ENTER"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnBackButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "BACK"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CLEAR",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "CLEAR ALL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						zipCodeViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
