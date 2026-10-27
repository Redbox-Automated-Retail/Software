using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions.Properties;

namespace Redbox.HAL.Common.GUI.Functions;

public class PictureDialog : Form
{
	private readonly Bitmap DisplayImage;

	private IContainer components;

	private Button button1;

	private PictureBox m_pictureBox;

	public PictureDialog(bool isFraud)
	{
		InitializeComponent();
		ResourceManager resourceManager = Resources.ResourceManager;
		DisplayImage = (Bitmap)resourceManager.GetObject(isFraud ? "Pirate" : "george_costanza");
		m_pictureBox.Image = DisplayImage;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		DisplayImage.Dispose();
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
		this.m_pictureBox = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.m_pictureBox).BeginInit();
		base.SuspendLayout();
		this.button1.Location = new System.Drawing.Point(311, 499);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(135, 86);
		this.button1.TabIndex = 0;
		this.button1.Text = "OK";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.m_pictureBox.Location = new System.Drawing.Point(12, 12);
		this.m_pictureBox.Name = "m_pictureBox";
		this.m_pictureBox.Size = new System.Drawing.Size(705, 466);
		this.m_pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.m_pictureBox.TabIndex = 1;
		this.m_pictureBox.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(770, 597);
		base.Controls.Add(this.m_pictureBox);
		base.Controls.Add(this.button1);
		base.Name = "PictureDialog";
		this.Text = "PictureDialog";
		((System.ComponentModel.ISupportInitialize)this.m_pictureBox).EndInit();
		base.ResumeLayout(false);
	}
}
