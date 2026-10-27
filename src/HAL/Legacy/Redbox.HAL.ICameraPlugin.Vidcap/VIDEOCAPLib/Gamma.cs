using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(GammaClass))]
[Guid("1EAB837B-0490-4326-B23C-61A84BB7FFBE")]
[ComVisible(true)]
public interface Gamma : IGamma
{
}
