using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.HardwareServices.KernelExtension;

public class DeviceServiceSimulatorPage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private CheckBox cbServiceConnected;

	private CheckBox cbCardReaderConnected;

	private CheckBox cbReaderTampered;

	private Panel panelTopHeader;

	private Label labelHeader;

	public DeviceServiceSimulatorPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		cbServiceConnected.Checked = DeviceServiceClientHelperSimulator.Instance.IsConnectedToDeviceService;
		cbCardReaderConnected.Checked = DeviceServiceClientHelperSimulator.Instance.IsCardReaderConnected;
		cbReaderTampered.Checked = DeviceServiceClientHelperSimulator.Instance.IsCardReaderTampered;
	}

	private void cbServiceConnected_CheckedChanged(object sender, EventArgs e)
	{
		DeviceServiceClientHelperSimulator.Instance.IsConnectedToDeviceService = cbServiceConnected.Checked;
	}

	private void cbCardReaderConnected_CheckedChanged(object sender, EventArgs e)
	{
		DeviceServiceClientHelperSimulator.Instance.IsCardReaderConnected = cbCardReaderConnected.Checked;
	}

	private void cbReaderTampered_CheckedChanged(object sender, EventArgs e)
	{
		DeviceServiceClientHelperSimulator.Instance.IsCardReaderTampered = cbReaderTampered.Checked;
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
		this.cbServiceConnected = new System.Windows.Forms.CheckBox();
		this.cbCardReaderConnected = new System.Windows.Forms.CheckBox();
		this.cbReaderTampered = new System.Windows.Forms.CheckBox();
		this.panelTopHeader = new System.Windows.Forms.Panel();
		this.labelHeader = new System.Windows.Forms.Label();
		this.panelTopHeader.SuspendLayout();
		base.SuspendLayout();
		this.cbServiceConnected.AutoSize = true;
		this.cbServiceConnected.Location = new System.Drawing.Point(29, 60);
		this.cbServiceConnected.Margin = new System.Windows.Forms.Padding(2);
		this.cbServiceConnected.Name = "cbServiceConnected";
		this.cbServiceConnected.Size = new System.Drawing.Size(154, 17);
		this.cbServiceConnected.TabIndex = 0;
		this.cbServiceConnected.Text = "Device Service Connected";
		this.cbServiceConnected.UseVisualStyleBackColor = true;
		this.cbServiceConnected.CheckedChanged += new System.EventHandler(cbServiceConnected_CheckedChanged);
		this.cbCardReaderConnected.AutoSize = true;
		this.cbCardReaderConnected.Location = new System.Drawing.Point(29, 81);
		this.cbCardReaderConnected.Margin = new System.Windows.Forms.Padding(2);
		this.cbCardReaderConnected.Name = "cbCardReaderConnected";
		this.cbCardReaderConnected.Size = new System.Drawing.Size(141, 17);
		this.cbCardReaderConnected.TabIndex = 1;
		this.cbCardReaderConnected.Text = "Card Reader Connected";
		this.cbCardReaderConnected.UseVisualStyleBackColor = true;
		this.cbCardReaderConnected.CheckedChanged += new System.EventHandler(cbCardReaderConnected_CheckedChanged);
		this.cbReaderTampered.AutoSize = true;
		this.cbReaderTampered.Location = new System.Drawing.Point(29, 102);
		this.cbReaderTampered.Margin = new System.Windows.Forms.Padding(2);
		this.cbReaderTampered.Name = "cbReaderTampered";
		this.cbReaderTampered.Size = new System.Drawing.Size(134, 17);
		this.cbReaderTampered.TabIndex = 2;
		this.cbReaderTampered.Text = "Card ReaderTampered";
		this.cbReaderTampered.UseVisualStyleBackColor = true;
		this.cbReaderTampered.CheckedChanged += new System.EventHandler(cbReaderTampered_CheckedChanged);
		this.panelTopHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.panelTopHeader.Controls.Add(this.labelHeader);
		this.panelTopHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
		this.panelTopHeader.Name = "panelTopHeader";
		this.panelTopHeader.Size = new System.Drawing.Size(412, 38);
		this.panelTopHeader.TabIndex = 3;
		this.labelHeader.AutoSize = true;
		this.labelHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelHeader.Location = new System.Drawing.Point(3, 6);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(276, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Device Service Simulator";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panelTopHeader);
		base.Controls.Add(this.cbReaderTampered);
		base.Controls.Add(this.cbCardReaderConnected);
		base.Controls.Add(this.cbServiceConnected);
		base.Margin = new System.Windows.Forms.Padding(2);
		base.Name = "DeviceServiceSimulatorPage";
		base.Size = new System.Drawing.Size(412, 122);
		this.panelTopHeader.ResumeLayout(false);
		this.panelTopHeader.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
