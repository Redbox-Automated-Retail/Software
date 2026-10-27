using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class DebuggerVariableForm : Form
{
	private IContainer components;

	private Label label1;

	private TextBox m_nameTextBox;

	private Label label2;

	private TextBox m_valueTextBox;

	private Panel m_topPanel;

	private Panel m_bottomPanel;

	private Panel m_middlePanel;

	public string VariableName
	{
		get
		{
			return m_nameTextBox.Text;
		}
		set
		{
			m_nameTextBox.Text = value;
		}
	}

	public string VariableValue
	{
		get
		{
			return m_valueTextBox.Text;
		}
		set
		{
			m_valueTextBox.Text = value;
		}
	}

	public DebuggerVariableForm()
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_nameTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_valueTextBox = new System.Windows.Forms.TextBox();
		this.m_topPanel = new System.Windows.Forms.Panel();
		this.m_bottomPanel = new System.Windows.Forms.Panel();
		this.m_middlePanel = new System.Windows.Forms.Panel();
		this.m_topPanel.SuspendLayout();
		this.m_bottomPanel.SuspendLayout();
		this.m_middlePanel.SuspendLayout();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(2, 5);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Name:";
		this.m_nameTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.m_nameTextBox.Location = new System.Drawing.Point(5, 20);
		this.m_nameTextBox.Name = "m_nameTextBox";
		this.m_nameTextBox.ReadOnly = true;
		this.m_nameTextBox.Size = new System.Drawing.Size(382, 20);
		this.m_nameTextBox.TabIndex = 1;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(3, 3);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(37, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Value:";
		this.m_valueTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_valueTextBox.Location = new System.Drawing.Point(5, 5);
		this.m_valueTextBox.Multiline = true;
		this.m_valueTextBox.Name = "m_valueTextBox";
		this.m_valueTextBox.ReadOnly = true;
		this.m_valueTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.m_valueTextBox.Size = new System.Drawing.Size(382, 200);
		this.m_valueTextBox.TabIndex = 3;
		this.m_topPanel.Controls.Add(this.label1);
		this.m_topPanel.Controls.Add(this.m_nameTextBox);
		this.m_topPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_topPanel.Location = new System.Drawing.Point(0, 0);
		this.m_topPanel.Name = "m_topPanel";
		this.m_topPanel.Padding = new System.Windows.Forms.Padding(5);
		this.m_topPanel.Size = new System.Drawing.Size(392, 45);
		this.m_topPanel.TabIndex = 4;
		this.m_bottomPanel.Controls.Add(this.m_valueTextBox);
		this.m_bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_bottomPanel.Location = new System.Drawing.Point(0, 66);
		this.m_bottomPanel.Name = "m_bottomPanel";
		this.m_bottomPanel.Padding = new System.Windows.Forms.Padding(5);
		this.m_bottomPanel.Size = new System.Drawing.Size(392, 210);
		this.m_bottomPanel.TabIndex = 5;
		this.m_middlePanel.Controls.Add(this.label2);
		this.m_middlePanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_middlePanel.Location = new System.Drawing.Point(0, 45);
		this.m_middlePanel.Name = "m_middlePanel";
		this.m_middlePanel.Size = new System.Drawing.Size(392, 21);
		this.m_middlePanel.TabIndex = 6;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(392, 276);
		base.Controls.Add(this.m_bottomPanel);
		base.Controls.Add(this.m_middlePanel);
		base.Controls.Add(this.m_topPanel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MinimumSize = new System.Drawing.Size(400, 300);
		base.Name = "DebuggerVariableForm";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Variable Detail";
		this.m_topPanel.ResumeLayout(false);
		this.m_topPanel.PerformLayout();
		this.m_bottomPanel.ResumeLayout(false);
		this.m_bottomPanel.PerformLayout();
		this.m_middlePanel.ResumeLayout(false);
		this.m_middlePanel.PerformLayout();
		base.ResumeLayout(false);
	}
}
