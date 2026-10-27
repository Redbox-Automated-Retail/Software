using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(false)]
[TypeLibType(16)]
public delegate void _ICiImageEvents_LogEventHandler([In][MarshalAs(UnmanagedType.BStr)] string LogSignature, [In][MarshalAs(UnmanagedType.BStr)] string LogRecord, [In][MarshalAs(UnmanagedType.BStr)] string RsltRecord);
