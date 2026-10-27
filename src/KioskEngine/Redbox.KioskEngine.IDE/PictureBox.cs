using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class PictureBox : UserControl
{
	private IContainer components;

	public PictureBox()
	{
		InitializeComponent();
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
		System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.PictureBox));
		base.SuspendLayout();
		componentResourceManager.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.MaximumSize = new System.Drawing.Size(580, 260);
		base.Name = "PictureBox";
		base.ResumeLayout(false);
	}
}
