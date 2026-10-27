using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ControlPanelViewController : BaseControlPanelController
{
	private static readonly string LoginIdleTimerName = "";

	private const int LOCAL_ACCOUNT_EXPIRE = 14;

	public static void ConfigureLoginView(LoginInfo loginInfo)
	{
		int timeOutTime = 60000;
		BaseControlPanelController.UpdateUserName("N/A");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			LoginModel loginModel = ((loginInfo.Model != null) ? loginInfo.Model : (loginInfo.Model = new LoginModel
			{
				Title = "Redbox Control Panel Login",
				ErrorMessage = ""
			}));
			loginModel.CancelAction = loginInfo.CancelAction;
			if (loginModel.CancelAction != null)
			{
				loginModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.CancelButtonCommand, CancelButtonCommand_Execute);
			}
			loginModel.ContinueAction = loginInfo.ContinueAction;
			if (loginModel.ContinueAction != null)
			{
				loginModel.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand<UserData>(Redbox.Rental.UI.Commands.ContinueButtonCommand, ContinueButtonCommand_Execute);
			}
			loginModel.AuthenticateAction = loginInfo.AuthenticateAction;
			viewUserControl.DataContext = loginModel;
			OrchestrationService.StartIdleTimer(timeOutTime, TimeOutAction, LoginIdleTimerName);
		}
	}

	public static void ClosingLoginView(LoginInfo loginInfo)
	{
	}

	private static string FailedLogin(string userName, string response)
	{
		string result = "Invalid UserName / Password";
		string message = $"Control Panel | LOGIN FAILED, reason: {response}";
		LogHelper.Instance.LogTo("UserLoginLog", message);
		LogHelper.Instance.LogTo("ControlPanelActionLog", message);
		if (response.Equals("MissingUser"))
		{
			result = "The specified user account does not exist.";
		}
		else if (response.Equals("AuthenticationFailed"))
		{
			result = "The user account could not be authenticated.";
		}
		else if (response.Equals("Error"))
		{
			result = "There was an error while authenticating, please try again.";
		}
		return result;
	}

	private static void SuccessfulLogin(UserData userData, string audit_message, bool isOnline)
	{
		BaseControlPanelController.UpdateUserName(userData.Id);
		BaseControlPanelController.InnerSendAuditEvent(userData.StoreNumber, "CP_Login", userData.Id, "CP Bundle", audit_message);
		string message = $"Control Panel | LOGIN SUCCESS for user : {userData.Id}, online = {isOnline.ToString()}";
		LogHelper.Instance.LogTo("UserLoginLog", message);
		LogHelper.Instance.LogTo("ControlPanelActionLog", message);
		if (!BaseControlPanelController.SecurityUserExists(userData.Id))
		{
			BaseControlPanelController.SecurityCreateUser(userData.Id, userData.Password, 14);
		}
		else
		{
			BaseControlPanelController.SecuritySlideExpirationDate(userData.Id, 14);
		}
	}

	public static void AuthenticationResult(IRemoteServiceResult result)
	{
		LoginModel viewModel = BaseViewController.GetViewModel<LoginModel>();
		if (viewModel == null)
		{
			return;
		}
		if (result != null)
		{
			if (result.Success)
			{
				SuccessfulLogin(viewModel.UserData, "Successful login into the Control Panel Application bundle.", isOnline: true);
				viewModel.ContinueAction?.Invoke();
			}
			else
			{
				ResultProperties resultProperties = BaseControlPanelController.ConvertResultProperties(result.Properties);
				if (!resultProperties.IsOnline || resultProperties.StandAlone)
				{
					if (!BaseControlPanelController.SecurityUserExists(resultProperties.UserName))
					{
						SuccessfulLogin(viewModel.UserData, "Successful login into the Control Panel Application bundle via backdoor.", isOnline: false);
						viewModel.ContinueAction?.Invoke();
					}
					else if (BaseControlPanelController.SecurityAuthenticate(resultProperties.UserName, resultProperties.Password))
					{
						SuccessfulLogin(viewModel.UserData, "Successful login into the Control Panel Application bundle using local account.", isOnline: false);
						viewModel.ContinueAction?.Invoke();
					}
					else
					{
						viewModel.ErrorMessage = FailedLogin(resultProperties.UserName, "AuthenticationFailed");
					}
				}
				else
				{
					viewModel.ErrorMessage = FailedLogin(resultProperties.UserName, resultProperties.Response);
				}
			}
		}
		else
		{
			viewModel.CancelAction?.Invoke();
		}
		viewModel.EnableLogin = true;
	}

	public static void CancelButtonCommand_Execute()
	{
		BaseViewController.GetViewModel<LoginModel>()?.CancelAction?.Invoke();
	}

	public static void ContinueButtonCommand_Execute(UserData userData)
	{
		LoginModel viewModel = BaseViewController.GetViewModel<LoginModel>();
		if (viewModel != null && viewModel.AuthenticateAction != null)
		{
			userData.AuthenticationResultCallback = AuthenticationResult;
			viewModel.UserData = userData;
			viewModel.AuthenticateAction(userData);
		}
	}

	public static void TimeOutAction()
	{
		Action obj = BaseViewController.GetViewModel<LoginModel>()?.CancelAction;
		OrchestrationService.RemoveIdleTimer(LoginIdleTimerName);
		LogHelper.Instance.LogTo("UserLoginLog", "Control Panel | Login timed out");
		obj?.Invoke();
	}
}
