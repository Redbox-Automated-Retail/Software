using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HardwareServices.KernelExtension;

public class ScriptEditorForm : Form
{
	private IContainer components;

	private ToolStrip m_toolStrip;

	private RichTextBox m_richTextBox;

	private ToolStripLabel toolStripLabel1;

	private ToolStripLabel toolStripLabel2;

	private ToolStripTextBox m_scenarioNameToolStripTextBox;

	private ToolStripTextBox m_halScriptToolStripTextBox;

	private ToolStripLabel toolStripLabel3;

	private ToolStripTextBox m_runTimeToolStripTextBox;

	private ToolStripLabel toolStripLabel5;

	private ToolStripLabel toolStripLabel4;

	private ErrorProvider m_errorProvider;

	public string Code
	{
		get
		{
			return m_richTextBox.Text;
		}
		set
		{
			m_richTextBox.Text = value;
		}
	}

	public string RunTime
	{
		get
		{
			return m_runTimeToolStripTextBox.Text;
		}
		set
		{
			m_runTimeToolStripTextBox.Text = value;
		}
	}

	public string ScriptName
	{
		get
		{
			return m_halScriptToolStripTextBox.Text;
		}
		set
		{
			m_halScriptToolStripTextBox.Text = value;
		}
	}

	public string ScenarioName
	{
		get
		{
			return m_scenarioNameToolStripTextBox.Text;
		}
		set
		{
			m_scenarioNameToolStripTextBox.Text = value;
		}
	}

	public ScriptEditorForm()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(RunTime))
		{
			RunTime = "00:30";
		}
		if (string.IsNullOrEmpty(ScenarioName))
		{
			ScenarioName = "New Scenario 1";
		}
		if (string.IsNullOrEmpty(ScriptName))
		{
			ScriptName = "immediate";
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
		this.components = new System.ComponentModel.Container();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.m_halScriptToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
		this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
		this.m_scenarioNameToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
		this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
		this.m_runTimeToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.m_richTextBox = new System.Windows.Forms.RichTextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_toolStrip.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.toolStripLabel2, this.m_scenarioNameToolStripTextBox, this.toolStripLabel5, this.toolStripLabel1, this.m_halScriptToolStripTextBox, this.toolStripLabel4, this.toolStripLabel3, this.m_runTimeToolStripTextBox });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(632, 23);
		this.m_toolStrip.TabIndex = 0;
		this.m_toolStrip.Text = "toolStrip1";
		this.toolStripLabel1.AutoSize = false;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(60, 20);
		this.toolStripLabel1.Text = "HAL Script:";
		this.m_halScriptToolStripTextBox.Name = "m_halScriptToolStripTextBox";
		this.m_halScriptToolStripTextBox.Size = new System.Drawing.Size(120, 21);
		this.toolStripLabel5.Name = "toolStripLabel5";
		this.toolStripLabel5.Size = new System.Drawing.Size(19, 13);
		this.toolStripLabel5.Text = "    ";
		this.toolStripLabel2.AutoSize = false;
		this.toolStripLabel2.Name = "toolStripLabel2";
		this.toolStripLabel2.Size = new System.Drawing.Size(82, 20);
		this.toolStripLabel2.Text = "Scenario Name:";
		this.m_scenarioNameToolStripTextBox.Name = "m_scenarioNameToolStripTextBox";
		this.m_scenarioNameToolStripTextBox.Size = new System.Drawing.Size(120, 21);
		this.toolStripLabel4.Name = "toolStripLabel4";
		this.toolStripLabel4.Size = new System.Drawing.Size(19, 13);
		this.toolStripLabel4.Text = "    ";
		this.toolStripLabel3.AutoSize = false;
		this.toolStripLabel3.Name = "toolStripLabel3";
		this.toolStripLabel3.Size = new System.Drawing.Size(55, 20);
		this.toolStripLabel3.Text = "Run Time:";
		this.m_runTimeToolStripTextBox.Name = "m_runTimeToolStripTextBox";
		this.m_runTimeToolStripTextBox.Size = new System.Drawing.Size(75, 21);
		this.m_richTextBox.AcceptsTab = true;
		this.m_richTextBox.DetectUrls = false;
		this.m_richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_richTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_richTextBox.Location = new System.Drawing.Point(0, 23);
		this.m_richTextBox.Name = "m_richTextBox";
		this.m_richTextBox.Size = new System.Drawing.Size(632, 433);
		this.m_richTextBox.TabIndex = 1;
		this.m_richTextBox.Text = "";
		this.m_richTextBox.WordWrap = false;
		this.m_errorProvider.ContainerControl = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(632, 456);
		base.Controls.Add(this.m_richTextBox);
		base.Controls.Add(this.m_toolStrip);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(640, 480);
		base.Name = "ScriptEditorForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Edit Hardware Mock Script";
		base.Load += new System.EventHandler(OnLoad);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
