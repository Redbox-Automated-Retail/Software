using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SignInView")]
public partial class SignInViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private SignInViewModel SignInViewModel => base.DataContext as SignInViewModel;

	public SignInViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return SignInViewModel?.ProcessGetSpeechControl();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		theme?.SetStyle(EmailAndPasswordButton);
		theme?.SetStyle(PhoneAndPinButton);
		theme?.SetStyle(CancelButton);
	}
}
