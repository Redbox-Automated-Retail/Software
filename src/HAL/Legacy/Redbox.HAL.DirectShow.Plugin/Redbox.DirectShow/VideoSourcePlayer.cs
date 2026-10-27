using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Redbox.DirectShow;

public class VideoSourcePlayer : Control
{
	public delegate void NewFrameHandler(object sender, ref Bitmap image);

	private IVideoSource videoSource;

	private Bitmap currentFrame;

	private Bitmap convertedFrame;

	private string lastMessage;

	private Color borderColor = Color.Black;

	private Size frameSize = new Size(320, 240);

	private bool autosize;

	private bool keepRatio;

	private bool needSizeUpdate;

	private bool firstFrameNotProcessed = true;

	private volatile bool requestedToStop;

	private Control parent;

	private object sync = new object();

	private IContainer components;

	[DefaultValue(false)]
	public bool AutoSizeControl
	{
		get
		{
			return autosize;
		}
		set
		{
			autosize = value;
			UpdatePosition();
		}
	}

	[DefaultValue(false)]
	public bool KeepAspectRatio
	{
		get
		{
			return keepRatio;
		}
		set
		{
			keepRatio = value;
			Invalidate();
		}
	}

	[DefaultValue(typeof(Color), "Black")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			Invalidate();
		}
	}

	[Browsable(false)]
	public IVideoSource VideoSource
	{
		get
		{
			return videoSource;
		}
		set
		{
			CheckForCrossThreadAccess();
			if (videoSource != null)
			{
				videoSource.NewFrame -= videoSource_NewFrame;
				videoSource.VideoSourceError -= videoSource_VideoSourceError;
				videoSource.PlayingFinished -= videoSource_PlayingFinished;
			}
			lock (sync)
			{
				if (currentFrame != null)
				{
					currentFrame.Dispose();
					currentFrame = null;
				}
			}
			videoSource = value;
			if (videoSource != null)
			{
				videoSource.NewFrame += videoSource_NewFrame;
				videoSource.VideoSourceError += videoSource_VideoSourceError;
				videoSource.PlayingFinished += videoSource_PlayingFinished;
			}
			else
			{
				frameSize = new Size(320, 240);
			}
			lastMessage = null;
			needSizeUpdate = true;
			firstFrameNotProcessed = true;
			Invalidate();
		}
	}

	[Browsable(false)]
	public bool IsRunning
	{
		get
		{
			CheckForCrossThreadAccess();
			if (videoSource == null)
			{
				return false;
			}
			return videoSource.IsRunning;
		}
	}

	public event NewFrameHandler NewFrame;

	public event PlayingFinishedEventHandler PlayingFinished;

	public VideoSourcePlayer()
	{
		InitializeComponent();
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
	}

	private void CheckForCrossThreadAccess()
	{
		if (!base.IsHandleCreated)
		{
			CreateControl();
			if (!base.IsHandleCreated)
			{
				CreateHandle();
			}
		}
		if (base.InvokeRequired)
		{
			throw new InvalidOperationException("Cross thread access to the control is not allowed.");
		}
	}

	public void Start()
	{
		CheckForCrossThreadAccess();
		requestedToStop = false;
		if (videoSource != null)
		{
			firstFrameNotProcessed = true;
			videoSource.Start();
			Invalidate();
		}
	}

	public void Stop()
	{
		CheckForCrossThreadAccess();
		requestedToStop = true;
		if (videoSource != null)
		{
			videoSource.Stop();
			if (currentFrame != null)
			{
				currentFrame.Dispose();
				currentFrame = null;
			}
			Invalidate();
		}
	}

	public void SignalToStop()
	{
		CheckForCrossThreadAccess();
		requestedToStop = true;
		if (videoSource != null)
		{
			videoSource.SignalToStop();
		}
	}

	public void WaitForStop()
	{
		CheckForCrossThreadAccess();
		if (!requestedToStop)
		{
			SignalToStop();
		}
		if (videoSource != null)
		{
			videoSource.WaitForStop();
			if (currentFrame != null)
			{
				currentFrame.Dispose();
				currentFrame = null;
			}
			Invalidate();
		}
	}

	public Bitmap GetCurrentVideoFrame()
	{
		lock (sync)
		{
			return (currentFrame == null) ? null : Image.Clone(currentFrame);
		}
	}

	private void VideoSourcePlayer_Paint(object sender, PaintEventArgs e)
	{
		if (!base.Visible)
		{
			return;
		}
		if (needSizeUpdate || firstFrameNotProcessed)
		{
			UpdatePosition();
			needSizeUpdate = false;
		}
		lock (sync)
		{
			Graphics graphics = e.Graphics;
			Rectangle clientRectangle = base.ClientRectangle;
			Pen pen = new Pen(borderColor, 1f);
			graphics.DrawRectangle(pen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
			if (videoSource != null)
			{
				if (currentFrame != null && lastMessage == null)
				{
					Bitmap bitmap = ((convertedFrame != null) ? convertedFrame : currentFrame);
					if (keepRatio)
					{
						double num = (double)bitmap.Width / (double)bitmap.Height;
						Rectangle rectangle = clientRectangle;
						if ((double)clientRectangle.Width < (double)clientRectangle.Height * num)
						{
							rectangle.Height = (int)((double)clientRectangle.Width / num);
						}
						else
						{
							rectangle.Width = (int)((double)clientRectangle.Height * num);
						}
						rectangle.X = (clientRectangle.Width - rectangle.Width) / 2;
						rectangle.Y = (clientRectangle.Height - rectangle.Height) / 2;
						graphics.DrawImage(bitmap, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
					}
					else
					{
						graphics.DrawImage(bitmap, clientRectangle.X + 1, clientRectangle.Y + 1, clientRectangle.Width - 2, clientRectangle.Height - 2);
					}
					firstFrameNotProcessed = false;
				}
				else
				{
					SolidBrush solidBrush = new SolidBrush(ForeColor);
					graphics.DrawString((lastMessage == null) ? "Connecting ..." : lastMessage, Font, solidBrush, new PointF(5f, 5f));
					solidBrush.Dispose();
				}
			}
			pen.Dispose();
		}
	}

	private void UpdatePosition()
	{
		if (autosize && Dock != DockStyle.Fill && base.Parent != null)
		{
			Rectangle clientRectangle = base.Parent.ClientRectangle;
			int num = frameSize.Width;
			int num2 = frameSize.Height;
			SuspendLayout();
			base.Location = new Point((clientRectangle.Width - num - 2) / 2, (clientRectangle.Height - num2 - 2) / 2);
			base.Size = new Size(num + 2, num2 + 2);
			ResumeLayout();
		}
	}

	private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
	{
		if (requestedToStop)
		{
			return;
		}
		Bitmap image = (Bitmap)eventArgs.Frame.Clone();
		if (NewFrame != null)
		{
			NewFrame(this, ref image);
		}
		lock (sync)
		{
			if (currentFrame != null)
			{
				if (currentFrame.Size != eventArgs.Frame.Size)
				{
					needSizeUpdate = true;
				}
				currentFrame.Dispose();
				currentFrame = null;
			}
			if (convertedFrame != null)
			{
				convertedFrame.Dispose();
				convertedFrame = null;
			}
			currentFrame = image;
			frameSize = currentFrame.Size;
			lastMessage = null;
			if (currentFrame.PixelFormat == PixelFormat.Format16bppGrayScale || currentFrame.PixelFormat == PixelFormat.Format48bppRgb || currentFrame.PixelFormat == PixelFormat.Format64bppArgb)
			{
				convertedFrame = Image.Convert16bppTo8bpp(currentFrame);
			}
		}
		Invalidate();
	}

	private void videoSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
	{
		lastMessage = eventArgs.Description;
		Invalidate();
	}

	private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		switch (reason)
		{
		case ReasonToFinishPlaying.EndOfStreamReached:
			lastMessage = "Video has finished";
			break;
		case ReasonToFinishPlaying.StoppedByUser:
			lastMessage = "Video was stopped";
			break;
		case ReasonToFinishPlaying.DeviceLost:
			lastMessage = "Video device was unplugged";
			break;
		case ReasonToFinishPlaying.VideoSourceError:
			lastMessage = "Video has finished because of error in video source";
			break;
		default:
			lastMessage = "Video has finished for unknown reason";
			break;
		}
		Invalidate();
		if (PlayingFinished != null)
		{
			PlayingFinished(this, reason);
		}
	}

	private void VideoSourcePlayer_ParentChanged(object sender, EventArgs e)
	{
		if (parent != null)
		{
			parent.SizeChanged -= parent_SizeChanged;
		}
		parent = base.Parent;
		if (parent != null)
		{
			parent.SizeChanged += parent_SizeChanged;
		}
	}

	private void parent_SizeChanged(object sender, EventArgs e)
	{
		UpdatePosition();
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
		base.SuspendLayout();
		base.Paint += new System.Windows.Forms.PaintEventHandler(VideoSourcePlayer_Paint);
		base.ParentChanged += new System.EventHandler(VideoSourcePlayer_ParentChanged);
		base.ResumeLayout(false);
	}
}
