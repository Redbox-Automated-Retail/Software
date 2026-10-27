using System;
using System.IO;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

public class TouchScreenFactory
{
	private GenericTouchScreen m_touchScreen;

	public static TouchScreenFactory Instance => Singleton<TouchScreenFactory>.Instance;

	public void Initialize()
	{
		try
		{
			LogHelper.Instance.Log("Detecting touch screen model");
			if (OutdoorElo.GetDriverProcess().Length != 0)
			{
				LogHelper.Instance.Log("Detected outdoor ELO");
				m_touchScreen = new OutdoorElo();
			}
			else if (File.Exists(Path.Combine(System.Environment.SystemDirectory, "EloIntf.dll")))
			{
				if (IndoorElo.Connected())
				{
					LogHelper.Instance.Log("Detected indoor ELO");
					m_touchScreen = new IndoorElo();
				}
				else
				{
					LogHelper.Instance.Log("EloIntf.dll exists but cannot connect to it as an indoor ELO.");
				}
			}
			else
			{
				LogHelper.Instance.Log("No known touch screen detected.");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error determining touchscreen model, using generic", e);
		}
		if (m_touchScreen == null)
		{
			LogHelper.Instance.Log("Using generic touch screen driver.");
			m_touchScreen = new GenericTouchScreen();
		}
	}

	public GenericTouchScreen GetTouchScreen()
	{
		return m_touchScreen;
	}

	private TouchScreenFactory()
	{
	}
}
