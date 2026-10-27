using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace VIDEOCAPLib;

internal sealed class _DVideoCapEvents_EventProvider : _DVideoCapEvents_Event, IDisposable
{
	private IConnectionPointContainer m_ConnectionPointContainer;

	private ArrayList m_aEventSinkHelpers;

	private IConnectionPoint m_ConnectionPoint;

	private void Init()
	{
		IConnectionPoint ppCP = null;
		Guid riid = new Guid(new byte[16]
		{
			1, 76, 68, 184, 159, 83, 118, 71, 176, 146,
			245, 147, 183, 174, 140, 194
		});
		m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
		m_ConnectionPoint = ppCP;
		m_aEventSinkHelpers = new ArrayList();
	}

	private void add_Click_Impl(_DVideoCapEvents_ClickEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = new _DVideoCapEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(dVideoCapEvents_SinkHelper, out pdwCookie);
			dVideoCapEvents_SinkHelper.m_dwCookie = pdwCookie;
			dVideoCapEvents_SinkHelper.m_ClickDelegate = P_0;
			m_aEventSinkHelpers.Add(dVideoCapEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_Click_Impl(_DVideoCapEvents_ClickEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_aEventSinkHelpers == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = (_DVideoCapEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (dVideoCapEvents_SinkHelper.m_ClickDelegate != null && ((dVideoCapEvents_SinkHelper.m_ClickDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(dVideoCapEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void add_MouseMove_Impl(_DVideoCapEvents_MouseMoveEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = new _DVideoCapEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(dVideoCapEvents_SinkHelper, out pdwCookie);
			dVideoCapEvents_SinkHelper.m_dwCookie = pdwCookie;
			dVideoCapEvents_SinkHelper.m_MouseMoveDelegate = P_0;
			m_aEventSinkHelpers.Add(dVideoCapEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_MouseMove_Impl(_DVideoCapEvents_MouseMoveEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_aEventSinkHelpers == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = (_DVideoCapEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (dVideoCapEvents_SinkHelper.m_MouseMoveDelegate != null && ((dVideoCapEvents_SinkHelper.m_MouseMoveDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(dVideoCapEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void add_DblClick_Impl(_DVideoCapEvents_DblClickEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = new _DVideoCapEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(dVideoCapEvents_SinkHelper, out pdwCookie);
			dVideoCapEvents_SinkHelper.m_dwCookie = pdwCookie;
			dVideoCapEvents_SinkHelper.m_DblClickDelegate = P_0;
			m_aEventSinkHelpers.Add(dVideoCapEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_DblClick_Impl(_DVideoCapEvents_DblClickEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_aEventSinkHelpers == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = (_DVideoCapEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (dVideoCapEvents_SinkHelper.m_DblClickDelegate != null && ((dVideoCapEvents_SinkHelper.m_DblClickDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(dVideoCapEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void add_Capturing_Impl(_DVideoCapEvents_CapturingEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = new _DVideoCapEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(dVideoCapEvents_SinkHelper, out pdwCookie);
			dVideoCapEvents_SinkHelper.m_dwCookie = pdwCookie;
			dVideoCapEvents_SinkHelper.m_CapturingDelegate = P_0;
			m_aEventSinkHelpers.Add(dVideoCapEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_Capturing_Impl(_DVideoCapEvents_CapturingEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_aEventSinkHelpers == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = (_DVideoCapEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (dVideoCapEvents_SinkHelper.m_CapturingDelegate != null && ((dVideoCapEvents_SinkHelper.m_CapturingDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(dVideoCapEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public _DVideoCapEvents_EventProvider(object P_0)
	{
		m_ConnectionPointContainer = (IConnectionPointContainer)P_0;
	}

	public void Finalize()
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 < count)
			{
				do
				{
					_DVideoCapEvents_SinkHelper dVideoCapEvents_SinkHelper = (_DVideoCapEvents_SinkHelper)m_aEventSinkHelpers[num];
					m_ConnectionPoint.Unadvise(dVideoCapEvents_SinkHelper.m_dwCookie);
					num++;
				}
				while (num < count);
			}
			Marshal.ReleaseComObject(m_ConnectionPoint);
		}
		catch (Exception)
		{
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void Dispose()
	{
		Finalize();
		GC.SuppressFinalize(this);
	}

	public event _DVideoCapEvents_ClickEventHandler Click
	{
		add
		{
			add_Click_Impl(value);
		}
		remove
		{
			remove_Click_Impl(value);
		}
	}

	public event _DVideoCapEvents_MouseMoveEventHandler MouseMove
	{
		add
		{
			add_MouseMove_Impl(value);
		}
		remove
		{
			remove_MouseMove_Impl(value);
		}
	}

	public event _DVideoCapEvents_DblClickEventHandler DblClick
	{
		add
		{
			add_DblClick_Impl(value);
		}
		remove
		{
			remove_DblClick_Impl(value);
		}
	}

	public event _DVideoCapEvents_CapturingEventHandler Capturing
	{
		add
		{
			add_Capturing_Impl(value);
		}
		remove
		{
			remove_Capturing_Impl(value);
		}
	}
}
