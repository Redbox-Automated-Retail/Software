using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.MSHALTester;

public class ConfirmationDialog : Form
{
	private string m_confirmText;

	private IContainer components;

	private TextBox m_confirmationBox;

	private Label m_label1;

	private Button m_okButton;

	private Button m_cancelButton;

	private ErrorProvider m_errorProvider;

	public ConfirmationDialog(string confirmText)
	{
		InitializeComponent();
		Setup(confirmText);
	}

	public ConfirmationDialog()
	{
		InitializeComponent();
		Setup("test");
	}

	private void Setup(string confirmText)
	{
		m_confirmText = (string.IsNullOrEmpty(confirmText) ? "test" : confirmText);
		m_label1.Text = $"Enter the word '{m_confirmText}' (without quotes)";
	}

	private void m_okButton_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		if (string.IsNullOrEmpty(m_confirmationBox.Text))
		{
			m_errorProvider.SetError(m_confirmationBox, "Enter a confirmation.");
			return;
		}
		if (!m_confirmationBox.Text.Equals(m_confirmText, StringComparison.CurrentCultureIgnoreCase))
		{
			m_errorProvider.SetError(m_confirmationBox, "Enter the correct confirmation.");
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void m_cancelButton_Click(object sender, EventArgs e)
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
		this.m_confirmationBox = new System.Windows.Forms.TextBox();
		this.m_label1 = new System.Windows.Forms.Label();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_confirmationBox.Location = new System.Drawing.Point(26, 52);
		this.m_confirmationBox.Name = "m_confirmationBox";
		this.m_confirmationBox.Size = new System.Drawing.Size(197, 20);
		this.m_confirmationBox.TabIndex = 0;
		this.m_label1.AutoSize = true;
		this.m_label1.Location = new System.Drawing.Point(23, 26);
		this.m_label1.Name = "m_label1";
		this.m_label1.Size = new System.Drawing.Size(202, 13);
		this.m_label1.TabIndex = 1;
		this.m_label1.Text = "Enter the word 'test' (without the quotes) :";
		this.m_okButton.Location = new System.Drawing.Point(26, 96);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(105, 60);
		this.m_okButton.TabIndex = 2;
		this.m_okButton.Text = "Ok";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(m_okButton_Click);
		this.m_cancelButton.Location = new System.Drawing.Point(158, 96);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(105, 60);
		this.m_cancelButton.TabIndex = 3;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(m_cancelButton_Click);
		this.m_errorProvider.ContainerControl = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(298, 186);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_label1);
		base.Controls.Add(this.m_confirmationBox);
		base.Name = "ConfirmationDialog";
		this.Text = "ConfirmationDialog";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
