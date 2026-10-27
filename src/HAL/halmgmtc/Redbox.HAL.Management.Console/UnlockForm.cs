using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class UnlockForm : Form
{
	private IContainer components;

	private ErrorProvider m_errorProvider;

	private Button m_okButton;

	private Button m_cancelButton;

	private TextBox m_textBox;

	private Label label1;

	[Browsable(false)]
	public string Password => m_textBox.Text;

	public UnlockForm()
	{
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_textBox, string.Empty);
		if (string.IsNullOrEmpty(m_textBox.Text))
		{
			m_errorProvider.SetError(m_textBox, "Password is a required field.");
			return;
		}
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
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.m_textBox = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_errorProvider.ContainerControl = this;
		this.m_okButton.Location = new System.Drawing.Point(205, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 2;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(205, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 3;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.label1.Location = new System.Drawing.Point(12, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(187, 35);
		this.label1.TabIndex = 0;
		this.label1.Text = "Enter the administrator password to unlock settings:";
		this.m_textBox.Location = new System.Drawing.Point(15, 44);
		this.m_textBox.MaxLength = 16;
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.Size = new System.Drawing.Size(165, 20);
		this.m_textBox.TabIndex = 1;
		this.m_textBox.UseSystemPasswordChar = true;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(292, 85);
		base.ControlBox = false;
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "UnlockForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Unlock Settings";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
