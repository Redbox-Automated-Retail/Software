using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Outerwall.Shell.HotKeys;

internal class HotKeyEventArgs : EventArgs
{
	public string Name { get; private set; }

	public ModifierKeys ModifierKeys { get; private set; }

	public IEnumerable<Key> Keys { get; private set; }

	public HotKeyEventArgs(string name, ModifierKeys modifierKeys, IEnumerable<Key> keys)
	{
		Name = name;
		ModifierKeys = modifierKeys;
		Keys = keys;
	}
}
