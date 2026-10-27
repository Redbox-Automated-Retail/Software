using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage;

[ComImport]
[Guid("63F6480C-997E-4FDE-AD63-A24E5F0FFDC7")]
[TypeLibType(4160)]
[ComVisible(true)]
public interface ICiRepair
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	void FaxStandardToFine();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void FaxRemoveBlankLines();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void FaxRemoveHeader();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(36)]
	double AutoDeskew();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(37)]
	EPageRotation AutoRotate();

	[DispId(39)]
	CiImage Image
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(39)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(39)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(16)]
	int pMinLineLength
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(16)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(16)]
		[param: In]
		set;
	}

	[DispId(17)]
	int pMaxLineGap
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(17)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(17)]
		[param: In]
		set;
	}

	[DispId(18)]
	double pMaxLineAngle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(18)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(18)]
		[param: In]
		set;
	}

	[DispId(19)]
	ELineCurvature pLineCurvature
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(19)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(19)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(20)]
	[TypeLibFunc(1088)]
	void CleanBorders();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(21)]
	void RemovePunchHoles();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(22)]
	void RemoveHalftone();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(23)]
	void SmoothCharacters([In] ESmoothType Type);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(24)]
	void CleanNoise([In] int NoiseSize);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(25)]
	void AutoInvertImage([In] int Threshold);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(26)]
	void AutoInvertBlocks([In] int MinWidth, [In] int MinHeight);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(29)]
	void ReconstructLines([In] ELineDirection Direction);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(30)]
	void AutoCrop([In] int NewLeftMargin, [In] int NewTopMargin, [In] int NewRightMargin, [In] int NewBottomMargin);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(31)]
	void AutoRegister([In] int NewLeftMargin, [In] int NewTopMargin);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(40)]
	void DeleteLines([In] ELineDirection Direction, [In] EBoolean bRepair = EBoolean.ciTrue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(41)]
	void BorderExtract([In] EBorderExtractFlags Flags = EBorderExtractFlags.ciBexBorderDeskewCrop, [In] EBorderExtractAlgorithm Algorithm = EBorderExtractAlgorithm.ciBeaCleaner);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(42)]
	void ClearBackground([In] double ThrLevel = 30.0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(43)]
	void Resize([In] EPageSize PageSize, [In] EPageOrientation PageOrientation = EPageOrientation.epgoAuto, [In] EImageAlignment ImageAlignment = EImageAlignment.epiaCenter, [In] double Width = 8.5, [In] double Height = 11.0, [In] ESizeUnit Unit = ESizeUnit.esuInch);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(44)]
	EBoolean IsBlankImage([In] int reserved0 = 0, [In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

	// C# has no syntax for parameterized property 'cx2l'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(45)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(45)]
	[TypeLibFunc(1088)]
	int get_cx2l([In] int i);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(45)]
	void set_cx2l([In] int i, [In] int pVal);

	// C# has no syntax for parameterized property 'cx3d'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(46)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(46)]
	double get_cx3d([In] int i);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[TypeLibFunc(1088)]
	[DispId(46)]
	void set_cx3d([In] int i, [In] double pVal);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(47)]
	void AdvancedBinarize([In] int targetDpi = 0, [In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(48)]
	[TypeLibFunc(1088)]
	void MinimizeBitsPerPixel([In] double reserved1 = 0.0, [In] double reserved2 = 0.0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(50)]
	void CleanNoiseExt([In] ECleanNoiseFlags Flags, [In] int maxNoiseSizeHorz, [In] int maxNoiseSizeVert, [In] int minObjectDistance, [In] int reserved0);
}
