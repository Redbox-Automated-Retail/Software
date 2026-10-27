using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.UpdateServices.Proxy;

namespace Redbox.UpdateServices.KernelExtension;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private TextBox m_updateMangerServiceTimeoutTextBox;

	private TextBox m_updateManagerServiceUrlTextBox;

	private Label label8;

	private Label label7;

	private ErrorProvider m_errorProvider;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	public GeneralPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		m_errorProvider.SetError(m_updateManagerServiceUrlTextBox, string.Empty);
		m_errorProvider.SetError(m_updateMangerServiceTimeoutTextBox, string.Empty);
		UpdateManagerServiceProxy.Instance.Url = m_updateManagerServiceUrlTextBox.Text;
		if (!int.TryParse(m_updateMangerServiceTimeoutTextBox.Text, out var result))
		{
			return false;
		}
		UpdateManagerServiceProxy.Instance.Timeout = result;
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_updateManagerServiceUrlTextBox.Text = UpdateManagerServiceProxy.Instance.Url;
		m_updateMangerServiceTimeoutTextBox.Text = UpdateManagerServiceProxy.Instance.Timeout.ToString();
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
		this.m_updateMangerServiceTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.m_updateManagerServiceUrlTextBox = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_updateMangerServiceTimeoutTextBox.Location = new System.Drawing.Point(107, 109);
		this.m_updateMangerServiceTimeoutTextBox.MaxLength = 6;
		this.m_updateMangerServiceTimeoutTextBox.Name = "m_updateMangerServiceTimeoutTextBox";
		this.m_updateMangerServiceTimeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_updateMangerServiceTimeoutTextBox.TabIndex = 27;
		this.m_updateManagerServiceUrlTextBox.Location = new System.Drawing.Point(107, 82);
		this.m_updateManagerServiceUrlTextBox.Name = "m_updateManagerServiceUrlTextBox";
		this.m_updateManagerServiceUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_updateManagerServiceUrlTextBox.TabIndex = 25;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(14, 111);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(87, 13);
		this.label8.TabIndex = 26;
		this.label8.Text = "Service Timeout:";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(30, 85);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(71, 13);
		this.label7.TabIndex = 24;
		this.label7.Text = "Service URL:";
		this.m_errorProvider.ContainerControl = this;
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(107, 59);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 28;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.IndianRed;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 29;
		this.m_headerLabel.Text = "  Update Service: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_updateMangerServiceTimeoutTextBox);
		base.Controls.Add(this.m_updateManagerServiceUrlTextBox);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
