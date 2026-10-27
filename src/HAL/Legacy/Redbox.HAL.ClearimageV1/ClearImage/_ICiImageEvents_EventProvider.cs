using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace ClearImage;

internal sealed class _ICiImageEvents_EventProvider : _ICiImageEvents_Event, IDisposable
{
	private IConnectionPointContainer m_ConnectionPointContainer;

	private ArrayList m_aEventSinkHelpers;

	private IConnectionPoint m_ConnectionPoint;

	private void Init()
	{
		IConnectionPoint ppCP = null;
		Guid riid = new Guid(new byte[16]
		{
			139, 241, 188, 242, 39, 11, 212, 17, 181, 245,
			156, 199, 103, 0, 0, 0
		});
		m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
		m_ConnectionPoint = ppCP;
		m_aEventSinkHelpers = new ArrayList();
	}

	private void add_Log_Impl(_ICiImageEvents_LogEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_ICiImageEvents_SinkHelper iCiImageEvents_SinkHelper = new _ICiImageEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(iCiImageEvents_SinkHelper, out pdwCookie);
			iCiImageEvents_SinkHelper.m_dwCookie = pdwCookie;
			iCiImageEvents_SinkHelper.m_LogDelegate = P_0;
			m_aEventSinkHelpers.Add(iCiImageEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_Log_Impl(_ICiImageEvents_LogEventHandler P_0)
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
				_ICiImageEvents_SinkHelper iCiImageEvents_SinkHelper = (_ICiImageEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (iCiImageEvents_SinkHelper.m_LogDelegate != null && ((iCiImageEvents_SinkHelper.m_LogDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(iCiImageEvents_SinkHelper.m_dwCookie);
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

	public _ICiImageEvents_EventProvider(object P_0)
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
					_ICiImageEvents_SinkHelper iCiImageEvents_SinkHelper = (_ICiImageEvents_SinkHelper)m_aEventSinkHelpers[num];
					m_ConnectionPoint.Unadvise(iCiImageEvents_SinkHelper.m_dwCookie);
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

	public event _ICiImageEvents_LogEventHandler Log
	{
		add
		{
			add_Log_Impl(value);
		}
		remove
		{
			remove_Log_Impl(value);
		}
	}
}
