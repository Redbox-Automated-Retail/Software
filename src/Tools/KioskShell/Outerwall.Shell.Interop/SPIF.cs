using System;

namespace Outerwall.Shell.Interop;

[Flags]
internal enum SPIF
{
	None = 0,
	SPIF_UPDATEINIFILE = 1,
	SPIF_SENDCHANGE = 2,
	SPIF_SENDWININICHANGE = SPIF_SENDCHANGE
}
