using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.Management.Console;

public class SensorRangeForm : Form
{
	private IContainer components;

	private Label label1;

	private Label label2;

	private TextBox m_startSensorTextBox;

	private TextBox m_endSensorTextBox;

	private Button m_pickStartSensorButton;

	private Button m_pickEndSensorButton;

	private Button m_okButton;

	private Button m_cancelButton;

	private ErrorProvider m_errorProvider;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int MinimumValue { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int MaximumValue { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? Start
	{
		get
		{
			if (m_startSensorTextBox.Text.Length > 0 && int.TryParse(m_startSensorTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_startSensorTextBox.Text = value.ToString();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? End
	{
		get
		{
			if (m_endSensorTextBox.Text.Length > 0 && int.TryParse(m_endSensorTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_endSensorTextBox.Text = value.ToString();
		}
	}

	public SensorRangeForm()
	{
		InitializeComponent();
	}

	public void DisableEnd()
	{
		m_endSensorTextBox.Enabled = false;
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_startSensorTextBox, string.Empty);
		m_errorProvider.SetError(m_endSensorTextBox, string.Empty);
		bool flag = false;
		if (Start.HasValue && (Start < MinimumValue || Start > MaximumValue))
		{
			m_errorProvider.SetError(m_startSensorTextBox, $"The Start Sensor value must be equal to or greater than {MinimumValue} and less than or equal to {MaximumValue}.");
			flag = true;
		}
		if (End.HasValue && (End < MinimumValue || End > MaximumValue))
		{
			m_errorProvider.SetError(m_endSensorTextBox, $"The End Sensor value must be equal to or greater than {MinimumValue} and less than or equal to {MaximumValue}.");
			flag = true;
		}
		if (Start.HasValue && End.HasValue && End < Start)
		{
			m_errorProvider.SetError(m_endSensorTextBox, "The End Sensor value must be equal to or greater than Start Sensor value.");
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

	private void OnPickStartSensor(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm
		{
			Text = "Enter Start Sensor"
		};
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_startSensorTextBox.Text = numberPadForm.Number;
			m_endSensorTextBox.Focus();
		}
	}

	private void OnPickEndSensor(object sender, EventArgs e)
	{
		NumberPadForm numberPadForm = new NumberPadForm
		{
			Text = "Enter End Sensor"
		};
		if (numberPadForm.ShowDialog() == DialogResult.OK)
		{
			m_endSensorTextBox.Text = numberPadForm.Number;
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
		this.m_startSensorTextBox = new System.Windows.Forms.TextBox();
		this.m_endSensorTextBox = new System.Windows.Forms.TextBox();
		this.m_pickStartSensorButton = new System.Windows.Forms.Button();
		this.m_pickEndSensorButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 12);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(68, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Start Sensor:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 46);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(65, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "End Sensor:";
		this.m_startSensorTextBox.Location = new System.Drawing.Point(86, 9);
		this.m_startSensorTextBox.MaxLength = 2;
		this.m_startSensorTextBox.Name = "m_startSensorTextBox";
		this.m_startSensorTextBox.Size = new System.Drawing.Size(76, 20);
		this.m_startSensorTextBox.TabIndex = 1;
		this.m_startSensorTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_startSensorTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_endSensorTextBox.Location = new System.Drawing.Point(86, 43);
		this.m_endSensorTextBox.MaxLength = 2;
		this.m_endSensorTextBox.Name = "m_endSensorTextBox";
		this.m_endSensorTextBox.Size = new System.Drawing.Size(76, 20);
		this.m_endSensorTextBox.TabIndex = 3;
		this.m_endSensorTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_endSensorTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		this.m_pickStartSensorButton.Location = new System.Drawing.Point(200, 9);
		this.m_pickStartSensorButton.Name = "m_pickStartSensorButton";
		this.m_pickStartSensorButton.Size = new System.Drawing.Size(55, 23);
		this.m_pickStartSensorButton.TabIndex = 4;
		this.m_pickStartSensorButton.TabStop = false;
		this.m_pickStartSensorButton.Text = "...";
		this.m_pickStartSensorButton.UseVisualStyleBackColor = true;
		this.m_pickStartSensorButton.Click += new System.EventHandler(OnPickStartSensor);
		this.m_pickEndSensorButton.Location = new System.Drawing.Point(200, 43);
		this.m_pickEndSensorButton.Name = "m_pickEndSensorButton";
		this.m_pickEndSensorButton.Size = new System.Drawing.Size(55, 23);
		this.m_pickEndSensorButton.TabIndex = 5;
		this.m_pickEndSensorButton.TabStop = false;
		this.m_pickEndSensorButton.Text = "...";
		this.m_pickEndSensorButton.UseVisualStyleBackColor = true;
		this.m_pickEndSensorButton.Click += new System.EventHandler(OnPickEndSensor);
		this.m_okButton.Location = new System.Drawing.Point(86, 81);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 4;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(180, 81);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 5;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_errorProvider.ContainerControl = this;
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(267, 116);
		base.ControlBox = false;
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_pickEndSensorButton);
		base.Controls.Add(this.m_pickStartSensorButton);
		base.Controls.Add(this.m_endSensorTextBox);
		base.Controls.Add(this.m_startSensorTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "SensorRangeForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Enter Sensor Range";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
