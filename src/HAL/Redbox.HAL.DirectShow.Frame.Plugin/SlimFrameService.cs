using System.Threading;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal sealed class SlimFrameService : AbstractDirectShowControl
{
	private DirectShowFrameControl Control;

	protected override bool OnStartup(string moniker)
	{
		Control = new DirectShowFrameControl(moniker, GrabTime, Debug);
		bool flag = false;
		VideoCapabilities[] videoCapabilities = Control.VideoCapabilities;
		foreach (VideoCapabilities videoCapabilities2 in videoCapabilities)
		{
			if (videoCapabilities2.FrameSize == ImageSize)
			{
				flag = true;
				Control.VideoResolution = videoCapabilities2;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", ImageSize.Width, ImageSize.Height);
			Control = null;
			return false;
		}
		Thread.Sleep(500);
		Control.PlayingFinished += base.PlayingFinished;
		Control.Start();
		Thread.Sleep(WakeupPause);
		LogHelper.Instance.Log("{0} Control running.", LogHeader);
		return true;
	}

	protected override void OnSnap(string fileName)
	{
		if (!Control.SimulateTrigger(fileName, SnapTimeout))
		{
			LogHelper.Instance.Log("{0} Image received wait timed out.", LogHeader);
		}
	}

	protected override bool OnShutdown()
	{
		Control.SignalToStop();
		Control.WaitForStop();
		Control.PlayingFinished -= base.PlayingFinished;
		Control = null;
		LogHelper.Instance.Log("{0} plugin shutdown.", LogHeader);
		return true;
	}

	internal SlimFrameService()
	{
	}
}
