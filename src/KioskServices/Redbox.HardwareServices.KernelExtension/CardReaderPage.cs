using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;

namespace Redbox.HardwareServices.KernelExtension;

public class CardReaderPage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Panel panelTopHeader;

	private Label labelHeader;

	private GroupBox groupBoxDeviceService;

	private CheckBox checkBoxDeviceServiceCardReaderConnected;

	private CheckBox checkBoxDeviceServiceConnected;

	private GroupBox groupBoxUSBCardReader;

	private CheckBox checkBoxUSBCardReaderConnected;

	private Panel panelBottom;

	private Button buttonRefresh;

	private GroupBox groupBoxTrackDataService;

	private Label labelRegisteredServiceValue;

	private Label labelRegisteredService;

	private TextBox textBoxDeviceServiceClientVersion;

	private Label labelDeviceServiceClientVersion;

	private CheckBox checkBoxDeviceServiceClientCompatible;

	private TextBox textBoxDeviceServiceVersion;

	private Label labelDeviceServiceVersionLabel;

	private CheckBox checkBoxSupportsEMV;

	private Panel panelLoadingData;

	private Label labelLoading;

	public CardReaderPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		ValidateVersionModel validateVersionModel = null;
		bool supportsEMV = false;
		panelLoadingData.Visible = true;
		Task.Factory.StartNew(delegate
		{
			validateVersionModel = deviceServiceClientHelper?.ValidateVersion();
			supportsEMV = deviceServiceClientHelper?.SupportsEMV ?? false;
		}).ContinueWith(delegate
		{
			checkBoxDeviceServiceClientCompatible.Checked = validateVersionModel?.IsCompatible ?? false;
			checkBoxDeviceServiceCardReaderConnected.Checked = deviceServiceClientHelper?.IsCardReaderConnected ?? false;
			checkBoxSupportsEMV.Checked = supportsEMV;
			panelLoadingData.Visible = false;
		}, CancellationToken.None, TaskContinuationOptions.None, service?.UITaskScheduler);
		textBoxDeviceServiceVersion.Text = deviceServiceClientHelper?.DeviceServiceVersion?.ToString();
		textBoxDeviceServiceClientVersion.Text = deviceServiceClientHelper?.DeviceServiceClientVersion?.ToString();
		checkBoxDeviceServiceConnected.Checked = deviceServiceClientHelper?.IsConnectedToDeviceService ?? false;
		checkBoxUSBCardReaderConnected.Checked = CardReaderDetector.IsUSBCardReaderAttached();
		string text = ServiceLocator.Instance.GetService<ITrackDataService>()?.GetType()?.ToString();
		if (!string.IsNullOrEmpty(text) && text.Contains("."))
		{
			text = text.Split('.').Last();
		}
		labelRegisteredServiceValue.Text = text ?? "";
	}

	private void buttonRefresh_Click(object sender, EventArgs e)
	{
		LoadValues(null);
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
		this.panelTopHeader = new System.Windows.Forms.Panel();
		this.labelHeader = new System.Windows.Forms.Label();
		this.groupBoxDeviceService = new System.Windows.Forms.GroupBox();
		this.checkBoxSupportsEMV = new System.Windows.Forms.CheckBox();
		this.textBoxDeviceServiceClientVersion = new System.Windows.Forms.TextBox();
		this.labelDeviceServiceClientVersion = new System.Windows.Forms.Label();
		this.checkBoxDeviceServiceClientCompatible = new System.Windows.Forms.CheckBox();
		this.textBoxDeviceServiceVersion = new System.Windows.Forms.TextBox();
		this.labelDeviceServiceVersionLabel = new System.Windows.Forms.Label();
		this.checkBoxDeviceServiceCardReaderConnected = new System.Windows.Forms.CheckBox();
		this.checkBoxDeviceServiceConnected = new System.Windows.Forms.CheckBox();
		this.groupBoxUSBCardReader = new System.Windows.Forms.GroupBox();
		this.checkBoxUSBCardReaderConnected = new System.Windows.Forms.CheckBox();
		this.panelBottom = new System.Windows.Forms.Panel();
		this.buttonRefresh = new System.Windows.Forms.Button();
		this.groupBoxTrackDataService = new System.Windows.Forms.GroupBox();
		this.labelRegisteredServiceValue = new System.Windows.Forms.Label();
		this.labelRegisteredService = new System.Windows.Forms.Label();
		this.panelLoadingData = new System.Windows.Forms.Panel();
		this.labelLoading = new System.Windows.Forms.Label();
		this.panelTopHeader.SuspendLayout();
		this.groupBoxDeviceService.SuspendLayout();
		this.groupBoxUSBCardReader.SuspendLayout();
		this.panelBottom.SuspendLayout();
		this.groupBoxTrackDataService.SuspendLayout();
		this.panelLoadingData.SuspendLayout();
		base.SuspendLayout();
		this.panelTopHeader.BackColor = System.Drawing.Color.Tomato;
		this.panelTopHeader.Controls.Add(this.labelHeader);
		this.panelTopHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
		this.panelTopHeader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.panelTopHeader.Name = "panelTopHeader";
		this.panelTopHeader.Size = new System.Drawing.Size(744, 47);
		this.panelTopHeader.TabIndex = 0;
		this.labelHeader.AutoSize = true;
		this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelHeader.Location = new System.Drawing.Point(4, 7);
		this.labelHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(267, 31);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Credit Card Reader";
		this.groupBoxDeviceService.Controls.Add(this.panelLoadingData);
		this.groupBoxDeviceService.Controls.Add(this.checkBoxSupportsEMV);
		this.groupBoxDeviceService.Controls.Add(this.textBoxDeviceServiceClientVersion);
		this.groupBoxDeviceService.Controls.Add(this.labelDeviceServiceClientVersion);
		this.groupBoxDeviceService.Controls.Add(this.checkBoxDeviceServiceClientCompatible);
		this.groupBoxDeviceService.Controls.Add(this.textBoxDeviceServiceVersion);
		this.groupBoxDeviceService.Controls.Add(this.labelDeviceServiceVersionLabel);
		this.groupBoxDeviceService.Controls.Add(this.checkBoxDeviceServiceCardReaderConnected);
		this.groupBoxDeviceService.Controls.Add(this.checkBoxDeviceServiceConnected);
		this.groupBoxDeviceService.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupBoxDeviceService.Location = new System.Drawing.Point(0, 47);
		this.groupBoxDeviceService.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxDeviceService.Name = "groupBoxDeviceService";
		this.groupBoxDeviceService.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxDeviceService.Size = new System.Drawing.Size(744, 209);
		this.groupBoxDeviceService.TabIndex = 1;
		this.groupBoxDeviceService.TabStop = false;
		this.groupBoxDeviceService.Text = "Device Service Card Reader";
		this.checkBoxSupportsEMV.Location = new System.Drawing.Point(51, 155);
		this.checkBoxSupportsEMV.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.checkBoxSupportsEMV.Name = "checkBoxSupportsEMV";
		this.checkBoxSupportsEMV.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxSupportsEMV.Size = new System.Drawing.Size(327, 30);
		this.checkBoxSupportsEMV.TabIndex = 7;
		this.checkBoxSupportsEMV.Text = "Device Service Supports EMV";
		this.checkBoxSupportsEMV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxSupportsEMV.UseVisualStyleBackColor = true;
		this.textBoxDeviceServiceClientVersion.Location = new System.Drawing.Point(609, 118);
		this.textBoxDeviceServiceClientVersion.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.textBoxDeviceServiceClientVersion.Name = "textBoxDeviceServiceClientVersion";
		this.textBoxDeviceServiceClientVersion.Size = new System.Drawing.Size(99, 22);
		this.textBoxDeviceServiceClientVersion.TabIndex = 6;
		this.labelDeviceServiceClientVersion.AutoSize = true;
		this.labelDeviceServiceClientVersion.Location = new System.Drawing.Point(409, 122);
		this.labelDeviceServiceClientVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelDeviceServiceClientVersion.Name = "labelDeviceServiceClientVersion";
		this.labelDeviceServiceClientVersion.Size = new System.Drawing.Size(193, 17);
		this.labelDeviceServiceClientVersion.TabIndex = 5;
		this.labelDeviceServiceClientVersion.Text = "Device Service Client Version";
		this.checkBoxDeviceServiceClientCompatible.Location = new System.Drawing.Point(51, 118);
		this.checkBoxDeviceServiceClientCompatible.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.checkBoxDeviceServiceClientCompatible.Name = "checkBoxDeviceServiceClientCompatible";
		this.checkBoxDeviceServiceClientCompatible.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceClientCompatible.Size = new System.Drawing.Size(327, 30);
		this.checkBoxDeviceServiceClientCompatible.TabIndex = 4;
		this.checkBoxDeviceServiceClientCompatible.Text = "Device Service Client Compatible";
		this.checkBoxDeviceServiceClientCompatible.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceClientCompatible.UseVisualStyleBackColor = true;
		this.textBoxDeviceServiceVersion.Location = new System.Drawing.Point(609, 47);
		this.textBoxDeviceServiceVersion.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.textBoxDeviceServiceVersion.Name = "textBoxDeviceServiceVersion";
		this.textBoxDeviceServiceVersion.Size = new System.Drawing.Size(99, 22);
		this.textBoxDeviceServiceVersion.TabIndex = 3;
		this.labelDeviceServiceVersionLabel.AutoSize = true;
		this.labelDeviceServiceVersionLabel.Location = new System.Drawing.Point(444, 50);
		this.labelDeviceServiceVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelDeviceServiceVersionLabel.Name = "labelDeviceServiceVersionLabel";
		this.labelDeviceServiceVersionLabel.Size = new System.Drawing.Size(154, 17);
		this.labelDeviceServiceVersionLabel.TabIndex = 2;
		this.labelDeviceServiceVersionLabel.Text = "Device Service Version";
		this.checkBoxDeviceServiceCardReaderConnected.Location = new System.Drawing.Point(51, 81);
		this.checkBoxDeviceServiceCardReaderConnected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.checkBoxDeviceServiceCardReaderConnected.Name = "checkBoxDeviceServiceCardReaderConnected";
		this.checkBoxDeviceServiceCardReaderConnected.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceCardReaderConnected.Size = new System.Drawing.Size(327, 30);
		this.checkBoxDeviceServiceCardReaderConnected.TabIndex = 1;
		this.checkBoxDeviceServiceCardReaderConnected.Text = "Device Service Card Reader Connected";
		this.checkBoxDeviceServiceCardReaderConnected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceCardReaderConnected.UseVisualStyleBackColor = true;
		this.checkBoxDeviceServiceConnected.Location = new System.Drawing.Point(51, 44);
		this.checkBoxDeviceServiceConnected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.checkBoxDeviceServiceConnected.Name = "checkBoxDeviceServiceConnected";
		this.checkBoxDeviceServiceConnected.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceConnected.Size = new System.Drawing.Size(327, 30);
		this.checkBoxDeviceServiceConnected.TabIndex = 0;
		this.checkBoxDeviceServiceConnected.Text = "Device Service Connected";
		this.checkBoxDeviceServiceConnected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceConnected.UseVisualStyleBackColor = true;
		this.groupBoxUSBCardReader.Controls.Add(this.checkBoxUSBCardReaderConnected);
		this.groupBoxUSBCardReader.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupBoxUSBCardReader.Location = new System.Drawing.Point(0, 256);
		this.groupBoxUSBCardReader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxUSBCardReader.Name = "groupBoxUSBCardReader";
		this.groupBoxUSBCardReader.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxUSBCardReader.Size = new System.Drawing.Size(744, 89);
		this.groupBoxUSBCardReader.TabIndex = 2;
		this.groupBoxUSBCardReader.TabStop = false;
		this.groupBoxUSBCardReader.Text = "USB Card Reader (Classic)";
		this.checkBoxUSBCardReaderConnected.Location = new System.Drawing.Point(51, 39);
		this.checkBoxUSBCardReaderConnected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.checkBoxUSBCardReaderConnected.Name = "checkBoxUSBCardReaderConnected";
		this.checkBoxUSBCardReaderConnected.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxUSBCardReaderConnected.Size = new System.Drawing.Size(327, 30);
		this.checkBoxUSBCardReaderConnected.TabIndex = 1;
		this.checkBoxUSBCardReaderConnected.Text = "USB Card Reader Connected";
		this.checkBoxUSBCardReaderConnected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxUSBCardReaderConnected.UseVisualStyleBackColor = true;
		this.panelBottom.Controls.Add(this.buttonRefresh);
		this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelBottom.Location = new System.Drawing.Point(0, 445);
		this.panelBottom.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(744, 60);
		this.panelBottom.TabIndex = 3;
		this.buttonRefresh.Location = new System.Drawing.Point(288, 16);
		this.buttonRefresh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.buttonRefresh.Name = "buttonRefresh";
		this.buttonRefresh.Size = new System.Drawing.Size(100, 28);
		this.buttonRefresh.TabIndex = 0;
		this.buttonRefresh.Text = "Refresh";
		this.buttonRefresh.UseVisualStyleBackColor = true;
		this.buttonRefresh.Click += new System.EventHandler(buttonRefresh_Click);
		this.groupBoxTrackDataService.Controls.Add(this.labelRegisteredServiceValue);
		this.groupBoxTrackDataService.Controls.Add(this.labelRegisteredService);
		this.groupBoxTrackDataService.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxTrackDataService.Location = new System.Drawing.Point(0, 345);
		this.groupBoxTrackDataService.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxTrackDataService.Name = "groupBoxTrackDataService";
		this.groupBoxTrackDataService.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBoxTrackDataService.Size = new System.Drawing.Size(744, 100);
		this.groupBoxTrackDataService.TabIndex = 4;
		this.groupBoxTrackDataService.TabStop = false;
		this.groupBoxTrackDataService.Text = "Track Data Service";
		this.labelRegisteredServiceValue.AutoSize = true;
		this.labelRegisteredServiceValue.Location = new System.Drawing.Point(193, 41);
		this.labelRegisteredServiceValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelRegisteredServiceValue.Name = "labelRegisteredServiceValue";
		this.labelRegisteredServiceValue.Size = new System.Drawing.Size(40, 17);
		this.labelRegisteredServiceValue.TabIndex = 1;
		this.labelRegisteredServiceValue.Text = "none";
		this.labelRegisteredService.AutoSize = true;
		this.labelRegisteredService.Location = new System.Drawing.Point(51, 42);
		this.labelRegisteredService.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelRegisteredService.Name = "labelRegisteredService";
		this.labelRegisteredService.Size = new System.Drawing.Size(132, 17);
		this.labelRegisteredService.TabIndex = 0;
		this.labelRegisteredService.Text = "Registered Service:";
		this.panelLoadingData.Controls.Add(this.labelLoading);
		this.panelLoadingData.Location = new System.Drawing.Point(10, 81);
		this.panelLoadingData.Name = "panelLoadingData";
		this.panelLoadingData.Size = new System.Drawing.Size(337, 100);
		this.panelLoadingData.TabIndex = 8;
		this.labelLoading.AutoSize = true;
		this.labelLoading.Location = new System.Drawing.Point(133, 25);
		this.labelLoading.Name = "labelLoading";
		this.labelLoading.Size = new System.Drawing.Size(71, 17);
		this.labelLoading.TabIndex = 0;
		this.labelLoading.Text = "Loading...";
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBoxTrackDataService);
		base.Controls.Add(this.panelBottom);
		base.Controls.Add(this.groupBoxUSBCardReader);
		base.Controls.Add(this.groupBoxDeviceService);
		base.Controls.Add(this.panelTopHeader);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "CardReaderPage";
		base.Size = new System.Drawing.Size(744, 505);
		this.panelTopHeader.ResumeLayout(false);
		this.panelTopHeader.PerformLayout();
		this.groupBoxDeviceService.ResumeLayout(false);
		this.groupBoxDeviceService.PerformLayout();
		this.groupBoxUSBCardReader.ResumeLayout(false);
		this.panelBottom.ResumeLayout(false);
		this.groupBoxTrackDataService.ResumeLayout(false);
		this.groupBoxTrackDataService.PerformLayout();
		this.panelLoadingData.ResumeLayout(false);
		this.panelLoadingData.PerformLayout();
		base.ResumeLayout(false);
	}
}
