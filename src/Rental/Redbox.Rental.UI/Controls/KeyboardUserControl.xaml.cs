using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls;

public partial class KeyboardUserControl : BaseUserControl, IValueConverter, IComponentConnector
{
	public static readonly DependencyProperty KeyboardModeProperty = BaseUserControl.CreateDependencyProperty<KeyboardUserControl>("KeyboardMode", typeof(KeyboardMode), KeyboardMode.NONE_OR_UNKNOWN);

	public bool CapKeyState
	{
		get
		{
			return CapButton.State;
		}
		set
		{
			CapButton.State = value;
		}
	}

	public KeyboardMode KeyboardMode
	{
		get
		{
			return (KeyboardMode)GetValue(KeyboardModeProperty);
		}
		set
		{
			SetValue(KeyboardModeProperty, value);
			InitVisibility(KeyboardMode);
		}
	}

	public event Action KeyTouched;

	public event Action<SymbolMode> KeyPressed;

	public KeyboardUserControl()
	{
		base.Resources.Add("KeyboardUserControl", this);
		InitializeComponent();
		AddClickHandlers();
	}

	protected override void DataContextHasChanged()
	{
		if (base.DataContext is KeyboardModel)
		{
			KeyboardModel keyboardModel = base.DataContext as KeyboardModel;
			KeyState keyState;
			if (KeyboardMode == KeyboardMode.PROMOTIONS_CODE)
			{
				keyState = KeyState.UPPER;
			}
			else
			{
				CapKeyState = keyboardModel.KeyboardCapLock;
				keyState = ((!CapButton.State) ? KeyState.LOWER : KeyState.UPPER);
			}
			ConvertKeyboardKeys(keyState);
		}
	}

	private void RoundedButton_Clicked(object sender, RoutedEventArgs e)
	{
		KeyboardModel keyboardModel = base.DataContext as KeyboardModel;
		RoundedButton roundedButton = sender as RoundedButton;
		object content = roundedButton.Content;
		if (CapKeyState)
		{
			CapKeyState = false;
			ConvertKeyboardKeys(KeyState.LOWER);
		}
		if (keyboardModel == null || content == null)
		{
			return;
		}
		switch (roundedButton.Tag as string)
		{
		case "key":
			if (keyboardModel.KeyButtonCommand.CanExecute(null))
			{
				keyboardModel.KeyButtonCommand.Execute(content as string);
			}
			break;
		case "back":
			if (keyboardModel.BackButtonCommand.CanExecute(null))
			{
				keyboardModel.BackButtonCommand.Execute(null);
			}
			break;
		case "space":
			if (keyboardModel.SpaceButtonCommand.CanExecute(null))
			{
				keyboardModel.SpaceButtonCommand.Execute(null);
			}
			break;
		case "clear":
			if (keyboardModel.ClearButtonCommand.CanExecute(null))
			{
				keyboardModel.ClearButtonCommand.Execute(null);
			}
			break;
		case "domain":
			if (keyboardModel.DomainButtonCommand.CanExecute(null))
			{
				keyboardModel.DomainButtonCommand.Execute((content as TextBlock).Text);
			}
			break;
		case "dotcom":
			if (keyboardModel.DotComButtonCommand.CanExecute(null))
			{
				keyboardModel.DotComButtonCommand.Execute((content as TextBlock).Text);
			}
			break;
		}
		KeyTouched?.Invoke();
	}

	private void CapButtonClicked(object sender, RoutedEventArgs e)
	{
		if ("ABC".Equals(CapButtonText.Text))
		{
			CapKeyState = true;
		}
		else
		{
			CapKeyState = false;
		}
		ConvertKeyboardKeys((!CapButton.State) ? KeyState.LOWER : KeyState.UPPER);
	}

