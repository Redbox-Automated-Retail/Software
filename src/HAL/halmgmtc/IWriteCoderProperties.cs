using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public interface IWriteCoderProperties
{
	void WriteCoderProperties(Stream outStream);
}
