using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class PersonalizationGeneralPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private CheckBox cb_simulateSuccess;

	private CheckBox cb_LiveCall;

	private Label m_headerLabel;

	private TextBox tb_CustProfileNum;

	private Label label1;

	private TextBox tb_MobilePhoneNumber;

	private Label label2;

	private TextBox tb_loginEmail;

	private Label label4;

	public PersonalizationGeneralPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			cb_LiveCall.Checked = instance.LiveCall;
			cb_simulateSuccess.Checked = instance.SimulateSuccess;
			tb_loginEmail.Text = instance.LoginEmail;
			tb_CustProfileNum.Text = instance.CustomerProfileNumber;
			tb_MobilePhoneNumber.Text = instance.MobilePhoneNumber;
		}
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance == null)
		{
			return false;
		}
		instance.LoginEmail = tb_loginEmail.Text;
		instance.CustomerProfileNumber = tb_CustProfileNum.Text;
		instance.MobilePhoneNumber = tb_MobilePhoneNumber.Text;
		return true;
	}

	private void OnLiveCallChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.LiveCall = cb_LiveCall.Checked;
		}
		ServiceLocator.Instance.RemoveService(typeof(IPersonalizationServices));
		if (instance.LiveCall)
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxy.Instance);
		}
		else
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxySimulator.Instance);
		}
	}

	private void OnSimulateSuccessChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.SimulateSuccess = cb_simulateSuccess.Checked;
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
		this.cb_simulateSuccess = new System.Windows.Forms.CheckBox();
		this.cb_LiveCall = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.tb_CustProfileNum = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.tb_MobilePhoneNumber = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.tb_loginEmail = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.cb_simulateSuccess.AutoSize = true;
		this.cb_simulateSuccess.Location = new System.Drawing.Point(19, 85);
		this.cb_simulateSuccess.Name = "cb_simulateSuccess";
		this.cb_simulateSuccess.Size = new System.Drawing.Size(110, 17);
		this.cb_simulateSuccess.TabIndex = 68;
		this.cb_simulateSuccess.Text = "Simulate Success";
		this.cb_simulateSuccess.UseVisualStyleBackColor = true;
		this.cb_simulateSuccess.CheckedChanged += new System.EventHandler(OnSimulateSuccessChanged);
		this.cb_LiveCall.AutoSize = true;
		this.cb_LiveCall.Location = new System.Drawing.Point(19, 53);
		this.cb_LiveCall.Name = "cb_LiveCall";
		this.cb_LiveCall.Size = new System.Drawing.Size(66, 17);
		this.cb_LiveCall.TabIndex = 67;
		this.cb_LiveCall.Text = "Live Call";
		this.cb_LiveCall.UseVisualStyleBackColor = true;
		this.cb_LiveCall.CheckedChanged += new System.EventHandler(OnLiveCallChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(1, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 66;
		this.m_headerLabel.Text = "Personalization Services: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.tb_CustProfileNum.Location = new System.Drawing.Point(191, 143);
		this.tb_CustProfileNum.Name = "tb_CustProfileNum";
		this.tb_CustProfileNum.Size = new System.Drawing.Size(236, 20);
		this.tb_CustProfileNum.TabIndex = 70;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(18, 146);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(126, 13);
		this.label1.TabIndex = 69;
		this.label1.Text = "Customer Profile Number:";
		this.tb_MobilePhoneNumber.Location = new System.Drawing.Point(191, 169);
		this.tb_MobilePhoneNumber.Name = "tb_MobilePhoneNumber";
		this.tb_MobilePhoneNumber.Size = new System.Drawing.Size(236, 20);
		this.tb_MobilePhoneNumber.TabIndex = 72;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(18, 172);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(115, 13);
		this.label2.TabIndex = 71;
		this.label2.Text = "Mobile Phone Number:";
		this.tb_loginEmail.Location = new System.Drawing.Point(191, 117);
		this.tb_loginEmail.Name = "tb_loginEmail";
		this.tb_loginEmail.Size = new System.Drawing.Size(236, 20);
		this.tb_loginEmail.TabIndex = 74;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(18, 120);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(64, 13);
		this.label4.TabIndex = 73;
		this.label4.Text = "Login Email:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.tb_loginEmail);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.tb_MobilePhoneNumber);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_CustProfileNum);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cb_simulateSuccess);
		base.Controls.Add(this.cb_LiveCall);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "PersonalizationGeneralPreferencePage";
		base.Size = new System.Drawing.Size(534, 361);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
