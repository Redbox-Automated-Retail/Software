using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.ControlPanel.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ControlPanelLoginView")]
public partial class LoginViewControl : ViewControl, IComponentConnector
{
	public LoginViewControl()
	{
		InitializeComponent();
		InitVisibilityHasChanged();
	}

	protected override void VisibleHasChanged()
	{
		if (base.IsVisible)
		{
			SetFocus(UserIdElem);
		}
	}

	private void LoginButton_Pressed(object sender, RoutedEventArgs e)
	{
		LoginModel model = GetModel<LoginModel>();
		if (model != null)
		{
			model.EnableLogin = false;
			ViewControl.InvokeRoutedCommand(model.ContinueButtonCommand, new UserData
			{
				Id = UserIdElem.Text,
				Password = PasswordElem.Password
			});
		}
	}

	private void CloseButton_Pressed(object sender, RoutedEventArgs e)
	{
		LoginModel model = GetModel<LoginModel>();
		if (model != null)
		{
			ViewControl.InvokeRoutedCommand(model.CancelButtonCommand);
		}
	}

	private void ClearErrorMessage()
	{
		if (!string.IsNullOrEmpty(ErrorTextElem.Text))
		{
			LoginModel model = GetModel<LoginModel>();
			if (model != null)
			{
				model.ErrorMessage = "";
			}
		}
	}

	private void UserIdElem_KeyUp(object sender, KeyEventArgs e)
	{
		ClearErrorMessage();
	}

	private void PasswordElem_KeyUp(object sender, KeyEventArgs e)
	{
		ClearErrorMessage();
	}

	private void LoginButtonElem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(UserIdElem.Text) && !string.IsNullOrWhiteSpace(PasswordElem.Password))
		{
			CloseButtonElem.IsEnabled = LoginButtonElem.IsEnabled;
			SpinElem.Visibility = (LoginButtonElem.IsEnabled ? Visibility.Collapsed : Visibility.Visible);
			return;
		}
		LoginModel model = GetModel<LoginModel>();
		if (model != null)
		{
			model.ErrorMessage = "Please enter UserName and Password";
		}
	}
}
