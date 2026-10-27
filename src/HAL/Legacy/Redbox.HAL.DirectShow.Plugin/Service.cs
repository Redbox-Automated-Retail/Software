using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Plugin;

internal sealed class Service : ICameraPlugin
{
	public bool IsRunning { get; private set; }

	public bool SupportsReset => true;

	internal int SnapTimeout { get; private set; }

	internal string ImagePath { get; private set; }

	internal Size ImageSize { get; private set; }

	private Player Worker { get; set; }

	private string SnapFile { get; set; }

	private int WakeupPause { get; set; }

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		if (Worker == null)
		{
			Worker = new Player(this);
			Worker.Visible = false;
		}
		IsRunning = Worker.Connect();
		Thread.Sleep(WakeupPause);
		if (!IsRunning)
		{
			Worker.Disconnect();
			Worker.Close();
			Worker.Dispose();
			Worker = null;
		}
		return IsRunning;
	}

	public string Snap()
	{
		if (!IsRunning)
		{
			return null;
		}
		string text = Worker.Snap();
		if (text == null && Worker.InError)
		{
			Shutdown();
			LogHelper.Instance.Log("The player deteted an error; shut down graph.");
		}
		return text;
	}

	public bool Stop()
	{
		return Shutdown();
	}

	public void TurnOff()
	{
		Shutdown();
	}

	public void InitWithProperties(IDictionary<string, object> properties)
	{
		if (properties.ContainsKey("WorkingPath"))
		{
			ImagePath = properties["WorkingPath"] as string;
		}
		if (properties.ContainsKey("VideoFormat"))
		{
			ImageSize = (Size)properties["VideoFormat"];
		}
		if (properties.ContainsKey("SnapTimeout"))
		{
			try
			{
				SnapTimeout = (int)Convert.ChangeType(properties["SnapTimeout"], typeof(int));
			}
			catch
			{
				SnapTimeout = 5000;
			}
		}
		if (properties.ContainsKey("WakeupPause"))
		{
			try
			{
				WakeupPause = (int)Convert.ChangeType(properties["WakeupPause"], typeof(int));
			}
			catch
			{
				WakeupPause = 5000;
			}
		}
	}

	internal Service()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
		WakeupPause = 5000;
	}

	private bool Shutdown()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("DirectShowService::Shutdown(): camera isn't running - bypass stop.");
			return false;
		}
		IsRunning = false;
		Worker.Disconnect();
		Worker.Close();
		Worker.Dispose();
		Worker = null;
		LogHelper.Instance.Log(">> DirectShow plugin shutdown.");
		return true;
	}
}
