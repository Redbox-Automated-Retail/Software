using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.DirectShow;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.MSHALTester;

public class CameraPreviewForm : Form
{
	private delegate void SetTextCallback(string msg);

	private string CurrentImage;

	private PlayerDevice videoDevice;

	private readonly Size ImageSize;

	private readonly ButtonAspectsManager Manager;

	private readonly AutoResetEvent ImageGrabbedWaiter = new AutoResetEvent(initialState: false);

	private readonly string ImagesFolder;

	private readonly AtomicFlag SnapFlag = new AtomicFlag();

	private readonly HardwareService Service;

	private IContainer components;

	private VideoSourcePlayer videoSourcePlayer;

	private Button button1;

	private Button button2;

	private Button button3;

	private Button button4;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private TextBox m_numberOfBarcodesBox;

	private TextBox m_decodeTimeBox;

	private TextBox m_barcodeBox;

	private TextBox m_snapStatusBox;

	private TextBox m_secureReadTB;

	private TextBox m_detectedErrorsTB;

	private BackgroundWorker backgroundWorker1;

	public CameraPreviewForm(HardwareService service, ButtonAspectsManager manager, string imageFolder)
	{
		InitializeComponent();
		Service = service;
		ImagesFolder = imageFolder;
		ImageSize = new Size(640, 480);
		Manager = manager;
	}

	private void OnLoad(object sender, EventArgs e)
	{
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			WriteErrorText("Found no devices.");
			return;
		}
		videoDevice = new PlayerDevice(filterInfoCollection[0].MonikerString, 500, debug: false);
		if (videoDevice == null)
		{
			WriteErrorText("Found no devices.");
			return;
		}
		bool flag = false;
		VideoCapabilities[] videoCapabilities = videoDevice.VideoCapabilities;
		foreach (VideoCapabilities videoCapabilities2 in videoCapabilities)
		{
			if (videoCapabilities2.FrameSize == ImageSize)
			{
				flag = true;
				videoDevice.VideoResolution = videoCapabilities2;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", base.Size.Width, base.Size.Height);
		}
		videoDevice.PlayingFinished += videoSource_PlayingFinished;
		videoSourcePlayer.NewFrame += SnapFrame;
		videoSourcePlayer.VideoSource = videoDevice;
		videoSourcePlayer.Start();
		Thread.Sleep(5000);
		ResetTextBoxes();
	}

