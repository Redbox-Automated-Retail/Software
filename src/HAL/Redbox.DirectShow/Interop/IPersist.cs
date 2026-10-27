using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComImport]
[Guid("0000010c-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IPersist
{
	[PreserveSig]
	int GetClassID(out Guid pClassID);
}
