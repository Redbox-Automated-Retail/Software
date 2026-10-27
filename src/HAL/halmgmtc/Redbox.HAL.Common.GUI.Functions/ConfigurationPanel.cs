using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class ConfigurationPanel : UserControl
{
	private DataGridViewCellStyle m_valueChangedStyle;

	private DataGridViewCellStyle m_defaultStyle;

	private List<ConfigItem> m_controllerConfig = new List<ConfigItem>();

	private List<ConfigItem> m_cameraConfig = new List<ConfigItem>();

	private List<ConfigItem> m_barcodeConfig = new List<ConfigItem>();

	private IContainer components;

	private ListBox m_configItemListBox;

	private DataGridView m_propertyDataGridView;

	private DataGridViewTextBoxColumn Property;

	private DataGridViewTextBoxColumn Value;

	private TextBox m_descriptionTB;

	private bool IsDirty { get; set; }

	public ConfigurationPanel()
	{
		InitializeComponent();
		m_defaultStyle = m_propertyDataGridView.DefaultCellStyle;
		m_valueChangedStyle = new DataGridViewCellStyle
		{
			Font = new Font(Font.Name, Font.Size, FontStyle.Bold)
		};
		m_configItemListBox.SelectedIndexChanged += OnListBoxSelectionChanged;
		m_propertyDataGridView.CellEndEdit += OnDataChanged;
		m_propertyDataGridView.SelectionChanged += OnConfigPropertiesItemChanged;
		m_propertyDataGridView.CellClick += OnButtonClick;
		LoadConfiguration();
		Dock = DockStyle.Fill;
	}

	private void LoadConfiguration()
	{
	}

	private void OnConfigPropertiesItemChanged(object sender, EventArgs e)
	{
		ConfigPropertiesItemChangedInner();
	}

	private void SetStyle(DataGridViewCell cell)
	{
		if (cell != null && cell.Tag is ConfigItem { Value: not null, DefaultValue: not null } configItem)
		{
			if (configItem.DefaultValue.Equals(configItem.Value))
			{
				cell.Style = m_defaultStyle;
			}
			else
			{
				cell.Style = m_valueChangedStyle;
			}
		}
	}

	private void OnDataChanged(object sender, DataGridViewCellEventArgs e)
	{
		DataGridViewCell dataGridViewCell = m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
		ConfigItem configItem = (ConfigItem)dataGridViewCell.Tag;
		if (configItem != null)
		{
			object obj = dataGridViewCell.Value;
			if (configItem.ItemType == typeof(bool))
			{
				obj = ((dataGridViewCell.Value.ToString() == "Yes") ? "True" : "False");
			}
			if (configItem.ItemType == typeof(int) && !int.TryParse(obj.ToString(), out var _))
			{
				MessageBox.Show("Data entered is not an integer");
				dataGridViewCell.Value = configItem.Value;
			}
			else
			{
				configItem.Value = obj;
				SetStyle(dataGridViewCell);
				IsDirty = true;
			}
		}
	}

	private void ConfigPropertiesItemChangedInner()
	{
		DataGridViewSelectedRowCollection selectedRows = m_propertyDataGridView.SelectedRows;
		if (selectedRows == null || selectedRows.Count <= 0 || selectedRows[0].Cells[1].Tag == null)
		{
			m_descriptionTB.Text = string.Empty;
		}
		else if (!(selectedRows[0].Cells[1].Tag is ConfigItem configItem))
		{
			m_descriptionTB.Text = string.Empty;
		}
		else
		{
			m_descriptionTB.Text = configItem.Description;
		}
	}

	private void OnReset(object sender, EventArgs e)
	{
		m_barcodeConfig.FindAll((ConfigItem x) => !x.ReadOnly && x.CustomEditor == null).ForEach(delegate(ConfigItem x)
		{
			x.Value = x.DefaultValue;
		});
		m_cameraConfig.FindAll((ConfigItem x) => !x.ReadOnly && x.CustomEditor == null).ForEach(delegate(ConfigItem x)
		{
			x.Value = x.DefaultValue;
		});
		m_controllerConfig.FindAll((ConfigItem x) => !x.ReadOnly && x.CustomEditor == null).ForEach(delegate(ConfigItem x)
		{
			x.Value = x.DefaultValue;
		});
		ListBoxSelectionChangedInner();
		IsDirty = false;
		MessageBox.Show("Successfully Reset Properties to Previously Loaded Values.", "Configuration Panel", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void OnButtonClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.RowIndex <= m_propertyDataGridView.Rows.Count - 1 && e.ColumnIndex <= 1)
		{
			_ = m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell;
		}
	}

	private void OnListBoxSelectionChanged(object sender, EventArgs args)
	{
		ListBoxSelectionChangedInner();
	}

	private void PopulateDataGrid(List<ConfigItem> list)
	{
		if (list == null)
		{
			return;
		}
		m_propertyDataGridView.Rows.Clear();
		foreach (ConfigItem item in list)
		{
			if (item.CustomEditor != null)
			{
				continue;
			}
			int index = m_propertyDataGridView.Rows.Add();
			DataGridViewRow dataGridViewRow = m_propertyDataGridView.Rows[index];
			dataGridViewRow.Cells[0].Value = item.DisplayName;
			dataGridViewRow.Cells[0].ReadOnly = true;
			DataGridViewCell dataGridViewCell = null;
			if (item.ValidValues != null)
			{
				dataGridViewCell = new DataGridViewComboBoxCell();
				(dataGridViewCell as DataGridViewComboBoxCell).FlatStyle = FlatStyle.Flat;
				foreach (string validValue in item.ValidValues)
				{
					(dataGridViewCell as DataGridViewComboBoxCell).Items.Add(validValue);
				}
				if (!(dataGridViewCell as DataGridViewComboBoxCell).Items.Contains(item.Value))
				{
					(dataGridViewCell as DataGridViewComboBoxCell).Items.Add(item.Value);
				}
			}
			else if (item.ItemType == typeof(bool))
			{
				dataGridViewCell = new DataGridViewComboBoxCell();
				(dataGridViewCell as DataGridViewComboBoxCell).FlatStyle = FlatStyle.Flat;
				(dataGridViewCell as DataGridViewComboBoxCell).Items.Add("Yes");
				(dataGridViewCell as DataGridViewComboBoxCell).Items.Add("No");
				(dataGridViewCell as DataGridViewComboBoxCell).Value = ((item.Value.ToString().ToLower() == "true") ? "Yes" : "No");
			}
			if (dataGridViewCell != null)
			{
				dataGridViewRow.Cells[1] = dataGridViewCell;
			}
			if (item.ItemType != typeof(bool))
			{
				dataGridViewRow.Cells[1].Value = item.Value;
			}
			dataGridViewRow.Cells[1].ReadOnly = item.ReadOnly;
			if (item.ReadOnly)
			{
				dataGridViewRow.DefaultCellStyle.ForeColor = Color.Gray;
			}
			dataGridViewRow.Cells[1].Tag = item;
			SetStyle(dataGridViewRow.Cells[1]);
		}
	}

	private void ListBoxSelectionChangedInner()
	{
		ConfigPropertiesItemChangedInner();
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
		this.m_configItemListBox = new System.Windows.Forms.ListBox();
		this.m_propertyDataGridView = new System.Windows.Forms.DataGridView();
		this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_descriptionTB = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_propertyDataGridView).BeginInit();
		base.SuspendLayout();
		this.m_configItemListBox.FormattingEnabled = true;
		this.m_configItemListBox.Location = new System.Drawing.Point(3, 3);
		this.m_configItemListBox.Name = "m_configItemListBox";
		this.m_configItemListBox.Size = new System.Drawing.Size(344, 95);
		this.m_configItemListBox.TabIndex = 0;
		this.m_propertyDataGridView.AllowUserToAddRows = false;
		this.m_propertyDataGridView.AllowUserToDeleteRows = false;
		this.m_propertyDataGridView.AllowUserToResizeRows = false;
		this.m_propertyDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_propertyDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
		this.m_propertyDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_propertyDataGridView.Columns.AddRange(this.Property, this.Value);
		this.m_propertyDataGridView.Location = new System.Drawing.Point(3, 135);
		this.m_propertyDataGridView.MultiSelect = false;
		this.m_propertyDataGridView.Name = "m_propertyDataGridView";
		this.m_propertyDataGridView.RowHeadersVisible = false;
		this.m_propertyDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_propertyDataGridView.Size = new System.Drawing.Size(340, 330);
		this.m_propertyDataGridView.TabIndex = 1;
		this.Property.HeaderText = "Property";
		this.Property.Name = "Property";
		this.Property.ReadOnly = true;
		this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Value.HeaderText = "Value";
		this.Value.Name = "Value";
		this.m_descriptionTB.Location = new System.Drawing.Point(3, 471);
		this.m_descriptionTB.Multiline = true;
		this.m_descriptionTB.Name = "m_descriptionTB";
		this.m_descriptionTB.ReadOnly = true;
		this.m_descriptionTB.Size = new System.Drawing.Size(340, 66);
		this.m_descriptionTB.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_descriptionTB);
		base.Controls.Add(this.m_propertyDataGridView);
		base.Controls.Add(this.m_configItemListBox);
		base.Name = "ConfigurationPanel";
		base.Size = new System.Drawing.Size(350, 540);
		((System.ComponentModel.ISupportInitialize)this.m_propertyDataGridView).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
