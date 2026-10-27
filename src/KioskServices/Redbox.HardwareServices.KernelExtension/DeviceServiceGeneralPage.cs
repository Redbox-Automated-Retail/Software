using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.HardwareServices.KernelExtension;

public class DeviceServiceGeneralPage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Panel panelTopHeader;

	private Label labelHeader;

	private CheckBox cbLiveCall;

	public DeviceServiceGeneralPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		ServiceLocator.Instance.RemoveService(typeof(IDeviceServiceClientHelper));
		ServiceLocator.Instance.AddService(typeof(IDeviceServiceClientHelper), DeviceServiceClientHelperFactory.Create());
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.DetectCardReaderAndRegisterTrackDataService();
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		cbLiveCall.Checked = !DeviceServiceClientHelperSimulator.Instance.UseSimulatedCardReader;
	}

	private void cbLiveCall_CheckedChanged(object sender, EventArgs e)
	{
		DeviceServiceClientHelperSimulator.Instance.UseSimulatedCardReader = !cbLiveCall.Checked;
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
		this.cbLiveCall = new System.Windows.Forms.CheckBox();
		this.panelTopHeader.SuspendLayout();
		base.SuspendLayout();
		this.panelTopHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.panelTopHeader.Controls.Add(this.labelHeader);
		this.panelTopHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
		this.panelTopHeader.Name = "panelTopHeader";
		this.panelTopHeader.Size = new System.Drawing.Size(521, 38);
		this.panelTopHeader.TabIndex = 2;
		this.labelHeader.AutoSize = true;
		this.labelHeader.BackColor = System.Drawing.Color.ForestGreen;
		this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelHeader.Location = new System.Drawing.Point(3, 6);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(260, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Device Service General";
		this.cbLiveCall.AutoSize = true;
		this.cbLiveCall.Location = new System.Drawing.Point(20, 57);
		this.cbLiveCall.Name = "cbLiveCall";
		this.cbLiveCall.Size = new System.Drawing.Size(156, 17);
		this.cbLiveCall.TabIndex = 3;
		this.cbLiveCall.Text = "Live Call (off uses simulator)";
		this.cbLiveCall.UseVisualStyleBackColor = true;
		this.cbLiveCall.CheckedChanged += new System.EventHandler(cbLiveCall_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.cbLiveCall);
		base.Controls.Add(this.panelTopHeader);
		base.Name = "DeviceServiceGeneralPage";
		base.Size = new System.Drawing.Size(521, 319);
		this.panelTopHeader.ResumeLayout(false);
		this.panelTopHeader.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
