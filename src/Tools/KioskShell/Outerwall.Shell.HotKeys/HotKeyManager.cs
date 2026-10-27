using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Outerwall.Shell.HotKeys;

internal class HotKeyManager : IDisposable
{
	private readonly List<HotKey> _hotKeys = new List<HotKey>();

	private readonly LowLevelKeyboardHook _hook;

	private readonly List<Key> _keys = new List<Key>();

	private readonly object _disposeSync = new object();

	private readonly object _monitorSync = new object();

	private bool _disposed;

	private bool _tracking;

	private bool _suspend;

	private ModifierKeys _modifiers;

	public event EventHandler<HotKeyEventArgs> HotKeyInvokedEvent;

	public HotKeyManager()
	{
		_hook = new LowLevelKeyboardHook();
		_hook.KeyPressEvent += Monitor;
	}

	~HotKeyManager()
	{
		Dispose(disposing: false);
	}

	public void Start()
	{
		VerifyNotDisposed();
		lock (_monitorSync)
		{
			if (!_hook.IsHooked)
			{
				_hook.SetHook();
			}
		}
	}

	public void Stop()
	{
		VerifyNotDisposed();
		lock (_monitorSync)
		{
			if (_hook.IsHooked)
			{
				_hook.UnHook();
				_tracking = false;
				_suspend = false;
				_modifiers = ModifierKeys.None;
				_keys.Clear();
			}
		}
	}

	public void AddHotKey(HotKey hotKey)
	{
		VerifyNotDisposed();
		lock (_monitorSync)
		{
			lock (((ICollection)_hotKeys).SyncRoot)
			{
				_hotKeys.Add(hotKey);
				_suspend = true;
			}
		}
	}

	public void RemoveHotKey(HotKey hotKey)
	{
		VerifyNotDisposed();
		lock (_monitorSync)
		{
			lock (((ICollection)_hotKeys).SyncRoot)
			{
				_hotKeys.Remove(hotKey);
				_suspend = true;
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		lock (_monitorSync)
		{
			_tracking = false;
			_suspend = false;
			_modifiers = ModifierKeys.None;
			_keys.Clear();
			_hook.KeyPressEvent -= Monitor;
			lock (_disposeSync)
			{
				try
				{
					if (disposing)
					{
						_hook.UnHook();
						_hook.Dispose();
					}
					_disposed = true;
				}
				catch (Exception innerException)
				{
					throw new ApplicationException("Unable to dispose " + GetType().Name, innerException);
				}
			}
		}
	}

	private void Monitor(object sender, KeyPressEventEventArgs e)
	{
		lock (_monitorSync)
		{
			if (e.KeyState == KeyState.KeyUp)
			{
				if (_tracking)
				{
					_tracking = false;
					_suspend = false;
					_modifiers = ModifierKeys.None;
					_keys.Clear();
				}
				return;
			}
			if (_suspend)
			{
				if (_tracking)
				{
					return;
				}
				_suspend = false;
			}
			_modifiers |= e.ModifierKeys;
			if (_modifiers == ModifierKeys.None)
			{
				return;
			}
			_tracking = true;
			if (e.Key == Key.None)
			{
				return;
			}
			_keys.Add(e.Key);
			_keys.Sort();
			lock (((ICollection)_hotKeys).SyncRoot)
			{
				foreach (HotKey hotKey in _hotKeys)
				{
					if (hotKey.ModifierKeys != _modifiers)
					{
						continue;
					}
					int count = _keys.Count;
					if (hotKey.Keys.Count == count && hotKey.Keys.Intersect(_keys).Count() == count)
					{
						_suspend = true;
						e.Handled = true;
						hotKey.Execute();
						EventHandler<HotKeyEventArgs> eventHandler = HotKeyInvokedEvent;
						if (eventHandler != null)
						{
							HotKeyEventArgs e2 = new HotKeyEventArgs(hotKey.Name, hotKey.ModifierKeys, hotKey.Keys);
							eventHandler(this, e2);
						}
						break;
					}
				}
			}
		}
	}

	private void VerifyNotDisposed()
	{
		lock (_disposeSync)
		{
			if (_disposed)
			{
				throw new InvalidOperationException("This hot key manager has been disposed and operations can no longer be performed on it.");
			}
		}
	}
}
