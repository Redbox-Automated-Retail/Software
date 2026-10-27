using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class InputBoxForm : Form
{
	private IContainer components;

	private Label m_label;

	private Button m_okButton;

	private Button m_cancelButton;

	private ErrorProvider m_errorProvider;

	private MaskedTextBox m_textBox;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Label
	{
		get
		{
			return m_label.Text;
		}
		set
		{
			m_label.Text = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Value
	{
		get
		{
			return m_textBox.Text;
		}
		set
		{
			m_textBox.Text = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsValueRequired { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ErrorMessage { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string InputMask
	{
		get
		{
			return m_textBox.Mask;
		}
		set
		{
			m_textBox.Mask = value;
		}
	}

	public InputBoxForm()
	{
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_textBox, string.Empty);
		if (IsValueRequired && string.IsNullOrEmpty(m_textBox.Text))
		{
			m_errorProvider.SetError(m_textBox, ErrorMessage ?? "A value is required.");
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
		this.m_label = new System.Windows.Forms.Label();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_textBox = new System.Windows.Forms.MaskedTextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_label.AutoSize = true;
		this.m_label.Location = new System.Drawing.Point(9, 9);
		this.m_label.Name = "m_label";
		this.m_label.Size = new System.Drawing.Size(104, 13);
		this.m_label.TabIndex = 0;
		this.m_label.Text = "Place your text here:";
		this.m_okButton.Location = new System.Drawing.Point(291, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 2;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(291, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 3;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_errorProvider.ContainerControl = this;
		this.m_textBox.Location = new System.Drawing.Point(13, 25);
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.Size = new System.Drawing.Size(258, 20);
		this.m_textBox.TabIndex = 1;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(378, 78);
		base.ControlBox = false;
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_label);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "InputBoxForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Input Box";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
