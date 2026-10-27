using System;
using System.Linq;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models;

public class PhoneAndPinViewModel : DependencyObject
{
	public enum DisplayType
	{
		Phone,
		Pin,
		PinConfirm
	}

	public Action<DisplayType> OnDisplayClicked;

	public Action OnStartButtonClicked;

	public Action OnSignInButtonClicked;

	public Action OnTermsButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	private string _pinDisplayText;

	private string _pinConfirmDisplayText;

	public static readonly DependencyProperty KeypadModelProperty = DependencyProperty.Register("KeypadModel", typeof(KeypadModel), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PhoneDisplayTextProperty = DependencyProperty.Register("PhoneDisplayText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SecurePinDisplayTextProperty = DependencyProperty.Register("SecurePinDisplayText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SecurePinConfirmDisplayTextProperty = DependencyProperty.Register("SecurePinConfirmDisplayText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ErrorMessageTextProperty = DependencyProperty.Register("ErrorMessageText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty MobilePhoneErrorMessageTextProperty = DependencyProperty.Register("MobilePhoneErrorMessageText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null, MobilePhoneErrorMessageChanged));

	public static readonly DependencyProperty MobilePhoneErrorVisibilityProperty = DependencyProperty.Register("MobilePhoneErrorVisibility", typeof(Visibility), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CheckMarkVisibilityProperty = DependencyProperty.Register("CheckMarkVisibility", typeof(Visibility), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty IsSignInButtonEnabledProperty = DependencyProperty.Register("IsSignInButtonEnabled", typeof(bool), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty IsStartButtonEnabledProperty = DependencyProperty.Register("IsStartButtonEnabled", typeof(bool), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(true)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DisplayTypeFocusProperty = DependencyProperty.Register("DisplayTypeFocus", typeof(DisplayType), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(DisplayType.Phone)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ConfirmPinButtonStateProperty = DependencyProperty.Register("ConfirmPinButtonState", typeof(int), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(0)
	{
		AffectsRender = true
	});

	public PhoneAndPinInfo PhoneAndPinInfo { get; set; }

	public int PhoneRequiredLength { get; set; }

	public int PinRequiredLength { get; set; }

	public string HeaderLabelText { get; set; }

	public string ADAPhoneButtonText { get; set; }

	public string ADAPinButtonText { get; set; }

	public string StartButtonText { get; set; }

	public string SignInButtonText { get; set; }

	public string SubHeadLabelText { get; set; }

	public string PhoneLabelText { get; set; }

	public string PinLabelText { get; set; }

	public string PinConfirmLabelText { get; set; }

	public Visibility PinConfirmVisibility { get; set; }

	public string TermsTitleText { get; set; }

	public string TermsText { get; set; }

	public Visibility TermsBarVisibility { get; set; }

	public Visibility TextClubOptionVisibility { get; set; }

	public string TextClubText { get; set; }

	public bool TextClubEnabled { get; set; }

	public string TermsButtonText { get; set; }

	public DynamicRoutedCommand CheckMarkCommand { get; set; }

	public bool SignUpForTextClub => CheckMarkVisibility == Visibility.Visible;

	public KeypadModel KeypadModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (KeypadModel)GetValue(KeypadModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeypadModelProperty, value);
			});
		}
	}

	public string PhoneDisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PhoneDisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhoneDisplayTextProperty, value);
			});
		}
	}

