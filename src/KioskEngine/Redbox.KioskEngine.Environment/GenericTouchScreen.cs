using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class GenericTouchScreen : IDisposable
{
	private readonly LowLevelMouseHook m_hook;

	private bool m_disposed;

	public virtual string Driver => string.Empty;

	public TouchScreenState State
	{
		get
		{
			TouchScreenState touchScreenState = (m_disposed ? TouchScreenState.Unavailable : (m_hook.IsHooked ? TouchScreenState.Disabled : TouchScreenState.Enabled));
			LogHelper.Instance.Log("Touchscreen is currently {0}", touchScreenState);
			return touchScreenState;
		}
	}

	public GenericTouchScreen()
	{
		using Process process = Process.GetCurrentProcess();
		using ProcessModule processModule = process.MainModule;
		IntPtr moduleHandle = GetModuleHandle(processModule.ModuleName);
		m_hook = new LowLevelMouseHook(moduleHandle);
	}

	~GenericTouchScreen()
	{
		Dispose(disposing: false);
	}

	public void Toggle()
	{
		try
		{
			ToggleScreen();
		}
		catch (InvalidOperationException e)
		{
			LogHelper.Instance.Log("Touch screen control is no longer available.", e);
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Error Toggling touch screen.", e2);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void ToggleScreen()
	{
		if (m_hook.IsHooked)
		{
			m_hook.UnHook();
			Windows10TouchScreen.Enable();
		}
		else
		{
			m_hook.SetHook();
			Windows10TouchScreen.Disable();
		}
	}

	protected virtual bool Enabled()
	{
		return !m_hook.IsHooked;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_hook.Dispose();
		}
		m_disposed = true;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
}
