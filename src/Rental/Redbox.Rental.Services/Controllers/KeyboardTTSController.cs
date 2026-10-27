using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.Services.Controllers;

public class KeyboardTTSController : BaseTTSController<KeyboardTTSController, KeyboardLogic>
{
	private class TTSConstants
	{
		public const string Keyboard = "Keyboard";
	}

	private static string Prompt { get; set; }

	private static bool TextMode { get; set; } = true;

	public static void AddSpeechParts(ISpeechControl speechControl, KeyboardLogic model)
	{
		BaseTTSController<KeyboardTTSController, KeyboardLogic>.StopIdleTimer();
		TextMode = model.InitControlTextMode();
		AddSpeechPartKeyboard(speechControl, model);
	}

	private static string GetCharacterMessage(string code, string character, KeyboardLogic keyboardLogic)
	{
		string text = (TextMode ? keyboardLogic.Model.KeyboardText : keyboardLogic.Model.KeyboardCode);
		string result = null;
		if (character == "pound sign")
		{
			if (!string.IsNullOrEmpty(text))
			{
				switch (code)
				{
				case "PLUS0":
				case "PLUS1":
				case "PLUS2":
				case "PLUS3":
					result = $"The email domain you have selected is {text}.";
					break;
				case "PLUS4":
					result = $"The domain text you have selected is {text}.";
					break;
				case "PLUS8":
					result = $"The text you have entered is {text}.";
					break;
				case "PLUS9":
				{
					string text2 = "";
					string text3 = text;
					for (int i = 0; i < text3.Length; i++)
					{
						text2 += ConvertToWord(text3[i].ToString());
					}
					result = $"The characters you entered are {text2}.";
					break;
				}
				}
			}
		}
		else
		{
			result = $"You have selected the keyboard character {character}.";
		}
		return result;
	}

	private static string ConvertToWord(string character)
	{
		switch (character)
		{
		case " ":
			return "space";
		case "_":
			return "underscore";
		case "-":
			return "hyphen";
		case ".":
			return "dot";
		case "!":
			return "exclamation mark";
		case "#":
			return "pound sign";
		case "$":
			return "dollar sign";
		case "%":
			return "percent sign";
		case "^":
			return "caret";
		case "&":
			return "ampersand";
		case "*":
			return "asterisk";
		case "(":
			return "left parenthesis";
		case ")":
			return "right parenthesis";
		case "?":
			return "question mark";
		case "<":
			return "less-than sign";
		case ">":
			return "greater-than sign";
		case "~":
			return "tilde sign";
		case "`":
			return "acute sign";
		case "+":
			return "plus sign";
		case "=":
			return "equal sign";
		case "{":
			return "open brace";
		case "}":
			return "close brace";
		case "[":
			return "open bracket";
		case "]":
			return "closed bracket";
		case "|":
			return "pipe";
		case "\\":
			return "backslash";
		case "/":
			return "forward slash";
		case ":":
			return "colon";
		case ";":
			return "semicolon";
		case "\"":
			return "double quote mark";
		case "'":
			return "single quote mark";
		case ",":
			return "comma";
		case "POUND":
			return "#";
		default:
			if (character.Length == 1)
			{
				if (char.IsLetter(character[0]) && char.IsUpper(character[0]))
				{
					return string.Format(", capital {0}{1}", character, (character == "A") ? "," : "");
				}
				return $", '{character}'";
			}
			return character;
		}
	}

