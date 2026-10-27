using System;
using System.Windows.Input;

namespace Outerwall.Shell;

internal class KeyPressEventEventArgs : EventArgs
{
	public KeyState KeyState { get; private set; }

	public int ScanCode { get; private set; }

	public Key Key { get; private set; }

	public ModifierKeys ModifierKeys { get; private set; }

	public bool Handled { get; set; }

	public KeyPressEventEventArgs(KeyState keyState, int scanCode, Key key, ModifierKeys modifierKeys)
	{
		KeyState = keyState;
		ScanCode = scanCode;
		Key = key;
		ModifierKeys = modifierKeys;
		Handled = false;
	}
}
