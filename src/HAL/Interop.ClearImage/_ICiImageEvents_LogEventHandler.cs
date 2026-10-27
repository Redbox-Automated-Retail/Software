using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(false)]
public delegate void _ICiImageEvents_LogEventHandler([In][MarshalAs(UnmanagedType.BStr)] string LogSignature, [In][MarshalAs(UnmanagedType.BStr)] string LogRecord, [In][MarshalAs(UnmanagedType.BStr)] string RsltRecord);
