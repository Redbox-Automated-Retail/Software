using System.Runtime.InteropServices;

namespace VIDEOCAPLib;

[TypeLibType(TypeLibTypeFlags.FHidden)]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class _DVideoCapEvents_SinkHelper : _DVideoCapEvents
{
	public _DVideoCapEvents_ClickEventHandler m_ClickDelegate;

	public _DVideoCapEvents_MouseMoveEventHandler m_MouseMoveDelegate;

	public _DVideoCapEvents_DblClickEventHandler m_DblClickDelegate;

	public _DVideoCapEvents_CapturingEventHandler m_CapturingDelegate;

	public int m_dwCookie;

	public void Click()
	{
		if (m_ClickDelegate != null)
		{
			m_ClickDelegate();
		}
	}

	public void MouseMove(short P_0, short P_1, int P_2, int P_3)
	{
		if (m_MouseMoveDelegate != null)
		{
			m_MouseMoveDelegate(P_0, P_1, P_2, P_3);
		}
	}

	public void DblClick()
	{
		if (m_DblClickDelegate != null)
		{
			m_DblClickDelegate();
		}
	}

	public void Capturing(int P_0, int P_1)
	{
		if (m_CapturingDelegate != null)
		{
			m_CapturingDelegate(P_0, P_1);
		}
	}

	internal _DVideoCapEvents_SinkHelper()
	{
		m_dwCookie = 0;
		m_ClickDelegate = null;
		m_MouseMoveDelegate = null;
		m_DblClickDelegate = null;
		m_CapturingDelegate = null;
	}
}
