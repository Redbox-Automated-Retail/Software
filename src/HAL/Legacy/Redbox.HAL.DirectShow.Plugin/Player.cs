using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.DirectShow;
using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Plugin;

public class Player : Form
{
	private delegate void RunDisconnectCallback();

	private readonly AutoResetEvent CaptureEvent;

	private readonly int CaptureTimeout;

	private readonly string WorkingPath;

	private readonly Size VideoSize;

	private IContainer components;

	private VideoSourcePlayer videoSourcePlayer;

	internal bool InError { get; private set; }

	private VideoCaptureDevice VideoDevice { get; set; }

	private string CurrentImage { get; set; }

	internal Player(Service service)
	{
		InitializeComponent();
		WorkingPath = service.ImagePath;
		VideoSize = service.ImageSize;
		CaptureTimeout = service.SnapTimeout;
		CaptureEvent = new AutoResetEvent(initialState: false);
	}

	internal bool Connect()
	{
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("DirectShow: unable to find any video devices.");
			return false;
		}
		VideoDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
		bool flag = false;
		VideoCapabilities[] videoCapabilities = VideoDevice.VideoCapabilities;
		foreach (VideoCapabilities videoCapabilities2 in videoCapabilities)
		{
			if (videoCapabilities2.FrameSize == VideoSize)
			{
				flag = true;
				VideoDevice.VideoResolution = videoCapabilities2;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", new object[2]
			{
				base.Size.Width,
				base.Size.Height
			});
		}
		flag = false;
		VideoCapabilities[] snapshotCapabilities = VideoDevice.SnapshotCapabilities;
		foreach (VideoCapabilities videoCapabilities3 in snapshotCapabilities)
		{
			if (videoCapabilities3.FrameSize == VideoSize)
			{
				flag = true;
				VideoDevice.SnapshotResolution = videoCapabilities3;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find snapshot capability with size {0}w X {1}h", new object[2]
			{
				base.Size.Width,
				base.Size.Height
			});
		}
		VideoDevice.ProvideSnapshots = true;
		VideoDevice.SnapshotFrame += SnapshotCallback;
		VideoDevice.PlayingFinished += videoSource_PlayingFinished;
		videoSourcePlayer.VideoSource = VideoDevice;
		videoSourcePlayer.Start();
		LogHelper.Instance.Log(">> DirectShow.connect() ok");
		return true;
	}

	internal string Snap()
	{
		if (VideoDevice == null)
		{
			return CurrentImage = null;
		}
		try
		{
			VideoDevice.SimulateTrigger();
			if (!CaptureEvent.WaitOne(CaptureTimeout))
			{
				CurrentImage = null;
				CaptureEvent.Reset();
			}
			return CurrentImage;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DirectShow player.Snap() caught an exception.", ex);
			return CurrentImage = null;
		}
	}

	internal void Disconnect()
	{
		if (videoSourcePlayer == null)
		{
			return;
		}
		if (videoSourcePlayer.InvokeRequired)
		{
			RunDisconnectCallback method = Disconnect;
			Invoke(method);
			return;
		}
		try
		{
			videoSourcePlayer.SignalToStop();
			videoSourcePlayer.WaitForStop();
			if (VideoDevice != null)
			{
				VideoDevice.SnapshotFrame -= SnapshotCallback;
				VideoDevice.PlayingFinished -= videoSource_PlayingFinished;
			}
			VideoSourcePlayer obj = videoSourcePlayer;
			VideoCaptureDevice videoSource = (VideoDevice = null);
			obj.VideoSource = videoSource;
			LogHelper.Instance.Log("Player.Disconnect() is finished.");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DirectShow player.Disconnect() caught an exception.", ex);
		}
	}

	internal void ClearErrorState()
	{
		InError = false;
	}

	private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			LogHelper.Instance.Log("[CAMERA ERROR]Video capture ended with an error status {0}", new object[1] { reason.ToString() });
			InError = true;
		}
	}

	private void SnapshotCallback(object sender, NewFrameEventArgs eventArgs)
	{
		Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
		DateTime now = DateTime.Now;
		string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.jpg";
		CurrentImage = Path.Combine(WorkingPath, path);
		bitmap.Save(CurrentImage);
		CaptureEvent.Set();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.videoSourcePlayer = new Redbox.DirectShow.VideoSourcePlayer();
		base.SuspendLayout();
		this.videoSourcePlayer.Location = new System.Drawing.Point(33, 12);
		this.videoSourcePlayer.Name = "videoSourcePlayer";
		this.videoSourcePlayer.Size = new System.Drawing.Size(640, 480);
		this.videoSourcePlayer.TabIndex = 0;
		this.videoSourcePlayer.Text = "videoSourcePlayer";
		this.videoSourcePlayer.VideoSource = null;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(718, 523);
		base.Controls.Add(this.videoSourcePlayer);
		base.Name = "Player";
		this.Text = "Player";
		base.ResumeLayout(false);
	}
}
