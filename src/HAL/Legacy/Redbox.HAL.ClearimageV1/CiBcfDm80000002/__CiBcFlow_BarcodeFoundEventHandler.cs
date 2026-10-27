using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfDm80000002;

[ComVisible(false)]
[TypeLibType(16)]
public delegate void __CiBcFlow_BarcodeFoundEventHandler([In][Out][MarshalAs(UnmanagedType.Interface)] ref CiBarcode Bc);
