using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class MarketingOptinPreferencesPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private ComboBox m_confirmationStatusComboBox;

	private Label m_confirmationStatusLabel;

	private CheckBox m_commFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	private TextBox txtCustProfileNum;

	private Label label1;

	private TextBox txtMobilePhoneNumber;

	private Label label2;

	private CheckBox cbPromptForPerks;

	private CheckBox cbPromptForEarlyId;

	private CheckBox cbTextClubEnabled;

	private ComboBox cbEmailVerified;

	private Label labelEmailVerified;

	private Panel emailConfirmationApiPanel;

	private Button emailConfSend;

	private Label emailConfirmationAccountLabel;

	private TextBox emailConfAccountNumberTextbox;

	private Label emailConfirmationEmailLabel;

	private TextBox emailConfAddressTextbox;

	private Label emailConfirmationPanelLabel;

	public MarketingOptinPreferencesPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		service.CustomerProfileNumber = txtCustProfileNum.Text;
		service.MobilePhoneNumber = txtMobilePhoneNumber.Text;
		service.PromptForEarlyId = cbPromptForEarlyId.Checked;
		service.PromptForPerks = cbPromptForPerks.Checked;
		service.TextClubEnabled = cbTextClubEnabled.Checked;
		bool? emailVerified = null;
		if (bool.TryParse(cbEmailVerified?.Text, out var result))
		{
			emailVerified = result;
		}
		service.EmailVerified = emailVerified;
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_confirmationStatusComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(ConfirmationStatus));
		foreach (string item in names)
		{
			m_confirmationStatusComboBox.Items.Add(item);
		}
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		m_confirmationStatusComboBox.SelectedIndex = (int)service.ConfirmationStatusCode;
		m_liveCallCheckBox.Checked = service.LiveCall;
		m_commFailureCheckBox.Checked = service.SimulateCommunicationsFailure;
		txtCustProfileNum.Text = service.CustomerProfileNumber;
		txtMobilePhoneNumber.Text = service.MobilePhoneNumber;
		cbPromptForEarlyId.Checked = service.PromptForEarlyId;
		cbPromptForPerks.Checked = service.PromptForPerks;
		cbTextClubEnabled.Checked = service.TextClubEnabled;
		cbEmailVerified.Text = ((!service.EmailVerified.HasValue) ? "null" : service.EmailVerified.ToString());
		UpdateSimulatedOptions();
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IMarketingServices>().LiveCall = m_liveCallCheckBox.Checked;
			UpdateSimulatedOptions();
		}
	}

	private void UpdateSimulatedOptions()
	{
		bool liveCall = ServiceLocator.Instance.GetService<IMarketingServices>().LiveCall;
		m_confirmationStatusComboBox.Enabled = !liveCall;
		m_commFailureCheckBox.Enabled = !liveCall;
		txtCustProfileNum.Enabled = !liveCall;
		txtMobilePhoneNumber.Enabled = !liveCall;
		cbPromptForEarlyId.Enabled = !liveCall;
		cbPromptForPerks.Enabled = !liveCall;
		cbTextClubEnabled.Enabled = !liveCall;
		cbEmailVerified.Enabled = !liveCall;
	}

	private void OnCommFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IMarketingServices>().SimulateCommunicationsFailure = m_commFailureCheckBox.Checked;
		}
	}

	private void OnConfirmationStatusSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IMarketingServices>().ConfirmationStatusCode = (ConfirmationStatus)Enum.Parse(typeof(ConfirmationStatus), (string)m_confirmationStatusComboBox.SelectedItem, ignoreCase: true);
		}
	}

	private void MarketingOptinPreferencesPage_Load(object sender, EventArgs e)
	{
	}

	private void OnEmailConfirmationSend(object sender, EventArgs e)
	{
		emailConfSend.Enabled = false;
		Task.Run(delegate
		{
			ServiceLocator.Instance.GetService<IMarketingServices>().SendEmailConfirm(ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberInteger, emailConfAddressTextbox.Text, emailConfAccountNumberTextbox.Text, callback);
		});
		void callback(IEmailConfirmResult emailConfirmResult)
		{
			MessageBox.Show(emailConfirmResult.ToJson(), "Result", MessageBoxButtons.OK);
			emailConfSend.Enabled = true;
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
		this.m_confirmationStatusComboBox = new System.Windows.Forms.ComboBox();
		this.m_confirmationStatusLabel = new System.Windows.Forms.Label();
		this.m_commFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.txtCustProfileNum = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtMobilePhoneNumber = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.cbPromptForPerks = new System.Windows.Forms.CheckBox();
		this.cbPromptForEarlyId = new System.Windows.Forms.CheckBox();
		this.cbTextClubEnabled = new System.Windows.Forms.CheckBox();
		this.cbEmailVerified = new System.Windows.Forms.ComboBox();
		this.labelEmailVerified = new System.Windows.Forms.Label();
		this.emailConfirmationApiPanel = new System.Windows.Forms.Panel();
		this.emailConfirmationPanelLabel = new System.Windows.Forms.Label();
		this.emailConfAddressTextbox = new System.Windows.Forms.TextBox();
		this.emailConfirmationEmailLabel = new System.Windows.Forms.Label();
		this.emailConfirmationAccountLabel = new System.Windows.Forms.Label();
		this.emailConfAccountNumberTextbox = new System.Windows.Forms.TextBox();
		this.emailConfSend = new System.Windows.Forms.Button();
		this.emailConfirmationApiPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_confirmationStatusComboBox.FormattingEnabled = true;
		this.m_confirmationStatusComboBox.Location = new System.Drawing.Point(144, 75);
		this.m_confirmationStatusComboBox.Name = "m_confirmationStatusComboBox";
		this.m_confirmationStatusComboBox.Size = new System.Drawing.Size(262, 21);
		this.m_confirmationStatusComboBox.TabIndex = 0;
		this.m_confirmationStatusComboBox.SelectedIndexChanged += new System.EventHandler(OnConfirmationStatusSelectedIndexChanged);
		this.m_confirmationStatusLabel.AutoSize = true;
		this.m_confirmationStatusLabel.Location = new System.Drawing.Point(12, 78);
		this.m_confirmationStatusLabel.Name = "m_confirmationStatusLabel";
		this.m_confirmationStatusLabel.Size = new System.Drawing.Size(101, 13);
		this.m_confirmationStatusLabel.TabIndex = 1;
		this.m_confirmationStatusLabel.Text = "Confirmation Status:";
		this.m_commFailureCheckBox.AutoSize = true;
		this.m_commFailureCheckBox.Location = new System.Drawing.Point(144, 53);
		this.m_commFailureCheckBox.Name = "m_commFailureCheckBox";
		this.m_commFailureCheckBox.Size = new System.Drawing.Size(132, 17);
		this.m_commFailureCheckBox.TabIndex = 2;
		this.m_commFailureCheckBox.Text = "Communication Failure";
		this.m_commFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_commFailureCheckBox.CheckedChanged += new System.EventHandler(OnCommFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(15, 53);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 3;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(519, 38);
		this.m_headerLabel.TabIndex = 41;
		this.m_headerLabel.Text = "  Kiosk Services: Marketing Optin";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.txtCustProfileNum.Location = new System.Drawing.Point(144, 102);
		this.txtCustProfileNum.Name = "txtCustProfileNum";
		this.txtCustProfileNum.Size = new System.Drawing.Size(262, 20);
		this.txtCustProfileNum.TabIndex = 57;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 104);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(126, 13);
		this.label1.TabIndex = 56;
		this.label1.Text = "Customer Profile Number:";
		this.txtMobilePhoneNumber.Location = new System.Drawing.Point(144, 128);
		this.txtMobilePhoneNumber.Name = "txtMobilePhoneNumber";
		this.txtMobilePhoneNumber.Size = new System.Drawing.Size(262, 20);
		this.txtMobilePhoneNumber.TabIndex = 59;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 130);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(115, 13);
		this.label2.TabIndex = 58;
		this.label2.Text = "Mobile Phone Number:";
		this.cbPromptForPerks.AutoSize = true;
		this.cbPromptForPerks.Location = new System.Drawing.Point(144, 180);
		this.cbPromptForPerks.Name = "cbPromptForPerks";
		this.cbPromptForPerks.Size = new System.Drawing.Size(107, 17);
		this.cbPromptForPerks.TabIndex = 60;
		this.cbPromptForPerks.Text = "Prompt For Perks";
		this.cbPromptForPerks.UseVisualStyleBackColor = true;
		this.cbPromptForEarlyId.AutoSize = true;
		this.cbPromptForEarlyId.Location = new System.Drawing.Point(144, 203);
		this.cbPromptForEarlyId.Name = "cbPromptForEarlyId";
		this.cbPromptForEarlyId.Size = new System.Drawing.Size(117, 17);
		this.cbPromptForEarlyId.TabIndex = 61;
		this.cbPromptForEarlyId.Text = "Prompt For Early ID";
		this.cbPromptForEarlyId.UseVisualStyleBackColor = true;
		this.cbTextClubEnabled.AutoSize = true;
		this.cbTextClubEnabled.Location = new System.Drawing.Point(144, 226);
		this.cbTextClubEnabled.Name = "cbTextClubEnabled";
		this.cbTextClubEnabled.Size = new System.Drawing.Size(113, 17);
		this.cbTextClubEnabled.TabIndex = 62;
		this.cbTextClubEnabled.Text = "Text Club Enabled";
		this.cbTextClubEnabled.UseVisualStyleBackColor = true;
		this.cbEmailVerified.FormattingEnabled = true;
		this.cbEmailVerified.Items.AddRange(new object[3] { "null", "True", "False" });
		this.cbEmailVerified.Location = new System.Drawing.Point(144, 154);
		this.cbEmailVerified.Name = "cbEmailVerified";
		this.cbEmailVerified.Size = new System.Drawing.Size(262, 21);
		this.cbEmailVerified.TabIndex = 63;
		this.labelEmailVerified.AutoSize = true;
		this.labelEmailVerified.Location = new System.Drawing.Point(12, 157);
		this.labelEmailVerified.Name = "labelEmailVerified";
		this.labelEmailVerified.Size = new System.Drawing.Size(70, 13);
		this.labelEmailVerified.TabIndex = 64;
		this.labelEmailVerified.Text = "Email Verified";
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfSend);
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfirmationAccountLabel);
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfAccountNumberTextbox);
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfirmationEmailLabel);
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfAddressTextbox);
		this.emailConfirmationApiPanel.Controls.Add(this.emailConfirmationPanelLabel);
		this.emailConfirmationApiPanel.Location = new System.Drawing.Point(28, 249);
		this.emailConfirmationApiPanel.Name = "panel1";
		this.emailConfirmationApiPanel.Size = new System.Drawing.Size(378, 102);
		this.emailConfirmationApiPanel.TabIndex = 65;
		this.emailConfirmationPanelLabel.AutoSize = true;
		this.emailConfirmationPanelLabel.Location = new System.Drawing.Point(3, 3);
		this.emailConfirmationPanelLabel.Name = "label3";
		this.emailConfirmationPanelLabel.Size = new System.Drawing.Size(144, 13);
		this.emailConfirmationPanelLabel.TabIndex = 0;
		this.emailConfirmationPanelLabel.Text = "Email Confirmaton API Tester";
		this.emailConfAddressTextbox.Location = new System.Drawing.Point(99, 19);
		this.emailConfAddressTextbox.Name = "emailConfAddressTextbox";
		this.emailConfAddressTextbox.Size = new System.Drawing.Size(156, 20);
		this.emailConfAddressTextbox.TabIndex = 66;
		this.emailConfirmationEmailLabel.AutoSize = true;
		this.emailConfirmationEmailLabel.Location = new System.Drawing.Point(3, 22);
		this.emailConfirmationEmailLabel.Name = "label4";
		this.emailConfirmationEmailLabel.Size = new System.Drawing.Size(76, 13);
		this.emailConfirmationEmailLabel.TabIndex = 67;
		this.emailConfirmationEmailLabel.Text = "Email Address:";
		this.emailConfirmationAccountLabel.AutoSize = true;
		this.emailConfirmationAccountLabel.Location = new System.Drawing.Point(3, 45);
		this.emailConfirmationAccountLabel.Name = "label5";
		this.emailConfirmationAccountLabel.Size = new System.Drawing.Size(90, 13);
		this.emailConfirmationAccountLabel.TabIndex = 69;
		this.emailConfirmationAccountLabel.Text = "Account Number:";
		this.emailConfAccountNumberTextbox.Location = new System.Drawing.Point(99, 42);
		this.emailConfAccountNumberTextbox.Name = "emailConfAccountNumberTextbox";
		this.emailConfAccountNumberTextbox.Size = new System.Drawing.Size(156, 20);
		this.emailConfAccountNumberTextbox.TabIndex = 68;
		this.emailConfSend.Location = new System.Drawing.Point(274, 19);
		this.emailConfSend.Name = "emailConfSend";
		this.emailConfSend.Size = new System.Drawing.Size(75, 23);
		this.emailConfSend.TabIndex = 70;
		this.emailConfSend.Text = "Send";
		this.emailConfSend.UseVisualStyleBackColor = true;
		this.emailConfSend.Click += new System.EventHandler(OnEmailConfirmationSend);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.emailConfirmationApiPanel);
		base.Controls.Add(this.labelEmailVerified);
		base.Controls.Add(this.cbEmailVerified);
		base.Controls.Add(this.cbTextClubEnabled);
		base.Controls.Add(this.cbPromptForEarlyId);
		base.Controls.Add(this.cbPromptForPerks);
		base.Controls.Add(this.txtMobilePhoneNumber);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.txtCustProfileNum);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_commFailureCheckBox);
		base.Controls.Add(this.m_confirmationStatusLabel);
		base.Controls.Add(this.m_confirmationStatusComboBox);
		base.Name = "MarketingOptinPreferencesPage";
		base.Size = new System.Drawing.Size(519, 382);
		base.Load += new System.EventHandler(MarketingOptinPreferencesPage_Load);
		this.emailConfirmationApiPanel.ResumeLayout(false);
		this.emailConfirmationApiPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
