using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public interface ICodeProgress
{
	void SetProgress(long inSize, long outSize);
}