	protected void AddClickHandlers()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (RoundedButton item in RentalUserControl.FindLogicalChildren<RoundedButton>(MainGridElem))
		{
			if (!"cap_shift".Equals(item.Tag as string))
			{
				item.Click += RoundedButton_Clicked;
			}
			theme?.SetStyle(item);
		}
	}

	protected void InitVisibility(KeyboardMode KeyboardMode)
	{
		switch (KeyboardMode)
		{
		case KeyboardMode.ACCOUNT_SIGN_IN:
			SymbolKeysElem.Visibility = Visibility.Collapsed;
			CodeKeysElem.Visibility = Visibility.Visible;
			EmailKeysElem.Visibility = Visibility.Collapsed;
			PromoCodeElem.Visibility = Visibility.Collapsed;
			CaseKeysElem.Visibility = Visibility.Visible;
			CaseKeysElem.Margin = new Thickness(75.0, 0.0, 650.0, 0.0);
			break;
		case KeyboardMode.EMAIL_SYMBOLS:
			SymbolButtonTextElem.Text = (CapKeyState ? CapButtonText.Text.ToUpper() : CapButtonText.Text.ToLower());
			SymbolKeysElem.Visibility = Visibility.Visible;
			CodeKeysElem.Visibility = Visibility.Collapsed;
			EmailKeysElem.Visibility = Visibility.Collapsed;
			PromoCodeElem.Visibility = Visibility.Collapsed;
			CaseKeysElem.Visibility = Visibility.Collapsed;
			break;
		case KeyboardMode.PROMOTIONS_CODE:
			SymbolKeysElem.Visibility = Visibility.Collapsed;
			CodeKeysElem.Visibility = Visibility.Collapsed;
			EmailKeysElem.Visibility = Visibility.Collapsed;
			PromoCodeElem.Visibility = Visibility.Visible;
			CaseKeysElem.Visibility = Visibility.Collapsed;
			break;
		case KeyboardMode.EMAIL_COMING_SOON:
		case KeyboardMode.EMAIL_RECEIPTS_PROMO:
		case KeyboardMode.ACCOUNT_EMAIL_CHANGED:
		case KeyboardMode.PERKS_SIGN_UP:
		case KeyboardMode.REDBOX_PLUS_SIGN_UP:
		case KeyboardMode.LEAD_GENERATION_SIGN_UP:
			SymbolKeysElem.Visibility = Visibility.Collapsed;
			CodeKeysElem.Visibility = Visibility.Collapsed;
			EmailKeysElem.Visibility = Visibility.Visible;
			PromoCodeElem.Visibility = Visibility.Collapsed;
			CaseKeysElem.Visibility = Visibility.Visible;
			CaseKeysElem.Margin = new Thickness(0.0, 0.0, 730.0, 0.0);
			break;
		case KeyboardMode.STARZ_SIGN_UP:
			break;
		}
	}

	protected void ConvertKeyboardKeys(KeyState keyState)
	{
		ConvertKeyboardKeyGroup(QCodeElements, keyState);
		ConvertKeyboardKeyGroup(ACodeElements, keyState);
		ConvertKeyboardKeyGroup(ZCodeElements, keyState);
		ConvertKeyboardKeyGroup(QEmailElements, keyState);
		ConvertKeyboardKeyGroup(AEmailElements, keyState);
		ConvertKeyboardKeyGroup(AEmailElements2, keyState);
		ConvertKeyboardKeyGroup(ZEmailElements, keyState);
		ConvertKeyboardKeyGroup(ZEmailElements2, keyState);
		string text = CapButtonText.Text;
		if (keyState == KeyState.LOWER)
		{
			CapButtonText.Text = text.ToUpper();
		}
		else
		{
			CapButtonText.Text = text.ToLower();
		}
		HandleWPFHit();
	}

	protected void ConvertKeyboardKeyGroup(Grid grid, KeyState keyState)
	{
		foreach (object child in grid.Children)
		{
			if (!((child as Button)?.Content is string { Length: 1 } text))
			{
				continue;
			}
			short num = (short)text[0];
			if (keyState == KeyState.LOWER)
			{
				if (num >= 65 && num <= 90)
				{
					(child as Button).Content = text.ToLower();
				}
			}
			else if (num >= 97 && num <= 122)
			{
				(child as Button).Content = text.ToUpper();
			}
		}
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool flag = false;
		KeyboardMode keyboardMode = KeyboardMode;
		switch ((string)value)
		{
		case "SymbolKeysElem":
			flag = false;
			break;
		case "CaseKeysElem":
			flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN || keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO || keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED || keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
			break;
		case "EmailKeysElem":
			flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN || keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO || keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED || keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
			break;
		case "CodeKeysElem":
			flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN || keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO || keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED || keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
			break;
		case "PromoCodeElem":
			flag = keyboardMode == KeyboardMode.PROMOTIONS_CODE;
			break;
		}
		return (!flag) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return DependencyProperty.UnsetValue;
	}

	private void SwitchToLettersPressed(object sender, RoutedEventArgs e)
	{
		KeyPressed?.Invoke(SymbolMode.EMAIL_LETTER_KEY_PRESSED);
	}

	private void CodeSwitchToSymbolsPressed(object sender, RoutedEventArgs e)
	{
		KeyPressed?.Invoke(SymbolMode.CODE__SYMBOL_KEY_PRESSED);
	}

	private void EmailSwitchToSymbolsPressed(object sender, RoutedEventArgs e)
	{
		KeyPressed?.Invoke(SymbolMode.EMAIL_SYMBOL_KEY_PRESSED);
	}

	private void RoundedButton_Click(object sender, RoutedEventArgs e)
	{
	}
}
