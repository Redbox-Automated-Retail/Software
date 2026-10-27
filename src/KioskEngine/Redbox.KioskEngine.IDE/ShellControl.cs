using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class ShellControl : UserControl
{
	private IContainer components;

	private ShellTextBox m_shellTextBox;

	public Color ShellTextForeColor
	{
		get
		{
			if (m_shellTextBox == null)
			{
				return Color.Green;
			}
			return m_shellTextBox.ForeColor;
		}
		set
		{
			if (m_shellTextBox != null)
			{
				m_shellTextBox.ForeColor = value;
			}
		}
	}

	public Color ShellTextBackColor
	{
		get
		{
			if (m_shellTextBox == null)
			{
				return Color.Black;
			}
			return m_shellTextBox.BackColor;
		}
		set
		{
			if (m_shellTextBox != null)
			{
				m_shellTextBox.BackColor = value;
			}
		}
	}

	public Font ShellTextFont
	{
		get
		{
			if (m_shellTextBox == null)
			{
				return new Font("Tahoma", 8f);
			}
			return m_shellTextBox.Font;
		}
		set
		{
			if (m_shellTextBox != null)
			{
				m_shellTextBox.Font = value;
			}
		}
	}

	public string Prompt
	{
		get
		{
			return m_shellTextBox.Prompt;
		}
		set
		{
			m_shellTextBox.Prompt = value;
		}
	}

	public event EventCommandEntered CommandEntered;

	public ShellControl()
	{
		InitializeComponent();
	}

	public void Clear()
	{
		m_shellTextBox.Reset();
	}

	public void WriteText(string text)
	{
		m_shellTextBox.WriteText(text);
	}

	public string[] GetCommandHistory()
	{
		return m_shellTextBox.GetCommandHistory();
	}

	protected virtual void OnCommandEntered(string command)
	{
		if (CommandEntered != null)
		{
			CommandEntered(command, new CommandEnteredEventArgs(command));
		}
	}

	internal void FireCommandEntered(string command)
	{
		OnCommandEntered(command);
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
		this.m_shellTextBox = new Redbox.KioskEngine.IDE.ShellTextBox();
		base.SuspendLayout();
		this.m_shellTextBox.AcceptsReturn = true;
		this.m_shellTextBox.AcceptsTab = true;
		this.m_shellTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_shellTextBox.Location = new System.Drawing.Point(0, 0);
		this.m_shellTextBox.MaxLength = 0;
		this.m_shellTextBox.Multiline = true;
		this.m_shellTextBox.Name = "m_shellTextBox";
		this.m_shellTextBox.Prompt = "> ";
		this.m_shellTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.m_shellTextBox.Size = new System.Drawing.Size(337, 327);
		this.m_shellTextBox.TabIndex = 0;
		this.m_shellTextBox.Text = "> ";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_shellTextBox);
		base.Name = "ShellControl";
		base.Size = new System.Drawing.Size(337, 327);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
