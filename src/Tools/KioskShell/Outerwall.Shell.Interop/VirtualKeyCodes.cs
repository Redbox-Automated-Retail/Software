using System;

namespace Outerwall.Shell.Interop;

[Flags]
internal enum VirtualKeyCodes : uint
{
	VK_SHIFT = 0x10u,
	VK_CONTROL = 0x11u,
	VK_MENU = 0x12u,
	VK_LSHIFT = 0xA0u,
	VK_RSHIFT = 0xA1u
}
