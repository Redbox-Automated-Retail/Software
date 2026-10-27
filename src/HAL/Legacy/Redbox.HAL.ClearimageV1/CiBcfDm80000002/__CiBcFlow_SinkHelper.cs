using System.Runtime.InteropServices;
using ClearImage;

namespace CiBcfDm80000002;

[TypeLibType(TypeLibTypeFlags.FHidden)]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class __CiBcFlow_SinkHelper : __CiBcFlow
{
	public __CiBcFlow_BarcodeFoundEventHandler m_BarcodeFoundDelegate;

	public int m_dwCookie;

	public void BarcodeFound(ref CiBarcode P_0)
	{
		if (m_BarcodeFoundDelegate != null)
		{
			m_BarcodeFoundDelegate(ref P_0);
		}
	}

	internal __CiBcFlow_SinkHelper()
	{
		m_dwCookie = 0;
		m_BarcodeFoundDelegate = null;
	}
}
