using System;

namespace Outerwall.Shell.Interop;

[Flags]
internal enum KBDLLHOOKSTRUCTFlags : uint
{
	LLKHF_EXTENDED = 1u,
	LLKHF_INJECTED = 0x10u,
	LLKHF_ALTDOWN = 0x20u,
	LLKHF_UP = 0x80u
}
