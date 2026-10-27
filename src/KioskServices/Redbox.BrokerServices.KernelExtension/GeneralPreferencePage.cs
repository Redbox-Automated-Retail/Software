using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.BrokerServices.Proxy;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.KernelExtension;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private TextBox m_brokerServicesTimeoutTextBox;

	private Label label4;

	private TextBox m_brokerServicesUrlTextBox;

	private Label label5;

	private ErrorProvider m_errorProvider;

	private Label m_headerLabel;

	public GeneralPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_brokerServicesUrlTextBox.Text = BrokerServicesProxy.Instance.Url;
		m_brokerServicesTimeoutTextBox.Text = BrokerServicesProxy.Instance.Timeout.ToString();
	}

	private void OnLeaveBrokerServicesUrlTextBox(object sender, EventArgs e)
	{
		string url = BrokerServicesProxy.Instance.Url;
		if (!string.IsNullOrEmpty(m_brokerServicesUrlTextBox.Text))
		{
			url = m_brokerServicesUrlTextBox.Text;
		}
		BrokerServicesProxy.Instance.Url = url;
	}

	private void OnLeaveBrokerServicesTimeoutTextBox(object sender, EventArgs e)
	{
		if (int.TryParse(m_brokerServicesTimeoutTextBox.Text, out var result))
		{
			BrokerServicesProxy.Instance.Timeout = result;
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
		this.m_brokerServicesTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_brokerServicesUrlTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_brokerServicesTimeoutTextBox.Location = new System.Drawing.Point(196, 199);
		this.m_brokerServicesTimeoutTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
		this.m_brokerServicesTimeoutTextBox.MaxLength = 6;
		this.m_brokerServicesTimeoutTextBox.Name = "m_brokerServicesTimeoutTextBox";
		this.m_brokerServicesTimeoutTextBox.Size = new System.Drawing.Size(180, 29);
		this.m_brokerServicesTimeoutTextBox.TabIndex = 4;
		this.m_brokerServicesTimeoutTextBox.Leave += new System.EventHandler(OnLeaveBrokerServicesTimeoutTextBox);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(26, 205);
		this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(160, 25);
		this.label4.TabIndex = 3;
		this.label4.Text = "Service Timeout:";
		this.m_brokerServicesUrlTextBox.Location = new System.Drawing.Point(196, 150);
		this.m_brokerServicesUrlTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
		this.m_brokerServicesUrlTextBox.Name = "m_brokerServicesUrlTextBox";
		this.m_brokerServicesUrlTextBox.Size = new System.Drawing.Size(728, 29);
		this.m_brokerServicesUrlTextBox.TabIndex = 2;
		this.m_brokerServicesUrlTextBox.Leave += new System.EventHandler(OnLeaveBrokerServicesUrlTextBox);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(55, 155);
		this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(127, 25);
		this.label5.TabIndex = 1;
		this.label5.Text = "Service URL:";
		this.m_errorProvider.ContainerControl = this;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkMagenta;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(994, 70);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Broker Service: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(11f, 24f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_brokerServicesTimeoutTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_brokerServicesUrlTextBox);
		base.Controls.Add(this.label5);
		base.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(994, 934);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
