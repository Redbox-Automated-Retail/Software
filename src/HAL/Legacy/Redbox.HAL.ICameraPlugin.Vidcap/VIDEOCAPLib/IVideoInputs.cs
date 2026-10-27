using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[InterfaceType(2)]
[TypeLibType(4096)]
[Guid("A6E98D4D-D86D-47D2-9C88-9C9CDA17B09C")]
[ComVisible(true)]
public interface IVideoInputs : IEnumerable
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
	[property: DispId(4)]
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object get_Item([MarshalAs(UnmanagedType.Struct)] object Key);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	short FindVideoInput([MarshalAs(UnmanagedType.BStr)] string strVideoInput);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.BStr)]
	string FindVideoInputName(short index);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-4)]
	[TypeLibFunc(1)]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	new IEnumerator GetEnumerator();
}
