using System;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment;

public class LowLevelMouseHook : WindowsHook
{
	private IntPtr m_handle;

	public LowLevelMouseHook(IntPtr handle)
		: base(HookType.WH_MOUSE_LL)
	{
		m_handle = handle;
	}

	public override bool SetHook()
	{
		return SetHook(m_handle, 0u);
	}

	protected override bool HookFunction(int code, IntPtr wParam, IntPtr lParam)
	{
		if (((WindowsAPIObjects.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WindowsAPIObjects.MSLLHOOKSTRUCT))).flags != 0)
		{
			return true;
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		m_handle = IntPtr.Zero;
	}
}