	private static void AddAlphaCharacterEvent(ISpeechPart speechPart, KeyboardLogic keyboardLogic, string keyCode, string alphaCharacter)
	{
		bool specialKey = false;
		bool commandKey = false;
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = keyCode,
			Action = delegate
			{
				if (alphaCharacter != "#")
				{
					if (keyboardLogic.KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
					{
						switch (alphaCharacter)
						{
						case "CODE":
							keyboardLogic.SetControlTextMode(mode: false);
							commandKey = true;
							break;
						case "MAIL":
							keyboardLogic.SetControlTextMode(mode: true);
							commandKey = true;
							break;
						case "@gmail.com":
						case "@yahoo.com":
						case "@hotmail.com":
						case "@aol.com":
							keyboardLogic.DomainButtonCommand_Execute(alphaCharacter);
							specialKey = true;
							break;
						case ".com":
							keyboardLogic.DotComButtonCommand_Execute(alphaCharacter);
							specialKey = true;
							break;
						case "POUND":
							keyboardLogic.KeyButtonCommand_Execute("#");
							break;
						default:
							keyboardLogic.KeyButtonCommand_Execute(alphaCharacter);
							break;
						}
					}
					else
					{
						keyboardLogic.KeyButtonCommand_Execute(alphaCharacter);
					}
				}
			},
			ActionText = delegate
			{
				if (specialKey && keyboardLogic.Model.KeyboardText.Length == 0)
				{
					return "Your selection could not be entered. Please enter an email name, then a domain selection.";
				}
				if (commandKey)
				{
					TextMode = keyboardLogic.GetControlTextMode();
					if (!TextMode)
					{
						return "Please enter your password.";
					}
					return "Please enter your email address.";
				}
				return GetCharacterMessage(keyCode, ConvertToWord(alphaCharacter), keyboardLogic);
			}
		});
	}

	private static void AddStandardKeyPress(ISpeechPart speechPart, KeyboardLogic keyboardLogic)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			Action = delegate
			{
				if (keyboardLogic.ValidateKeyboardText())
				{
					keyboardLogic.ContinueButtonExecute();
				}
				else
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = keyboardLogic.Model.KeyboardError
					});
				}
			},
			KeyCode = "ENTER",
			Function = "TTSRepeatSpeechPart",
			Value = "Keyboard"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			Action = delegate
			{
				keyboardLogic.BackButtonCommand_Execute();
			},
			ActionText = () => (keyboardLogic.Model.KeyboardText.Length <= 0) ? null : "You have removed the last character entered.",
			KeyCode = "MINUS",
			Function = "TTSNextSpeechPart",
			Value = "Keyboard"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			Action = delegate
			{
				keyboardLogic.ClearButtonCommand_Execute();
			},
			ActionText = () => (keyboardLogic.Model.KeyboardText.Length != 0) ? null : "You have removed all characters entered.",
			KeyCode = "CLEAR",
			Function = "TTSNextSpeechPart",
			Value = "Keyboard"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			Action = delegate
			{
				keyboardLogic.CancelButtonCommand_Execute();
			},
			KeyCode = "CANCEL",
			Function = "TTSRepeatSpeechPart",
			Value = "Keyboard"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			Action = delegate
			{
				keyboardLogic.Instructions = false;
				if (!keyboardLogic.Instructions)
				{
					Prompt = keyboardLogic.Model.MessageText;
				}
			},
			KeyCode = "TAB",
			Function = "TTSRepeatSpeechPart",
			Value = "Keyboard"
		});
		if (keyboardLogic.KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
		{
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS0", "@yahoo.com");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS1", "@gmail.com");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS2", "@hotmail.com");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS3", "@aol.com");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS4", ".com");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS", "@");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS", ".");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS0", "_");
		}
		if (keyboardLogic.KeyboardMode == KeyboardMode.ACCOUNT_SIGN_IN)
		{
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS5", "MAIL");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS6", "CODE");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS1", "!");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS2", "~");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS3", "POUND");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS4", "$");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS5", "%");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS6", "^");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS7", "&");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS8", "*");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS9", "(");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS10", ")");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS11", "=");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS12", "[");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS13", "]");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS14", "{");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS15", "}");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS16", "|");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS17", "\\");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS18", "/");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS19", ";");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS20", ":");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS21", "'");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS22", "\"");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS23", ",");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS24", "<");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS25", ">");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS26", "?");
			AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUS27", "`");
		}
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUSPLUS", "+");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUSPLUSMINUS", "-");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS8", "#");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS9", "#");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "00", " ");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "0", "0");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "1", "1");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "2", "2");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "22", "a");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS22", "A");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "222", "b");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS222", "B");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "2222", "c");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS2222", "C");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "3", "3");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "33", "d");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS33", "D");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "333", "e");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS333", "E");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "3333", "f");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS3333", "F");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "4", "4");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "44", "g");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS44", "G");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "444", "h");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS444", "H");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "4444", "i");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS4444", "I");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "5", "5");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "55", "j");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS55", "J");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "555", "k");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS555", "K");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "5555", "l");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS5555", "L");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "6", "6");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "66", "m");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS66", "M");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "666", "n");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS666", "N");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "6666", "o");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS6666", "O");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "7", "7");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "77", "p");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS77", "P");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "777", "q");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS777", "Q");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "7777", "r");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS7777", "R");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "77777", "s");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS77777", "S");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "8", "8");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "88", "t");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS88", "T");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "888", "u");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS888", "U");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "8888", "v");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS8888", "V");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "9", "9");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "99", "w");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS99", "W");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "999", "x");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS999", "X");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "9999", "y");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS9999", "Y");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "99999", "z");
		AddAlphaCharacterEvent(speechPart, keyboardLogic, "PLUS99999", "Z");
	}

	private static void AddSpeechPartKeyboard(ISpeechControl speechControl, KeyboardLogic model)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Keyboard",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			KeyboardLogic keyboardLogic = model;
			if (keyboardLogic != null)
			{
				AddStandardKeyPress(speechPart, keyboardLogic);
				if (keyboardLogic.Instructions)
				{
					string text;
					string text2;
					string text3;
					switch (keyboardLogic.KeyboardMode)
					{
					case KeyboardMode.PROMOTIONS_CODE:
						text = "PROMO CODE";
						text2 = "Please enter your promo code.";
						text3 = "";
						break;
					case KeyboardMode.EMAIL_RECEIPTS_PROMO:
						text = "EMAIL ADDRESS";
						text2 = "Please enter your email address if you'd like to receive receipts and promotions.";
						text3 = "";
						break;
					case KeyboardMode.ACCOUNT_SIGN_IN:
						text = "";
						text2 = "Sign in to something awesome.";
						text3 = "";
						break;
					case KeyboardMode.PERKS_SIGN_UP:
						text = "Sign up for Redbox Perks";
						text2 = "Please enter the email address you would like to use for Redbox Perks";
						text3 = "Don't worry, we'll never share your info - it's safe with us";
						break;
					default:
						text = "EMAIL ADDRESS";
						text2 = "What email address would you like your reminder sent to? If you ask for other reminders during this visit, we'll send them here too.";
						text3 = "Redbox will never sell your email address to any third parties. We promise.";
						break;
					}
					string keyboardError = keyboardLogic.Model.KeyboardError;
					if (!string.IsNullOrEmpty(text))
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = text
						});
					}
					if (!string.IsNullOrEmpty(text2))
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = text2
						});
					}
					if (!string.IsNullOrEmpty(keyboardError))
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = $"Whoops, there was an error: {keyboardError}"
						});
					}
					speechPart.Texts.Add(new TextPart
					{
						Text = "Once you are done, press the ENTER key."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Anytime if you want to skip through the instructions, press the tab key."
					});
					if (!string.IsNullOrEmpty(text3))
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = text3
						});
					}
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing the number 2 twice, will give you lower case A"
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing PLUS, 2, 2, will give you a capital A"
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "For the Space character, press the 0 key 2 times."
					});
					if (keyboardLogic.KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the at sign character, press the PLUS key."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the dot character, press PLUS twice."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the underscore character, press the PLUS twice, then 0."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "Pressing clear, clears the {{keyboardLogic.TitleText}} field entirely."
						});
					}
					speechPart.Texts.Add(new TextPart
					{
						Text = "For the hyphen character, press PLUS twice, then MINUS."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "The minus key acts as the backspace key and removes the preceding key entry."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To hear the entered text, press PLUS eight."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To hear the entered characters, press PLUS nine."
					});
					if (keyboardLogic.KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = "When entering your email address you may add a common domain to your email name quickly."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To add the domain @YAHOO.COM to the entered text, press PLUS zero."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To add the domain @GMAIL.COM to the entered text, press PLUS one."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To add the domain @HOTMAIL.COM to the entered text, press PLUS two."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To add the domain @AOL.COM to the entered text, press PLUS three."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To add the domain dot COM to the entered text, press PLUS four."
						});
					}
					if (keyboardLogic.KeyboardMode == KeyboardMode.ACCOUNT_SIGN_IN)
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = "To enter your email address, press PLUS five."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "To enter your password, press PLUS six."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "When entering your password, the following special characters are avalible."
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the exclamation mark character, PLUS twice, then 1"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the tilde sign character, PLUS twice, then 2"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the pound sign character, PLUS twice, then 3"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the dollar sign character, PLUS twice, then 4"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the percent sign character, PLUS twice, then 5"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the caret character, PLUS twice, then 6"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the ampersand character, PLUS twice, then 7"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the asterisk character, PLUS twice, then 8"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the left parenthesis character, PLUS twice, then 9"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the right parenthesis character, PLUS twice, then 10"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the equal sign character, PLUS twice, then 11"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the open bracket character, PLUS twice, then 12"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the close bracket character, PLUS twice, then 13"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the open brace character, PLUS twice, then 14"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the close brace character, PLUS twice, then 15"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the pipe sign character, PLUS twice, then 16"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the backslash character, PLUS twice, then 17"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the forward slash character, PLUS twice, then 18"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the semicolon sign character, PLUS twice, then 19"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the colon sign character, PLUS twice, then 20"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the single quote mark character, PLUS twice, then 21"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the double quote mark character, PLUS twice, then 22"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the comma character, PLUS twice, then 23"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the less-than sign character, PLUS twice, then 24"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the greater-than sign character, PLUS twice, then 25"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the question mark character, PLUS twice, then 26"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = "For the acute sign character, PLUS twice, then 27"
						});
					}
				}
				else if (!string.IsNullOrEmpty(Prompt))
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "You have turned instructions off."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = Prompt
					});
					Prompt = null;
				}
				BaseTTSController<KeyboardTTSController, KeyboardLogic>.StartIdleTimer();
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
