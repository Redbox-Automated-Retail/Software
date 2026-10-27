using System;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment;

public abstract class WindowsHook : IDisposable
{
	private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

	private readonly HookType m_type;

	private HookCallback m_callback;

	private IntPtr m_handle = IntPtr.Zero;

	private bool m_disposed;

	public bool IsHooked => m_handle != IntPtr.Zero;

	protected WindowsHook(HookType hookType)
	{
		m_type = hookType;
		m_callback = InternalHookFunction;
	}

	~WindowsHook()
	{
		Dispose(disposing: false);
	}

	public abstract bool SetHook();

	public bool UnHook()
	{
		TestDisposed();
		if (IsHooked)
		{
			bool result = UnhookWindowsHookEx(m_handle);
			m_handle = IntPtr.Zero;
			return result;
		}
		return false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected bool SetHook(IntPtr moduleHandle, uint threadId)
	{
		TestDisposed();
		if (!IsHooked)
		{
			m_handle = SetWindowsHookEx((int)m_type, m_callback, moduleHandle, threadId);
			return m_handle != IntPtr.Zero;
		}
		return false;
	}

	protected virtual void Dispose(bool disposing)
	{
		UnhookWindowsHookEx(m_handle);
		m_handle = IntPtr.Zero;
		m_disposed = true;
	}

	protected abstract bool HookFunction(int code, IntPtr wParam, IntPtr lParam);

	private IntPtr InternalHookFunction(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode < 0)
		{
			return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
		}
		if (HookFunction(nCode, wParam, lParam))
		{
			return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
		}
		return new IntPtr(1);
	}

	private void TestDisposed()
	{
		if (m_disposed)
		{
			throw new InvalidOperationException("This windows hook has been disposed and operations can no longer be performed on it.");
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookCallback lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
}
