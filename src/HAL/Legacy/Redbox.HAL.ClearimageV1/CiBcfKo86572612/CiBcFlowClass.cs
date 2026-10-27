using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfKo86572612;

[ComImport]
[ClassInterface(0)]
[TypeLibType(2)]
[Guid("3D82041F-7D5C-43C8-9D59-069491C3B51C")]
[ComSourceInterfaces("CiBcfKo86572612.__CiBcFlow\0")]
[ComVisible(true)]
public class CiBcFlowClass : _CiBcFlow, CiBcFlow, __CiBcFlow_Event
{
	[DispId(1073938433)]
	public virtual extern CiBarcodes Barcodes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938433)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938433)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(1073938434)]
	public virtual extern string info
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938434)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938434)]
		[TypeLibFunc(64)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1073938435)]
	public virtual extern bool bDir
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938435)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938435)]
		[param: In]
		set;
	}

	[DispId(1073938436)]
	public virtual extern FBarcodeType TypeE
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938436)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938436)]
		[param: In]
		set;
	}

	[DispId(1073938437)]
	public virtual extern FBarcodeDirections Directions
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938437)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938437)]
		[param: In]
		set;
	}

	[DispId(1073938438)]
	public virtual extern EBoolean AutoDetect1D
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938438)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938438)]
		[param: In]
		set;
	}

	[DispId(1073938439)]
	public virtual extern EBoolean ValidateOptChecksum
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938439)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938439)]
		[param: In]
		set;
	}

	[DispId(1073938440)]
	public virtual extern EBarcodeAlgorithm algorithm
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938440)]
		[TypeLibFunc(64)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938440)]
		[TypeLibFunc(64)]
		[param: In]
		set;
	}

	[DispId(1073938441)]
	public virtual extern FBarcodeDiag DiagFlags
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938441)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938441)]
		[param: In]
		set;
	}

	[DispId(1745027077)]
	public virtual extern CiServer Ci
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027077)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027077)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(1745027076)]
	public virtual extern CiImage Image
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027076)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027076)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(1745027075)]
	public virtual extern CiImage ImageSrc
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027075)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027075)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(1745027074)]
	public virtual extern string name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027074)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1745027073)]
	public virtual extern string Version
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027073)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1745027072)]
	public virtual extern object frm
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027072)]
		[return: MarshalAs(UnmanagedType.Struct)]
		get;
	}

	public virtual extern event __CiBcFlow_BarcodeFoundEventHandler BarcodeFound;

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809350)]
	public virtual extern int Find([In] int nMaxCnt);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809351)]
	public virtual extern int Exec([In] int nCode);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809352)]
	[return: MarshalAs(UnmanagedType.Struct)]
	public virtual extern object GetObject([In][MarshalAs(UnmanagedType.BStr)] string sNameIn);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809353)]
	public virtual extern void SetObject([In][MarshalAs(UnmanagedType.BStr)] string sNameIn, [In][MarshalAs(UnmanagedType.Struct)] object oValue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809354)]
	public virtual extern void SetParam([In][MarshalAs(UnmanagedType.BStr)] string sNameIn, [In][MarshalAs(UnmanagedType.BStr)] string sValue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809355)]
	[return: MarshalAs(UnmanagedType.BStr)]
	public virtual extern string GetParam([In][MarshalAs(UnmanagedType.BStr)] string sNameIn);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809356)]
	[return: MarshalAs(UnmanagedType.Struct)]
	public virtual extern object LoadConfig([In][MarshalAs(UnmanagedType.BStr)] string sApp, [In][MarshalAs(UnmanagedType.BStr)] string sSection);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809357)]
	[return: MarshalAs(UnmanagedType.Struct)]
	public virtual extern object SaveConfig([In][MarshalAs(UnmanagedType.BStr)] string sApp, [In][MarshalAs(UnmanagedType.BStr)] string sSection);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809358)]
	[return: MarshalAs(UnmanagedType.Struct)]
	public virtual extern object Configure();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(64)]
	[DispId(1610809360)]
	public virtual extern void ShowError([Optional][DefaultParameterValue("")][In][Out][MarshalAs(UnmanagedType.Struct)] ref object sMsg);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809361)]
	[TypeLibFunc(64)]
	public virtual extern bool IsGlobalParam([In][MarshalAs(UnmanagedType.BStr)] string sName);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(64)]
	[DispId(1610809362)]
	public virtual extern bool IsFormParam([In][MarshalAs(UnmanagedType.BStr)] string sName);
}
