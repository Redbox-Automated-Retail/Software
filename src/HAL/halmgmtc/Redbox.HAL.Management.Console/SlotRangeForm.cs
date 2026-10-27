using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

public class SlotRangeForm : Form
{
	private bool skipSlotCheck;

	private IContainer components;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private TextBox m_startDeckTextBox;

	private TextBox m_startSlotTextBox;

	private TextBox m_endDeckTextBox;

	private TextBox m_endSlotTextBox;

	private Button m_pickStartDeckButton;

	private Button m_pickStartSlotButton;

	private Button m_pickEndDeckButton;

	private Button m_pickEndSlotButton;

	private Button m_okButton;

	private Button m_cancelButton;

	private ErrorProvider m_errorProvider;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SyncRange Range => new SyncRange(int.Parse(m_startDeckTextBox.Text), int.Parse(m_endDeckTextBox.Text), new Range(int.Parse(m_startSlotTextBox.Text), int.Parse(m_endSlotTextBox.Text)));

	public SlotRangeForm(bool skipSlotCheck)
	{
		this.skipSlotCheck = skipSlotCheck;
		InitializeComponent();
	}

	private void OnOK(object sender, EventArgs e)
	{
		bool flag = false;
		m_errorProvider.SetError(m_pickStartDeckButton, string.Empty);
		m_errorProvider.SetError(m_pickStartSlotButton, string.Empty);
		m_errorProvider.SetError(m_pickEndDeckButton, string.Empty);
		m_errorProvider.SetError(m_pickEndSlotButton, string.Empty);
		if (string.IsNullOrEmpty(m_startDeckTextBox.Text))
		{
			m_errorProvider.SetError(m_pickStartDeckButton, "The Start Deck value is required.");
			flag = true;
		}
		if (string.IsNullOrEmpty(m_endDeckTextBox.Text))
		{
			m_errorProvider.SetError(m_pickEndDeckButton, "The End Deck value is required.");
			flag = true;
		}
		if (string.IsNullOrEmpty(m_startSlotTextBox.Text))
		{
			m_errorProvider.SetError(m_pickStartSlotButton, "The Start Slot value is required.");
			flag = true;
		}
		if (string.IsNullOrEmpty(m_endSlotTextBox.Text))
		{
			m_errorProvider.SetError(m_pickEndSlotButton, "The End Slot value is required.");
			flag = true;
		}
		if (!flag && Range.EndDeck < Range.StartDeck)
		{
			m_errorProvider.SetError(m_pickEndDeckButton, "The End Deck value must be greater than or equal to the Start Deck value.");
			flag = true;
		}
		if (skipSlotCheck && !flag && Range.EndDeck == Range.StartDeck && Range.Slots.End < Range.Slots.Start)
		{
			m_errorProvider.SetError(m_pickEndSlotButton, "The End Slot value must be greater than or equal to the Start Slot value.");
			flag = true;
		}
		else if (!skipSlotCheck && !flag && Range.Slots.End < Range.Slots.Start)
		{
			m_errorProvider.SetError(m_pickEndSlotButton, "The End Slot value must be greater than or equal to the Start Slot value.");
			flag = true;
		}
		if (!flag)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private void OnPickStartDeck(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm();
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_startDeckTextBox.Text = numberPadForm.Number;
		}
	}

