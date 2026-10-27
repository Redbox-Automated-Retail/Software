using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class DeviceServicePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Panel panelTopHeader;

	private Label labelHeader;

	private GroupBox groupBoxShutDown;

	private TextBox textBoxDeviceServiceClientVersion;

	private Label labelDeviceServiceClientVersion;

	private CheckBox checkBoxDeviceServiceClientCompatible;

	private TextBox textBoxDeviceServiceVersion;

	private Label labelDeviceServiceVersionLabel;

	private CheckBox checkBoxDeviceServiceCardReaderConnected;

	private CheckBox checkBoxDeviceServiceConnected;

	private Panel panelBottom;

	private Button buttonRefresh;

	private TextBox textBoxResult;

	private Label label2;

	private Button buttonDeviceServiceShutdown;

	private ComboBox comboBoxShutdownReason;

	private Label label1;

	private CheckBox checkBoxForceShutdown;

	private Panel panelLoading;

	private Label labelLoading;

	private CheckBox checkBoxSupportsEMV;

	private Button buttonDeviceServiceSendTamperedAlert;

	public DeviceServicePage()
	{
		InitializeComponent();
		foreach (object value in Enum.GetValues(typeof(ShutDownReason)))
		{
			comboBoxShutdownReason.Items.Add(value.ToString());
		}
		comboBoxShutdownReason.SelectedIndex = 0;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		panelLoading.Visible = true;
		ValidateVersionModel validateVersionModel = null;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		bool supportsEMV = false;
		Task.Factory.StartNew(delegate
		{
			validateVersionModel = deviceServiceClientHelper?.ValidateVersion();
			supportsEMV = deviceServiceClientHelper?.SupportsEMV ?? false;
		}).ContinueWith(delegate
		{
			checkBoxDeviceServiceClientCompatible.Checked = validateVersionModel?.IsCompatible ?? false;
			checkBoxDeviceServiceCardReaderConnected.Checked = deviceServiceClientHelper?.IsCardReaderConnected ?? false;
			checkBoxSupportsEMV.Checked = supportsEMV;
			panelLoading.Visible = false;
		}, CancellationToken.None, TaskContinuationOptions.None, service?.UITaskScheduler);
		textBoxDeviceServiceVersion.Text = deviceServiceClientHelper?.DeviceServiceVersion?.ToString();
		textBoxDeviceServiceClientVersion.Text = deviceServiceClientHelper?.DeviceServiceClientVersion?.ToString();
		checkBoxDeviceServiceConnected.Checked = deviceServiceClientHelper?.IsConnectedToDeviceService ?? false;
	}

	private void buttonDeviceServiceShutdown_Click(object sender, EventArgs e)
	{
		bool result = false;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		ShutDownReason shutDownReason = ShutDownReason.None;
		Enum.TryParse<ShutDownReason>(comboBoxShutdownReason.SelectedItem.ToString(), out shutDownReason);
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		Task.Factory.StartNew(delegate
		{
			textBoxResult.Text = "processing...";
		}, CancellationToken.None, TaskCreationOptions.None, service.UITaskScheduler).ContinueWith(delegate
		{
			result = deviceServiceClientHelper?.ShutDownDeviceService(checkBoxForceShutdown.Checked, shutDownReason) ?? false;
		}).ContinueWith(delegate
		{
			textBoxResult.Text = result.ToString();
		}, service.UITaskScheduler);
	}

	private void buttonDeviceServiceSendTamperedAlert_Click(object sender, EventArgs e)
	{
		ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>().SendTamperedCardReaderKioskAlertMessage();
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
		this.groupBoxShutDown = new System.Windows.Forms.GroupBox();
		this.textBoxResult = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.buttonDeviceServiceShutdown = new System.Windows.Forms.Button();
		this.comboBoxShutdownReason = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.checkBoxForceShutdown = new System.Windows.Forms.CheckBox();
		this.textBoxDeviceServiceClientVersion = new System.Windows.Forms.TextBox();
		this.labelDeviceServiceClientVersion = new System.Windows.Forms.Label();
		this.checkBoxDeviceServiceClientCompatible = new System.Windows.Forms.CheckBox();
		this.textBoxDeviceServiceVersion = new System.Windows.Forms.TextBox();
		this.labelDeviceServiceVersionLabel = new System.Windows.Forms.Label();
		this.checkBoxDeviceServiceCardReaderConnected = new System.Windows.Forms.CheckBox();
		this.checkBoxDeviceServiceConnected = new System.Windows.Forms.CheckBox();
		this.panelBottom = new System.Windows.Forms.Panel();
		this.buttonRefresh = new System.Windows.Forms.Button();
		this.panelLoading = new System.Windows.Forms.Panel();
		this.labelLoading = new System.Windows.Forms.Label();
		this.checkBoxSupportsEMV = new System.Windows.Forms.CheckBox();
		this.buttonDeviceServiceSendTamperedAlert = new System.Windows.Forms.Button();
		this.panelTopHeader.SuspendLayout();
		this.groupBoxShutDown.SuspendLayout();
		this.panelBottom.SuspendLayout();
		this.panelLoading.SuspendLayout();
		base.SuspendLayout();
		this.panelTopHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.panelTopHeader.Controls.Add(this.labelHeader);
		this.panelTopHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
		this.panelTopHeader.Name = "panelTopHeader";
		this.panelTopHeader.Size = new System.Drawing.Size(558, 38);
		this.panelTopHeader.TabIndex = 1;
		this.labelHeader.AutoSize = true;
		this.labelHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelHeader.Location = new System.Drawing.Point(3, 6);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(170, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Device Service";
		this.groupBoxShutDown.Controls.Add(this.textBoxResult);
		this.groupBoxShutDown.Controls.Add(this.label2);
		this.groupBoxShutDown.Controls.Add(this.buttonDeviceServiceShutdown);
		this.groupBoxShutDown.Controls.Add(this.comboBoxShutdownReason);
		this.groupBoxShutDown.Controls.Add(this.label1);
		this.groupBoxShutDown.Controls.Add(this.checkBoxForceShutdown);
		this.groupBoxShutDown.Location = new System.Drawing.Point(0, 216);
		this.groupBoxShutDown.Name = "groupBoxShutDown";
		this.groupBoxShutDown.Size = new System.Drawing.Size(550, 143);
		this.groupBoxShutDown.TabIndex = 2;
		this.groupBoxShutDown.TabStop = false;
		this.groupBoxShutDown.Text = "Shut Down";
		this.textBoxResult.Location = new System.Drawing.Point(298, 96);
		this.textBoxResult.Name = "textBoxResult";
		this.textBoxResult.Size = new System.Drawing.Size(145, 20);
		this.textBoxResult.TabIndex = 11;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(255, 99);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(37, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Result";
		this.buttonDeviceServiceShutdown.Location = new System.Drawing.Point(11, 96);
		this.buttonDeviceServiceShutdown.Name = "buttonDeviceServiceShutdown";
		this.buttonDeviceServiceShutdown.Size = new System.Drawing.Size(189, 23);
		this.buttonDeviceServiceShutdown.TabIndex = 9;
		this.buttonDeviceServiceShutdown.Text = "Shut Down Device Service";
		this.buttonDeviceServiceShutdown.UseVisualStyleBackColor = true;
		this.buttonDeviceServiceShutdown.Click += new System.EventHandler(buttonDeviceServiceShutdown_Click);
		this.comboBoxShutdownReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxShutdownReason.FormattingEnabled = true;
		this.comboBoxShutdownReason.Location = new System.Drawing.Point(130, 50);
		this.comboBoxShutdownReason.Name = "comboBoxShutdownReason";
		this.comboBoxShutdownReason.Size = new System.Drawing.Size(187, 21);
		this.comboBoxShutdownReason.TabIndex = 8;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(17, 53);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(95, 13);
		this.label1.TabIndex = 7;
		this.label1.Text = "Shutdown Reason";
		this.checkBoxForceShutdown.AutoSize = true;
		this.checkBoxForceShutdown.Location = new System.Drawing.Point(17, 29);
		this.checkBoxForceShutdown.Name = "checkBoxForceShutdown";
		this.checkBoxForceShutdown.Size = new System.Drawing.Size(104, 17);
		this.checkBoxForceShutdown.TabIndex = 6;
		this.checkBoxForceShutdown.Text = "Force Shutdown";
		this.checkBoxForceShutdown.UseVisualStyleBackColor = true;
		this.textBoxDeviceServiceClientVersion.Location = new System.Drawing.Point(427, 104);
		this.textBoxDeviceServiceClientVersion.Name = "textBoxDeviceServiceClientVersion";
		this.textBoxDeviceServiceClientVersion.Size = new System.Drawing.Size(75, 20);
		this.textBoxDeviceServiceClientVersion.TabIndex = 13;
		this.labelDeviceServiceClientVersion.AutoSize = true;
		this.labelDeviceServiceClientVersion.Location = new System.Drawing.Point(277, 107);
		this.labelDeviceServiceClientVersion.Name = "labelDeviceServiceClientVersion";
		this.labelDeviceServiceClientVersion.Size = new System.Drawing.Size(147, 13);
		this.labelDeviceServiceClientVersion.TabIndex = 12;
		this.labelDeviceServiceClientVersion.Text = "Device Service Client Version";
		this.checkBoxDeviceServiceClientCompatible.Location = new System.Drawing.Point(8, 104);
		this.checkBoxDeviceServiceClientCompatible.Name = "checkBoxDeviceServiceClientCompatible";
		this.checkBoxDeviceServiceClientCompatible.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceClientCompatible.Size = new System.Drawing.Size(245, 24);
		this.checkBoxDeviceServiceClientCompatible.TabIndex = 11;
		this.checkBoxDeviceServiceClientCompatible.Text = "Device Service Client Compatible";
		this.checkBoxDeviceServiceClientCompatible.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceClientCompatible.UseVisualStyleBackColor = true;
		this.textBoxDeviceServiceVersion.Location = new System.Drawing.Point(427, 46);
		this.textBoxDeviceServiceVersion.Name = "textBoxDeviceServiceVersion";
		this.textBoxDeviceServiceVersion.Size = new System.Drawing.Size(75, 20);
		this.textBoxDeviceServiceVersion.TabIndex = 10;
		this.labelDeviceServiceVersionLabel.AutoSize = true;
		this.labelDeviceServiceVersionLabel.Location = new System.Drawing.Point(303, 49);
		this.labelDeviceServiceVersionLabel.Name = "labelDeviceServiceVersionLabel";
		this.labelDeviceServiceVersionLabel.Size = new System.Drawing.Size(118, 13);
		this.labelDeviceServiceVersionLabel.TabIndex = 9;
		this.labelDeviceServiceVersionLabel.Text = "Device Service Version";
		this.checkBoxDeviceServiceCardReaderConnected.Location = new System.Drawing.Point(8, 74);
		this.checkBoxDeviceServiceCardReaderConnected.Name = "checkBoxDeviceServiceCardReaderConnected";
		this.checkBoxDeviceServiceCardReaderConnected.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceCardReaderConnected.Size = new System.Drawing.Size(245, 24);
		this.checkBoxDeviceServiceCardReaderConnected.TabIndex = 8;
		this.checkBoxDeviceServiceCardReaderConnected.Text = "Device Service Card Reader Connected";
		this.checkBoxDeviceServiceCardReaderConnected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceCardReaderConnected.UseVisualStyleBackColor = true;
		this.checkBoxDeviceServiceConnected.Location = new System.Drawing.Point(8, 44);
		this.checkBoxDeviceServiceConnected.Name = "checkBoxDeviceServiceConnected";
		this.checkBoxDeviceServiceConnected.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxDeviceServiceConnected.Size = new System.Drawing.Size(245, 24);
		this.checkBoxDeviceServiceConnected.TabIndex = 7;
		this.checkBoxDeviceServiceConnected.Text = "Device Service Connected";
		this.checkBoxDeviceServiceConnected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxDeviceServiceConnected.UseVisualStyleBackColor = true;
		this.panelBottom.Controls.Add(this.buttonRefresh);
		this.panelBottom.Location = new System.Drawing.Point(0, 161);
		this.panelBottom.Name = "panelBottom";
		this.panelBottom.Size = new System.Drawing.Size(558, 49);
		this.panelBottom.TabIndex = 14;
		this.buttonRefresh.Location = new System.Drawing.Point(216, 13);
		this.buttonRefresh.Name = "buttonRefresh";
		this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
		this.buttonRefresh.TabIndex = 0;
		this.buttonRefresh.Text = "Refresh";
		this.buttonRefresh.UseVisualStyleBackColor = true;
		this.buttonRefresh.Click += new System.EventHandler(buttonRefresh_Click);
		this.panelLoading.Controls.Add(this.labelLoading);
		this.panelLoading.Location = new System.Drawing.Point(3, 74);
		this.panelLoading.Name = "panelLoading";
		this.panelLoading.Size = new System.Drawing.Size(259, 83);
		this.panelLoading.TabIndex = 15;
		this.labelLoading.AutoSize = true;
		this.labelLoading.Location = new System.Drawing.Point(102, 21);
		this.labelLoading.Name = "labelLoading";
		this.labelLoading.Size = new System.Drawing.Size(60, 13);
		this.labelLoading.TabIndex = 0;
		this.labelLoading.Text = "Loading ....";
		this.labelLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkBoxSupportsEMV.Location = new System.Drawing.Point(8, 133);
		this.checkBoxSupportsEMV.Name = "checkBoxSupportsEMV";
		this.checkBoxSupportsEMV.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.checkBoxSupportsEMV.Size = new System.Drawing.Size(245, 24);
		this.checkBoxSupportsEMV.TabIndex = 16;
		this.checkBoxSupportsEMV.Text = "Device Service Supports EMV";
		this.checkBoxSupportsEMV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.checkBoxSupportsEMV.UseVisualStyleBackColor = true;
		this.buttonDeviceServiceSendTamperedAlert.Location = new System.Drawing.Point(11, 365);
		this.buttonDeviceServiceSendTamperedAlert.Name = "buttonDeviceServiceSendTamperedAlert";
		this.buttonDeviceServiceSendTamperedAlert.Size = new System.Drawing.Size(189, 23);
		this.buttonDeviceServiceSendTamperedAlert.TabIndex = 17;
		this.buttonDeviceServiceSendTamperedAlert.TabStop = false;
		this.buttonDeviceServiceSendTamperedAlert.Text = "Send Device Tampered Alert";
		this.buttonDeviceServiceSendTamperedAlert.UseVisualStyleBackColor = true;
		this.buttonDeviceServiceSendTamperedAlert.Click += new System.EventHandler(buttonDeviceServiceSendTamperedAlert_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonDeviceServiceSendTamperedAlert);
		base.Controls.Add(this.panelLoading);
		base.Controls.Add(this.checkBoxSupportsEMV);
		base.Controls.Add(this.panelBottom);
		base.Controls.Add(this.textBoxDeviceServiceClientVersion);
		base.Controls.Add(this.labelDeviceServiceClientVersion);
		base.Controls.Add(this.checkBoxDeviceServiceClientCompatible);
		base.Controls.Add(this.textBoxDeviceServiceVersion);
		base.Controls.Add(this.labelDeviceServiceVersionLabel);
		base.Controls.Add(this.checkBoxDeviceServiceCardReaderConnected);
		base.Controls.Add(this.checkBoxDeviceServiceConnected);
		base.Controls.Add(this.groupBoxShutDown);
		base.Controls.Add(this.panelTopHeader);
		base.Name = "DeviceServicePage";
		base.Size = new System.Drawing.Size(558, 410);
		this.panelTopHeader.ResumeLayout(false);
		this.panelTopHeader.PerformLayout();
		this.groupBoxShutDown.ResumeLayout(false);
		this.groupBoxShutDown.PerformLayout();
		this.panelBottom.ResumeLayout(false);
		this.panelLoading.ResumeLayout(false);
		this.panelLoading.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
