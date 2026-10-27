using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(VideoInputsClass))]
[Guid("A6E98D4D-D86D-47D2-9C88-9C9CDA17B09C")]
[ComVisible(true)]
public interface VideoInputs : IVideoInputs
{
}
