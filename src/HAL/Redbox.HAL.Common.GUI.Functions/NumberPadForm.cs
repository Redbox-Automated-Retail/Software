using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class NumberPadForm : Form
{
	private IContainer components;

	private NumberPad m_numberPad;

	private Button m_okButton;

	private Button m_cancelButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Number
	{
		get
		{
			return m_numberPad.Number;
		}
		set
		{
			m_numberPad.Text = value;
		}
	}

	public NumberPadForm()
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
		this.m_numberPad = new Redbox.HAL.Common.GUI.Functions.NumberPad();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_numberPad.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_numberPad.Location = new System.Drawing.Point(3, 3);
		this.m_numberPad.Name = "m_numberPad";
		this.m_numberPad.Size = new System.Drawing.Size(378, 260);
		this.m_numberPad.TabIndex = 0;
		this.m_okButton.Location = new System.Drawing.Point(289, 152);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(89, 48);
		this.m_okButton.TabIndex = 1;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(289, 206);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(89, 54);
		this.m_cancelButton.TabIndex = 2;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(384, 266);
		base.ControlBox = false;
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_numberPad);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "NumberPadForm";
		base.Padding = new System.Windows.Forms.Padding(3);
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Enter Number";
		base.ResumeLayout(false);
	}
}
