using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class OutputToolWindow : ToolWindow
{
	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripLabel toolStripLabel1;

	private ToolStripComboBox m_outputSourceToolStripComboBox;

	private ToolStripSeparator toolStripSeparator1;

	private RichTextBox m_richTextBox;

	public OutputToolWindow()
	{
		InitializeComponent();
	}

	public void UpdateText(string text)
	{
		if (m_richTextBox == null || m_richTextBox.IsDisposed)
		{
			return;
		}
		if (m_richTextBox.InvokeRequired)
		{
			m_richTextBox.Invoke((MethodInvoker)delegate
			{
				m_richTextBox.AppendText("\n" + DateTime.Now.ToString("yyyy-dd-MM hh:mm:ss,fff : ") + text);
				m_richTextBox.ScrollToCaret();
			});
		}
		else
		{
			m_richTextBox.AppendText("\n" + DateTime.Now.ToString("yyyy-dd-MM hh:mm:ss,fff : ") + text);
			m_richTextBox.ScrollToCaret();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.OutputToolWindow));
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.m_outputSourceToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_richTextBox = new System.Windows.Forms.RichTextBox();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.toolStripLabel1, this.m_outputSourceToolStripComboBox, this.toolStripSeparator1 });
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(515, 25);
		this.m_toolStrip.TabIndex = 0;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(107, 22);
		this.toolStripLabel1.Text = "Show output from:";
		this.m_outputSourceToolStripComboBox.Name = "m_outputSourceToolStripComboBox";
		this.m_outputSourceToolStripComboBox.Size = new System.Drawing.Size(121, 25);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.m_richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_richTextBox.Location = new System.Drawing.Point(0, 25);
		this.m_richTextBox.Name = "m_richTextBox";
		this.m_richTextBox.Size = new System.Drawing.Size(515, 221);
		this.m_richTextBox.TabIndex = 1;
		this.m_richTextBox.Text = "";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(515, 246);
		base.Controls.Add(this.m_richTextBox);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "OutputToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Output";
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
