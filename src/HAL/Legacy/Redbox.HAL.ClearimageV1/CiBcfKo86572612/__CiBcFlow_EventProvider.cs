using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace CiBcfKo86572612;

internal sealed class __CiBcFlow_EventProvider : __CiBcFlow_Event, IDisposable
{
	private IConnectionPointContainer m_ConnectionPointContainer;

	private ArrayList m_aEventSinkHelpers;

	private IConnectionPoint m_ConnectionPoint;

	private void Init()
	{
		IConnectionPoint ppCP = null;
		Guid riid = new Guid(new byte[16]
		{
			95, 142, 79, 250, 129, 4, 101, 71, 157, 171,
			110, 1, 182, 109, 45, 237
		});
		m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
		m_ConnectionPoint = ppCP;
		m_aEventSinkHelpers = new ArrayList();
	}

	private void add_BarcodeFound_Impl(__CiBcFlow_BarcodeFoundEventHandler P_0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			__CiBcFlow_SinkHelper _CiBcFlow_SinkHelper = new __CiBcFlow_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(_CiBcFlow_SinkHelper, out pdwCookie);
			_CiBcFlow_SinkHelper.m_dwCookie = pdwCookie;
			_CiBcFlow_SinkHelper.m_BarcodeFoundDelegate = P_0;
			m_aEventSinkHelpers.Add(_CiBcFlow_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	private void remove_BarcodeFound_Impl(__CiBcFlow_BarcodeFoundEventHandler P_0)
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
				__CiBcFlow_SinkHelper _CiBcFlow_SinkHelper = (__CiBcFlow_SinkHelper)m_aEventSinkHelpers[num];
				if (_CiBcFlow_SinkHelper.m_BarcodeFoundDelegate != null && ((_CiBcFlow_SinkHelper.m_BarcodeFoundDelegate.Equals(P_0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(_CiBcFlow_SinkHelper.m_dwCookie);
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

	public __CiBcFlow_EventProvider(object P_0)
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
					__CiBcFlow_SinkHelper _CiBcFlow_SinkHelper = (__CiBcFlow_SinkHelper)m_aEventSinkHelpers[num];
					m_ConnectionPoint.Unadvise(_CiBcFlow_SinkHelper.m_dwCookie);
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

	public event __CiBcFlow_BarcodeFoundEventHandler BarcodeFound
	{
		add
		{
			add_BarcodeFound_Impl(value);
		}
		remove
		{
			remove_BarcodeFound_Impl(value);
		}
	}
}
