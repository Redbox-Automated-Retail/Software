using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class TaskRunnerForm : Form
{
	private IContainer components;

	private Button m_runButton;

	private Button m_closeButton;

	private Label label1;

	private ComboBox m_processComboBox;

	private ErrorProvider m_errorProvider;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ProcessToRun { get; set; }

	public event EventHandler Run;

	public TaskRunnerForm()
	{
		InitializeComponent();
	}

	private void OnRun(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_processComboBox, string.Empty);
		if (string.IsNullOrEmpty(m_processComboBox.Text))
		{
			m_errorProvider.SetError(m_processComboBox, "A process is required.");
			return;
		}
		ProcessToRun = m_processComboBox.Text;
		if (Run != null)
		{
			Run(this, EventArgs.Empty);
		}
		Close();
	}

	private void OnClose(object sender, EventArgs e)
	{
		Close();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Cursor.Show();
	}

	private void OnFormClosed(object sender, FormClosedEventArgs e)
	{
		Cursor.Hide();
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
		this.m_runButton = new System.Windows.Forms.Button();
		this.m_closeButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.m_processComboBox = new System.Windows.Forms.ComboBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_runButton.Location = new System.Drawing.Point(305, 12);
		this.m_runButton.Name = "m_runButton";
		this.m_runButton.Size = new System.Drawing.Size(75, 23);
		this.m_runButton.TabIndex = 2;
		this.m_runButton.Text = "Run";
		this.m_runButton.UseVisualStyleBackColor = true;
		this.m_runButton.Click += new System.EventHandler(OnRun);
		this.m_closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_closeButton.Location = new System.Drawing.Point(305, 41);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(75, 23);
		this.m_closeButton.TabIndex = 3;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 17);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(78, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Process to run:";
		this.m_processComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_processComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
		this.m_processComboBox.FormattingEnabled = true;
		this.m_processComboBox.Location = new System.Drawing.Point(15, 33);
		this.m_processComboBox.Name = "m_processComboBox";
		this.m_processComboBox.Size = new System.Drawing.Size(251, 21);
		this.m_processComboBox.TabIndex = 1;
		this.m_errorProvider.ContainerControl = this;
		base.AcceptButton = this.m_runButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_closeButton;
		base.ClientSize = new System.Drawing.Size(392, 82);
		base.Controls.Add(this.m_processComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_closeButton);
		base.Controls.Add(this.m_runButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "TaskRunnerForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Run Task";
		base.TopMost = true;
		base.Load += new System.EventHandler(OnLoad);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnFormClosed);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
