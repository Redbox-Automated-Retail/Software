using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_allowDebuggingCheckBox;

	private Label label1;

	private ComboBox m_environmentComboBox;

	private Label label12;

	private ComboBox m_applicationComboBox;

	private MaskedTextBox m_rebootTimeMaskedTextBox;

	private Label label11;

	private CheckBox m_enableMouseCursorCheckBox;

	private Label label2;

	private Panel m_colorPanel;

	private Button m_editColorButton;

	private CheckBox m_sendAnalyticsOffPeakCheckbox;

	private CheckBox m_verboseLoggingCheckBox;

	private Label m_headerLabel;

	private Button button1;

	public GeneralPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
		if (service != null)
		{
			service.IsEnabled = m_allowDebuggingCheckBox.Checked;
		}
		preferencePage.SetValue("Environment", m_environmentComboBox.Text);
		preferencePage.SetValue("Application", m_applicationComboBox.Text);
		preferencePage.SetValue("RebootTime", m_rebootTimeMaskedTextBox.Text);
		IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service2 != null)
		{
			service2.BackgroundColor = m_colorPanel.BackColor;
		}
		preferencePage.SetValue("EnableMouseCursor", m_enableMouseCursorCheckBox.Checked);
		preferencePage.SetValue("SendAnalyticsOffPeak", m_sendAnalyticsOffPeakCheckbox.Checked);
		preferencePage.SetValue("VerboseLogging", m_verboseLoggingCheckBox.Checked);
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_environmentComboBox.Text = preferencePage.GetValue<string>("Environment");
		m_applicationComboBox.Text = preferencePage.GetValue<string>("Application");
		m_rebootTimeMaskedTextBox.Text = GetRebootTime(preferencePage);
		IDebugService service = ServiceLocator.Instance.GetService<IDebugService>();
		if (service == null)
		{
			m_allowDebuggingCheckBox.Enabled = false;
		}
		else
		{
			m_allowDebuggingCheckBox.Checked = service.IsEnabled;
		}
		m_enableMouseCursorCheckBox.Checked = preferencePage.GetValue("EnableMouseCursor", defaultValue: true);
		m_sendAnalyticsOffPeakCheckbox.Checked = preferencePage.GetValue("SendAnalyticsOffPeak", defaultValue: true);
		m_verboseLoggingCheckBox.Checked = preferencePage.GetValue("VerboseLogging", defaultValue: false);
		IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service2 != null)
		{
			m_colorPanel.BackColor = service2.BackgroundColor;
		}
		m_isLoaded = true;
	}

	private static string GetRebootTime(IPreferencePage preferencePage)
	{
		string text = preferencePage.GetValue<string>("RebootTime");
		if (string.IsNullOrEmpty(text))
		{
			string s = IdHelper.GeneratePseudoUniqueId(1, "12345");
			string s2 = IdHelper.GeneratePseudoUniqueId(2, "012345");
			text = $"{int.Parse(s):00}:{int.Parse(s2):00}";
			preferencePage.SetValue("RebootTime", text);
		}
		return text;
	}

	private void OnEditColor(object sender, EventArgs e)
	{
		ColorDialog colorDialog = new ColorDialog();
		if (colorDialog.ShowDialog() == DialogResult.OK)
		{
			m_colorPanel.BackColor = colorDialog.Color;
			Form form = Application.OpenForms["HostForm"];
			if (form != null)
			{
				form.BackColor = m_colorPanel.BackColor;
			}
		}
	}

	private void OnEnableMouseCursorCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			if (m_enableMouseCursorCheckBox.Checked)
			{
				Cursor.Show();
			}
			else
			{
				Cursor.Hide();
			}
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		throw new Exception("This is a test");
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
		this.m_allowDebuggingCheckBox = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_environmentComboBox = new System.Windows.Forms.ComboBox();
		this.label12 = new System.Windows.Forms.Label();
		this.m_applicationComboBox = new System.Windows.Forms.ComboBox();
		this.m_rebootTimeMaskedTextBox = new System.Windows.Forms.MaskedTextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.m_enableMouseCursorCheckBox = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_colorPanel = new System.Windows.Forms.Panel();
		this.m_editColorButton = new System.Windows.Forms.Button();
		this.m_sendAnalyticsOffPeakCheckbox = new System.Windows.Forms.CheckBox();
		this.m_verboseLoggingCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_allowDebuggingCheckBox.AutoSize = true;
		this.m_allowDebuggingCheckBox.Location = new System.Drawing.Point(89, 53);
		this.m_allowDebuggingCheckBox.Name = "m_allowDebuggingCheckBox";
		this.m_allowDebuggingCheckBox.Size = new System.Drawing.Size(106, 17);
		this.m_allowDebuggingCheckBox.TabIndex = 0;
		this.m_allowDebuggingCheckBox.Text = "Allow Debugging";
		this.m_allowDebuggingCheckBox.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(14, 101);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(69, 13);
		this.label1.TabIndex = 5;
		this.label1.Text = "Environment:";
		this.m_environmentComboBox.FormattingEnabled = true;
		this.m_environmentComboBox.Items.AddRange(new object[4] { "Development", "QA", "UAT", "Production" });
		this.m_environmentComboBox.Location = new System.Drawing.Point(89, 98);
		this.m_environmentComboBox.Name = "m_environmentComboBox";
		this.m_environmentComboBox.Size = new System.Drawing.Size(240, 21);
		this.m_environmentComboBox.TabIndex = 6;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(21, 131);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(62, 13);
		this.label12.TabIndex = 7;
		this.label12.Text = "Application:";
		this.m_applicationComboBox.FormattingEnabled = true;
		this.m_applicationComboBox.Items.AddRange(new object[2] { "DVD", "Games" });
		this.m_applicationComboBox.Location = new System.Drawing.Point(89, 128);
		this.m_applicationComboBox.Name = "m_applicationComboBox";
		this.m_applicationComboBox.Size = new System.Drawing.Size(240, 21);
		this.m_applicationComboBox.TabIndex = 8;
		this.m_rebootTimeMaskedTextBox.Location = new System.Drawing.Point(89, 158);
		this.m_rebootTimeMaskedTextBox.Mask = "00:00";
		this.m_rebootTimeMaskedTextBox.Name = "m_rebootTimeMaskedTextBox";
		this.m_rebootTimeMaskedTextBox.Size = new System.Drawing.Size(44, 20);
		this.m_rebootTimeMaskedTextBox.TabIndex = 10;
		this.m_rebootTimeMaskedTextBox.ValidatingType = typeof(System.DateTime);
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(12, 161);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(71, 13);
		this.label11.TabIndex = 9;
		this.label11.Text = "Reboot Time:";
		this.m_enableMouseCursorCheckBox.AutoSize = true;
		this.m_enableMouseCursorCheckBox.Location = new System.Drawing.Point(89, 76);
		this.m_enableMouseCursorCheckBox.Name = "m_enableMouseCursorCheckBox";
		this.m_enableMouseCursorCheckBox.Size = new System.Drawing.Size(127, 17);
		this.m_enableMouseCursorCheckBox.TabIndex = 1;
		this.m_enableMouseCursorCheckBox.Text = "Enable Mouse Cursor";
		this.m_enableMouseCursorCheckBox.UseVisualStyleBackColor = true;
		this.m_enableMouseCursorCheckBox.CheckedChanged += new System.EventHandler(OnEnableMouseCursorCheckChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(259, 57);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(95, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Background Color:";
		this.m_colorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_colorPanel.Location = new System.Drawing.Point(360, 53);
		this.m_colorPanel.Name = "m_colorPanel";
		this.m_colorPanel.Size = new System.Drawing.Size(22, 24);
		this.m_colorPanel.TabIndex = 3;
		this.m_colorPanel.DoubleClick += new System.EventHandler(OnEditColor);
		this.m_editColorButton.Location = new System.Drawing.Point(388, 53);
		this.m_editColorButton.Name = "m_editColorButton";
		this.m_editColorButton.Size = new System.Drawing.Size(25, 23);
		this.m_editColorButton.TabIndex = 4;
		this.m_editColorButton.Text = "...";
		this.m_editColorButton.UseVisualStyleBackColor = true;
		this.m_editColorButton.Click += new System.EventHandler(OnEditColor);
		this.m_sendAnalyticsOffPeakCheckbox.AutoSize = true;
		this.m_sendAnalyticsOffPeakCheckbox.Location = new System.Drawing.Point(89, 243);
		this.m_sendAnalyticsOffPeakCheckbox.Name = "m_sendAnalyticsOffPeakCheckbox";
		this.m_sendAnalyticsOffPeakCheckbox.Size = new System.Drawing.Size(189, 17);
		this.m_sendAnalyticsOffPeakCheckbox.TabIndex = 12;
		this.m_sendAnalyticsOffPeakCheckbox.Text = "Send Analytics Data Off-Peak only";
		this.m_sendAnalyticsOffPeakCheckbox.UseVisualStyleBackColor = true;
		this.m_verboseLoggingCheckBox.AutoSize = true;
		this.m_verboseLoggingCheckBox.Location = new System.Drawing.Point(89, 220);
		this.m_verboseLoggingCheckBox.Name = "m_verboseLoggingCheckBox";
		this.m_verboseLoggingCheckBox.Size = new System.Drawing.Size(106, 17);
		this.m_verboseLoggingCheckBox.TabIndex = 13;
		this.m_verboseLoggingCheckBox.Text = "Verbose Logging";
		this.m_verboseLoggingCheckBox.UseVisualStyleBackColor = true;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 14;
		this.m_headerLabel.Text = "  Engine Core: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.button1.Location = new System.Drawing.Point(89, 311);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(149, 23);
		this.button1.TabIndex = 15;
		this.button1.Text = "Throw Test Exception";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.button1);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_verboseLoggingCheckBox);
		base.Controls.Add(this.m_sendAnalyticsOffPeakCheckbox);
		base.Controls.Add(this.m_editColorButton);
		base.Controls.Add(this.m_colorPanel);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_enableMouseCursorCheckBox);
		base.Controls.Add(this.m_rebootTimeMaskedTextBox);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.m_applicationComboBox);
		base.Controls.Add(this.m_environmentComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_allowDebuggingCheckBox);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
