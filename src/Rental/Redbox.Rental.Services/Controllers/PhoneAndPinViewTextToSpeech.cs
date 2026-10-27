using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PhoneAndPinViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForPhoneAndPin = "CheckForPhoneAndPin";

		public const string HasNoDigitWithInstructions = "HasNoDigitWithInstructions";

		public const string HasDigitWithInstructions = "HasDigitWithInstructions";

		public const string HasDigitWithoutInstructions = "HasDigitWithoutInstructions";

		public const string Terms = "Terms";

		public const string JoinTextClub = "JoinTextClub";
	}

	public static ISpeechControl GetSpeechControl(Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_phone_and_pin_view";
		AddSpeechPartCheckForPhoneAndPin(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasNoDigitWithInstructions(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasDigitWithInstructions(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasDigitWithoutInstructions(result, getPhoneAndPinViewModelFunction);
		PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
		if (phoneAndPinViewModel.TermsBarVisibility == Visibility.Visible)
		{
			AddSpeechPartTerms(result, getPhoneAndPinViewModelFunction);
		}
		if (phoneAndPinViewModel.TextClubOptionVisibility == Visibility.Visible)
		{
			AddSpeechPartJoinTextClub(result, getPhoneAndPinViewModelFunction);
		}
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForPhoneAndPin(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "CheckForPhoneAndPin",
			Sequence = 0,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				string text = ((phoneAndPinViewModel.PhoneDisplayText.Length > 0 && phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0) ? "HasDigitWithInstructions" : "HasNoDigitWithInstructions");
				speechPart.RegularExpression = new RegularExpression
				{
					Success = text,
					Failure = text
				};
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasNoDigitWithInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
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
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				AddPlusKeyAction(speechPart, phoneAndPinViewModel);
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = phoneAndPinViewModel.HeaderLabelText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = phoneAndPinViewModel.SubHeadLabelText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Please enter your phone number and pin with the help of the keypad."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The keypad is located below the screen."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The numbers are arranged in a standard telephone keypad pattern."
				});
				if (phoneAndPinViewModel.PinConfirmVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing plus switches between phone, pin and confirm pin."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing cancel will go back without signing up."
					});
				}
				else
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing plus switches between phone and pin."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing cancel will go back without signing in."
					});
				}
				AddTabKeyActionForTerms(speechPart, phoneAndPinViewModel, "Press Tab to hear terms and conditions.");
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddPlusKeyAction(ISpeechPart speechPart, PhoneAndPinViewModel phoneAndPinViewModel)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "PLUS",
			Action = delegate
			{
				if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
				{
					phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
				}
				else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
				{
					if (phoneAndPinViewModel.PinConfirmVisibility == Visibility.Visible)
					{
						phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.PinConfirm);
					}
					else
					{
						phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
					}
				}
				else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
				{
					phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
				}
			}
		});
	}

	private static void AddSpeechPartHasDigitWithInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
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
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				AddPlusKeyAction(speechPart, phoneAndPinViewModel);
				AddEnterKeyAction(speechPart, phoneAndPinViewModel, "HasDigitWithoutInstructions");
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnBackButtonClicked();
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
						phoneAndPinViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				if (phoneAndPinViewModel.PhoneDisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {phoneAndPinViewModel.TTSPhoneDisplayText} as your phone number."
					});
				}
				if (phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {phoneAndPinViewModel.TTSPinDisplayText} as your pin."
					});
				}
				if (phoneAndPinViewModel.PinConfirmDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinConfirmDisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {phoneAndPinViewModel.TTSPinConfirmDisplayText} as your pin confirmation."
					});
				}
				if (phoneAndPinViewModel.IsSignInButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "If these are correct, press the enter key."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing clear clears all the digits from the current field."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To remove a single digit, press the minus key, which acts as the backspace key."
				});
				if (phoneAndPinViewModel.PinConfirmVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing plus switches between phone, pin and confirm pin."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing cancel will go back without signing up."
					});
				}
				else
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing plus switches between phone and pin."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing cancel will go back without signing in."
					});
				}
				AddTabKeyActionForTerms(speechPart, phoneAndPinViewModel, "Press Tab to hear terms and conditions.");
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddEnterKeyAction(ISpeechPart speechPart, PhoneAndPinViewModel phoneAndPinViewModel, string nextSpeechPart)
	{
		if (phoneAndPinViewModel.TextClubOptionVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
				},
				Function = "TTSSkipToSpeechPart",
				Value = "JoinTextClub",
				Text = "ENTER"
			});
			return;
		}
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				if (phoneAndPinViewModel.IsSignInButtonEnabled)
				{
					phoneAndPinViewModel.ProcessOnSignInButtonClicked();
				}
			},
			Function = "TTSSkipToSpeechPart",
			Value = nextSpeechPart,
			Text = "ENTER"
		});
	}

	private static void AddTabKeyActionForTerms(ISpeechPart speechPart, PhoneAndPinViewModel phoneAndPinViewModel, string instructions)
	{
		if (phoneAndPinViewModel.TermsBarVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "TAB",
				Action = delegate
				{
				},
				Function = "TTSSkipToSpeechPart",
				Value = "Terms",
				Text = "tab"
			});
			if (!string.IsNullOrEmpty(instructions))
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = instructions
				});
			}
		}
	}

	private static void AddSpeechPartHasDigitWithoutInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
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
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				AddPlusKeyAction(speechPart, phoneAndPinViewModel);
				AddEnterKeyAction(speechPart, phoneAndPinViewModel, "HasDigitWithInstructions");
				AddTabKeyActionForTerms(speechPart, phoneAndPinViewModel, string.Empty);
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnBackButtonClicked();
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
						phoneAndPinViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
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
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartTerms(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Terms",
			Sequence = 4,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 100,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel viewModel = getPhoneAndPinViewModelFunction();
			if (viewModel != null)
			{
				string value = "HasNoDigitWithInstructions";
				if (!string.IsNullOrEmpty(viewModel.PhoneDisplayText) || !string.IsNullOrEmpty(viewModel.PinConfirmDisplayText) || !string.IsNullOrEmpty(viewModel.PinDisplayText))
				{
					value = "HasDigitWithInstructions";
				}
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = value,
					Text = "cancel"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						viewModel.ProcessOnTermsButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = value,
					Text = "Terms"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To return to phone and pin entry press the cancel key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To hear the complete terms and conditions document press 1."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.TermsTitleText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.TermsText
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartJoinTextClub(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "JoinTextClub",
			Sequence = 5,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 100,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel viewModel = getPhoneAndPinViewModelFunction();
			if (viewModel != null)
			{
				string value = "HasNoDigitWithInstructions";
				if (!string.IsNullOrEmpty(viewModel.PhoneDisplayText) || !string.IsNullOrEmpty(viewModel.PinConfirmDisplayText) || !string.IsNullOrEmpty(viewModel.PinDisplayText))
				{
					value = "HasDigitWithInstructions";
				}
				if (viewModel.SignUpForTextClub)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "2",
						Action = delegate
						{
							viewModel.CheckMarkVisibility = Visibility.Hidden;
						},
						Function = "TTSRepeatSpeechPart",
						Text = "2"
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "If you do not wish to receive text messages with special deals press 2."
					});
				}
				else
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "1",
						Action = delegate
						{
							viewModel.CheckMarkVisibility = Visibility.Visible;
						},
						Function = "TTSRepeatSpeechPart",
						Text = "1"
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To join our text club to get special deals press 1."
					});
				}
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = value,
					Text = "cancel"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						if (viewModel.IsSignInButtonEnabled)
						{
							viewModel.ProcessOnSignInButtonClicked();
						}
					},
					Function = "TTSSkipToSpeechPart",
					Value = value,
					Text = "ENTER"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "If everything is correct, press the enter key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To return to phone and pin entry press the cancel key."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
