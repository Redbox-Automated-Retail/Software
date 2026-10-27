using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.UpdateServices.KernelExtension;

public class ConfigPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_loading;

	private IContainer components;

	private CheckBox m_checkboxActiveOnly;

	private ComboBox m_configFileComboBox;

	private Label label1;

	private Label label2;

	private Label m_headerLabel;

	private CheckedListBox m_categoriesListBox;

	private ListView m_configValuesListView;

	private Label m_valuesLabel;

	private ColumnHeader m_categoryColumn;

	private ColumnHeader m_settingNameColumn;

	private ColumnHeader m_settingValueColumn;

	private ColumnHeader m_effDateColumn;

	private ColumnHeader m_expDateColumn;

	private ColumnHeader m_dataTypeColumn;

	public ConfigPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		preferencePage.SetValue("SelectedConfig", m_configFileComboBox.Text ?? string.Empty);
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_loading = true;
		m_configFileComboBox.Items.Clear();
		string value = preferencePage.GetValue("SelectedConfig", string.Empty);
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service == null)
		{
			return;
		}
		List<ConfigurationClientItem> items = service.GetItems();
		foreach (ConfigurationClientItem item in items)
		{
			m_configFileComboBox.Items.Add(item.Key);
		}
		if (items.Count == 1)
		{
			m_configFileComboBox.SelectedIndex = 0;
		}
		else if (value != string.Empty && m_configFileComboBox.Items.Contains(value))
		{
			m_configFileComboBox.Text = value;
		}
		_ = (string)m_configFileComboBox.SelectedItem;
		RefreshCategories();
		m_loading = false;
		RefreshSettings();
	}

	private void RefreshCategories()
	{
		m_categoriesListBox.Items.Clear();
		m_categoriesListBox.SuspendLayout();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null)
		{
			string text = (string)m_configFileComboBox.SelectedItem;
			if (text != null && text != string.Empty)
			{
				List<string> list = new List<string>();
				if (service.GetCategoryNames(text, out list))
				{
					foreach (string item in list)
					{
						m_categoriesListBox.Items.Add(item, isChecked: true);
					}
				}
			}
		}
		m_categoriesListBox.ResumeLayout();
	}

	private void RefreshSettings()
	{
		if (m_loading)
		{
			return;
		}
		m_configValuesListView.Items.Clear();
		m_configValuesListView.SuspendLayout();
		string text = (string)m_configFileComboBox.SelectedItem;
		if (text != null && text != string.Empty)
		{
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (service != null)
			{
				foreach (string checkedItem in m_categoriesListBox.CheckedItems)
				{
					List<string> list = new List<string>();
					service.GetSettingNames(text, checkedItem, out list);
					foreach (string item in list)
					{
						if (m_checkboxActiveOnly.Checked)
						{
							ConfigurationData value = new ConfigurationData();
							if (service.TryGetConfigurationData(text, checkedItem, item, out value))
							{
								m_configValuesListView.Items.Add(CreateListItem(value));
							}
							continue;
						}
						List<ConfigurationData> list2 = new List<ConfigurationData>();
						service.GetAllConfigurationData(text, checkedItem, item, out list2);
						foreach (ConfigurationData item2 in list2)
						{
							m_configValuesListView.Items.Add(CreateListItem(item2));
						}
					}
				}
			}
		}
		m_configValuesListView.ResumeLayout();
	}

	private ListViewItem CreateListItem(ConfigurationData data)
	{
		return new ListViewItem(new string[6]
		{
			data.CategoryName,
			data.SettingName,
			data.SettingDataType,
			(data.Value.GetType() == typeof(string)) ? data.Value : "(Object)",
			data.EffectiveTime.ToString("dd-MMM-yyyy"),
			(!data.ExpireTime.HasValue) ? string.Empty : data.ExpireTime.Value.ToString("dd-MMM-yyyy")
		});
	}

	private void m_configFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		RefreshCategories();
		RefreshSettings();
	}

	private void m_categoriesListBox_SelectedValueChanged(object sender, EventArgs e)
	{
		RefreshSettings();
	}

	private void m_checkboxActiveOnly_CheckedChanged(object sender, EventArgs e)
	{
		RefreshSettings();
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
		this.m_checkboxActiveOnly = new System.Windows.Forms.CheckBox();
		this.m_configFileComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_categoriesListBox = new System.Windows.Forms.CheckedListBox();
		this.m_configValuesListView = new System.Windows.Forms.ListView();
		this.m_categoryColumn = new System.Windows.Forms.ColumnHeader();
		this.m_settingNameColumn = new System.Windows.Forms.ColumnHeader();
		this.m_settingValueColumn = new System.Windows.Forms.ColumnHeader();
		this.m_effDateColumn = new System.Windows.Forms.ColumnHeader();
		this.m_expDateColumn = new System.Windows.Forms.ColumnHeader();
		this.m_valuesLabel = new System.Windows.Forms.Label();
		this.m_dataTypeColumn = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.m_checkboxActiveOnly.AutoSize = true;
		this.m_checkboxActiveOnly.Checked = true;
		this.m_checkboxActiveOnly.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_checkboxActiveOnly.Location = new System.Drawing.Point(414, 60);
		this.m_checkboxActiveOnly.Name = "m_checkboxActiveOnly";
		this.m_checkboxActiveOnly.Size = new System.Drawing.Size(110, 17);
		this.m_checkboxActiveOnly.TabIndex = 0;
		this.m_checkboxActiveOnly.Text = "Show Active Only";
		this.m_checkboxActiveOnly.UseVisualStyleBackColor = true;
		this.m_checkboxActiveOnly.CheckedChanged += new System.EventHandler(m_checkboxActiveOnly_CheckedChanged);
		this.m_configFileComboBox.FormattingEnabled = true;
		this.m_configFileComboBox.Location = new System.Drawing.Point(75, 57);
		this.m_configFileComboBox.Name = "m_configFileComboBox";
		this.m_configFileComboBox.Size = new System.Drawing.Size(297, 21);
		this.m_configFileComboBox.TabIndex = 1;
		this.m_configFileComboBox.SelectedIndexChanged += new System.EventHandler(m_configFileComboBox_SelectedIndexChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(13, 60);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(56, 13);
		this.label1.TabIndex = 3;
		this.label1.Text = "Config File";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(13, 96);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(57, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Categories";
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.IndianRed;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 5;
		this.m_headerLabel.Text = "  Update Service: Configuration";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_categoriesListBox.FormattingEnabled = true;
		this.m_categoriesListBox.Location = new System.Drawing.Point(77, 96);
		this.m_categoriesListBox.Name = "m_categoriesListBox";
		this.m_categoriesListBox.Size = new System.Drawing.Size(295, 109);
		this.m_categoriesListBox.TabIndex = 6;
		this.m_categoriesListBox.SelectedValueChanged += new System.EventHandler(m_categoriesListBox_SelectedValueChanged);
		this.m_configValuesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[6] { this.m_categoryColumn, this.m_settingNameColumn, this.m_dataTypeColumn, this.m_settingValueColumn, this.m_effDateColumn, this.m_expDateColumn });
		this.m_configValuesListView.FullRowSelect = true;
		this.m_configValuesListView.GridLines = true;
		this.m_configValuesListView.HideSelection = false;
		this.m_configValuesListView.Location = new System.Drawing.Point(16, 237);
		this.m_configValuesListView.Name = "m_configValuesListView";
		this.m_configValuesListView.Size = new System.Drawing.Size(508, 244);
		this.m_configValuesListView.TabIndex = 7;
		this.m_configValuesListView.UseCompatibleStateImageBehavior = false;
		this.m_configValuesListView.View = System.Windows.Forms.View.Details;
		this.m_categoryColumn.Text = "Category";
		this.m_categoryColumn.Width = 74;
		this.m_settingNameColumn.Text = "Setting Name";
		this.m_settingNameColumn.Width = 96;
		this.m_settingValueColumn.Text = "Setting Value";
		this.m_settingValueColumn.Width = 108;
		this.m_effDateColumn.Text = "Eff Date";
		this.m_effDateColumn.Width = 73;
		this.m_expDateColumn.Text = "Exp Date";
		this.m_expDateColumn.Width = 71;
		this.m_valuesLabel.AutoSize = true;
		this.m_valuesLabel.Location = new System.Drawing.Point(16, 221);
		this.m_valuesLabel.Name = "m_valuesLabel";
		this.m_valuesLabel.Size = new System.Drawing.Size(104, 13);
		this.m_valuesLabel.TabIndex = 8;
		this.m_valuesLabel.Text = "Configuration Values";
		this.m_dataTypeColumn.Text = "Type";
		this.m_dataTypeColumn.Width = 64;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_valuesLabel);
		base.Controls.Add(this.m_configValuesListView);
		base.Controls.Add(this.m_categoriesListBox);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_configFileComboBox);
		base.Controls.Add(this.m_checkboxActiveOnly);
		base.Name = "ConfigPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
