using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal sealed class LegacyService : ICameraPlugin
{
	private int SnapTimeout;

	private string ImagePath;

	private Size ImageSize;

	private int WakeupPause;

	private int GrabTime;

	private string SnapFile;

	private VCDevice3 Device;

	private int m_waitingState = Constants.NotRunning;

	private readonly AutoResetEvent ImageReceived = new AutoResetEvent(initialState: false);

	public bool IsRunning { get; private set; }

	public bool SupportsReset => true;

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("DirectShow Frame: unable to find any video devices.");
			return false;
		}
		Device = new VCDevice3(filterInfoCollection[0].MonikerString, GrabTime, debug: false);
		bool flag = false;
		VideoCapabilities[] videoCapabilities = Device.VideoCapabilities;
		foreach (VideoCapabilities videoCapabilities2 in videoCapabilities)
		{
			if (videoCapabilities2.FrameSize == ImageSize)
			{
				flag = true;
				Device.VideoResolution = videoCapabilities2;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", ImageSize.Width, ImageSize.Height);
			Device = null;
			return false;
		}
		Thread.Sleep(500);
		Device.SnapshotFrame += GetFrame;
		Device.PlayingFinished += PlayingFinished;
		Device.Start();
		Thread.Sleep(WakeupPause);
		LogHelper.Instance.Log("DirectShow frame control running.");
		return IsRunning = true;
	}

	public void Snap(string fileName)
	{
		if (IsRunning)
		{
			Interlocked.Exchange(ref m_waitingState, Constants.Running);
			Device.SimulateTrigger();
			if (!ImageReceived.WaitOne(SnapTimeout))
			{
				ImageReceived.Reset();
			}
			SnapFile = null;
			Interlocked.CompareExchange(ref m_waitingState, Constants.NotRunning, Constants.Running);
		}
	}

	public bool Stop()
	{
		return Shutdown();
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
	}

	internal LegacyService()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
		WakeupPause = 5000;
		GrabTime = 500;
	}

	private bool Shutdown()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("DirectShowService::Shutdown(): camera isn't running - bypass stop.");
			return false;
		}
		IsRunning = false;
		Device.SignalToStop();
		Device.WaitForStop();
		Device.SnapshotFrame -= GetFrame;
		Device.PlayingFinished -= PlayingFinished;
		Device = null;
		LogHelper.Instance.Log(">> DirectShow Snap plugin shutdown.");
		return true;
	}

	private void GetFrame(object sender, NewFrameEventArgs eventArgs)
	{
		if (Interlocked.CompareExchange(ref m_waitingState, Constants.NotRunning, Constants.Running) != Constants.NotRunning)
		{
			Bitmap frame = eventArgs.Frame;
			DateTime now = DateTime.Now;
			string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.jpg";
			SnapFile = Path.Combine(ImagePath, path);
			frame.Save(SnapFile);
			ImageReceived.Set();
		}
	}

	private void PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			LogHelper.Instance.Log("[DirectShow ERROR]Video capture ended with an error status {0}", reason.ToString());
		}
	}

	private int GetIntProperty(object o, int def)
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
}
