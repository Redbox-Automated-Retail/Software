using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public interface ISetDecoderProperties
{
	void SetDecoderProperties(byte[] properties);
}
