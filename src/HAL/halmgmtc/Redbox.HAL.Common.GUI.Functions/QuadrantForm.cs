using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class QuadrantForm : Form
{
	private IContainer components;

	private Label label1;

	private Label label2;

	private Label label3;

	private TextBox m_offsetTextBox;

	private TextBox m_startSlotTextBox;

	private TextBox m_endSlotTextBox;

	private ErrorProvider m_errorProvider;

	private Button m_okButton;

	private Button m_cancelButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? Offset
	{
		get
		{
			if (int.TryParse(m_offsetTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_offsetTextBox.Clear();
			if (value.HasValue)
			{
				m_offsetTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? StartSlot
	{
		get
		{
			if (int.TryParse(m_startSlotTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_startSlotTextBox.Clear();
			if (value.HasValue)
			{
				m_startSlotTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? EndSlot
	{
		get
		{
			if (int.TryParse(m_endSlotTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_endSlotTextBox.Clear();
			if (value.HasValue)
			{
				m_endSlotTextBox.Text = value.ToString();
			}
		}
	}

	public QuadrantForm()
	{
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_offsetTextBox, string.Empty);
		m_errorProvider.SetError(m_startSlotTextBox, string.Empty);
		m_errorProvider.SetError(m_endSlotTextBox, string.Empty);
		if (!Offset.HasValue || Offset < 0)
		{
			m_errorProvider.SetError(m_offsetTextBox, "Offset is a required value and must be a postive integer value.");
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.m_offsetTextBox = new System.Windows.Forms.TextBox();
		this.m_startSlotTextBox = new System.Windows.Forms.TextBox();
		this.m_endSlotTextBox = new System.Windows.Forms.TextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Offset:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 43);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(53, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Start Slot:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(12, 75);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(50, 13);
		this.label3.TabIndex = 4;
		this.label3.Text = "End Slot:";
		this.m_offsetTextBox.Location = new System.Drawing.Point(75, 12);
		this.m_offsetTextBox.Name = "m_offsetTextBox";
		this.m_offsetTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_offsetTextBox.TabIndex = 1;
		this.m_startSlotTextBox.Location = new System.Drawing.Point(75, 40);
		this.m_startSlotTextBox.Name = "m_startSlotTextBox";
		this.m_startSlotTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_startSlotTextBox.TabIndex = 3;
		this.m_endSlotTextBox.Location = new System.Drawing.Point(75, 72);
		this.m_endSlotTextBox.Name = "m_endSlotTextBox";
		this.m_endSlotTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_endSlotTextBox.TabIndex = 5;
		this.m_errorProvider.ContainerControl = this;
		this.m_okButton.Location = new System.Drawing.Point(15, 107);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 6;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(133, 108);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 7;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(220, 142);
		base.ControlBox = false;
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_endSlotTextBox);
		base.Controls.Add(this.m_startSlotTextBox);
		base.Controls.Add(this.m_offsetTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "QuadrantForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Edit Deck Quadrant";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
