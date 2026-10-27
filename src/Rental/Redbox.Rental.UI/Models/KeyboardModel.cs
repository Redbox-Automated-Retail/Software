using System;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class KeyboardModel : BaseModel<KeyboardModel>
{
	public static readonly DependencyProperty KeyboardTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyboardText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty KeyboardCodeProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyboardCode", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty KeyboardStarsProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyboardStars", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty KeyboardErrorProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyboardError", TYPES.STRING);

	public static readonly DependencyProperty TitleTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("TitleText", TYPES.STRING);

	public static readonly DependencyProperty MessageTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("MessageText", TYPES.STRING);

	public static readonly DependencyProperty MarketingTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("MarketingText", TYPES.STRING);

	public static readonly DependencyProperty EmailPromptTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("EmailPromptText", TYPES.STRING);

	public static readonly DependencyProperty CodePromptTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("CodePromptText", TYPES.STRING);

	public static readonly DependencyProperty BackButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("BackButtonText", TYPES.STRING);

	public static readonly DependencyProperty SpaceButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("SpaceButtonText", TYPES.STRING);

	public static readonly DependencyProperty ClearButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("ClearButtonText", TYPES.STRING);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING);

	public static readonly DependencyProperty ContinueButtonEnabledProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("ContinueButtonEnabled", TYPES.BOOL, false);

	public static readonly DependencyProperty ForgotPasswordButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("ForgotPasswordButtonText", TYPES.STRING);

	public static readonly DependencyProperty EmailButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("EmailButtonText", TYPES.STRING);

	public static readonly DependencyProperty PasswordButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("PasswordButtonText", TYPES.STRING);

	public static readonly DependencyProperty KeyboardCapLockProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyboardCapLock", TYPES.BOOL, true);

	public static readonly DependencyProperty KeyEmailModeProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("KeyEmailMode", TYPES.BOOL, false);

	public static readonly DependencyProperty EmailChangeTextSizeProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("MessageTextSize", TYPES.INT, 24);

	public static readonly DependencyProperty SymbolsButtonTextProperty = BaseModel<KeyboardModel>.CreateDependencyProperty("SymbolsButtonText", TYPES.STRING);

	public KeyboardLogic Logic { get; set; }

	public Action<bool> SetControlTextMode { get; set; }

	public DynamicRoutedCommand KeyButtonCommand { get; set; }

	public DynamicRoutedCommand CapButtonCommand { get; set; }

	public DynamicRoutedCommand ShiftButtonCommand { get; set; }

	public DynamicRoutedCommand BackButtonCommand { get; set; }

	public DynamicRoutedCommand ClearButtonCommand { get; set; }

	public DynamicRoutedCommand DomainButtonCommand { get; set; }

	public DynamicRoutedCommand DotComButtonCommand { get; set; }

	public DynamicRoutedCommand SpaceButtonCommand { get; set; }

	public DynamicRoutedCommand ForgotPasswordButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public DynamicRoutedCommand TermsButtonCommand { get; set; }

	public DynamicRoutedCommand EmailSelectedCommand { get; set; }

	public DynamicRoutedCommand CodeSelectedCommand { get; set; }

	public string KeyboardText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KeyboardTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyboardTextProperty, value);
			});
		}
	}

	public string KeyboardCode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KeyboardCodeProperty));
		}
		set
		{
			string text = string.Empty;
			if (value != null)
			{
				int num = value.Length - 1;
				for (int i = 0; i < value.Length; i++)
				{
					text = ((i != num) ? (text + "*") : (text + value[num]));
				}
			}
			KeyboardStars = text;
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyboardCodeProperty, value);
			});
		}
	}

	public string KeyboardStars
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KeyboardStarsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyboardStarsProperty, value);
			});
		}
	}

	public string KeyboardError
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(KeyboardErrorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyboardErrorProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string MessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MessageTextProperty, value);
			});
		}
	}

	public string MarketingText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MarketingTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MarketingTextProperty, value);
			});
		}
	}

	public string EmailPromptText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(EmailPromptTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmailPromptTextProperty, value);
			});
		}
	}

	public string CodePromptText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CodePromptTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CodePromptTextProperty, value);
			});
		}
	}

	public string CancelButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonTextProperty, value);
			});
		}
	}

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}

	public bool ContinueButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ContinueButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonEnabledProperty, value);
			});
		}
	}

	public string BackButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BackButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonTextProperty, value);
			});
		}
	}

	public string SpaceButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SpaceButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SpaceButtonTextProperty, value);
			});
		}
	}

	public string ClearButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ClearButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ClearButtonTextProperty, value);
			});
		}
	}

	public string ForgotPasswordButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ForgotPasswordButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ForgotPasswordButtonTextProperty, value);
			});
		}
	}

	public string EmailButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(EmailButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmailButtonTextProperty, value);
			});
		}
	}

	public string PasswordButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PasswordButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PasswordButtonTextProperty, value);
			});
		}
	}

	public bool KeyboardCapLock
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(KeyboardCapLockProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyboardCapLockProperty, value);
			});
		}
	}

	public bool KeyEmailMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(KeyEmailModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeyEmailModeProperty, value);
			});
		}
	}

	public int MessageTextSize
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(EmailChangeTextSizeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmailChangeTextSizeProperty, value);
			});
		}
	}

	public string SymbolsButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SymbolsButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SymbolsButtonTextProperty, value);
			});
		}
	}
}
