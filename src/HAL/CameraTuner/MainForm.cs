using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.DirectShow;
using Redbox.HAL.CameraTuner.Properties;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.CameraTuner;

public class MainForm : Form
{
	private delegate void SetTextCallback(string msg);

	private readonly AtomicFlag SnapFlag = new AtomicFlag();

	private readonly TunerLog TunerLogInstance;

	private readonly Size ImageSize;

	private readonly string ImagesFolder;

	private readonly AutoResetEvent ImageGrabbedWaiter;

	private readonly bool Details;

	private string CurrentImage;

	private FilterInfoCollection videoDevices;

	private PlayerDevice videoDevice;

	private IContainer components;

	private VideoSourcePlayer videoSourcePlayer;

	private Button button1;

	private Button button2;

	private Button button3;

	private TextBox m_numberOfBarcodesBox;

	private TextBox m_decodeTimeBox;

	private TextBox m_barcodeBox;

	private TextBox m_snapStatusBox;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label6;

	private TextBox m_detectedErrorsTB;

	private Button button4;

	private BackgroundWorker backgroundWorker1;

	private Label label5;

	private TextBox m_secureReadTB;

	public MainForm(TunerLog log)
	{
		InitializeComponent();
		TunerLogInstance = log;
		ImageGrabbedWaiter = new AutoResetEvent(initialState: false);
		ImageSize = new Size(640, 480);
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		ImagesFolder = service.InstallPath("Video");
		try
		{
			if (!Directory.Exists(ImagesFolder))
			{
				Directory.CreateDirectory(ImagesFolder);
			}
		}
		catch
		{
		}
		Details = Settings.Default.DetailedLog;
	}

	private void OnLoad(object sender, EventArgs e)
	{
		videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (videoDevices == null || videoDevices.Count == 0)
		{
			WriteErrorText("Found no devices.");
			return;
		}
		videoDevice = new PlayerDevice(videoDevices[0].MonikerString, 500, debug: false);
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
		Thread.Sleep(Settings.Default.WakeupPause);
		ResetView();
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
			string path = ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile("jpg");
			CurrentImage = Path.Combine(ImagesFolder, path);
			if (Details)
			{
				LogHelper.Instance.Log("Snapframe called captured image {0}", CurrentImage);
			}
			image.Save(CurrentImage);
			ImageGrabbedWaiter.Set();
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		OnShutdown();
		Application.Exit();
	}

	private void ResetView()
	{
		m_barcodeBox.Text = "None";
		m_decodeTimeBox.Text = "0.0 s";
		m_snapStatusBox.Text = string.Empty;
		TextBox secureReadTB = m_secureReadTB;
		string text = (m_numberOfBarcodesBox.Text = "0");
		secureReadTB.Text = text;
		Application.DoEvents();
	}

	private void OnShutdown()
	{
		ShutdownPlayer();
		TunerLogInstance.Dispose();
	}

	private void ShutdownPlayer()
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

