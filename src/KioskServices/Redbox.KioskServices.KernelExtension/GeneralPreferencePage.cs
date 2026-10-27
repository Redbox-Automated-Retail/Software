using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private readonly IStoreServices m_storeServices;

	private bool m_isLoaded;

	private IContainer components;

	private TextBox m_kioskServicesTimeoutTextBox;

	private Label label4;

	private TextBox m_kioskServicesUrlTextBox;

	private Label label5;

	private ComboBox m_syncTypeComboBox;

	private CheckBox m_runInitialSyncCheckBox;

	private CheckBox m_standAloneModeCheckBox;

	private ErrorProvider m_errorProvider;

	private TextBox m_storeNumberTextBox;

	private Label label2;

	private CheckBox m_restfulCallCheckBox;

	private CheckBox m_synchronizeTimeToServerCheckBox;

	private Label lblCurrentVersions;

	private ListView m_currentVersionsListView;

	private ColumnHeader applicationName;

	private ColumnHeader versionNumber;

	private Label m_headerLabel;

	public GeneralPreferencePage()
	{
		InitializeComponent();
		m_storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_kioskServicesUrlTextBox.Text = KioskServicesProxy.Instance.Url;
		m_storeNumberTextBox.Text = m_storeServices.StoreNumberString;
		m_standAloneModeCheckBox.Checked = m_storeServices.IsStandAlone;
		m_kioskServicesTimeoutTextBox.Text = KioskServicesProxy.Instance.Timeout.ToString();
		m_runInitialSyncCheckBox.Checked = m_storeServices.RunInitialSync;
		m_restfulCallCheckBox.Checked = KioskServicesProxy.Instance.Protocol == KioskServicesProtocol.REST;
		m_syncTypeComboBox.SelectedIndex = m_syncTypeComboBox.FindStringExact(m_storeServices.InitialSyncType);
		if (m_syncTypeComboBox.SelectedIndex == -1)
		{
			m_syncTypeComboBox.SelectedIndex = 0;
		}
		m_synchronizeTimeToServerCheckBox.Checked = m_storeServices.SynchronizeTimeToServer;
		m_currentVersionsListView.BeginUpdate();
		m_currentVersionsListView.Items.Clear();
		foreach (KeyValuePair<string, string> currentVersion in KioskServicesProxy.Instance.GetCurrentVersions())
		{
			ListViewItem listViewItem = new ListViewItem(currentVersion.Key);
			listViewItem.SubItems.Add(currentVersion.Value);
			m_currentVersionsListView.Items.Add(listViewItem);
		}
		m_currentVersionsListView.EndUpdate();
		m_isLoaded = true;
	}

	private void OnStandAloneModeCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_storeServices.IsStandAlone = m_standAloneModeCheckBox.Checked;
		}
	}

	private void OnLeaveStoreNumberTextBox(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_storeServices.StoreNumberString = m_storeNumberTextBox.Text;
		}
	}

	private void OnLeaveKioskServicesUrlTextBox(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			string url = KioskServicesProxy.Instance.Url;
			if (!string.IsNullOrEmpty(m_kioskServicesUrlTextBox.Text))
			{
				url = m_kioskServicesUrlTextBox.Text;
			}
			KioskServicesProxy.Instance.Url = url;
		}
	}

	private void OnLeaveKioskServicesTimeoutTextBox(object sender, EventArgs e)
	{
		if (m_isLoaded && int.TryParse(m_kioskServicesTimeoutTextBox.Text, out var result))
		{
			KioskServicesProxy.Instance.Timeout = result;
		}
	}

	private void OnRunInitialSyncCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_storeServices.RunInitialSync = m_runInitialSyncCheckBox.Checked;
			m_syncTypeComboBox.Enabled = m_runInitialSyncCheckBox.Checked;
		}
	}

	private void OnSelectedSyncTypeChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_storeServices.InitialSyncType = m_syncTypeComboBox.SelectedItem.ToString();
		}
	}

	private void OnRestfulCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			KioskServicesProxy.Instance.Protocol = ((!m_restfulCallCheckBox.Checked) ? KioskServicesProtocol.Remoting : KioskServicesProtocol.REST);
		}
	}

	private void OnSynchronizeTimeToServerCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_storeServices.SynchronizeTimeToServer = m_synchronizeTimeToServerCheckBox.Checked;
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
		this.m_kioskServicesTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_kioskServicesUrlTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_syncTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_runInitialSyncCheckBox = new System.Windows.Forms.CheckBox();
		this.m_standAloneModeCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_storeNumberTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_restfulCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_synchronizeTimeToServerCheckBox = new System.Windows.Forms.CheckBox();
		this.m_currentVersionsListView = new System.Windows.Forms.ListView();
		this.applicationName = new System.Windows.Forms.ColumnHeader();
		this.versionNumber = new System.Windows.Forms.ColumnHeader();
		this.lblCurrentVersions = new System.Windows.Forms.Label();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_kioskServicesTimeoutTextBox.Location = new System.Drawing.Point(106, 86);
		this.m_kioskServicesTimeoutTextBox.MaxLength = 6;
		this.m_kioskServicesTimeoutTextBox.Name = "m_kioskServicesTimeoutTextBox";
		this.m_kioskServicesTimeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_kioskServicesTimeoutTextBox.TabIndex = 3;
		this.m_kioskServicesTimeoutTextBox.Leave += new System.EventHandler(OnLeaveKioskServicesTimeoutTextBox);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(13, 89);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(87, 13);
		this.label4.TabIndex = 2;
		this.label4.Text = "Service Timeout:";
		this.m_kioskServicesUrlTextBox.Location = new System.Drawing.Point(106, 60);
		this.m_kioskServicesUrlTextBox.Name = "m_kioskServicesUrlTextBox";
		this.m_kioskServicesUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_kioskServicesUrlTextBox.TabIndex = 1;
		this.m_kioskServicesUrlTextBox.Leave += new System.EventHandler(OnLeaveKioskServicesUrlTextBox);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(29, 63);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(71, 13);
		this.label5.TabIndex = 0;
		this.label5.Text = "Service URL:";
		this.m_syncTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_syncTypeComboBox.Enabled = false;
		this.m_syncTypeComboBox.FormattingEnabled = true;
		this.m_syncTypeComboBox.Items.AddRange(new object[2] { "Foreground", "Background" });
		this.m_syncTypeComboBox.Location = new System.Drawing.Point(212, 178);
		this.m_syncTypeComboBox.Name = "m_syncTypeComboBox";
		this.m_syncTypeComboBox.Size = new System.Drawing.Size(169, 21);
		this.m_syncTypeComboBox.TabIndex = 8;
		this.m_syncTypeComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedSyncTypeChanged);
		this.m_runInitialSyncCheckBox.AutoSize = true;
		this.m_runInitialSyncCheckBox.Location = new System.Drawing.Point(106, 178);
		this.m_runInitialSyncCheckBox.Name = "m_runInitialSyncCheckBox";
		this.m_runInitialSyncCheckBox.Size = new System.Drawing.Size(100, 17);
		this.m_runInitialSyncCheckBox.TabIndex = 7;
		this.m_runInitialSyncCheckBox.Text = "Run Initial Sync";
		this.m_runInitialSyncCheckBox.UseVisualStyleBackColor = true;
		this.m_runInitialSyncCheckBox.Click += new System.EventHandler(OnRunInitialSyncCheckChanged);
		this.m_standAloneModeCheckBox.AutoSize = true;
		this.m_standAloneModeCheckBox.Location = new System.Drawing.Point(106, 205);
		this.m_standAloneModeCheckBox.Name = "m_standAloneModeCheckBox";
		this.m_standAloneModeCheckBox.Size = new System.Drawing.Size(114, 17);
		this.m_standAloneModeCheckBox.TabIndex = 9;
		this.m_standAloneModeCheckBox.Text = "Stand Alone Mode";
		this.m_standAloneModeCheckBox.UseVisualStyleBackColor = true;
		this.m_standAloneModeCheckBox.CheckedChanged += new System.EventHandler(OnStandAloneModeCheckChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_storeNumberTextBox.Location = new System.Drawing.Point(106, 152);
		this.m_storeNumberTextBox.Name = "m_storeNumberTextBox";
		this.m_storeNumberTextBox.Size = new System.Drawing.Size(277, 20);
		this.m_storeNumberTextBox.TabIndex = 6;
		this.m_storeNumberTextBox.Leave += new System.EventHandler(OnLeaveStoreNumberTextBox);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(25, 155);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(75, 13);
		this.label2.TabIndex = 5;
		this.label2.Text = "Store Number:";
		this.m_restfulCallCheckBox.AutoSize = true;
		this.m_restfulCallCheckBox.Location = new System.Drawing.Point(106, 112);
		this.m_restfulCallCheckBox.Name = "m_restfulCallCheckBox";
		this.m_restfulCallCheckBox.Size = new System.Drawing.Size(86, 17);
		this.m_restfulCallCheckBox.TabIndex = 4;
		this.m_restfulCallCheckBox.Text = "RESTful Call";
		this.m_restfulCallCheckBox.UseVisualStyleBackColor = true;
		this.m_restfulCallCheckBox.CheckedChanged += new System.EventHandler(OnRestfulCallCheckChanged);
		this.m_synchronizeTimeToServerCheckBox.AutoSize = true;
		this.m_synchronizeTimeToServerCheckBox.Location = new System.Drawing.Point(106, 229);
		this.m_synchronizeTimeToServerCheckBox.Name = "m_synchronizeTimeToServerCheckBox";
		this.m_synchronizeTimeToServerCheckBox.Size = new System.Drawing.Size(160, 17);
		this.m_synchronizeTimeToServerCheckBox.TabIndex = 10;
		this.m_synchronizeTimeToServerCheckBox.Text = "Synchronize Time To Server";
		this.m_synchronizeTimeToServerCheckBox.UseVisualStyleBackColor = true;
		this.m_synchronizeTimeToServerCheckBox.CheckedChanged += new System.EventHandler(OnSynchronizeTimeToServerCheckChanged);
		this.m_currentVersionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.applicationName, this.versionNumber });
		this.m_currentVersionsListView.FullRowSelect = true;
		this.m_currentVersionsListView.Location = new System.Drawing.Point(106, 274);
		this.m_currentVersionsListView.Name = "m_currentVersionsListView";
		this.m_currentVersionsListView.Size = new System.Drawing.Size(399, 216);
		this.m_currentVersionsListView.TabIndex = 11;
		this.m_currentVersionsListView.UseCompatibleStateImageBehavior = false;
		this.m_currentVersionsListView.View = System.Windows.Forms.View.Details;
		this.applicationName.Text = "Name";
		this.applicationName.Width = 189;
		this.versionNumber.Text = "Version";
		this.versionNumber.Width = 189;
		this.lblCurrentVersions.AutoSize = true;
		this.lblCurrentVersions.Location = new System.Drawing.Point(13, 274);
		this.lblCurrentVersions.Name = "lblCurrentVersions";
		this.lblCurrentVersions.Size = new System.Drawing.Size(87, 13);
		this.lblCurrentVersions.TabIndex = 12;
		this.lblCurrentVersions.Text = "Current Versions:";
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 38;
		this.m_headerLabel.Text = "  Kiosk Services: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.lblCurrentVersions);
		base.Controls.Add(this.m_currentVersionsListView);
		base.Controls.Add(this.m_synchronizeTimeToServerCheckBox);
		base.Controls.Add(this.m_restfulCallCheckBox);
		base.Controls.Add(this.m_storeNumberTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_syncTypeComboBox);
		base.Controls.Add(this.m_runInitialSyncCheckBox);
		base.Controls.Add(this.m_standAloneModeCheckBox);
		base.Controls.Add(this.m_kioskServicesTimeoutTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_kioskServicesUrlTextBox);
		base.Controls.Add(this.label5);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
