using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class AuthSvcsPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_enableLocalAccountCachingCheckBox;

	private Button m_removeButton;

	private Label label3;

	private ComboBox m_responseTypeComboBox;

	private Label label2;

	private ListView m_usersListView;

	private ColumnHeader m_userNameColumnHeader;

	private ColumnHeader m_responseTypeColumnHeader;

	private Button m_addButton;

	private TextBox m_userNameTextBox;

	private Label label1;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private ErrorProvider m_errorProvider;

	private Label m_headerLabel;

	public AuthSvcsPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_responseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(AuthenticationResponse));
		foreach (string item in names)
		{
			m_responseTypeComboBox.Items.Add(item);
		}
		m_responseTypeComboBox.SelectedIndex = 0;
		IAuthenticationServices service = ServiceLocator.Instance.GetService<IAuthenticationServices>();
		m_liveCallCheckBox.Checked = service.LiveCall;
		m_simulateServerBusyCheckBox.Checked = service.SimulateServerBusyError;
		m_enableLocalAccountCachingCheckBox.Checked = service.EnableLocalAccountCaching;
		m_simulateCommunicationsFailureCheckBox.Checked = service.SimulateCommunicationsFailure;
		m_usersListView.BeginUpdate();
		m_usersListView.Items.Clear();
		names = service.Users;
		for (int i = 0; i < names.Length; i++)
		{
			string[] array = names[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2)
			{
				ListViewItem listViewItem = new ListViewItem(array[0]);
				listViewItem.SubItems.Add(array[1]);
				m_usersListView.Items.Add(listViewItem);
			}
		}
		m_usersListView.EndUpdate();
		m_isLoaded = true;
	}

	private void OnAdd(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_userNameTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_userNameTextBox.Text))
		{
			m_errorProvider.SetError(m_userNameTextBox, "User Name is a required value.");
			return;
		}
		ListViewItem listViewItem = new ListViewItem(m_userNameTextBox.Text);
		listViewItem.SubItems.Add(m_responseTypeComboBox.SelectedItem.ToString());
		m_usersListView.Items.Add(listViewItem);
		SaveUsers();
		m_userNameTextBox.Clear();
	}

	private void OnRemove(object sender, EventArgs e)
	{
		if (m_usersListView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_usersListView.SelectedItems)
		{
			m_usersListView.Items.Remove(selectedItem);
		}
		SaveUsers();
	}

	private void OnSelectedUserChanged(object sender, EventArgs e)
	{
		m_removeButton.Enabled = m_usersListView.SelectedItems.Count > 0;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IAuthenticationServices>().LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IAuthenticationServices>().SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IAuthenticationServices>().SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void SaveUsers()
	{
		List<string> list = new List<string>();
		foreach (ListViewItem item in m_usersListView.Items)
		{
			list.Add($"{item.SubItems[0].Text}|{item.SubItems[1].Text}");
		}
		ServiceLocator.Instance.GetService<IAuthenticationServices>().Users = list.ToArray();
	}

	private void OnEnableLocalAccountCachingCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IAuthenticationServices>().EnableLocalAccountCaching = m_enableLocalAccountCachingCheckBox.Checked;
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
		this.m_enableLocalAccountCachingCheckBox = new System.Windows.Forms.CheckBox();
		this.m_removeButton = new System.Windows.Forms.Button();
		this.label3 = new System.Windows.Forms.Label();
		this.m_responseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_usersListView = new System.Windows.Forms.ListView();
		this.m_userNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_responseTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_addButton = new System.Windows.Forms.Button();
		this.m_userNameTextBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_enableLocalAccountCachingCheckBox.AutoSize = true;
		this.m_enableLocalAccountCachingCheckBox.Location = new System.Drawing.Point(18, 78);
		this.m_enableLocalAccountCachingCheckBox.Name = "m_enableLocalAccountCachingCheckBox";
		this.m_enableLocalAccountCachingCheckBox.Size = new System.Drawing.Size(167, 17);
		this.m_enableLocalAccountCachingCheckBox.TabIndex = 35;
		this.m_enableLocalAccountCachingCheckBox.Text = "Enable local account caching";
		this.m_enableLocalAccountCachingCheckBox.UseVisualStyleBackColor = true;
		this.m_enableLocalAccountCachingCheckBox.CheckedChanged += new System.EventHandler(OnEnableLocalAccountCachingCheckChanged);
		this.m_removeButton.Enabled = false;
		this.m_removeButton.Location = new System.Drawing.Point(18, 471);
		this.m_removeButton.Name = "m_removeButton";
		this.m_removeButton.Size = new System.Drawing.Size(75, 23);
		this.m_removeButton.TabIndex = 34;
		this.m_removeButton.Text = "Remove";
		this.m_removeButton.UseVisualStyleBackColor = true;
		this.m_removeButton.Leave += new System.EventHandler(OnRemove);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(15, 230);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(37, 13);
		this.label3.TabIndex = 33;
		this.label3.Text = "Users:";
		this.m_responseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_responseTypeComboBox.FormattingEnabled = true;
		this.m_responseTypeComboBox.Location = new System.Drawing.Point(113, 202);
		this.m_responseTypeComboBox.Name = "m_responseTypeComboBox";
		this.m_responseTypeComboBox.Size = new System.Drawing.Size(284, 21);
		this.m_responseTypeComboBox.TabIndex = 32;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 205);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(85, 13);
		this.label2.TabIndex = 31;
		this.label2.Text = "Response Type:";
		this.m_usersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.m_userNameColumnHeader, this.m_responseTypeColumnHeader });
		this.m_usersListView.FullRowSelect = true;
		this.m_usersListView.Location = new System.Drawing.Point(18, 246);
		this.m_usersListView.Name = "m_usersListView";
		this.m_usersListView.Size = new System.Drawing.Size(506, 219);
		this.m_usersListView.TabIndex = 30;
		this.m_usersListView.UseCompatibleStateImageBehavior = false;
		this.m_usersListView.View = System.Windows.Forms.View.Details;
		this.m_usersListView.SelectedIndexChanged += new System.EventHandler(OnSelectedUserChanged);
		this.m_userNameColumnHeader.Text = "User Name";
		this.m_userNameColumnHeader.Width = 150;
		this.m_responseTypeColumnHeader.Text = "Response Type";
		this.m_responseTypeColumnHeader.Width = 200;
		this.m_addButton.Location = new System.Drawing.Point(403, 200);
		this.m_addButton.Name = "m_addButton";
		this.m_addButton.Size = new System.Drawing.Size(75, 23);
		this.m_addButton.TabIndex = 29;
		this.m_addButton.Text = "Add";
		this.m_addButton.UseVisualStyleBackColor = true;
		this.m_addButton.Leave += new System.EventHandler(OnAdd);
		this.m_userNameTextBox.Location = new System.Drawing.Point(113, 176);
		this.m_userNameTextBox.Name = "m_userNameTextBox";
		this.m_userNameTextBox.Size = new System.Drawing.Size(284, 20);
		this.m_userNameTextBox.TabIndex = 28;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(44, 179);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(63, 13);
		this.label1.TabIndex = 27;
		this.label1.Text = "User Name:";
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(18, 124);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 26;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(18, 101);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 25;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(18, 55);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 24;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 36;
		this.m_headerLabel.Text = "  Kiosk Services: User Authentication";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_enableLocalAccountCachingCheckBox);
		base.Controls.Add(this.m_removeButton);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_responseTypeComboBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_usersListView);
		base.Controls.Add(this.m_addButton);
		base.Controls.Add(this.m_userNameTextBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "AuthSvcsPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
