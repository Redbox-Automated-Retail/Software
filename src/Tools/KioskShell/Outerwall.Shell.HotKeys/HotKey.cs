using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Outerwall.Shell.HotKeys;

internal class HotKey
{
	private readonly Action _action;

	public string Name { get; private set; }

	public ModifierKeys ModifierKeys { get; private set; }

	public List<Key> Keys { get; private set; }

	public HotKey(string name, string sequence, Action action)
		: this(name, ParseModifierKeys(sequence), ParseKeys(sequence), action)
	{
	}

	public HotKey(string name, ModifierKeys modifierKeys, Key key, Action action)
		: this(name, modifierKeys, new Key[1] { key }, action)
	{
	}

	public HotKey(string name, ModifierKeys modifierKeys, IEnumerable<Key> keys, Action action)
	{
		if (modifierKeys == ModifierKeys.None || keys == null || !keys.Any())
		{
			throw new ArgumentException("The specified hot key is invalid. A combination of modifier keys and keys is required.");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action", "No action was supplied that this hot key should invoke.");
		}
		Name = name ?? string.Empty;
		ModifierKeys = modifierKeys;
		Keys = new List<Key>(keys);
		Keys.Sort();
		_action = action;
	}

	internal void Execute()
	{
#if NETFRAMEWORK
		_action.BeginInvoke(null, new object());
#else
		// Delegate BeginInvoke is not supported on .NET; the action still runs on a thread-pool thread.
		System.Threading.Tasks.Task.Run(_action);
#endif
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} + {2}", Name, ModifierKeys, string.Join(",", Keys.ConvertAll((Key k) => k.ToString()).ToArray()));
	}

	private static ModifierKeys ParseModifierKeys(string sequence)
	{
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		try
		{
			string value = sequence.Substring(0, sequence.IndexOf('+'));
			return (ModifierKeys)Enum.Parse(typeof(ModifierKeys), value);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"The hot key sequence \"{sequence}\" is invalid.", innerException);
		}
	}

	private static IEnumerable<Key> ParseKeys(string sequence)
	{
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		try
		{
			return (from s in sequence.Substring(sequence.IndexOf('+') + 1).Split(',')
				select (Key)Enum.Parse(typeof(Key), s.Trim())).ToList();
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"The hot key sequence \"{sequence}\" is invalid.", innerException);
		}
	}
}