	private void button4_Click(object sender, EventArgs e)
	{
		button1.BackColor = Color.Red;
		button1.Enabled = false;
		ResetView();
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

	private void button4_Click_1(object sender, EventArgs e)
	{
		WriteErrorText(string.Empty);
	}

	private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
	{
		bool flag = false;
		using (new AtomicFlagHelper(SnapFlag))
		{
			flag = ImageGrabbedWaiter.WaitOne(5000);
		}
		e.Result = (flag ? ScanResult.Scan(CurrentImage) : ScanResult.ErrorResult());
	}

	private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		button1.BackColor = Color.LightGray;
		button1.Enabled = true;
		ScanResult scanResult = e.Result as ScanResult;
		m_snapStatusBox.Text = ((!scanResult.SnapOk) ? "CAPTURE ERROR" : "SUCCESS");
		m_numberOfBarcodesBox.Text = scanResult.ReadCount.ToString();
		m_decodeTimeBox.Text = $"{scanResult.ExecutionTime.Seconds}.{scanResult.ExecutionTime.Milliseconds} s";
		m_barcodeBox.Text = scanResult.ScannedMatrix;
		m_secureReadTB.Text = scanResult.SecureCount.ToString();
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
		this.m_numberOfBarcodesBox = new System.Windows.Forms.TextBox();
		this.m_decodeTimeBox = new System.Windows.Forms.TextBox();
		this.m_barcodeBox = new System.Windows.Forms.TextBox();
		this.m_snapStatusBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.m_detectedErrorsTB = new System.Windows.Forms.TextBox();
		this.button4 = new System.Windows.Forms.Button();
		this.videoSourcePlayer = new Redbox.DirectShow.VideoSourcePlayer();
		this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		this.label5 = new System.Windows.Forms.Label();
		this.m_secureReadTB = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Location = new System.Drawing.Point(31, 504);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(128, 61);
		this.button1.TabIndex = 1;
		this.button1.Text = "Snap And Decode";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(button4_Click);
		this.button2.BackColor = System.Drawing.Color.LightGray;
		this.button2.Location = new System.Drawing.Point(31, 591);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(128, 63);
		this.button2.TabIndex = 2;
		this.button2.Text = "Camera Properties";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.button3.BackColor = System.Drawing.Color.GreenYellow;
		this.button3.Location = new System.Drawing.Point(527, 603);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(144, 60);
		this.button3.TabIndex = 3;
		this.button3.Text = "Exit";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.m_numberOfBarcodesBox.BackColor = System.Drawing.Color.White;
		this.m_numberOfBarcodesBox.Location = new System.Drawing.Point(305, 504);
		this.m_numberOfBarcodesBox.Name = "m_numberOfBarcodesBox";
		this.m_numberOfBarcodesBox.ReadOnly = true;
		this.m_numberOfBarcodesBox.Size = new System.Drawing.Size(100, 20);
		this.m_numberOfBarcodesBox.TabIndex = 4;
		this.m_decodeTimeBox.BackColor = System.Drawing.Color.White;
		this.m_decodeTimeBox.Location = new System.Drawing.Point(305, 534);
		this.m_decodeTimeBox.Name = "m_decodeTimeBox";
		this.m_decodeTimeBox.ReadOnly = true;
		this.m_decodeTimeBox.Size = new System.Drawing.Size(100, 20);
		this.m_decodeTimeBox.TabIndex = 5;
		this.m_barcodeBox.BackColor = System.Drawing.Color.White;
		this.m_barcodeBox.Location = new System.Drawing.Point(305, 568);
		this.m_barcodeBox.Name = "m_barcodeBox";
		this.m_barcodeBox.ReadOnly = true;
		this.m_barcodeBox.Size = new System.Drawing.Size(100, 20);
		this.m_barcodeBox.TabIndex = 6;
		this.m_snapStatusBox.BackColor = System.Drawing.Color.White;
		this.m_snapStatusBox.Location = new System.Drawing.Point(305, 603);
		this.m_snapStatusBox.Name = "m_snapStatusBox";
		this.m_snapStatusBox.ReadOnly = true;
		this.m_snapStatusBox.Size = new System.Drawing.Size(100, 20);
		this.m_snapStatusBox.TabIndex = 7;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(172, 507);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(89, 13);
		this.label1.TabIndex = 9;
		this.label1.Text = "Number of Codes";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(172, 537);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(30, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Time";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(172, 575);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(47, 13);
		this.label3.TabIndex = 11;
		this.label3.Text = "Barcode";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(172, 610);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(65, 13);
		this.label4.TabIndex = 12;
		this.label4.Text = "Snap Status";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(509, 502);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(83, 13);
		this.label6.TabIndex = 14;
		this.label6.Text = "Detected errors:";
		this.m_detectedErrorsTB.Location = new System.Drawing.Point(512, 518);
		this.m_detectedErrorsTB.Name = "m_detectedErrorsTB";
		this.m_detectedErrorsTB.ReadOnly = true;
		this.m_detectedErrorsTB.Size = new System.Drawing.Size(159, 20);
		this.m_detectedErrorsTB.TabIndex = 15;
		this.button4.BackColor = System.Drawing.Color.LightGray;
		this.button4.Location = new System.Drawing.Point(512, 544);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(126, 44);
		this.button4.TabIndex = 16;
		this.button4.Text = "Clear errors box";
		this.button4.UseVisualStyleBackColor = false;
		this.button4.Click += new System.EventHandler(button4_Click_1);
		this.videoSourcePlayer.Location = new System.Drawing.Point(31, 12);
		this.videoSourcePlayer.Name = "videoSourcePlayer";
		this.videoSourcePlayer.Size = new System.Drawing.Size(640, 480);
		this.videoSourcePlayer.TabIndex = 0;
		this.videoSourcePlayer.Text = "videoSourcePlayer1";
		this.videoSourcePlayer.VideoSource = null;
		this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker1_DoWork);
		this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(172, 641);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(108, 13);
		this.label5.TabIndex = 17;
		this.label5.Text = "Secure Marker Count";
		this.m_secureReadTB.Location = new System.Drawing.Point(305, 634);
		this.m_secureReadTB.Name = "m_secureReadTB";
		this.m_secureReadTB.Size = new System.Drawing.Size(100, 20);
		this.m_secureReadTB.TabIndex = 18;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.DimGray;
		base.ClientSize = new System.Drawing.Size(723, 670);
		base.Controls.Add(this.m_secureReadTB);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.button4);
		base.Controls.Add(this.m_detectedErrorsTB);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_snapStatusBox);
		base.Controls.Add(this.m_barcodeBox);
		base.Controls.Add(this.m_decodeTimeBox);
		base.Controls.Add(this.m_numberOfBarcodesBox);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.videoSourcePlayer);
		base.Name = "MainForm";
		this.Text = "Redbox Camera Tuner";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
