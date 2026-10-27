using Redbox.Controls;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class KeyboardControl : ViewUserControl
{
	public KeyboardMode KeyboardMode { get; protected set; }

	protected KeyboardMode LastKeyMode { get; set; }

	protected KeyboardUserControl KeyboardUserControl { get; set; }

	protected BorderButton BorderControlElem { get; set; }

	protected KeyboardModel Model => base.DataContext as KeyboardModel;

	protected void InitKeyboardUserControl(KeyboardUserControl keyboardUserControl, BorderButton borderControlElem)
	{
		KeyboardUserControl = keyboardUserControl;
		BorderControlElem = borderControlElem;
		keyboardUserControl.KeyTouched += KeyTouched;
		keyboardUserControl.KeyPressed += KeyPressed;
	}

	protected virtual void KeyTouched()
	{
		if (BorderControlElem.ButtonState != 0)
		{
			BorderControlElem.ButtonState = 0;
		}
	}

	protected virtual void KeyPressed(SymbolMode mode)
	{
		KeyboardMode lastKeyMode = LastKeyMode;
		LastKeyMode = KeyboardUserControl.KeyboardMode;
		switch (mode)
		{
		case SymbolMode.EMAIL_LETTER_KEY_PRESSED:
			KeyboardUserControl.KeyboardMode = lastKeyMode;
			break;
		case SymbolMode.EMAIL_SYMBOL_KEY_PRESSED:
			KeyboardUserControl.KeyboardMode = KeyboardMode.EMAIL_SYMBOLS;
			break;
		}
	}

	protected virtual void ContinueTouched()
	{
		if (Model.ContinueButtonCommand.CanExecute(null))
		{
			Model.ContinueButtonCommand.Execute(null);
		}
		if (!string.IsNullOrEmpty(Model?.KeyboardError))
		{
			BorderControlElem.ButtonState = 1;
		}
	}
}
