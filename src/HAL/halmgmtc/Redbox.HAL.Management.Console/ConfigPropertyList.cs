using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class ConfigPropertyList : UserControl
{
	public class RowComparerAscending : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			return (x as DataGridViewRow).Cells[0].Value.ToString().CompareTo((y as DataGridViewRow).Cells[0].Value.ToString());
		}
	}

	public class RowComparerDescending : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			return (y as DataGridViewRow).Cells[0].Value.ToString().CompareTo((x as DataGridViewRow).Cells[0].Value.ToString());
		}
	}

	private DataGridViewCellStyle m_valueChangedStyle;

	private DataGridViewCellStyle m_defaultStyle;

	private XmlDocument m_camera;

	private XmlDocument m_controller;

	private List<ConfigItem> m_cameraConfig;

	private List<ConfigItem> m_controllerConfig;

	private IContainer components;

	private Label m_descriptionLabel;

	private ListBox m_configItemListBox;

	private DataGridView m_propertyDataGridView;

	private DataGridViewTextBoxColumn Property;

	private DataGridViewTextBoxColumn Value;

	private Panel panel1;

	private Button m_alphSortButton;

	private Label m_filterLabel;

	private TextBox m_filterTextBox;

	private Button m_alphDescSortButton;

	private Button m_resetChangesButton;

	public ConfigPropertyList()
	{
		InitializeComponent();
		m_defaultStyle = m_propertyDataGridView.DefaultCellStyle;
		m_valueChangedStyle = new DataGridViewCellStyle
		{
			Font = new Font(Font.Name, Font.Size, FontStyle.Bold)
		};
		SetToolTips();
		m_configItemListBox.SelectedIndexChanged += OnListBoxSelectionChanged;
		m_propertyDataGridView.CellEndEdit += OnDataChanged;
		m_propertyDataGridView.SelectionChanged += OnConfigPropertiesItemChanged;
		m_propertyDataGridView.CellClick += OnButtonClick;
		ProfileManager.Instance.Connected += delegate
		{
			LoadConfiguration();
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			m_descriptionLabel.Text = string.Empty;
			m_configItemListBox.Items.Clear();
			m_propertyDataGridView.Rows.Clear();
			m_camera = null;
			m_controller = null;
		};
		EnvironmentHelper.Save += OnSave;
		Dock = DockStyle.Fill;
	}

	public void LoadConfiguration()
	{
		m_configItemListBox.Items.Clear();
		m_propertyDataGridView.Rows.Clear();
		m_descriptionLabel.Text = string.Empty;
		m_cameraConfig = new List<ConfigItem>();
		m_camera = LoadConfigurationInner(Configuration.Camera, m_cameraConfig);
		if (m_camera != null)
		{
			m_configItemListBox.Items.Add(Configuration.Camera);
		}
		m_controllerConfig = new List<ConfigItem>();
		m_controller = LoadConfigurationInner(Configuration.Controller, m_controllerConfig);
		if (m_controller != null)
		{
			m_configItemListBox.Items.Add(Configuration.Controller);
		}
		EnvironmentHelper.IsDirty = false;
	}

	public void Save()
	{
		if (m_cameraConfig != null)
		{
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.SetConfiguration("camera", m_camera.InnerXml);
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
			if (!hardwareCommandResult.Success)
			{
				SaveFailed();
				return;
			}
			foreach (ConfigItem item in m_cameraConfig)
			{
				item.DefaultValue = item.Value;
			}
		}
		if (m_controllerConfig != null)
		{
			HardwareCommandResult hardwareCommandResult2 = ProfileManager.Instance.Service.SetConfiguration("controller", m_controller.InnerXml);
			CommonFunctions.ProcessCommandResult(hardwareCommandResult2);
			if (!hardwareCommandResult2.Success)
			{
				SaveFailed();
				return;
			}
			foreach (ConfigItem item2 in m_controllerConfig)
			{
				item2.DefaultValue = item2.Value;
			}
		}
		m_propertyDataGridView.SuspendLayout();
		ListBoxSelectionChangedInner();
		m_propertyDataGridView.ResumeLayout();
		EnvironmentHelper.IsDirty = false;
	}

	private void SetToolTips()
	{
		ToolTip toolTip = new ToolTip();
		ToolTip toolTip2 = new ToolTip();
		ToolTip toolTip3 = new ToolTip();
		int num = (toolTip3.AutoPopDelay = 5000);
		int autoPopDelay = (toolTip2.AutoPopDelay = num);
		toolTip.AutoPopDelay = autoPopDelay;
		num = (toolTip3.InitialDelay = 1000);
		autoPopDelay = (toolTip2.InitialDelay = num);
		toolTip.InitialDelay = autoPopDelay;
		num = (toolTip3.ReshowDelay = 500);
		autoPopDelay = (toolTip2.ReshowDelay = num);
		toolTip.ReshowDelay = autoPopDelay;
		bool flag = (toolTip3.ShowAlways = true);
		bool showAlways = (toolTip2.ShowAlways = flag);
		toolTip.ShowAlways = showAlways;
		toolTip.SetToolTip(m_alphSortButton, "Sort Ascending");
		toolTip2.SetToolTip(m_alphDescSortButton, "Sort Descending");
		toolTip3.SetToolTip(m_resetChangesButton, "Reset All Configuration to Defaults");
	}

	private void SaveFailed()
	{
		MessageBox.Show("Save Failed!");
	}

	private XmlDocument LoadConfigurationInner(Configuration conf, List<ConfigItem> list)
	{
		if (!ProfileManager.Instance.IsConnected)
		{
			return null;
		}
		HardwareCommandResult configuration = ProfileManager.Instance.Service.GetConfiguration(conf.ToString());
		CommonFunctions.ProcessCommandResult(configuration);
		if (!configuration.Success)
		{
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(configuration.CommandMessages[0]);
		}
		catch (Exception e)
		{
			OutputWindow.Instance.Append("Failed to load xml: " + conf);
			LogHelper.Instance.Log("An exception was thrown when trying to load configuration from Hal", e, LogEntryType.Error);
			return null;
		}
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.ChildNodes.Count > 0 && childNode.ChildNodes[0].Name == "property")
			{
				CreateSubNodeConfigItems(childNode, list);
				continue;
			}
			ConfigItem configItem = new ConfigItem(childNode);
			if (configItem != null)
			{
				list.Add(configItem);
			}
		}
		return xmlDocument;
	}

	private void CreateSubNodeConfigItems(XmlNode node, List<ConfigItem> list)
	{
		string attributeValue = node.GetAttributeValue<string>("display-name");
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ConfigItem configItem = new ConfigItem(childNode);
			configItem.DisplayName = attributeValue + " - " + configItem.DisplayName;
			list.Add(configItem);
		}
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

	private void OnListBoxSelectionChanged(object sender, EventArgs args)
	{
		ListBoxSelectionChangedInner();
	}

	private void ListBoxSelectionChangedInner()
	{
		switch ((Configuration)m_configItemListBox.SelectedIndex)
		{
		case Configuration.Camera:
		{
			PopulateDataGrid(m_cameraConfig);
			int index = m_propertyDataGridView.Rows.Add();
			m_propertyDataGridView.Rows[index].Cells[0].Value = "Camera Settings";
			DataGridViewButtonCell dataGridViewButtonCell = new DataGridViewButtonCell();
			dataGridViewButtonCell.Value = "Launch Camera Tuner";
			dataGridViewButtonCell.FlatStyle = FlatStyle.Flat;
			dataGridViewButtonCell.Tag = "CAMERA TUNER";
			m_propertyDataGridView.Rows[index].Cells[1] = dataGridViewButtonCell;
			break;
		}
		case Configuration.Controller:
			PopulateDataGrid(m_controllerConfig);
			break;
		}
		m_filterTextBox.Text = string.Empty;
		ConfigPropertiesItemChangedInner();
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
				EnvironmentHelper.IsDirty = true;
			}
		}
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

	private void OnConfigPropertiesItemChanged(object sender, EventArgs e)
	{
		ConfigPropertiesItemChangedInner();
	}

	private void ConfigPropertiesItemChangedInner()
	{
		DataGridViewSelectedRowCollection selectedRows = m_propertyDataGridView.SelectedRows;
		if (selectedRows == null || selectedRows.Count <= 0 || selectedRows[0].Cells[1].Tag == null)
		{
			m_descriptionLabel.Text = string.Empty;
		}
		else if (!(selectedRows[0].Cells[1].Tag is ConfigItem configItem))
		{
			m_descriptionLabel.Text = string.Empty;
		}
		else
		{
			m_descriptionLabel.Text = configItem.Description;
		}
	}

	private void OnSave(object sender, EventArgs e)
	{
		Save();
	}

	private void OnReset(object sender, EventArgs e)
	{
		m_cameraConfig.FindAll((ConfigItem x) => !x.ReadOnly && x.CustomEditor == null).ForEach(delegate(ConfigItem x)
		{
			x.Value = x.DefaultValue;
		});
		m_controllerConfig.FindAll((ConfigItem x) => !x.ReadOnly && x.CustomEditor == null).ForEach(delegate(ConfigItem x)
		{
			x.Value = x.DefaultValue;
		});
		ListBoxSelectionChangedInner();
		EnvironmentHelper.IsDirty = false;
		MessageBox.Show("Successfully Reset Properties to Previously Loaded Values.", "Hal Management Console " + Program.Version, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void OnButtonClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.RowIndex <= m_propertyDataGridView.Rows.Count - 1 && e.ColumnIndex <= 1 && m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell dataGridViewButtonCell && dataGridViewButtonCell.Tag.ToString() == "CAMERA TUNER")
		{
			CommonFunctions.OnLaunchCameraProperties(sender, e);
		}
	}

	private void m_alphSortButton_Click(object sender, EventArgs e)
	{
		m_propertyDataGridView.Sort(new RowComparerAscending());
	}

	private void m_alphSortDescButton_Click(object sender, EventArgs e)
	{
		m_propertyDataGridView.Sort(new RowComparerDescending());
	}

	private void m_filterTextBox_TextChanged(object sender, EventArgs e)
	{
		foreach (DataGridViewRow item in (IEnumerable)m_propertyDataGridView.Rows)
		{
			if (string.IsNullOrEmpty(m_filterTextBox.Text))
			{
				item.Visible = true;
			}
			else if (!item.Cells[0].Value.ToString().ToLower().Contains(m_filterTextBox.Text.ToLower()))
			{
				item.Visible = false;
			}
			else
			{
				item.Visible = true;
			}
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
		this.m_descriptionLabel = new System.Windows.Forms.Label();
		this.m_configItemListBox = new System.Windows.Forms.ListBox();
		this.m_propertyDataGridView = new System.Windows.Forms.DataGridView();
		this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_resetChangesButton = new System.Windows.Forms.Button();
		this.m_alphDescSortButton = new System.Windows.Forms.Button();
		this.m_filterLabel = new System.Windows.Forms.Label();
		this.m_filterTextBox = new System.Windows.Forms.TextBox();
		this.m_alphSortButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_propertyDataGridView).BeginInit();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_descriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_descriptionLabel.Location = new System.Drawing.Point(6, 431);
		this.m_descriptionLabel.Name = "m_descriptionLabel";
		this.m_descriptionLabel.Padding = new System.Windows.Forms.Padding(10);
		this.m_descriptionLabel.Size = new System.Drawing.Size(342, 97);
		this.m_descriptionLabel.TabIndex = 0;
		this.m_descriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_configItemListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_configItemListBox.FormattingEnabled = true;
		this.m_configItemListBox.Location = new System.Drawing.Point(6, 3);
		this.m_configItemListBox.Name = "m_configItemListBox";
		this.m_configItemListBox.Size = new System.Drawing.Size(342, 56);
		this.m_configItemListBox.TabIndex = 1;
		this.m_propertyDataGridView.AllowUserToAddRows = false;
		this.m_propertyDataGridView.AllowUserToDeleteRows = false;
		this.m_propertyDataGridView.AllowUserToResizeRows = false;
		this.m_propertyDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_propertyDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
		this.m_propertyDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_propertyDataGridView.Columns.AddRange(this.Property, this.Value);
		this.m_propertyDataGridView.Location = new System.Drawing.Point(6, 101);
		this.m_propertyDataGridView.MultiSelect = false;
		this.m_propertyDataGridView.Name = "m_propertyDataGridView";
		this.m_propertyDataGridView.RowHeadersVisible = false;
		this.m_propertyDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_propertyDataGridView.Size = new System.Drawing.Size(342, 327);
		this.m_propertyDataGridView.TabIndex = 2;
		this.Property.HeaderText = "Property";
		this.Property.Name = "Property";
		this.Property.Width = 125;
		this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Value.HeaderText = "Value";
		this.Value.Name = "Value";
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.Controls.Add(this.m_resetChangesButton);
		this.panel1.Controls.Add(this.m_alphDescSortButton);
		this.panel1.Controls.Add(this.m_filterLabel);
		this.panel1.Controls.Add(this.m_filterTextBox);
		this.panel1.Controls.Add(this.m_alphSortButton);
		this.panel1.Location = new System.Drawing.Point(6, 65);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(342, 30);
		this.panel1.TabIndex = 3;
		this.m_resetChangesButton.BackColor = System.Drawing.Color.Transparent;
		this.m_resetChangesButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.m_resetChangesButton.Enabled = false;
		this.m_resetChangesButton.Image = Redbox.HAL.Management.Console.Properties.Resources.undo;
		this.m_resetChangesButton.Location = new System.Drawing.Point(72, 3);
		this.m_resetChangesButton.Name = "m_resetChangesButton";
		this.m_resetChangesButton.Size = new System.Drawing.Size(29, 27);
		this.m_resetChangesButton.TabIndex = 4;
		this.m_resetChangesButton.UseVisualStyleBackColor = false;
		this.m_resetChangesButton.Visible = false;
		this.m_resetChangesButton.Click += new System.EventHandler(OnReset);
		this.m_alphDescSortButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.m_alphDescSortButton.BackColor = System.Drawing.Color.Transparent;
		this.m_alphDescSortButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.m_alphDescSortButton.Image = Redbox.HAL.Management.Console.Properties.Resources.sortDescending2;
		this.m_alphDescSortButton.Location = new System.Drawing.Point(37, 3);
		this.m_alphDescSortButton.Name = "m_alphDescSortButton";
		this.m_alphDescSortButton.Size = new System.Drawing.Size(29, 27);
		this.m_alphDescSortButton.TabIndex = 5;
		this.m_alphDescSortButton.Tag = "Sort Descending";
		this.m_alphDescSortButton.UseVisualStyleBackColor = false;
		this.m_alphDescSortButton.Click += new System.EventHandler(m_alphSortDescButton_Click);
		this.m_filterLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.m_filterLabel.Location = new System.Drawing.Point(107, 6);
		this.m_filterLabel.Name = "m_filterLabel";
		this.m_filterLabel.Size = new System.Drawing.Size(50, 20);
		this.m_filterLabel.TabIndex = 4;
		this.m_filterLabel.Text = "Filter: ";
		this.m_filterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.m_filterTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_filterTextBox.Location = new System.Drawing.Point(163, 7);
		this.m_filterTextBox.Name = "m_filterTextBox";
		this.m_filterTextBox.Size = new System.Drawing.Size(176, 20);
		this.m_filterTextBox.TabIndex = 1;
		this.m_filterTextBox.TextChanged += new System.EventHandler(m_filterTextBox_TextChanged);
		this.m_alphSortButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.m_alphSortButton.BackColor = System.Drawing.Color.Transparent;
		this.m_alphSortButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.m_alphSortButton.Image = Redbox.HAL.Management.Console.Properties.Resources.sortAscending2;
		this.m_alphSortButton.Location = new System.Drawing.Point(3, 3);
		this.m_alphSortButton.Name = "m_alphSortButton";
		this.m_alphSortButton.Size = new System.Drawing.Size(28, 27);
		this.m_alphSortButton.TabIndex = 0;
		this.m_alphSortButton.Tag = "Sort Ascending";
		this.m_alphSortButton.UseVisualStyleBackColor = false;
		this.m_alphSortButton.Click += new System.EventHandler(m_alphSortButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.m_propertyDataGridView);
		base.Controls.Add(this.m_configItemListBox);
		base.Controls.Add(this.m_descriptionLabel);
		base.Name = "ConfigPropertyList";
		base.Size = new System.Drawing.Size(351, 538);
		((System.ComponentModel.ISupportInitialize)this.m_propertyDataGridView).EndInit();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
	}
}
