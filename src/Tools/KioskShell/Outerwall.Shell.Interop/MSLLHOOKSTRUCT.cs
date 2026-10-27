using System;

namespace Outerwall.Shell.Interop;

internal struct MSLLHOOKSTRUCT
{
	public POINT pt;

	public uint mouseData;

	public uint flags;

	public uint time;

	public IntPtr dwExtraInfo;
}
