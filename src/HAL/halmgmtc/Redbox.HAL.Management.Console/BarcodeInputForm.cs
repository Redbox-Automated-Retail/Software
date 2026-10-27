using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console;

public class BarcodeInputForm : Form
{
	private IContainer components;

	private ListBox m_listBox;

	private Button m_addButton;

	private Button m_removeButton;

	private Button m_clearButton;

	private Button m_cancelButton;

	private Button m_okButton;

	private NumberPad m_numberPad;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string[] Barcodes
	{
		get
		{
			List<string> list = new List<string>();
			foreach (string item in m_listBox.Items)
			{
				list.Add(item);
			}
			return list.ToArray();
		}
		set
		{
			m_listBox.Items.Clear();
			m_listBox.Items.AddRange(value);
		}
	}

	public BarcodeInputForm()
	{
		InitializeComponent();
	}

	private void OnNumberChanged(object sender, EventArgs e)
	{
		m_addButton.Enabled = !string.IsNullOrEmpty(m_numberPad.Number);
	}

	private void OnAdd(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(m_numberPad.Number))
		{
			int selectedIndex = m_listBox.Items.Add(m_numberPad.Number);
			m_listBox.SelectedIndex = selectedIndex;
			m_numberPad.Clear();
		}
		SetButtonState();
		m_numberPad.Focus();
	}

	private void OnRemove(object sender, EventArgs e)
	{
		m_listBox.Items.RemoveAt(m_listBox.SelectedIndex);
		if (m_listBox.Items.Count > 0)
		{
			m_listBox.SelectedIndex = m_listBox.Items.Count - 1;
		}
		SetButtonState();
		m_numberPad.Focus();
	}

	private void OnClear(object sender, EventArgs e)
	{
		m_listBox.Items.Clear();
		SetButtonState();
		m_numberPad.Focus();
	}

	private void OnSelectedIndexChanged(object sender, EventArgs e)
	{
		SetButtonState();
		m_numberPad.Focus();
	}

	private void SetButtonState()
	{
		m_removeButton.Enabled = m_listBox.SelectedIndex != -1;
		m_clearButton.Enabled = m_listBox.Items.Count > 0;
		m_okButton.Enabled = m_listBox.Items.Count > 0;
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
		this.m_listBox = new System.Windows.Forms.ListBox();
		this.m_addButton = new System.Windows.Forms.Button();
		this.m_removeButton = new System.Windows.Forms.Button();
		this.m_clearButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_numberPad = new Redbox.HAL.Common.GUI.Functions.NumberPad();
		base.SuspendLayout();
		this.m_listBox.FormattingEnabled = true;
		this.m_listBox.Location = new System.Drawing.Point(3, 3);
		this.m_listBox.Name = "m_listBox";
		this.m_listBox.Size = new System.Drawing.Size(429, 173);
		this.m_listBox.TabIndex = 4;
		this.m_listBox.SelectedIndexChanged += new System.EventHandler(OnSelectedIndexChanged);
		this.m_addButton.Enabled = false;
		this.m_addButton.Location = new System.Drawing.Point(440, 182);
		this.m_addButton.Name = "m_addButton";
		this.m_addButton.Size = new System.Drawing.Size(75, 44);
		this.m_addButton.TabIndex = 1;
		this.m_addButton.Text = "Add";
		this.m_addButton.UseVisualStyleBackColor = true;
		this.m_addButton.Click += new System.EventHandler(OnAdd);
		this.m_removeButton.Enabled = false;
		this.m_removeButton.Location = new System.Drawing.Point(440, 232);
		this.m_removeButton.Name = "m_removeButton";
		this.m_removeButton.Size = new System.Drawing.Size(75, 44);
		this.m_removeButton.TabIndex = 2;
		this.m_removeButton.Text = "Remove";
		this.m_removeButton.UseVisualStyleBackColor = true;
		this.m_removeButton.Click += new System.EventHandler(OnRemove);
		this.m_clearButton.Enabled = false;
		this.m_clearButton.Location = new System.Drawing.Point(440, 282);
		this.m_clearButton.Name = "m_clearButton";
		this.m_clearButton.Size = new System.Drawing.Size(75, 44);
		this.m_clearButton.TabIndex = 3;
		this.m_clearButton.Text = "Clear";
		this.m_clearButton.UseVisualStyleBackColor = true;
		this.m_clearButton.Click += new System.EventHandler(OnClear);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(440, 53);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 44);
		this.m_cancelButton.TabIndex = 6;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_okButton.Enabled = false;
		this.m_okButton.Location = new System.Drawing.Point(440, 3);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 44);
		this.m_okButton.TabIndex = 5;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_numberPad.Location = new System.Drawing.Point(3, 182);
		this.m_numberPad.Name = "m_numberPad";
		this.m_numberPad.Size = new System.Drawing.Size(426, 178);
		this.m_numberPad.TabIndex = 0;
		this.m_numberPad.NumberChanged += new System.EventHandler(OnNumberChanged);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(521, 365);
		base.ControlBox = false;
		base.Controls.Add(this.m_numberPad);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_clearButton);
		base.Controls.Add(this.m_removeButton);
		base.Controls.Add(this.m_addButton);
		base.Controls.Add(this.m_listBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "BarcodeInputForm";
		base.Padding = new System.Windows.Forms.Padding(3);
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Barcodes";
		base.ResumeLayout(false);
	}
}
