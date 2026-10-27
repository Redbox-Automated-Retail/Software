using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.KernelExtension;

public class PromotionServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private TextBox m_amountTextBox;

	private Label label4;

	private CheckBox m_simulateServerBusyCheckBox;

	private ErrorProvider m_errorProvider;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private Button m_removeButton;

	private Button m_addButton;

	private ComboBox m_responseTypeComboBox;

	private Label label3;

	private TextBox m_codeTextBox;

	private ListView m_promoCodeListView;

	private ColumnHeader m_codeColumnHeader;

	private ColumnHeader m_amountColumnHeader;

	private ColumnHeader m_responseTypeColumnHeader;

	private CheckBox m_liveCallCheckBox;

	private Label label2;

	private Label label1;

	private Label label5;

	private ComboBox m_promoIntentComboBox;

	private ColumnHeader m_promoIntentColumnHeader;

	private Label m_headerLabel;

	public PromotionServicesPreferencePage()
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
		string[] names = Enum.GetNames(typeof(PromotionValidationResponse));
		foreach (string item in names)
		{
			m_responseTypeComboBox.Items.Add(item);
		}
		m_promoIntentComboBox.Items.Clear();
		names = Enum.GetNames(typeof(PromotionIntentCode));
		foreach (string item2 in names)
		{
			m_promoIntentComboBox.Items.Add(item2);
		}
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		m_responseTypeComboBox.SelectedIndex = 0;
		m_promoIntentComboBox.SelectedIndex = 0;
		m_liveCallCheckBox.Checked = service.LiveCall;
		m_simulateServerBusyCheckBox.Checked = service.SimulateServerBusyError;
		m_simulateCommunicationsFailureCheckBox.Checked = service.SimulateCommunicationsFailure;
		m_promoCodeListView.BeginUpdate();
		m_promoCodeListView.Items.Clear();
		names = service.PromoCodes;
		for (int i = 0; i < names.Length; i++)
		{
			string[] array = names[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 4)
			{
				ListViewItem listViewItem = new ListViewItem(array[0]);
				listViewItem.SubItems.Add(array[1]);
				listViewItem.SubItems.Add(array[2]);
				listViewItem.SubItems.Add(array[3]);
				m_promoCodeListView.Items.Add(listViewItem);
			}
		}
		m_promoCodeListView.EndUpdate();
		m_isLoaded = true;
	}

	private void OnAdd(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_codeTextBox, string.Empty);
		m_errorProvider.SetError(m_amountTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_codeTextBox.Text))
		{
			m_errorProvider.SetError(m_codeTextBox, "Promotion Code is a required value.");
			return;
		}
		if (string.IsNullOrEmpty(m_amountTextBox.Text) || !decimal.TryParse(m_amountTextBox.Text, out var _))
		{
			m_errorProvider.SetError(m_amountTextBox, "Amount is a required value; it must be a valid decimal value.");
			return;
		}
		ListViewItem listViewItem = new ListViewItem(m_codeTextBox.Text);
		listViewItem.SubItems.Add(m_amountTextBox.Text);
		listViewItem.SubItems.Add(m_promoIntentComboBox.SelectedItem.ToString());
		listViewItem.SubItems.Add(m_responseTypeComboBox.SelectedItem.ToString());
		m_promoCodeListView.Items.Add(listViewItem);
		SavePromoCodes();
		m_codeTextBox.Clear();
		m_amountTextBox.Clear();
		m_codeTextBox.Focus();
	}

	private void OnRemove(object sender, EventArgs e)
	{
		if (m_promoCodeListView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_promoCodeListView.SelectedItems)
		{
			m_promoCodeListView.Items.Remove(selectedItem);
		}
		SavePromoCodes();
	}

	private void OnSelectedPromoCodeChanged(object sender, EventArgs e)
	{
		m_removeButton.Enabled = m_promoCodeListView.SelectedItems.Count > 0;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IPromotionServices>().LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IPromotionServices>().SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			ServiceLocator.Instance.GetService<IPromotionServices>().SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void SavePromoCodes()
	{
		List<string> list = new List<string>();
		foreach (ListViewItem item in m_promoCodeListView.Items)
		{
			list.Add($"{item.SubItems[0].Text}|{item.SubItems[1].Text}|{item.SubItems[2].Text}|{item.SubItems[3].Text}");
		}
		ServiceLocator.Instance.GetService<IPromotionServices>().PromoCodes = list.ToArray();
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
		this.m_amountTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_removeButton = new System.Windows.Forms.Button();
		this.m_addButton = new System.Windows.Forms.Button();
		this.m_responseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_codeTextBox = new System.Windows.Forms.TextBox();
		this.m_codeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_promoCodeListView = new System.Windows.Forms.ListView();
		this.m_amountColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_promoIntentColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_responseTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.m_promoIntentComboBox = new System.Windows.Forms.ComboBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_amountTextBox.Location = new System.Drawing.Point(114, 164);
		this.m_amountTextBox.Name = "m_amountTextBox";
		this.m_amountTextBox.Size = new System.Drawing.Size(80, 20);
		this.m_amountTextBox.TabIndex = 6;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(64, 168);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(46, 13);
		this.label4.TabIndex = 5;
		this.label4.Text = "Amount:";
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(28, 100);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 2;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(28, 77);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 1;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureCheckChanged);
		this.m_removeButton.Enabled = false;
		this.m_removeButton.Location = new System.Drawing.Point(21, 469);
		this.m_removeButton.Name = "m_removeButton";
		this.m_removeButton.Size = new System.Drawing.Size(75, 23);
		this.m_removeButton.TabIndex = 12;
		this.m_removeButton.Text = "Remove";
		this.m_removeButton.UseVisualStyleBackColor = true;
		this.m_removeButton.Click += new System.EventHandler(OnRemove);
		this.m_addButton.Location = new System.Drawing.Point(380, 216);
		this.m_addButton.Name = "m_addButton";
		this.m_addButton.Size = new System.Drawing.Size(75, 23);
		this.m_addButton.TabIndex = 9;
		this.m_addButton.Text = "Add";
		this.m_addButton.UseVisualStyleBackColor = true;
		this.m_addButton.Click += new System.EventHandler(OnAdd);
		this.m_responseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_responseTypeComboBox.FormattingEnabled = true;
		this.m_responseTypeComboBox.Location = new System.Drawing.Point(114, 216);
		this.m_responseTypeComboBox.Name = "m_responseTypeComboBox";
		this.m_responseTypeComboBox.Size = new System.Drawing.Size(260, 21);
		this.m_responseTypeComboBox.TabIndex = 8;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(23, 219);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(85, 13);
		this.label3.TabIndex = 7;
		this.label3.Text = "Response Type:";
		this.m_codeTextBox.Location = new System.Drawing.Point(114, 139);
		this.m_codeTextBox.MaxLength = 64;
		this.m_codeTextBox.Name = "m_codeTextBox";
		this.m_codeTextBox.Size = new System.Drawing.Size(148, 20);
		this.m_codeTextBox.TabIndex = 4;
		this.m_codeColumnHeader.Text = "Code";
		this.m_codeColumnHeader.Width = 115;
		this.m_promoCodeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_codeColumnHeader, this.m_amountColumnHeader, this.m_promoIntentColumnHeader, this.m_responseTypeColumnHeader });
		this.m_promoCodeListView.FullRowSelect = true;
		this.m_promoCodeListView.Location = new System.Drawing.Point(21, 267);
		this.m_promoCodeListView.Name = "m_promoCodeListView";
		this.m_promoCodeListView.Size = new System.Drawing.Size(499, 196);
		this.m_promoCodeListView.TabIndex = 11;
		this.m_promoCodeListView.UseCompatibleStateImageBehavior = false;
		this.m_promoCodeListView.View = System.Windows.Forms.View.Details;
		this.m_promoCodeListView.SelectedIndexChanged += new System.EventHandler(OnSelectedPromoCodeChanged);
		this.m_amountColumnHeader.Text = "Amount";
		this.m_amountColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_amountColumnHeader.Width = 75;
		this.m_promoIntentColumnHeader.Text = "Intent";
		this.m_promoIntentColumnHeader.Width = 130;
		this.m_responseTypeColumnHeader.Text = "Response Type";
		this.m_responseTypeColumnHeader.Width = 150;
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(28, 54);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(75, 142);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(35, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Code:";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(18, 250);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(90, 13);
		this.label1.TabIndex = 10;
		this.label1.Text = "Promotion Codes:";
		this.m_promoIntentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_promoIntentComboBox.FormattingEnabled = true;
		this.m_promoIntentComboBox.Location = new System.Drawing.Point(114, 189);
		this.m_promoIntentComboBox.Name = "m_promoIntentComboBox";
		this.m_promoIntentComboBox.Size = new System.Drawing.Size(260, 21);
		this.m_promoIntentComboBox.TabIndex = 13;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(23, 197);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(70, 13);
		this.label5.TabIndex = 14;
		this.label5.Text = "Promo Intent:";
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 42;
		this.m_headerLabel.Text = "  Kiosk Services: Promotion Authorization";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_promoIntentComboBox);
		base.Controls.Add(this.m_amountTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_removeButton);
		base.Controls.Add(this.m_addButton);
		base.Controls.Add(this.m_responseTypeComboBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_codeTextBox);
		base.Controls.Add(this.m_promoCodeListView);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Name = "PromotionServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
