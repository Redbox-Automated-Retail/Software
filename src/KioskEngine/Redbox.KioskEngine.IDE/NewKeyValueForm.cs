using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Alsing.SourceCode;
using Alsing.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class NewKeyValueForm : Form
{
	private IContainer components;

	private Label label1;

	private TextBox m_keyNameTextBox;

	private Panel m_panel;

	private Button m_cancelButton;

	private Button m_okButton;

	private ErrorProvider m_errorProvider;

	private Panel m_topPanel;

	private SyntaxBoxControl m_syntaxBoxControl;

	private SyntaxDocument m_syntaxDocument;

	public string KeyName
	{
		get
		{
			return m_keyNameTextBox.Text;
		}
		set
		{
			m_keyNameTextBox.Text = value;
		}
	}

	public string Value
	{
		get
		{
			return m_syntaxDocument.Text;
		}
		set
		{
			m_syntaxDocument.Text = value;
		}
	}

	public NewKeyValueForm()
	{
		InitializeComponent();
		m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly, "Redbox.KioskEngine.IDE.Lua.syn");
	}

	private void OnOK(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_keyNameTextBox = new System.Windows.Forms.TextBox();
		this.m_panel = new System.Windows.Forms.Panel();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_topPanel = new System.Windows.Forms.Panel();
		this.m_syntaxBoxControl = new Alsing.Windows.Forms.SyntaxBoxControl();
		this.m_syntaxDocument = new Alsing.SourceCode.SyntaxDocument(this.components);
		this.m_panel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		this.m_topPanel.SuspendLayout();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 10);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(28, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Key:";
		this.m_keyNameTextBox.Location = new System.Drawing.Point(46, 7);
		this.m_keyNameTextBox.MaxLength = 1024;
		this.m_keyNameTextBox.Name = "m_keyNameTextBox";
		this.m_keyNameTextBox.Size = new System.Drawing.Size(231, 20);
		this.m_keyNameTextBox.TabIndex = 0;
		this.m_panel.Controls.Add(this.m_cancelButton);
		this.m_panel.Controls.Add(this.m_okButton);
		this.m_panel.Dock = System.Windows.Forms.DockStyle.Right;
		this.m_panel.Location = new System.Drawing.Point(317, 0);
		this.m_panel.Name = "m_panel";
		this.m_panel.Size = new System.Drawing.Size(103, 305);
		this.m_panel.TabIndex = 2;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(16, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 1;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_okButton.Location = new System.Drawing.Point(16, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 0;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_errorProvider.ContainerControl = this;
		this.m_topPanel.Controls.Add(this.m_keyNameTextBox);
		this.m_topPanel.Controls.Add(this.label1);
		this.m_topPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_topPanel.Location = new System.Drawing.Point(0, 0);
		this.m_topPanel.Name = "m_topPanel";
		this.m_topPanel.Size = new System.Drawing.Size(317, 35);
		this.m_topPanel.TabIndex = 0;
		this.m_syntaxBoxControl.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight;
		this.m_syntaxBoxControl.AllowBreakPoints = false;
		this.m_syntaxBoxControl.AutoListPosition = null;
		this.m_syntaxBoxControl.AutoListSelectedText = "a123";
		this.m_syntaxBoxControl.AutoListVisible = false;
		this.m_syntaxBoxControl.BackColor = System.Drawing.Color.White;
		this.m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
		this.m_syntaxBoxControl.CopyAsRTF = false;
		this.m_syntaxBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_syntaxBoxControl.Document = this.m_syntaxDocument;
		this.m_syntaxBoxControl.FontName = "Courier new";
		this.m_syntaxBoxControl.HighLightActiveLine = true;
		this.m_syntaxBoxControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.m_syntaxBoxControl.InfoTipCount = 1;
		this.m_syntaxBoxControl.InfoTipPosition = null;
		this.m_syntaxBoxControl.InfoTipSelectedIndex = 1;
		this.m_syntaxBoxControl.InfoTipVisible = false;
		this.m_syntaxBoxControl.Location = new System.Drawing.Point(0, 35);
		this.m_syntaxBoxControl.LockCursorUpdate = false;
		this.m_syntaxBoxControl.Name = "m_syntaxBoxControl";
		this.m_syntaxBoxControl.ShowEOLMarker = true;
		this.m_syntaxBoxControl.ShowGutterMargin = false;
		this.m_syntaxBoxControl.ShowScopeIndicator = false;
		this.m_syntaxBoxControl.ShowTabGuides = true;
		this.m_syntaxBoxControl.ShowWhitespace = true;
		this.m_syntaxBoxControl.Size = new System.Drawing.Size(317, 270);
		this.m_syntaxBoxControl.SmoothScroll = false;
		this.m_syntaxBoxControl.SplitView = false;
		this.m_syntaxBoxControl.SplitviewH = -4;
		this.m_syntaxBoxControl.SplitviewV = -4;
		this.m_syntaxBoxControl.TabGuideColor = System.Drawing.Color.FromArgb(222, 219, 214);
		this.m_syntaxBoxControl.TabIndex = 1;
		this.m_syntaxBoxControl.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
		this.m_syntaxDocument.Lines = new string[1] { "" };
		this.m_syntaxDocument.MaxUndoBufferSize = 1000;
		this.m_syntaxDocument.Modified = false;
		this.m_syntaxDocument.UndoStep = 0;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(420, 305);
		base.ControlBox = false;
		base.Controls.Add(this.m_syntaxBoxControl);
		base.Controls.Add(this.m_topPanel);
		base.Controls.Add(this.m_panel);
		base.Name = "NewKeyValueForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "New Key/Value";
		this.m_panel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		this.m_topPanel.ResumeLayout(false);
		this.m_topPanel.PerformLayout();
		base.ResumeLayout(false);
	}
}
