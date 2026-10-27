using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("56A868A6-0Ad4-11CE-B03A-0020AF0BA770")]
internal interface IFileSourceFilter
{
	[PreserveSig]
	int Load([In][MarshalAs(UnmanagedType.LPWStr)] string fileName, [In][MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

	[PreserveSig]
	int GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string fileName, [Out][MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);
}
