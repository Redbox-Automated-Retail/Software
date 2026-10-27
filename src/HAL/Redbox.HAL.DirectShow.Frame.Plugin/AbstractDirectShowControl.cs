using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal abstract class AbstractDirectShowControl : ICameraPlugin
{
	protected readonly string LogHeader;

	protected int SnapTimeout;

	protected Size ImageSize;

	protected int WakeupPause;

	protected int GrabTime;

	protected bool Debug;

	public bool SupportsReset => true;

	public bool IsRunning { get; protected set; }

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("{0} Unable to find any video devices.", LogHeader);
			return false;
		}
		return IsRunning = OnStartup(filterInfoCollection[0].MonikerString);
	}

	public void Snap(string file)
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("{0} Control is not in the running state.", LogHeader);
		}
		else
		{
			OnSnap(file);
		}
	}

	public bool Stop()
	{
		if (!IsRunning)
		{
			return false;
		}
		IsRunning = false;
		return OnShutdown();
	}

	public void InitWithProperties(IDictionary<string, object> properties)
	{
		if (properties.ContainsKey("VideoFormat"))
		{
			ImageSize = (Size)properties["VideoFormat"];
		}
		if (properties.ContainsKey("WakeupPause"))
		{
			WakeupPause = GetIntProperty(properties["WakeupPause"], WakeupPause);
		}
		if (properties.ContainsKey("SnapTimeout"))
		{
			SnapTimeout = GetIntProperty(properties["SnapTimeout"], SnapTimeout);
		}
		if (properties.ContainsKey("GrabWait"))
		{
			GrabTime = GetIntProperty(properties["GrabWait"], GrabTime);
		}
		if (properties.ContainsKey("DebugControl"))
		{
			try
			{
				Debug = (bool)properties["DebugControl"];
			}
			catch
			{
				Debug = false;
			}
		}
		OnGetProperties(properties);
	}

	protected int GetIntProperty(object o, int def)
	{
		try
		{
			return (int)Convert.ChangeType(o, typeof(int));
		}
		catch
		{
			return def;
		}
	}

	protected void PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			LogHelper.Instance.Log("{0} ERROR: Video capture ended with an error status {1}", LogHeader, reason.ToString());
		}
	}

	protected AbstractDirectShowControl()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
		WakeupPause = 5000;
		GrabTime = 500;
		LogHeader = $"[DirectShowPlugin:{GetType().Name}]";
	}

	protected abstract void OnSnap(string fileName);

	protected abstract bool OnShutdown();

	protected abstract bool OnStartup(string moniker);

	protected virtual void OnGetProperties(IDictionary<string, object> properties)
	{
	}
}
