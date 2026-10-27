using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class OutputWindow : UserControl
{
	private static OutputWindow m_instance;

	private IContainer components;

	private ListBox m_outputListBox;

	public static OutputWindow Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new OutputWindow();
			}
			return m_instance;
		}
	}

	public void Clear()
	{
		m_outputListBox.Items.Clear();
	}

	public void Append(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			m_outputListBox.Items.Add(s);
		}
	}

	private OutputWindow()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		AutoScroll = true;
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
		this.m_outputListBox = new System.Windows.Forms.ListBox();
		base.SuspendLayout();
		this.m_outputListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_outputListBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_outputListBox.FormattingEnabled = true;
		this.m_outputListBox.Location = new System.Drawing.Point(0, 0);
		this.m_outputListBox.Name = "m_outputListBox";
		this.m_outputListBox.Size = new System.Drawing.Size(217, 145);
		this.m_outputListBox.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		base.Controls.Add(this.m_outputListBox);
		base.Name = "OutputWindow";
		base.Size = new System.Drawing.Size(217, 156);
		base.ResumeLayout(false);
	}
}
