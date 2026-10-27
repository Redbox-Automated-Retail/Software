using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class StartTimeForm : Form
{
	private bool m_activated;

	private IContainer components;

	private Label label1;

	private MaskedTextBox m_maskedTextBox;

	private ErrorProvider m_errorProvider;

	private Button m_subtractHourButton;

	private Button m_addHourButton;

	private Button m_todayButton;

	private Button m_tomorrowButton;

	private Button m_okButton;

	private Button m_cancelButton;

	private Button m_subtractMinute;

	private Button m_addMinute;

	public DateTime? StartTime
	{
		get
		{
			if (DateTime.TryParse(m_maskedTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				m_maskedTextBox.Text = value.Value.ToString("MM/dd/yyyy HH:mm");
			}
		}
	}

	public StartTimeForm()
	{
		InitializeComponent();
	}

	private void OnToday(object sender, EventArgs e)
	{
		m_maskedTextBox.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
	}

	private void OnTomorrow(object sender, EventArgs e)
	{
		m_maskedTextBox.Text = DateTime.Now.AddDays(1.0).ToString("MM/dd/yyyy HH:mm");
	}

	private void OnAddHour(object sender, EventArgs e)
	{
		if (StartTime.HasValue)
		{
			m_maskedTextBox.Text = StartTime.Value.AddHours(1.0).ToString("MM/dd/yyyy HH:mm");
		}
	}

	private void OnSubtractHour(object sender, EventArgs e)
	{
		if (StartTime.HasValue)
		{
			m_maskedTextBox.Text = StartTime.Value.AddHours(-1.0).ToString("MM/dd/yyyy HH:mm");
		}
	}

	private void OnAddMinute(object sender, EventArgs e)
	{
		if (StartTime.HasValue)
		{
			m_maskedTextBox.Text = StartTime.Value.AddMinutes(1.0).ToString("MM/dd/yyyy HH:mm");
		}
	}

	private void OnSubtractMinute(object sender, EventArgs e)
	{
		if (StartTime.HasValue)
		{
			m_maskedTextBox.Text = StartTime.Value.AddMinutes(-1.0).ToString("MM/dd/yyyy HH:mm");
		}
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_maskedTextBox, string.Empty);
		if (!StartTime.HasValue)
		{
			m_errorProvider.SetError(m_maskedTextBox, "A valid date and time is required.");
			return;
		}
		if (StartTime < DateTime.Today)
		{
			m_errorProvider.SetError(m_maskedTextBox, "Start time must be in the future.");
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

	private void OnActivated(object sender, EventArgs e)
	{
		if (!m_activated)
		{
			m_activated = true;
			m_maskedTextBox.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
			m_maskedTextBox.Focus();
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
		this.m_maskedTextBox = new System.Windows.Forms.MaskedTextBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_todayButton = new System.Windows.Forms.Button();
		this.m_addHourButton = new System.Windows.Forms.Button();
		this.m_subtractHourButton = new System.Windows.Forms.Button();
		this.m_tomorrowButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_addMinute = new System.Windows.Forms.Button();
		this.m_subtractMinute = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(58, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Start Time:";
		this.m_maskedTextBox.Location = new System.Drawing.Point(76, 6);
		this.m_maskedTextBox.Mask = "00/00/0000 90:00";
		this.m_maskedTextBox.Name = "m_maskedTextBox";
		this.m_maskedTextBox.Size = new System.Drawing.Size(135, 20);
		this.m_maskedTextBox.TabIndex = 1;
		this.m_errorProvider.ContainerControl = this;
		this.m_todayButton.Location = new System.Drawing.Point(15, 32);
		this.m_todayButton.Name = "m_todayButton";
		this.m_todayButton.Size = new System.Drawing.Size(95, 23);
		this.m_todayButton.TabIndex = 2;
		this.m_todayButton.Text = "Today";
		this.m_todayButton.UseVisualStyleBackColor = true;
		this.m_todayButton.Click += new System.EventHandler(OnToday);
		this.m_addHourButton.Location = new System.Drawing.Point(15, 61);
		this.m_addHourButton.Name = "m_addHourButton";
		this.m_addHourButton.Size = new System.Drawing.Size(95, 23);
		this.m_addHourButton.TabIndex = 4;
		this.m_addHourButton.Text = "Add Hour";
		this.m_addHourButton.UseVisualStyleBackColor = true;
		this.m_addHourButton.Click += new System.EventHandler(OnAddHour);
		this.m_subtractHourButton.Location = new System.Drawing.Point(116, 61);
		this.m_subtractHourButton.Name = "m_subtractHourButton";
		this.m_subtractHourButton.Size = new System.Drawing.Size(95, 23);
		this.m_subtractHourButton.TabIndex = 5;
		this.m_subtractHourButton.Text = "Subtract Hour";
		this.m_subtractHourButton.UseVisualStyleBackColor = true;
		this.m_subtractHourButton.Click += new System.EventHandler(OnSubtractHour);
		this.m_tomorrowButton.Location = new System.Drawing.Point(116, 32);
		this.m_tomorrowButton.Name = "m_tomorrowButton";
		this.m_tomorrowButton.Size = new System.Drawing.Size(95, 23);
		this.m_tomorrowButton.TabIndex = 3;
		this.m_tomorrowButton.Text = "Tomorrow";
		this.m_tomorrowButton.UseVisualStyleBackColor = true;
		this.m_tomorrowButton.Click += new System.EventHandler(OnTomorrow);
		this.m_okButton.Location = new System.Drawing.Point(242, 6);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 8;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(242, 35);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 9;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_addMinute.Location = new System.Drawing.Point(15, 90);
		this.m_addMinute.Name = "m_addMinute";
		this.m_addMinute.Size = new System.Drawing.Size(95, 23);
		this.m_addMinute.TabIndex = 6;
		this.m_addMinute.Text = "Add Minute";
		this.m_addMinute.UseVisualStyleBackColor = true;
		this.m_addMinute.Click += new System.EventHandler(OnAddMinute);
		this.m_subtractMinute.Location = new System.Drawing.Point(116, 90);
		this.m_subtractMinute.Name = "m_subtractMinute";
		this.m_subtractMinute.Size = new System.Drawing.Size(95, 23);
		this.m_subtractMinute.TabIndex = 7;
		this.m_subtractMinute.Text = "Subtract Minute";
		this.m_subtractMinute.UseVisualStyleBackColor = true;
		this.m_subtractMinute.Click += new System.EventHandler(OnSubtractMinute);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(329, 124);
		base.ControlBox = false;
		base.Controls.Add(this.m_subtractMinute);
		base.Controls.Add(this.m_addMinute);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_tomorrowButton);
		base.Controls.Add(this.m_subtractHourButton);
		base.Controls.Add(this.m_addHourButton);
		base.Controls.Add(this.m_todayButton);
		base.Controls.Add(this.m_maskedTextBox);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "StartTimeForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Set Start Time";
		base.Activated += new System.EventHandler(OnActivated);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
