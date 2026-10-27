using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public sealed class FrameControlLegacy : AbstractDirectShowFrameControl
{
	private int m_simulateTrigger = ControlConstants.NotTriggered;

	private int m_grabImage = ControlConstants.NotTriggered;

	private readonly ManualResetEvent FrameReceivedEvent = new ManualResetEvent(initialState: false);

	private readonly ManualResetEvent ImageCapturedEvent = new ManualResetEvent(initialState: false);

	protected override bool SnapRequested
	{
		get
		{
			int num = Interlocked.CompareExchange(ref m_grabImage, ControlConstants.NotTriggered, ControlConstants.Triggered);
			return ControlConstants.Triggered == num;
		}
	}

	protected override bool OnSimulateTrigger(string file, int captureWait)
	{
		ImageCapturedEvent.Reset();
		FrameReceivedEvent.Reset();
		if (Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.Triggered) == ControlConstants.Triggered)
		{
			return false;
		}
		bool flag = ImageCapturedEvent.WaitOne(captureWait);
		if (!flag)
		{
			LogHelper.Instance.Log("[{0}] The trigger event was not set.", ControlMoniker);
		}
		return flag;
	}

	protected override void OnImageCaptured()
	{
		FrameReceivedEvent.Set();
	}

	protected override bool OnGraphEvent()
	{
		bool result = true;
		if (Thread.VolatileRead(ref m_simulateTrigger) == ControlConstants.Triggered)
		{
			Interlocked.Exchange(ref m_grabImage, ControlConstants.Triggered);
			if (!FrameReceivedEvent.WaitOne(GrabWaitTime))
			{
				Interlocked.Exchange(ref m_grabImage, ControlConstants.NotTriggered);
				LogHelper.Instance.Log("[{0}] There are no captured frames from the device.", ControlMoniker);
				result = false;
			}
			else
			{
				ImageCapturedEvent.Set();
			}
			Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.NotTriggered);
		}
		return result;
	}

	protected override void OnFree()
	{
		FrameReceivedEvent.Close();
		ImageCapturedEvent.Close();
	}

	internal FrameControlLegacy(string moniker, int grab, bool debug)
		: base(moniker, grab, debug)
	{
	}
}
