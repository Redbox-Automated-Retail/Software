using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfKo86572612;

[ComImport]
[TypeLibType(4304)]
[Guid("8CED977A-C1AF-4C3D-8DF5-5A498C3DE968")]
[ComVisible(true)]
public interface _CiBcFlow
{
	[DispId(1073938433)]
	CiBarcodes Barcodes
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

	void _VtblGap8();

	[DispId(1073938434)]
	string info
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938434)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938434)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1073938435)]
	bool bDir
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938435)]
		[TypeLibFunc(64)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938435)]
		[TypeLibFunc(64)]
		[param: In]
		set;
	}

	[DispId(1073938436)]
	FBarcodeType TypeE
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
	FBarcodeDirections Directions
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
	EBoolean AutoDetect1D
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
	EBoolean ValidateOptChecksum
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
	EBarcodeAlgorithm algorithm
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938440)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(64)]
		[DispId(1073938440)]
		[param: In]
		set;
	}

	[DispId(1073938441)]
	FBarcodeDiag DiagFlags
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938441)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1073938441)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809350)]
	int Find([In] int nMaxCnt);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809351)]
	int Exec([In] int nCode);

	[DispId(1745027077)]
	CiServer Ci
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
	CiImage Image
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
	CiImage ImageSrc
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
	string name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027074)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1745027073)]
	string Version
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027073)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809352)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object GetObject([In][MarshalAs(UnmanagedType.BStr)] string sNameIn);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809353)]
	void SetObject([In][MarshalAs(UnmanagedType.BStr)] string sNameIn, [In][MarshalAs(UnmanagedType.Struct)] object oValue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809354)]
	void SetParam([In][MarshalAs(UnmanagedType.BStr)] string sNameIn, [In][MarshalAs(UnmanagedType.BStr)] string sValue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809355)]
	[return: MarshalAs(UnmanagedType.BStr)]
	string GetParam([In][MarshalAs(UnmanagedType.BStr)] string sNameIn);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809356)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object LoadConfig([In][MarshalAs(UnmanagedType.BStr)] string sApp, [In][MarshalAs(UnmanagedType.BStr)] string sSection);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809357)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object SaveConfig([In][MarshalAs(UnmanagedType.BStr)] string sApp, [In][MarshalAs(UnmanagedType.BStr)] string sSection);

	[DispId(1745027072)]
	object frm
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1745027072)]
		[return: MarshalAs(UnmanagedType.Struct)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809358)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object Configure();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809360)]
	[TypeLibFunc(64)]
	void ShowError([Optional][DefaultParameterValue("")][In][Out][MarshalAs(UnmanagedType.Struct)] ref object sMsg);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(64)]
	[DispId(1610809361)]
	bool IsGlobalParam([In][MarshalAs(UnmanagedType.BStr)] string sName);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(64)]
	[DispId(1610809362)]
	bool IsFormParam([In][MarshalAs(UnmanagedType.BStr)] string sName);
}
