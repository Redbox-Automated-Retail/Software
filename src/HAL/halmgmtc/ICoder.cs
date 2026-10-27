using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public interface ICoder
{
	void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress progress);
}
