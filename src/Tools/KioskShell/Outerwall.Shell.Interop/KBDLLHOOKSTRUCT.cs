using System;

namespace Outerwall.Shell.Interop;

internal struct KBDLLHOOKSTRUCT
{
	public uint vkCode;

	public uint scanCode;

	public KBDLLHOOKSTRUCTFlags flags;

	public uint time;

	public UIntPtr dwExtraInfo;
}
