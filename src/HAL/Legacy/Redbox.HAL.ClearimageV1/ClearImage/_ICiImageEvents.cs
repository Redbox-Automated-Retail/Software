using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[TypeLibType(4096)]
[Guid("F2BCF18B-0B27-11D4-B5F5-9CC767000000")]
[InterfaceType(2)]
[ComVisible(true)]
public interface _ICiImageEvents
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(1)]
	void Log([In][MarshalAs(UnmanagedType.BStr)] string LogSignature, [In][MarshalAs(UnmanagedType.BStr)] string LogRecord, [In][MarshalAs(UnmanagedType.BStr)] string RsltRecord);
}
