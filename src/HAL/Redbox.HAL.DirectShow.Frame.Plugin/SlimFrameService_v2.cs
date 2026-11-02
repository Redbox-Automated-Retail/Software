using System.Collections.Generic;
using System.Threading;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal sealed class SlimFrameService_v2 : AbstractDirectShowControl
{
	private FrameControls FrameControl = FrameControls.Legacy;

	private AbstractDirectShowFrameControl? Control;

	protected override bool OnStartup(string moniker)
	{
		Control = AbstractDirectShowFrameControl.GetControl(FrameControl, moniker, GrabTime, Debug);
		bool found = false;
		VideoCapabilities[] videoCapabilities = Control.VideoCapabilities;
		foreach (VideoCapabilities capability in videoCapabilities)
		{
			if (capability.FrameSize == ImageSize)
			{
				found = true;
				Control.VideoResolution = capability;
				break;
			}
		}
		if (!found)
		{
			LogHelper.Instance.Log("{0} Unable to find video capability with size {1}w X {2}h", LogHeader, ImageSize.Width, ImageSize.Height);
			Control = null;
			return false;
		}
		Thread.Sleep(500);
		Control.PlayingFinished += base.PlayingFinished;
		Control.Start();
		Thread.Sleep(WakeupPause);
		return true;
	}

	protected override void OnSnap(string fileName)
	{
		if (!Control!.SimulateTrigger(fileName, SnapTimeout))
		{
			LogHelper.Instance.Log("[SlimFrameService_v2] Image received wait timed out.");
		}
	}

	protected override bool OnShutdown()
	{
		Control!.SignalToStop();
		Control.PlayingFinished -= base.PlayingFinished;
		Control = null;
		LogHelper.Instance.Log("[SlimFrameService_v2] Plugin shutdown.");
		return true;
	}

	protected override void OnGetProperties(IDictionary<string, object> properties)
	{
		if (properties.ContainsKey("ControlType"))
		{
			FrameControl = Enum<FrameControls>.ParseIgnoringCase((properties["ControlType"] as string)!, FrameControls.Legacy);
		}
	}

	internal SlimFrameService_v2()
	{
	}
}
