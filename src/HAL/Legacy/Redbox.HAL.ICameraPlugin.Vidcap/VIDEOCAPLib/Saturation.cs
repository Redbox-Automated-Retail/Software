using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[Guid("A3B1776D-CBF2-4D81-B508-A927A15146BE")]
[CoClass(typeof(SaturationClass))]
[ComVisible(true)]
public interface Saturation : ISaturation
{
}
