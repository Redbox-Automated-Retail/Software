using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class LoginModel : BaseModel<LoginModel>
{
	public static readonly DependencyProperty TitleProperty = BaseModel<LoginModel>.CreateDependencyProperty("Title", TYPES.STRING);

	public static readonly DependencyProperty StoreNumberProperty = BaseModel<LoginModel>.CreateDependencyProperty("StoreNumber", TYPES.STRING);

	public static readonly DependencyProperty ErrorMessageProperty = BaseModel<LoginModel>.CreateDependencyProperty("ErrorMessage", TYPES.STRING);

	public static readonly DependencyProperty EnableLoginProperty = BaseModel<LoginModel>.CreateDependencyProperty("EnableLogin", TYPES.BOOL, true);

	public UserData UserData { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action<UserData> AuthenticateAction { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public string Title
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleProperty, value);
			});
		}
	}

	public string ErrorMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ErrorMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ErrorMessageProperty, value);
			});
		}
	}

	public string StoreNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StoreNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StoreNumberProperty, value);
			});
		}
	}

	public bool EnableLogin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(EnableLoginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EnableLoginProperty, value);
			});
		}
	}
}
