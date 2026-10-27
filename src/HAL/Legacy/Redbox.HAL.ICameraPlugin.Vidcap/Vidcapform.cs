using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AxVIDEOCAPLib;

namespace Redbox.HAL.ICameraPlugin.Vidcap;

public class Vidcapform : Form
{
	private IContainer components;

	private AxVideoCap axVideoCap1;

	public AxVideoCap Videocap => axVideoCap1;

	public string WorkingPath { get; set; }

	public Vidcapform()
	{
		InitializeComponent();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		string text = Path.Combine(WorkingPath, "SnapAndDecode.jpg");
		try
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
		catch (Exception)
		{
		}
		Videocap.SnapShotJPEG(text, 100);
	}

	private void m_cameraPropertiesButton_Click(object sender, EventArgs e)
	{
		Videocap.ShowVideoCapturePropertyPage();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.HAL.ICameraPlugin.Vidcap.Vidcapform));
		this.axVideoCap1 = new AxVIDEOCAPLib.AxVideoCap();
		((System.ComponentModel.ISupportInitialize)this.axVideoCap1).BeginInit();
		base.SuspendLayout();
		this.axVideoCap1.Enabled = true;
		this.axVideoCap1.Location = new System.Drawing.Point(12, 28);
		this.axVideoCap1.Name = "axVideoCap1";
		this.axVideoCap1.OcxState = (System.Windows.Forms.AxHost.State)resources.GetObject("axVideoCap1.OcxState");
		this.axVideoCap1.Size = new System.Drawing.Size(640, 381);
		this.axVideoCap1.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(682, 466);
		base.Controls.Add(this.axVideoCap1);
		this.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Name = "Vidcapform";
		((System.ComponentModel.ISupportInitialize)this.axVideoCap1).EndInit();
		base.ResumeLayout(false);
	}
}
