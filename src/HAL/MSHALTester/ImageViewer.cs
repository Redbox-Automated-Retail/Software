using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Redbox.HAL.MSHALTester;

public class ImageViewer : Form
{
	private IContainer components;

	private PictureBox m_picturebox;

	public ImageViewer()
	{
		InitializeComponent();
	}

	public void DisplayFile(string file)
	{
		if (!string.IsNullOrEmpty(file) && File.Exists(file))
		{
			m_picturebox.Image?.Dispose();
			m_picturebox.Image = Image.FromFile(file);
		}
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
		this.m_picturebox = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.m_picturebox).BeginInit();
		base.SuspendLayout();
		this.m_picturebox.Location = new System.Drawing.Point(29, 22);
		this.m_picturebox.Name = "m_picturebox";
		this.m_picturebox.Size = new System.Drawing.Size(640, 480);
		this.m_picturebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.m_picturebox.TabIndex = 0;
		this.m_picturebox.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(733, 552);
		base.Controls.Add(this.m_picturebox);
		base.Name = "ImageViewer";
		this.Text = "ImageViewer";
		((System.ComponentModel.ISupportInitialize)this.m_picturebox).EndInit();
		base.ResumeLayout(false);
	}
}