	private void OnPickStartSlot(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm();
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_startSlotTextBox.Text = numberPadForm.Number;
		}
	}

	private void OnPickEndDeck(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm();
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_endDeckTextBox.Text = numberPadForm.Number;
		}
	}

	private void OnPickEndSlot(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm();
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_endSlotTextBox.Text = numberPadForm.Number;
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_startDeckTextBox = new System.Windows.Forms.TextBox();
		this.m_startSlotTextBox = new System.Windows.Forms.TextBox();
		this.m_endDeckTextBox = new System.Windows.Forms.TextBox();
		this.m_endSlotTextBox = new System.Windows.Forms.TextBox();
		this.m_pickStartDeckButton = new System.Windows.Forms.Button();
		this.m_pickStartSlotButton = new System.Windows.Forms.Button();
		this.m_pickEndDeckButton = new System.Windows.Forms.Button();
		this.m_pickEndSlotButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(61, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Start Deck:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 41);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(53, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Start Slot:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(12, 70);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(58, 13);
		this.label3.TabIndex = 6;
		this.label3.Text = "End Deck:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(12, 99);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(50, 13);
		this.label4.TabIndex = 9;
		this.label4.Text = "End Slot:";
		this.m_startDeckTextBox.Location = new System.Drawing.Point(79, 12);
		this.m_startDeckTextBox.MaxLength = 2;
		this.m_startDeckTextBox.Name = "m_startDeckTextBox";
		this.m_startDeckTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_startDeckTextBox.TabIndex = 1;
		this.m_startDeckTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_startDeckTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_startSlotTextBox.Location = new System.Drawing.Point(79, 41);
		this.m_startSlotTextBox.MaxLength = 3;
		this.m_startSlotTextBox.Name = "m_startSlotTextBox";
		this.m_startSlotTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_startSlotTextBox.TabIndex = 4;
		this.m_startSlotTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_startSlotTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_endDeckTextBox.Location = new System.Drawing.Point(79, 70);
		this.m_endDeckTextBox.MaxLength = 2;
		this.m_endDeckTextBox.Name = "m_endDeckTextBox";
		this.m_endDeckTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_endDeckTextBox.TabIndex = 7;
		this.m_endDeckTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_endDeckTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_endSlotTextBox.Location = new System.Drawing.Point(79, 99);
		this.m_endSlotTextBox.MaxLength = 3;
		this.m_endSlotTextBox.Name = "m_endSlotTextBox";
		this.m_endSlotTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_endSlotTextBox.TabIndex = 10;
		this.m_endSlotTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_endSlotTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_pickStartDeckButton.Location = new System.Drawing.Point(185, 12);
		this.m_pickStartDeckButton.Name = "m_pickStartDeckButton";
		this.m_pickStartDeckButton.Size = new System.Drawing.Size(31, 23);
		this.m_pickStartDeckButton.TabIndex = 2;
		this.m_pickStartDeckButton.TabStop = false;
		this.m_pickStartDeckButton.Text = "...";
		this.m_pickStartDeckButton.UseVisualStyleBackColor = true;
		this.m_pickStartDeckButton.Click += new System.EventHandler(OnPickStartDeck);
		this.m_pickStartSlotButton.Location = new System.Drawing.Point(185, 41);
		this.m_pickStartSlotButton.Name = "m_pickStartSlotButton";
		this.m_pickStartSlotButton.Size = new System.Drawing.Size(31, 23);
		this.m_pickStartSlotButton.TabIndex = 5;
		this.m_pickStartSlotButton.TabStop = false;
		this.m_pickStartSlotButton.Text = "...";
		this.m_pickStartSlotButton.UseVisualStyleBackColor = true;
		this.m_pickStartSlotButton.Click += new System.EventHandler(OnPickStartSlot);
		this.m_pickEndDeckButton.Location = new System.Drawing.Point(185, 70);
		this.m_pickEndDeckButton.Name = "m_pickEndDeckButton";
		this.m_pickEndDeckButton.Size = new System.Drawing.Size(31, 23);
		this.m_pickEndDeckButton.TabIndex = 8;
		this.m_pickEndDeckButton.TabStop = false;
		this.m_pickEndDeckButton.Text = "...";
		this.m_pickEndDeckButton.UseVisualStyleBackColor = true;
		this.m_pickEndDeckButton.Click += new System.EventHandler(OnPickEndDeck);
		this.m_pickEndSlotButton.Location = new System.Drawing.Point(185, 99);
		this.m_pickEndSlotButton.Name = "m_pickEndSlotButton";
		this.m_pickEndSlotButton.Size = new System.Drawing.Size(31, 23);
		this.m_pickEndSlotButton.TabIndex = 11;
		this.m_pickEndSlotButton.TabStop = false;
		this.m_pickEndSlotButton.Text = "...";
		this.m_pickEndSlotButton.UseVisualStyleBackColor = true;
		this.m_pickEndSlotButton.Click += new System.EventHandler(OnPickEndSlot);
		this.m_okButton.Location = new System.Drawing.Point(245, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 12;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(245, 41);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 13;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_errorProvider.ContainerControl = this;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(332, 138);
		base.ControlBox = false;
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_pickEndSlotButton);
		base.Controls.Add(this.m_pickEndDeckButton);
		base.Controls.Add(this.m_pickStartSlotButton);
		base.Controls.Add(this.m_pickStartDeckButton);
		base.Controls.Add(this.m_endSlotTextBox);
		base.Controls.Add(this.m_endDeckTextBox);
		base.Controls.Add(this.m_startSlotTextBox);
		base.Controls.Add(this.m_startDeckTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "SlotRangeForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Deck/Slot Range";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