	public DisplayType DisplayTypeFocus
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DisplayType)GetValue(DisplayTypeFocusProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayTypeFocusProperty, value);
			});
		}
	}

	public string PinDisplayText
	{
		get
		{
			return _pinDisplayText;
		}
		set
		{
			_pinDisplayText = value;
			SecurePinDisplayText = GenerateSecurePinString(value);
		}
	}

	public string PinConfirmDisplayText
	{
		get
		{
			return _pinConfirmDisplayText;
		}
		set
		{
			_pinConfirmDisplayText = value;
			SecurePinConfirmDisplayText = GenerateSecurePinString(value);
		}
	}

	public string SecurePinDisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SecurePinDisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SecurePinDisplayTextProperty, value);
			});
		}
	}

	public string SecurePinConfirmDisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SecurePinConfirmDisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SecurePinConfirmDisplayTextProperty, value);
			});
		}
	}

	public string TTSPhoneDisplayText
	{
		get
		{
			string text = "";
			string phoneDisplayText = PhoneDisplayText;
			for (int i = 0; i < phoneDisplayText.Length; i++)
			{
				text = text + phoneDisplayText[i] + " ";
			}
			return text;
		}
	}

	public string TTSPinDisplayText
	{
		get
		{
			string text = "";
			string pinDisplayText = PinDisplayText;
			for (int i = 0; i < pinDisplayText.Length; i++)
			{
				text = text + pinDisplayText[i] + " ";
			}
			return text;
		}
	}

	public string TTSPinConfirmDisplayText
	{
		get
		{
			string text = "";
			string pinConfirmDisplayText = PinConfirmDisplayText;
			for (int i = 0; i < pinConfirmDisplayText.Length; i++)
			{
				text = text + pinConfirmDisplayText[i] + " ";
			}
			return text;
		}
	}

	public string ErrorMessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ErrorMessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ErrorMessageTextProperty, value);
			});
		}
	}

	public string MobilePhoneErrorMessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MobilePhoneErrorMessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MobilePhoneErrorMessageTextProperty, value);
			});
		}
	}

	public Visibility MobilePhoneErrorVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MobilePhoneErrorVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MobilePhoneErrorVisibilityProperty, value);
			});
		}
	}

	public bool IsSignInButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsSignInButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsSignInButtonEnabledProperty, value);
			});
		}
	}

	public bool IsStartButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsStartButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsStartButtonEnabledProperty, value);
			});
		}
	}

	public int ConfirmPinButtonState
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ConfirmPinButtonStateProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ConfirmPinButtonStateProperty, value);
			});
		}
	}

	public Visibility CheckMarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CheckMarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckMarkVisibilityProperty, value);
			});
		}
	}

	public void ProcessOnDisplayClicked(DisplayType displayType)
	{
		if (OnDisplayClicked != null)
		{
			OnDisplayClicked(displayType);
		}
	}

	public void ProcessOnStartButtonClicked()
	{
		if (OnStartButtonClicked != null)
		{
			IsSignInButtonEnabled = false;
			IsStartButtonEnabled = false;
			OnStartButtonClicked();
		}
	}

	public void ProcessOnSignInButtonClicked()
	{
		if (OnSignInButtonClicked != null)
		{
			IsSignInButtonEnabled = false;
			IsStartButtonEnabled = false;
			OnSignInButtonClicked();
		}
	}

	public void ProcessOnTermsButtonClicked()
	{
		if (OnTermsButtonClicked != null)
		{
			OnTermsButtonClicked();
		}
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}

	public void SetDefaultDisplays()
	{
		DisplayTypeFocus = DisplayType.Phone;
		PhoneDisplayText = string.Empty;
		PinDisplayText = Resources.phone_and_pin_default_pin_display;
		PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
		PinConfirmVisibility = Visibility.Collapsed;
		TextClubOptionVisibility = Visibility.Collapsed;
		TermsBarVisibility = Visibility.Collapsed;
		IsSignInButtonEnabled = false;
		IsStartButtonEnabled = true;
	}

	public void ObfuscatePins()
	{
		SecurePinDisplayText = string.Concat(SecurePinDisplayText.Select((char c) => (!char.IsDigit(c)) ? c : '*'));
		SecurePinConfirmDisplayText = string.Concat(SecurePinConfirmDisplayText.Select((char c) => (!char.IsDigit(c)) ? c : '*'));
	}

	private static void MobilePhoneErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is PhoneAndPinViewModel phoneAndPinViewModel)
		{
			phoneAndPinViewModel.MobilePhoneErrorVisibility = (string.IsNullOrEmpty(phoneAndPinViewModel.MobilePhoneErrorMessageText) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static int ValidPinCharacters(string value)
	{
		return value?.Count((char c) => char.IsDigit(c) || c.Equals('*') || c.Equals('_')) ?? 0;
	}

	private string GenerateSecurePinString(string value)
	{
		if (!(value == Resources.phone_and_pin_default_pin_display) && !string.IsNullOrWhiteSpace(value) && value.Length != 1)
		{
			return string.Concat(string.Concat(Enumerable.Repeat("* ", value.Length - 1)), value.Substring(value.Length - 1), string.Concat(Enumerable.Repeat(" _", PinRequiredLength - ValidPinCharacters(value))));
		}
		return string.Concat(value, string.Concat(Enumerable.Repeat(" _", PinRequiredLength - ValidPinCharacters(value))));
	}
}