	private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			string msg = reason.ToString();
			LogHelper.Instance.Log(msg);
			WriteErrorText(msg);
		}
	}

	private void SnapFrame(object sender, ref Bitmap image)
	{
		if (SnapFlag.Clear())
		{
			DateTime now = DateTime.Now;
			string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.jpg";
			CurrentImage = Path.Combine(ImagesFolder, path);
			LogHelper.Instance.Log("Snapframe called captured image {0}", CurrentImage);
			image.Save(CurrentImage);
			ImageGrabbedWaiter.Set();
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		button1.BackColor = Color.Red;
		button1.Enabled = false;
		ResetTextBoxes();
		SnapFlag.Set();
		backgroundWorker1.RunWorkerAsync();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		if (videoDevice != null)
		{
			videoDevice.DisplayPropertyPage(Process.GetCurrentProcess().MainWindowHandle);
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		WriteErrorText(string.Empty);
	}

	private void button4_Click(object sender, EventArgs e)
	{
		ShutdownPlayer();
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void ResetTextBoxes()
	{
		m_barcodeBox.Text = string.Empty;
		m_decodeTimeBox.Text = string.Empty;
		m_numberOfBarcodesBox.Text = string.Empty;
		m_snapStatusBox.Text = string.Empty;
		m_secureReadTB.Text = string.Empty;
		Application.DoEvents();
	}

	private void ShutdownPlayer()
	{
		try
		{
			if (videoSourcePlayer != null)
			{
				videoSourcePlayer.SignalToStop();
				videoSourcePlayer.WaitForStop();
				if (videoDevice != null)
				{
					videoSourcePlayer.NewFrame -= SnapFrame;
					videoDevice.PlayingFinished -= videoSource_PlayingFinished;
				}
				videoSourcePlayer.VideoSource = (videoDevice = null);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[CameraPreview] OnShutdown: caught an exception", e);
		}
	}

	private void WriteErrorText(string msg)
	{
		if (m_detectedErrorsTB.InvokeRequired)
		{
			SetTextCallback method = WriteErrorText;
			Invoke(method, msg);
		}
		else
		{
			m_detectedErrorsTB.Text = msg;
		}
	}

	private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
	{
		bool flag = false;
		using (new AtomicFlagHelper(SnapFlag))
		{
			flag = ImageGrabbedWaiter.WaitOne(5000);
		}
		if (!flag || !File.Exists(CurrentImage))
		{
			e.Result = ScanResult.New();
			return;
		}
		string fileName = Path.GetFileName(CurrentImage);
		using DecodeExecutor decodeExecutor = new DecodeExecutor(Service, fileName);
		decodeExecutor.Run();
		e.Result = decodeExecutor.ScanResult;
	}

	private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		button1.BackColor = Color.LightGray;
		button1.Enabled = true;
		ScanResult scanResult = e.Result as ScanResult;
		m_snapStatusBox.Text = ((!scanResult.SnapOk) ? "CAPTURE ERROR" : "SUCCESS");
		m_numberOfBarcodesBox.Text = scanResult.ReadCount.ToString();
		m_decodeTimeBox.Text = scanResult.ExecutionTime;
		m_barcodeBox.Text = scanResult.ScannedMatrix;
		m_secureReadTB.Text = scanResult.SecureCount.ToString();
		if (File.Exists(CurrentImage))
		{
			try
			{
				File.Delete(CurrentImage);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log($"Unable to delete image '{CurrentImage}'", e2);
			}
		}
		CurrentImage = null;
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
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.m_numberOfBarcodesBox = new System.Windows.Forms.TextBox();
		this.m_decodeTimeBox = new System.Windows.Forms.TextBox();
		this.m_barcodeBox = new System.Windows.Forms.TextBox();
		this.m_snapStatusBox = new System.Windows.Forms.TextBox();
		this.m_secureReadTB = new System.Windows.Forms.TextBox();
		this.m_detectedErrorsTB = new System.Windows.Forms.TextBox();
		this.videoSourcePlayer = new Redbox.DirectShow.VideoSourcePlayer();
		this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		base.SuspendLayout();
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Location = new System.Drawing.Point(24, 498);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(125, 60);
		this.button1.TabIndex = 1;
		this.button1.Text = "Snap and Decode";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.button2.BackColor = System.Drawing.Color.LightGray;
		this.button2.Location = new System.Drawing.Point(24, 574);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(125, 60);
		this.button2.TabIndex = 2;
		this.button2.Text = "Camera Properties";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.button3.BackColor = System.Drawing.Color.LightGray;
		this.button3.Location = new System.Drawing.Point(542, 553);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(125, 45);
		this.button3.TabIndex = 3;
		this.button3.Text = "Clear errors box";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.button4.BackColor = System.Drawing.Color.GreenYellow;
		this.button4.Location = new System.Drawing.Point(542, 604);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(125, 60);
		this.button4.TabIndex = 4;
		this.button4.Text = "Exit";
		this.button4.UseVisualStyleBackColor = false;
		this.button4.Click += new System.EventHandler(button4_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(181, 508);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(89, 13);
		this.label1.TabIndex = 5;
		this.label1.Text = "Number of Codes";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(181, 537);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(71, 13);
		this.label2.TabIndex = 6;
		this.label2.Text = "Decode Time";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(181, 566);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(47, 13);
		this.label3.TabIndex = 7;
		this.label3.Text = "Barcode";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(181, 600);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(62, 13);
		this.label4.TabIndex = 8;
		this.label4.Text = "SnapStatus";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(181, 635);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(108, 13);
		this.label5.TabIndex = 9;
		this.label5.Text = "Found Secure Code?";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(531, 503);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(81, 13);
		this.label6.TabIndex = 10;
		this.label6.Text = "Detected Errors";
		this.m_numberOfBarcodesBox.Location = new System.Drawing.Point(306, 505);
		this.m_numberOfBarcodesBox.Name = "m_numberOfBarcodesBox";
		this.m_numberOfBarcodesBox.Size = new System.Drawing.Size(100, 20);
		this.m_numberOfBarcodesBox.TabIndex = 11;
		this.m_decodeTimeBox.Location = new System.Drawing.Point(306, 534);
		this.m_decodeTimeBox.Name = "m_decodeTimeBox";
		this.m_decodeTimeBox.Size = new System.Drawing.Size(100, 20);
		this.m_decodeTimeBox.TabIndex = 12;
		this.m_barcodeBox.Location = new System.Drawing.Point(306, 566);
		this.m_barcodeBox.Name = "m_barcodeBox";
		this.m_barcodeBox.Size = new System.Drawing.Size(100, 20);
		this.m_barcodeBox.TabIndex = 13;
		this.m_snapStatusBox.Location = new System.Drawing.Point(306, 600);
		this.m_snapStatusBox.Name = "m_snapStatusBox";
		this.m_snapStatusBox.Size = new System.Drawing.Size(100, 20);
		this.m_snapStatusBox.TabIndex = 14;
		this.m_secureReadTB.Location = new System.Drawing.Point(306, 628);
		this.m_secureReadTB.Name = "m_secureReadTB";
		this.m_secureReadTB.Size = new System.Drawing.Size(100, 20);
		this.m_secureReadTB.TabIndex = 15;
		this.m_detectedErrorsTB.Location = new System.Drawing.Point(542, 527);
		this.m_detectedErrorsTB.Name = "m_detectedErrorsTB";
		this.m_detectedErrorsTB.Size = new System.Drawing.Size(122, 20);
		this.m_detectedErrorsTB.TabIndex = 16;
		this.videoSourcePlayer.Location = new System.Drawing.Point(24, 12);
		this.videoSourcePlayer.Name = "videoSourcePlayer";
		this.videoSourcePlayer.Size = new System.Drawing.Size(640, 480);
		this.videoSourcePlayer.TabIndex = 0;
		this.videoSourcePlayer.Text = "videoSourcePlayer";
		this.videoSourcePlayer.VideoSource = null;
		this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker1_DoWork);
		this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.DimGray;
		base.ClientSize = new System.Drawing.Size(724, 694);
		base.Controls.Add(this.m_detectedErrorsTB);
		base.Controls.Add(this.m_secureReadTB);
		base.Controls.Add(this.m_snapStatusBox);
		base.Controls.Add(this.m_barcodeBox);
		base.Controls.Add(this.m_decodeTimeBox);
		base.Controls.Add(this.m_numberOfBarcodesBox);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button4);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.videoSourcePlayer);
		base.Name = "CameraPreviewForm";
		this.Text = "CameraPreviewForm";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
