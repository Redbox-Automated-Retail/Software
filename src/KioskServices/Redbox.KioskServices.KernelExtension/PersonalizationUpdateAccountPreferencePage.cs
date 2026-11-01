using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class PersonalizationUpdateAccountPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private Panel panel1;

	private Label label14;

	private TextBox tb_Pin;

	private Label label13;

	private CheckBox cbTextClubOptIn;

	private CheckBox cbPunchCardOptIn;

	private Button btnUpdateAccount;

	private Label label12;

	private ComboBox cbUpdateAccountError;

	public PersonalizationUpdateAccountPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			cbUpdateAccountError.DataSource = Enum.GetValues(typeof(Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors));
			cbUpdateAccountError.SelectedItem = instance.UpdateAccountResponseCode;
		}
	}

	private void cbUpdateAccountError_SelectedIndexChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.UpdateAccountResponseCode = (Redbox.KioskServices.Proxy.ComponentModel.UpdateAccountResponseErrors)cbUpdateAccountError.SelectedItem;
		}
	}

	private void btnUpdateAccount_Click(object sender, EventArgs e)
	{
		IPersonalizationService personalizationService = ServiceLocator.Instance.GetService<IPersonalizationService>();
		Action<IUpdateAccountResult> action = updateAcountCallback;
		SecureString secureString = CardHelper.CopyToSecure(tb_Pin.Text.Trim());
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		personalizationService.UpdateAccount(instance.CustomerProfileNumber.Trim(), instance.LoginEmail.Trim(), instance.MobilePhoneNumber.Trim(), secureString, cbPunchCardOptIn.Checked, cbTextClubOptIn.Checked, action, false, Redbox.KioskServices.Proxy.ComponentModel.AccountSourceType.General);
	}

	private void updateAcountCallback(IUpdateAccountResult result)
	{
		if (result.Success)
		{
			MessageBox.Show("Call was successful");
		}
		else
		{
			MessageBox.Show($"Call was unsuccessful: {result.UpdateAccountResponseError} {result.ErrorMessage} ");
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.label14 = new System.Windows.Forms.Label();
		this.tb_Pin = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.cbTextClubOptIn = new System.Windows.Forms.CheckBox();
		this.cbPunchCardOptIn = new System.Windows.Forms.CheckBox();
		this.btnUpdateAccount = new System.Windows.Forms.Button();
		this.label12 = new System.Windows.Forms.Label();
		this.cbUpdateAccountError = new System.Windows.Forms.ComboBox();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(3, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 43;
		this.m_headerLabel.Text = "Personalization Services: Update Account";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.label14);
		this.panel1.Controls.Add(this.tb_Pin);
		this.panel1.Controls.Add(this.label13);
		this.panel1.Controls.Add(this.cbTextClubOptIn);
		this.panel1.Controls.Add(this.cbPunchCardOptIn);
		this.panel1.Controls.Add(this.btnUpdateAccount);
		this.panel1.Location = new System.Drawing.Point(35, 205);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(409, 105);
		this.panel1.TabIndex = 82;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(3, 7);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(123, 13);
		this.label14.TabIndex = 84;
		this.label14.Text = "Temporary Test Harness";
		this.tb_Pin.Location = new System.Drawing.Point(193, 43);
		this.tb_Pin.Name = "tb_Pin";
		this.tb_Pin.Size = new System.Drawing.Size(55, 20);
		this.tb_Pin.TabIndex = 83;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(155, 46);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(28, 13);
		this.label13.TabIndex = 82;
		this.label13.Text = "PIN:";
		this.cbTextClubOptIn.AutoSize = true;
		this.cbTextClubOptIn.Location = new System.Drawing.Point(158, 23);
		this.cbTextClubOptIn.Name = "cbTextClubOptIn";
		this.cbTextClubOptIn.Size = new System.Drawing.Size(103, 17);
		this.cbTextClubOptIn.TabIndex = 81;
		this.cbTextClubOptIn.Text = "Text Club Opt In";
		this.cbTextClubOptIn.UseVisualStyleBackColor = true;
		this.cbPunchCardOptIn.AutoSize = true;
		this.cbPunchCardOptIn.Location = new System.Drawing.Point(158, 3);
		this.cbPunchCardOptIn.Name = "cbPunchCardOptIn";
		this.cbPunchCardOptIn.Size = new System.Drawing.Size(110, 17);
		this.cbPunchCardOptIn.TabIndex = 80;
		this.cbPunchCardOptIn.Text = "Punchcard Opt In";
		this.cbPunchCardOptIn.UseVisualStyleBackColor = true;
		this.btnUpdateAccount.Location = new System.Drawing.Point(269, 75);
		this.btnUpdateAccount.Name = "btnUpdateAccount";
		this.btnUpdateAccount.Size = new System.Drawing.Size(117, 23);
		this.btnUpdateAccount.TabIndex = 79;
		this.btnUpdateAccount.Text = "Call UpdateAccount()";
		this.btnUpdateAccount.UseVisualStyleBackColor = true;
		this.btnUpdateAccount.Click += new System.EventHandler(btnUpdateAccount_Click);
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(14, 65);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(164, 13);
		this.label12.TabIndex = 81;
		this.label12.Text = "Update Account Response Error:";
		this.cbUpdateAccountError.FormattingEnabled = true;
		this.cbUpdateAccountError.Location = new System.Drawing.Point(187, 62);
		this.cbUpdateAccountError.Name = "cbUpdateAccountError";
		this.cbUpdateAccountError.Size = new System.Drawing.Size(236, 21);
		this.cbUpdateAccountError.TabIndex = 80;
		this.cbUpdateAccountError.SelectedIndexChanged += new System.EventHandler(cbUpdateAccountError_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.cbUpdateAccountError);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "PersonalizationUpdateAccountPreferencePage";
		base.Size = new System.Drawing.Size(490, 333);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
