using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console;

public class MoveToForm : Form
{
	private bool m_activated;

	private IContainer components;

	private Label label1;

	private Label label2;

	private TextBox m_deckTextBox;

	private TextBox m_slotTextBox;

	private Button m_pickDeckButton;

	private Button m_pickSlotButton;

	private Button m_okButton;

	private Button m_cancelButton;

	private RadioButton m_normalModeRadioButton;

	private RadioButton m_getModeRadioButton;

	private RadioButton m_putModeRadioButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? Deck
	{
		get
		{
			if (!string.IsNullOrEmpty(m_deckTextBox.Text) && int.TryParse(m_deckTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_deckTextBox.Text = value.ToString();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? Slot
	{
		get
		{
			if (!string.IsNullOrEmpty(m_slotTextBox.Text) && int.TryParse(m_slotTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_slotTextBox.Text = value.ToString();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Mode
	{
		get
		{
			if (m_normalModeRadioButton.Checked)
			{
				return string.Empty;
			}
			if (m_getModeRadioButton.Checked)
			{
				return "GET";
			}
			if (!m_putModeRadioButton.Checked)
			{
				return string.Empty;
			}
			return "PUT";
		}
	}

	public MoveToForm()
	{
		InitializeComponent();
	}

	public void ClearFields()
	{
		m_deckTextBox.Clear();
		m_slotTextBox.Clear();
	}

	public void DisableModes()
	{
		m_normalModeRadioButton.Enabled = false;
		m_getModeRadioButton.Enabled = false;
		m_putModeRadioButton.Enabled = false;
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private void OnPickDeck(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm
		{
			Text = "Enter Deck Number"
		};
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_deckTextBox.Text = numberPadForm.Number;
			m_slotTextBox.Focus();
		}
	}

	private void OnPickSlot(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm
		{
			Text = "Enter Slot Number"
		};
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_slotTextBox.Text = numberPadForm.Number;
			m_okButton.Focus();
		}
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_activated = false;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		m_activated = false;
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnActivated(object sender, EventArgs e)
	{
		if (!m_activated)
		{
			m_deckTextBox.Focus();
			m_activated = true;
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_deckTextBox = new System.Windows.Forms.TextBox();
		this.m_slotTextBox = new System.Windows.Forms.TextBox();
		this.m_pickDeckButton = new System.Windows.Forms.Button();
		this.m_pickSlotButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_normalModeRadioButton = new System.Windows.Forms.RadioButton();
		this.m_getModeRadioButton = new System.Windows.Forms.RadioButton();
		this.m_putModeRadioButton = new System.Windows.Forms.RadioButton();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(36, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Deck:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 41);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(28, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Slot:";
		this.m_deckTextBox.Location = new System.Drawing.Point(54, 9);
		this.m_deckTextBox.Name = "m_deckTextBox";
		this.m_deckTextBox.Size = new System.Drawing.Size(90, 20);
		this.m_deckTextBox.TabIndex = 1;
		this.m_deckTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_deckTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_slotTextBox.Location = new System.Drawing.Point(54, 41);
		this.m_slotTextBox.Name = "m_slotTextBox";
		this.m_slotTextBox.Size = new System.Drawing.Size(90, 20);
		this.m_slotTextBox.TabIndex = 3;
		this.m_slotTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_pickDeckButton.Location = new System.Drawing.Point(150, 7);
		this.m_pickDeckButton.Name = "m_pickDeckButton";
		this.m_pickDeckButton.Size = new System.Drawing.Size(54, 23);
		this.m_pickDeckButton.TabIndex = 4;
		this.m_pickDeckButton.TabStop = false;
		this.m_pickDeckButton.Text = "...";
		this.m_pickDeckButton.UseVisualStyleBackColor = true;
		this.m_pickDeckButton.Click += new System.EventHandler(OnPickDeck);
		this.m_pickSlotButton.Location = new System.Drawing.Point(150, 41);
		this.m_pickSlotButton.Name = "m_pickSlotButton";
		this.m_pickSlotButton.Size = new System.Drawing.Size(54, 23);
		this.m_pickSlotButton.TabIndex = 5;
		this.m_pickSlotButton.TabStop = false;
		this.m_pickSlotButton.Text = "...";
		this.m_pickSlotButton.UseVisualStyleBackColor = true;
		this.m_pickSlotButton.Click += new System.EventHandler(OnPickSlot);
		this.m_okButton.Location = new System.Drawing.Point(15, 158);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 7;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(129, 158);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 8;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_normalModeRadioButton.AutoSize = true;
		this.m_normalModeRadioButton.Checked = true;
		this.m_normalModeRadioButton.Location = new System.Drawing.Point(15, 76);
		this.m_normalModeRadioButton.Name = "m_normalModeRadioButton";
		this.m_normalModeRadioButton.Size = new System.Drawing.Size(88, 17);
		this.m_normalModeRadioButton.TabIndex = 4;
		this.m_normalModeRadioButton.TabStop = true;
		this.m_normalModeRadioButton.Text = "Normal Mode";
		this.m_normalModeRadioButton.UseVisualStyleBackColor = true;
		this.m_getModeRadioButton.AutoSize = true;
		this.m_getModeRadioButton.Location = new System.Drawing.Point(15, 99);
		this.m_getModeRadioButton.Name = "m_getModeRadioButton";
		this.m_getModeRadioButton.Size = new System.Drawing.Size(72, 17);
		this.m_getModeRadioButton.TabIndex = 5;
		this.m_getModeRadioButton.TabStop = true;
		this.m_getModeRadioButton.Text = "Get Mode";
		this.m_getModeRadioButton.UseVisualStyleBackColor = true;
		this.m_putModeRadioButton.AutoSize = true;
		this.m_putModeRadioButton.Location = new System.Drawing.Point(15, 122);
		this.m_putModeRadioButton.Name = "m_putModeRadioButton";
		this.m_putModeRadioButton.Size = new System.Drawing.Size(71, 17);
		this.m_putModeRadioButton.TabIndex = 6;
		this.m_putModeRadioButton.TabStop = true;
		this.m_putModeRadioButton.Text = "Put Mode";
		this.m_putModeRadioButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(219, 193);
		base.ControlBox = false;
		base.Controls.Add(this.m_putModeRadioButton);
		base.Controls.Add(this.m_getModeRadioButton);
		base.Controls.Add(this.m_normalModeRadioButton);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_pickSlotButton);
		base.Controls.Add(this.m_pickDeckButton);
		base.Controls.Add(this.m_slotTextBox);
		base.Controls.Add(this.m_deckTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "MoveToForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Move To Location";
		base.Activated += new System.EventHandler(OnActivated);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
