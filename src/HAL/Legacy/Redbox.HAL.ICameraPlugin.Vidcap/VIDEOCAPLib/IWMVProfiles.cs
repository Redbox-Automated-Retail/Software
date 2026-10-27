using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[InterfaceType(2)]
[TypeLibType(4096)]
[Guid("5B0BD413-EBC9-4B48-9F99-414B25C867CE")]
[ComVisible(true)]
public interface IWMVProfiles : IEnumerable
{
	[DispId(1)]
	short Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}

	// C# has no syntax for parameterized property 'Item'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(5)]
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object get_Item([MarshalAs(UnmanagedType.Struct)] object Key);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	short FindWMVProfile([MarshalAs(UnmanagedType.BStr)] string strProfile);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.BStr)]
	string FindWMVProfileDesc(short index);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.BStr)]
	string FindWMVProfileName(short index);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1)]
	[DispId(-4)]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	new IEnumerator GetEnumerator();
}
