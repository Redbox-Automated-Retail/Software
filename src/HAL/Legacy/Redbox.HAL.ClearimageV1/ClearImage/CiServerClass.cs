using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[ClassInterface(0)]
[Guid("D836E300-A317-4C7E-BE61-D650CE242589")]
[TypeLibType(2)]
[ComVisible(true)]
public class CiServerClass : ICiServer, CiServer
{
	[DispId(12)]
	public virtual extern int VerMajor
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(12)]
		get;
	}

	[DispId(13)]
	public virtual extern int VerMinor
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(13)]
		get;
	}

	[DispId(14)]
	public virtual extern int VerRelease
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(14)]
		get;
	}

	// C# has no syntax for parameterized property 'Info'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(16)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(16)]
	[return: MarshalAs(UnmanagedType.BStr)]
	public virtual extern string get_Info([In] EInfoType Type, [In] int nParam = 0);

	// C# has no syntax for parameterized property 'DevMode'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(20)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(20)]
	public virtual extern int get_DevMode([In] int bGlobalDevMode = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(20)]
	public virtual extern void set_DevMode([Optional][DefaultParameterValue(0)][In] int bGlobalDevMode, [In] int pVal);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiImage CreateImage();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiBarcodePro CreateBarcodePro();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	public virtual extern void OpenExt([In] int hModule, [In] int MasterId, [In] int pParam);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiRect CreateRect([In] int left = 0, [In] int top = 0, [In] int right = 0, [In] int bottom = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiRepair CreateRepair();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiTools CreateTools();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(8)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiBarcodeBasic CreateBarcodeBasic();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiPdf417 CreatePdf417();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(15)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiDataMatrix CreateDataMatrix();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(17)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiAdvColor CreateAdvColor();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(18)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiBarcode CreateBarcode();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(19)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern CiBarcodes CreateBarcodes();
}
