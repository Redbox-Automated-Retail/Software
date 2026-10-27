using System;
using System.Runtime.InteropServices;
using Outerwall.Shell.Interop;

namespace Outerwall.Shell;

internal class LowLevelMouseHook : WindowsHook
{
	public LowLevelMouseHook()
		: base(14)
	{
	}

	protected override bool HookFunction(int code, IntPtr wParam, IntPtr lParam)
	{
		return ((MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT))).flags == 1;
	}
}
