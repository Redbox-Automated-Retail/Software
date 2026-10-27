using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public interface ISetCoderProperties
{
	void SetCoderProperties(CoderPropID[] propIDs, object[] properties);
}
