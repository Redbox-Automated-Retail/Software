using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Outerwall.Shell.Interop;

namespace Outerwall.Shell;

internal class LowLevelKeyboardHook : WindowsHook
{
	public event EventHandler<KeyPressEventEventArgs> KeyPressEvent;

	public LowLevelKeyboardHook()
		: base(13)
	{
	}

	protected override bool HookFunction(int code, IntPtr wParam, IntPtr lParam)
	{
		EventHandler<KeyPressEventEventArgs> eventHandler = KeyPressEvent;
		if (eventHandler != null)
		{
			KBDLLHOOKSTRUCT kBDLLHOOKSTRUCT = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
			int state;
			switch (wParam.ToInt32())
			{
			default:
				state = 0;
				break;
			case 257:
			case 261:
				state = 2;
				break;
			case 256:
			case 260:
				state = 1;
				break;
			}
			KeyState keyState = (KeyState)state;
			if (keyState != KeyState.Unknown)
			{
				int scanCode = Convert.ToInt32(kBDLLHOOKSTRUCT.scanCode);
				Keys keys = (Keys)kBDLLHOOKSTRUCT.vkCode;
				Key key = ((kBDLLHOOKSTRUCT.vkCode < int.MaxValue) ? KeyInterop.KeyFromVirtualKey(Convert.ToInt32(kBDLLHOOKSTRUCT.vkCode)) : Key.None);
				if (key == Key.LWin || key == Key.RWin || key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt || key == Key.LeftShift || key == Key.RightShift)
				{
					key = Key.None;
				}
				ModifierKeys modifierKeys = ModifierKeys.None;
				modifierKeys |= TestAlt(ref keys);
				modifierKeys |= TestCtrl(ref keys);
				modifierKeys |= TestShift(ref keys);
				modifierKeys |= TestWindows(ref keys);
				KeyPressEventEventArgs e = new KeyPressEventEventArgs(keyState, scanCode, key, modifierKeys);
				eventHandler(this, e);
				return !e.Handled;
			}
		}
		return true;
	}

	private static ModifierKeys TestAlt(ref Keys keys)
	{
		if ((keys & Keys.Alt) == Keys.Alt)
		{
			keys &= ~Keys.Alt;
			return ModifierKeys.Alt;
		}
		if (keys == Keys.Menu || keys == Keys.LMenu || keys == Keys.RMenu)
		{
			keys = Keys.None;
			return ModifierKeys.Alt;
		}
		return ModifierKeys.None;
	}

	private static ModifierKeys TestCtrl(ref Keys keys)
	{
		if ((keys & Keys.Control) == Keys.Control)
		{
			keys &= ~Keys.Control;
			return ModifierKeys.Control;
		}
		if (keys == Keys.ControlKey || keys == Keys.LControlKey || keys == Keys.RControlKey)
		{
			keys = Keys.None;
			return ModifierKeys.Control;
		}
		return ModifierKeys.None;
	}

	private static ModifierKeys TestShift(ref Keys keys)
	{
		if ((keys & Keys.Shift) == Keys.Shift)
		{
			keys &= ~Keys.Shift;
			return ModifierKeys.Shift;
		}
		if (keys == Keys.ShiftKey || keys == Keys.LShiftKey || keys == Keys.RShiftKey)
		{
			keys = Keys.None;
			return ModifierKeys.Shift;
		}
		return ModifierKeys.None;
	}

	private static ModifierKeys TestWindows(ref Keys keys)
	{
		if (keys == Keys.LWin || keys == Keys.RWin)
		{
			keys = Keys.None;
			return ModifierKeys.Windows;
		}
		return ModifierKeys.None;
	}
}
