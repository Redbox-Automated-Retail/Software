using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class SignInViewModel : DependencyObject
{
	public BitmapImage HeaderImage { get; set; }

	public Style HeaderImageStyle { get; set; }

	public string HeaderLabelText { get; set; }

	public string AlreadyMemberText { get; set; }

	public string SignInLabelText { get; set; }

	public string EmailAndPasswordButtonText { get; set; }

	public string PhoneAndPinButtonText { get; set; }

	public string MobileSignInButtonText { get; set; }

	public string MobileMessageText { get; set; }

	public string MessageText { get; set; }

	public string RegisteredTrademarkSymbol { get; set; }

	public string Bullet1Text { get; set; }

	public string Bullet2Text { get; set; }

	public string Bullet3Text { get; set; }

	public string BottomMessageText { get; set; }

	public string SignUpButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public string InfoButtonText { get; set; }

	public string LegalText { get; set; }

	public Style ButtonTextStyle { get; set; }

	public string PrivacyText { get; set; }

	public string TermsButtonText { get; set; }

	public bool MobileSignInEnabled { get; set; }

	public Visibility MessageTextVisibility { get; set; }

	public Visibility BulletVisibility { get; set; }

	public Visibility InfoButtonVisibility { get; set; }

	public ICommand EmailAndPasswordButtonCommand { get; set; }

	public ICommand PhoneAndPinButtonCommand { get; set; }

	public ICommand CancelButtonCommand { get; set; }

	public ICommand SignUpButtonCommand { get; set; }

	public ICommand InfoButtonCommand { get; set; }

	public ICommand TermsButtonCommand { get; set; }

	public ICommand MobileSignInButtonCommand { get; set; }

	public Func<ISpeechControl> OnGetSpeechControl { get; set; }

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
