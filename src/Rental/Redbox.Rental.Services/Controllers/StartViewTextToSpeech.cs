using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class StartViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Welcome = "Welcome";

		public const string Jump = "Jump";

		public const string NarrationSpeed = "NarrationSpeed";

		public const string KeypadInstructions = "KeypadInstructions";

		public const string Buttons = "Buttons";
	}

	public static ISpeechControl GetSpeechControl(Func<StartViewModel> getViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_start_view";
		AddSpeechPartWelcome(result, getViewModelFunction);
		AddSpeechPartJump(result);
		AddSpeechPartNarrationSpeed(result, getViewModelFunction);
		AddSpeechPartKeypadInstructions(result, getViewModelFunction);
		AddSpeechPartButtons(result, getViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartWelcome(ISpeechControl speechControl, Func<StartViewModel> getViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Welcome",
			Sequence = 0,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 250,
			EndPause = 3000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			if (getViewModelFunction() != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSSkipToSpeechPart",
					Value = "4"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "6",
					Function = "TTSVolumeUp"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "4",
					Function = "TTSVolumeDown"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "5",
					Function = "TTSSkipToSpeechPart",
					Value = "2"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Welcome to Redbox!\r\n                                                        Please adjust your audio volume - Press 6 to increase the\r\n                                                        volume, Press 4 to decrease the volume. Press 5 to continue."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartJump(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Jump"
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.RegularExpression = new RegularExpression
			{
				Success = "3",
				Failure = "3"
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartNarrationSpeed(ISpeechControl speechControl, Func<StartViewModel> getViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "NarrationSpeed",
			Sequence = 2,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 0,
			EndPause = 3000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			if (getViewModelFunction() != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSSkipToSpeechPart",
					Value = "4"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "6",
					Function = "TTSFaster"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "4",
					Function = "TTSSlower"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "5",
					Function = "TTSSkipToSpeechPart",
					Value = "3"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "You can adjust the narration speed. Press 6 to increase speed,\r\n\t                                                    \t\t\tPress 4 to decrease speed. Press 5 to continue.."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartKeypadInstructions(ISpeechControl speechControl, Func<StartViewModel> getViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "KeypadInstructions",
			Sequence = 3,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			if (getViewModelFunction() != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSSkipToSpeechPart",
					Value = "4"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To skip these instructions in the future or move forward, you can press the tab key anytime.\r\n                                    the alpha numeric keypad consists of sixteen keys, four across and four down.\r\n                                    the three left columns of the keypad are identical to a standard touch-tone telephone keypad and an ATM keypad. The 5 key is marked with a raised dot.\r\n                                    to enter alphabetical entries, you can press the number key multiple times.\r\n                                    the minus key is located on the bottom of the first column and is marked with a raised minus.\r\n                                    The plus key is located on the bottom of the third column and is marked with a raised +.\r\n                                    the far right side of the keypad contains a column of important keys.\r\n                                    the tab key is at the top and is blue in color with no raised marking.\r\n                                    the clear key is the second key down and is yellow in color and marked with a raised back arrow.\r\n                                    the cancel key is the third key down and is red in color and marked with a raised x.\r\n                                    the enter key is the last key in the fourth column and is green in color and marked with a raised o.\r\n                                    The credit card reader is located above the keypad.\r\n                                    The DVD vending slot is to the right of the credit card reader.\r\n                                    You are all set to use the Redbox!"
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartButtons(ISpeechControl speechControl, Func<StartViewModel> getViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Buttons",
			Sequence = 4,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			StartViewModel viewModel = getViewModelFunction();
			if (viewModel != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSNextAudioSnippet"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						viewModel.ProcessRentMovieButtonClick();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To Rent Movies, Press 1."
				});
				if (viewModel.BuyMoviesEnabled)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "3",
						Action = delegate
						{
							viewModel.ProcessBuyMoviesButtonClick();
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To Purchase Movies, Press 3."
					});
				}
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						viewModel.ProcessPickupButtonClick();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "For Pick Up, Press 5."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						viewModel.ProcessReturnButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "For return, Press 6."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						viewModel.ProcessHelpButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "For help, Press 7."
				});
				if (viewModel.SignInButtonEnabled)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "8",
						Action = delegate
						{
							viewModel.ProcessSignInButtonClicked();
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To Sign In, Press 8."
					});
				}
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "0",
					Function = "TTSSkipToSpeechPart",
					Value = "Welcome"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To start over, Press 0."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
