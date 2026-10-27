using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class SwitchStatusForm : Form
{
	private IContainer components;

	private Label m_statusLabel;

	private Panel panel1;

	public string StatusLabel
	{
		get
		{
			return m_statusLabel.Text;
		}
		set
		{
			m_statusLabel.Text = value;
		}
	}

	public SwitchStatusForm()
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
		this.m_statusLabel = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_statusLabel.Location = new System.Drawing.Point(11, 8);
		this.m_statusLabel.Name = "m_statusLabel";
		this.m_statusLabel.Size = new System.Drawing.Size(355, 55);
		this.m_statusLabel.TabIndex = 1;
		this.m_statusLabel.Text = "Please Wait...";
		this.m_statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel1.Controls.Add(this.m_statusLabel);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(379, 73);
		this.panel1.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(379, 73);
		base.ControlBox = false;
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "SwitchStatusForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "SwitchStatusForm";
		base.TopMost = true;
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
