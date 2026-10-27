using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[ComImport]
[CoClass(typeof(TVChannelClass))]
[Guid("59083203-AADB-4D30-9F71-7076BA3048B6")]
[ComVisible(true)]
public interface TVChannel : ITVChannel
{
}
