using System;
using System.Security;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class KeyboardInfo
{
	public int MaxLength { get; set; }

	public Predicate<string> IsValidEntry { get; set; } = (string e) => false;

	public Action ForgotPasswordAction { get; set; } = delegate
	{
	};

	public Action CancelAction { get; set; } = delegate
	{
	};

	public Action<string> ContinueAction { get; set; } = delegate
	{
	};

	public Func<string, string, bool> IsValidEntries { get; set; } = (string e, string p) => false;

	public Action<string, SecureString> ContinueCodeAction { get; set; } = delegate
	{
	};

	public string KeyboardText { get; set; } = "";

	public string KeyboardCode { get; set; } = "";

	public KeyboardMode KeyboardMode { get; set; }

	public Action TimeoutAction { get; set; } = delegate
	{
	};

	public KeyboardModel keyboardModel { get; set; }

	public KeyboardLogic keyboardLogic { get; set; }

	public string ErrorMessage { get; set; }

	public bool KeepKeyboardTextOnInitialErrorMessage { get; set; }

	public SignInCancelButtonTypes SignInCancelButtonType { get; set; }
}
