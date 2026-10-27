using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class FindKeyValueForm : Form
{
	private IContainer components;

	private RadioButton m_searchByKeyRadioButton;

	private RadioButton m_searchByValueRadioButton;

	private TextBox m_valueTextBox;

	private Label label1;

	private ErrorProvider m_errorProvider;

	private Button m_cancelButton;

	private Button m_okButton;

	public KeyValueSearchType SearchType
	{
		get
		{
			if (!m_searchByKeyRadioButton.Checked)
			{
				return KeyValueSearchType.Value;
			}
			return KeyValueSearchType.Key;
		}
	}

	public string SearchFor
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

	public FindKeyValueForm()
	{
		InitializeComponent();
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

	private void OnCheckChanged(object sender, EventArgs e)
	{
		m_valueTextBox.Focus();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		m_valueTextBox.Focus();
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
		this.m_searchByKeyRadioButton = new System.Windows.Forms.RadioButton();
		this.m_searchByValueRadioButton = new System.Windows.Forms.RadioButton();
		this.m_valueTextBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_searchByKeyRadioButton.AutoSize = true;
		this.m_searchByKeyRadioButton.Checked = true;
		this.m_searchByKeyRadioButton.Location = new System.Drawing.Point(19, 22);
		this.m_searchByKeyRadioButton.Name = "m_searchByKeyRadioButton";
		this.m_searchByKeyRadioButton.Size = new System.Drawing.Size(43, 17);
		this.m_searchByKeyRadioButton.TabIndex = 1;
		this.m_searchByKeyRadioButton.TabStop = true;
		this.m_searchByKeyRadioButton.Text = "Key";
		this.m_searchByKeyRadioButton.UseVisualStyleBackColor = true;
		this.m_searchByKeyRadioButton.CheckedChanged += new System.EventHandler(OnCheckChanged);
		this.m_searchByValueRadioButton.AutoSize = true;
		this.m_searchByValueRadioButton.Location = new System.Drawing.Point(19, 45);
		this.m_searchByValueRadioButton.Name = "m_searchByValueRadioButton";
		this.m_searchByValueRadioButton.Size = new System.Drawing.Size(52, 17);
		this.m_searchByValueRadioButton.TabIndex = 2;
		this.m_searchByValueRadioButton.Text = "Value";
		this.m_searchByValueRadioButton.UseVisualStyleBackColor = true;
		this.m_searchByValueRadioButton.CheckedChanged += new System.EventHandler(OnCheckChanged);
		this.m_valueTextBox.Location = new System.Drawing.Point(86, 42);
		this.m_valueTextBox.Name = "m_valueTextBox";
		this.m_valueTextBox.Size = new System.Drawing.Size(184, 20);
		this.m_valueTextBox.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(83, 24);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(62, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Search For:";
		this.m_errorProvider.ContainerControl = this;
		this.m_okButton.Location = new System.Drawing.Point(313, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 3;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(313, 45);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 4;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(400, 87);
		base.ControlBox = false;
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_valueTextBox);
		base.Controls.Add(this.m_searchByValueRadioButton);
		base.Controls.Add(this.m_searchByKeyRadioButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "FindKeyValueForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Find Key/Value";
		base.Load += new System.EventHandler(OnLoad);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
