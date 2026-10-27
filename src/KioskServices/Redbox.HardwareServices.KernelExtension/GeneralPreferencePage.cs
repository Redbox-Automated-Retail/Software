using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HardwareServices.Proxy;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private ErrorProvider m_errorProvider;

	private TextBox m_hardwareServiceUrlTextBox;

	private Label label1;

	private CheckBox m_liveCallCheckBox;

	private TextBox m_hardwareServiceTimeoutTextBox;

	private Label label2;

	private Label m_headerLabel;

	public GeneralPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		m_errorProvider.SetError(m_hardwareServiceUrlTextBox, string.Empty);
		m_errorProvider.SetError(m_hardwareServiceTimeoutTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_hardwareServiceUrlTextBox.Text))
		{
			m_errorProvider.SetError(m_hardwareServiceUrlTextBox, "The Hardware Service URL is a required field.");
			return false;
		}
		if (string.IsNullOrEmpty(m_hardwareServiceTimeoutTextBox.Text))
		{
			m_errorProvider.SetError(m_hardwareServiceTimeoutTextBox, "The Hardware Service Timeout is a required field.");
			return false;
		}
		if (!int.TryParse(m_hardwareServiceTimeoutTextBox.Text, out var result))
		{
			m_errorProvider.SetError(m_hardwareServiceTimeoutTextBox, "The Hardware Service Timeout must be a valid integer value.");
			return false;
		}
		if (HardwareService.Instance.Url != m_hardwareServiceUrlTextBox.Text)
		{
			HardwareService.Instance.Url = m_hardwareServiceUrlTextBox.Text;
		}
		if (HardwareService.Instance.Timeout != result)
		{
			HardwareService.Instance.Timeout = result;
		}
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_hardwareServiceUrlTextBox.Text = HardwareService.Instance.Url;
		m_hardwareServiceTimeoutTextBox.Text = HardwareService.Instance.Timeout.ToString();
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
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_hardwareServiceUrlTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_hardwareServiceTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_errorProvider.ContainerControl = this;
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(107, 60);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(30, 86);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(71, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Service URL:";
		this.m_hardwareServiceUrlTextBox.Location = new System.Drawing.Point(107, 83);
		this.m_hardwareServiceUrlTextBox.Name = "m_hardwareServiceUrlTextBox";
		this.m_hardwareServiceUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_hardwareServiceUrlTextBox.TabIndex = 2;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(14, 113);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(87, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Service Timeout:";
		this.m_hardwareServiceTimeoutTextBox.Location = new System.Drawing.Point(107, 110);
		this.m_hardwareServiceTimeoutTextBox.Name = "m_hardwareServiceTimeoutTextBox";
		this.m_hardwareServiceTimeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_hardwareServiceTimeoutTextBox.TabIndex = 4;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 6;
		this.m_headerLabel.Text = "  Hardware Service: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_hardwareServiceTimeoutTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_hardwareServiceUrlTextBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
