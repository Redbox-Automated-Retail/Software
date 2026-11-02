using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Outerwall.Shell;

internal abstract class WindowsHook : IDisposable
{
	private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

	private readonly int _type;

	private HookCallback _callback;

	private object _hookSync = new object();

	private object _disposeSync = new object();

	private IntPtr _handle = IntPtr.Zero;

	private bool _disposed;

	public bool IsHooked
	{
		get
		{
			Monitor.Enter(_hookSync);
			bool result = _handle != IntPtr.Zero;
			Monitor.Exit(_hookSync);
			return result;
		}
	}

	protected WindowsHook(int hookType)
	{
		_type = hookType;
		_callback = InternalHookFunction;
	}

	~WindowsHook()
	{
		Dispose(disposing: false);
	}

	public virtual bool SetHook()
	{
		if (!Monitor.TryEnter(_hookSync))
		{
			return false;
		}
		try
		{
			using Process process = Process.GetCurrentProcess();
			using ProcessModule processModule = process.MainModule!;
			bool result = false;
			IntPtr moduleHandle = GetModuleHandle(processModule.ModuleName);
			if (_handle == IntPtr.Zero)
			{
				_handle = SetWindowsHookEx(_type, _callback, moduleHandle, 0u);
				result = _handle != IntPtr.Zero;
			}
			return result;
		}
		catch (Exception innerException)
		{
			throw new ApplicationException("Unable to establish Windows hook.", innerException);
		}
		finally
		{
			Monitor.Exit(_hookSync);
		}
	}

	public bool UnHook()
	{
		VerifyNotDisposed();
		Monitor.Enter(_hookSync);
		try
		{
			if (_handle != IntPtr.Zero)
			{
				bool result = UnhookWindowsHookEx(_handle);
				_handle = IntPtr.Zero;
				return result;
			}
			return false;
		}
		catch (Exception innerException)
		{
			throw new ApplicationException("Unable to unhook Windows hook.", innerException);
		}
		finally
		{
			Monitor.Exit(_hookSync);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		Monitor.Enter(_hookSync);
		try
		{
			lock (_disposeSync)
			{
				if (!_disposed)
				{
					UnhookWindowsHookEx(_handle);
					_handle = IntPtr.Zero;
					_disposed = true;
				}
			}
		}
		catch (Exception innerException)
		{
			throw new ApplicationException("Unable to dispose " + GetType().Name, innerException);
		}
		finally
		{
			Monitor.Exit(_hookSync);
		}
	}

	protected abstract bool HookFunction(int code, IntPtr wParam, IntPtr lParam);

	private IntPtr InternalHookFunction(int nCode, IntPtr wParam, IntPtr lParam)
	{
		IntPtr result = new IntPtr(1);
		try
		{
			if (nCode < 0)
			{
				result = CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
			}
			if (HookFunction(nCode, wParam, lParam))
			{
				result = CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
			}
			return result;
		}
		catch (Exception innerException)
		{
			throw new ApplicationException($"Unable to process this Windows message. nCode:{nCode}, wParam:{wParam}, lParam:{lParam}. The next hook will not be called.", innerException);
		}
	}

	private void VerifyNotDisposed()
	{
		lock (_disposeSync)
		{
			if (_disposed)
			{
				throw new InvalidOperationException("This Windows hook has been disposed and operations can no longer be performed.");
			}
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookCallback lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
}
