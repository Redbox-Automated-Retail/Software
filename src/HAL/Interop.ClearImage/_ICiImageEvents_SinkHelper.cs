using System.Runtime.InteropServices;

namespace ClearImage;

[TypeLibType(TypeLibTypeFlags.FHidden)]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class _ICiImageEvents_SinkHelper : _ICiImageEvents
{
	public _ICiImageEvents_LogEventHandler m_LogDelegate;

	public int m_dwCookie;

	public void Log(string P_0, string P_1, string P_2)
	{
		if (m_LogDelegate != null)
		{
			m_LogDelegate(P_0, P_1, P_2);
		}
	}

	internal _ICiImageEvents_SinkHelper()
	{
		m_dwCookie = 0;
		m_LogDelegate = null;
	}
}
