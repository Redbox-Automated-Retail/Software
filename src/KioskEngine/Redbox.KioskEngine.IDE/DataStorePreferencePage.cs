using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;

namespace Redbox.KioskEngine.IDE;

public class DataStorePreferencePage : UserControl, IPreferencePageHost
{
	private int? m_currentRowIndex;

	private IContainer components;

	private DataGridView m_dataGridView;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_deleteToolStripButton;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripLabel toolStripLabel1;

	private ToolStripComboBox m_storeToolStripComboBox;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripLabel toolStripLabel2;

	private ToolStripComboBox m_tableToolStripComboBox;

	private ToolStripButton m_propertiesToolStripButton;

	private SyntaxBoxControl m_syntaxBoxControl;

	private SyntaxDocument m_syntaxDocument;

	private ToolStripButton m_saveToolStripButton;

	private ToolStripButton m_findToolStripButton;

	private ToolStripButton m_addToolStripButton;

	private ToolStripSeparator toolStripSeparator3;

	private Label m_headerLabel;

	public DataStorePreferencePage()
	{
		InitializeComponent();
		m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly, "Redbox.KioskEngine.IDE.Lua.syn");
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		RefreshStores();
	}

	private void OnAdd(object sender, EventArgs e)
	{
		NewKeyValueForm newKeyValueForm = new NewKeyValueForm();
		if (newKeyValueForm.ShowDialog() == DialogResult.OK)
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service != null)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
				string tableName = m_tableToolStripComboBox.SelectedItem.ToString();
				service.Set(fileNameWithoutExtension, tableName, newKeyValueForm.KeyName, newKeyValueForm.Value);
				RefreshTable();
			}
		}
	}

	private void OnDelete(object sender, EventArgs e)
	{
		if (m_dataGridView.SelectedRows.Count == 0)
		{
			return;
		}
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (service == null)
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
		string tableName = m_tableToolStripComboBox.SelectedItem.ToString();
		foreach (DataGridViewRow selectedRow in m_dataGridView.SelectedRows)
		{
			service.Remove(fileNameWithoutExtension, tableName, selectedRow.Cells[0].FormattedValue as string);
		}
		RefreshTable();
	}

	private void OnSave(object sender, EventArgs e)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (service == null)
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
		string tableName = m_tableToolStripComboBox.SelectedItem.ToString();
		try
		{
			Cursor = Cursors.WaitCursor;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (DataGridViewRow item in (IEnumerable)m_dataGridView.Rows)
			{
				if (!((string)item.HeaderCell.Value != "*"))
				{
					dictionary[(string)item.Cells[0].FormattedValue] = item.Cells[1].FormattedValue as string;
				}
			}
			service.BulkSet(fileNameWithoutExtension, tableName, dictionary);
		}
		finally
		{
			Cursor = Cursors.Default;
		}
		RefreshTable();
	}

	private void OnSelectedStoreChanged(object sender, EventArgs e)
	{
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null || m_storeToolStripComboBox.SelectedItem == null)
			{
				return;
			}
			Cursor = Cursors.WaitCursor;
			ReadOnlyCollection<string> tables = service.GetTables(Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString()));
			m_tableToolStripComboBox.BeginUpdate();
			m_tableToolStripComboBox.Items.Clear();
			foreach (string item in tables)
			{
				m_tableToolStripComboBox.Items.Add(item);
			}
			m_tableToolStripComboBox.EndUpdate();
			if (m_tableToolStripComboBox.Items.Count > 0)
			{
				m_tableToolStripComboBox.SelectedIndex = 0;
			}
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnSelectedTableChanged(object sender, EventArgs e)
	{
		RefreshTable();
	}

	private void RefreshTable()
	{
		m_currentRowIndex = null;
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (service == null || m_tableToolStripComboBox.SelectedItem == null)
		{
			return;
		}
		try
		{
			Cursor = Cursors.WaitCursor;
			m_syntaxDocument.Clear();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
			string tableName = m_tableToolStripComboBox.SelectedItem.ToString();
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("Key", typeof(string));
			dataTable.Columns.Add("Value", typeof(string));
			foreach (string key in service.GetKeys(fileNameWithoutExtension, tableName))
			{
				DataRow dataRow = dataTable.NewRow();
				dataTable.Rows.Add(dataRow);
				dataRow.BeginEdit();
				dataRow["Key"] = key;
				dataRow["Value"] = service.Get(fileNameWithoutExtension, tableName, key);
				dataRow.EndEdit();
				dataRow.AcceptChanges();
			}
			m_dataGridView.DataSource = dataTable;
			m_dataGridView.Focus();
		}
		finally
		{
			Cursor = Cursors.Default;
			if (m_findToolStripButton.Checked)
			{
				m_findToolStripButton.Checked = false;
			}
		}
	}

	private void RefreshStores()
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			m_storeToolStripComboBox.BeginUpdate();
			m_storeToolStripComboBox.Items.Clear();
			ServiceLocator.Instance.GetService<IMacroService>();
			string[] files = Directory.GetFiles(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "*.data");
			foreach (string path in files)
			{
				m_storeToolStripComboBox.Items.Add(Path.GetFileName(path));
			}
			if (m_storeToolStripComboBox.Items.Count > 0)
			{
				m_storeToolStripComboBox.SelectedIndex = 0;
			}
			m_storeToolStripComboBox.EndUpdate();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		m_currentRowIndex = null;
		RefreshStores();
	}

	private void OnRowEnter(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex == -1)
		{
			m_currentRowIndex = null;
			m_syntaxDocument.Clear();
		}
		if (m_currentRowIndex != e.RowIndex)
		{
			if (m_currentRowIndex.HasValue && m_currentRowIndex != e.RowIndex && m_dataGridView[1, m_currentRowIndex.Value].Value as string != m_syntaxDocument.Text)
			{
				m_dataGridView[1, m_currentRowIndex.Value].Value = m_syntaxDocument.Text;
				m_dataGridView.Rows[m_currentRowIndex.Value].HeaderCell.Value = "*";
			}
			m_currentRowIndex = e.RowIndex;
			m_syntaxDocument.Text = m_dataGridView[1, e.RowIndex].FormattedValue as string;
		}
	}

	private void OnFind(object sender, EventArgs e)
	{
		if (m_findToolStripButton.Checked)
		{
			m_findToolStripButton.Checked = false;
			RefreshTable();
			return;
		}
		FindKeyValueForm findKeyValueForm = new FindKeyValueForm();
		if (findKeyValueForm.ShowDialog() == DialogResult.OK && m_dataGridView.DataSource is DataTable dataTable)
		{
			m_syntaxDocument.Clear();
			DataRow[] obj = ((findKeyValueForm.SearchType == KeyValueSearchType.Key) ? dataTable.Select($"[Key] = '{findKeyValueForm.SearchFor}'") : dataTable.Select($"[Value] LIKE '%{findKeyValueForm.SearchFor}%'"));
			DataTable dataTable2 = new DataTable
			{
				Columns = 
				{
					{
						"Key",
						typeof(string)
					},
					{
						"Value",
						typeof(string)
					}
				}
			};
			DataRow[] array = obj;
			foreach (DataRow dataRow in array)
			{
				dataTable2.Rows.Add(dataRow[0], dataRow[1]);
			}
			m_dataGridView.DataSource = dataTable2;
			m_dataGridView.Focus();
			m_findToolStripButton.Checked = true;
		}
	}

	private void OnDocumentModified(object sender, EventArgs e)
	{
		m_saveToolStripButton.Enabled = true;
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
		this.m_dataGridView = new System.Windows.Forms.DataGridView();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.m_storeToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
		this.m_tableToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.m_propertiesToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_findToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_addToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_deleteToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.m_saveToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_syntaxBoxControl = new Alsing.Windows.Forms.SyntaxBoxControl();
		this.m_syntaxDocument = new Alsing.SourceCode.SyntaxDocument(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).BeginInit();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_dataGridView.AllowUserToAddRows = false;
		this.m_dataGridView.AllowUserToDeleteRows = false;
		this.m_dataGridView.AllowUserToResizeRows = false;
		this.m_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_dataGridView.Location = new System.Drawing.Point(0, 70);
		this.m_dataGridView.Name = "m_dataGridView";
		this.m_dataGridView.ReadOnly = true;
		this.m_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_dataGridView.ShowCellErrors = false;
		this.m_dataGridView.ShowCellToolTips = false;
		this.m_dataGridView.ShowEditingIcon = false;
		this.m_dataGridView.ShowRowErrors = false;
		this.m_dataGridView.Size = new System.Drawing.Size(542, 254);
		this.m_dataGridView.TabIndex = 8;
		this.m_dataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(OnRowEnter);
		this.m_toolStrip.AutoSize = false;
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.m_refreshToolStripButton, this.toolStripSeparator1, this.toolStripLabel1, this.m_storeToolStripComboBox, this.toolStripLabel2, this.m_tableToolStripComboBox, this.m_propertiesToolStripButton, this.m_findToolStripButton, this.toolStripSeparator2, this.m_addToolStripButton,
			this.m_deleteToolStripButton, this.toolStripSeparator3, this.m_saveToolStripButton
		});
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 42);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(542, 25);
		this.m_toolStrip.TabIndex = 10;
		this.m_toolStrip.Text = "toolStrip1";
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.Text = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.toolStripLabel1.AutoSize = false;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(63, 20);
		this.toolStripLabel1.Text = "Data Store:";
		this.m_storeToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_storeToolStripComboBox.Name = "m_storeToolStripComboBox";
		this.m_storeToolStripComboBox.Size = new System.Drawing.Size(121, 23);
		this.m_storeToolStripComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedStoreChanged);
		this.toolStripLabel2.AutoSize = false;
		this.toolStripLabel2.Name = "toolStripLabel2";
		this.toolStripLabel2.Size = new System.Drawing.Size(37, 20);
		this.toolStripLabel2.Text = "Table:";
		this.m_tableToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_tableToolStripComboBox.Name = "m_tableToolStripComboBox";
		this.m_tableToolStripComboBox.Size = new System.Drawing.Size(121, 23);
		this.m_tableToolStripComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedTableChanged);
		this.m_propertiesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_propertiesToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Properties;
		this.m_propertiesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_propertiesToolStripButton.Name = "m_propertiesToolStripButton";
		this.m_propertiesToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_propertiesToolStripButton.Text = "Properties";
		this.m_findToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_findToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Filter;
		this.m_findToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_findToolStripButton.Name = "m_findToolStripButton";
		this.m_findToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_findToolStripButton.Text = "Find";
		this.m_findToolStripButton.Click += new System.EventHandler(OnFind);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_addToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_addToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Plus;
		this.m_addToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_addToolStripButton.Name = "m_addToolStripButton";
		this.m_addToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_addToolStripButton.Text = "Add";
		this.m_addToolStripButton.Click += new System.EventHandler(OnAdd);
		this.m_deleteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_deleteToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Cross;
		this.m_deleteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_deleteToolStripButton.Name = "m_deleteToolStripButton";
		this.m_deleteToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_deleteToolStripButton.Text = "Delete";
		this.m_deleteToolStripButton.Click += new System.EventHandler(OnDelete);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
		this.m_saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_saveToolStripButton.Enabled = false;
		this.m_saveToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Save;
		this.m_saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_saveToolStripButton.Name = "m_saveToolStripButton";
		this.m_saveToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_saveToolStripButton.Text = "Save";
		this.m_saveToolStripButton.Click += new System.EventHandler(OnSave);
		this.m_syntaxBoxControl.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight;
		this.m_syntaxBoxControl.AllowBreakPoints = false;
		this.m_syntaxBoxControl.AutoListPosition = null;
		this.m_syntaxBoxControl.AutoListSelectedText = "a123";
		this.m_syntaxBoxControl.AutoListVisible = false;
		this.m_syntaxBoxControl.BackColor = System.Drawing.Color.White;
		this.m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
		this.m_syntaxBoxControl.CopyAsRTF = false;
		this.m_syntaxBoxControl.Document = this.m_syntaxDocument;
		this.m_syntaxBoxControl.FontName = "Courier new";
		this.m_syntaxBoxControl.HighLightActiveLine = true;
		this.m_syntaxBoxControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.m_syntaxBoxControl.InfoTipCount = 1;
		this.m_syntaxBoxControl.InfoTipPosition = null;
		this.m_syntaxBoxControl.InfoTipSelectedIndex = 1;
		this.m_syntaxBoxControl.InfoTipVisible = false;
		this.m_syntaxBoxControl.Location = new System.Drawing.Point(0, 330);
		this.m_syntaxBoxControl.LockCursorUpdate = false;
		this.m_syntaxBoxControl.Name = "m_syntaxBoxControl";
		this.m_syntaxBoxControl.ShowEOLMarker = true;
		this.m_syntaxBoxControl.ShowGutterMargin = false;
		this.m_syntaxBoxControl.ShowScopeIndicator = false;
		this.m_syntaxBoxControl.ShowTabGuides = true;
		this.m_syntaxBoxControl.ShowWhitespace = true;
		this.m_syntaxBoxControl.Size = new System.Drawing.Size(542, 176);
		this.m_syntaxBoxControl.SmoothScroll = false;
		this.m_syntaxBoxControl.SplitView = false;
		this.m_syntaxBoxControl.SplitviewH = -4;
		this.m_syntaxBoxControl.SplitviewV = -4;
		this.m_syntaxBoxControl.TabGuideColor = System.Drawing.Color.FromArgb(222, 219, 214);
		this.m_syntaxBoxControl.TabIndex = 12;
		this.m_syntaxBoxControl.TabsToSpaces = true;
		this.m_syntaxBoxControl.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
		this.m_syntaxDocument.Lines = new string[1] { "" };
		this.m_syntaxDocument.MaxUndoBufferSize = 1000;
		this.m_syntaxDocument.Modified = false;
		this.m_syntaxDocument.UndoStep = 0;
		this.m_syntaxDocument.ModifiedChanged += new System.EventHandler(OnDocumentModified);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 30;
		this.m_headerLabel.Text = "  Engine Core: Database";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_syntaxBoxControl);
		base.Controls.Add(this.m_dataGridView);
		base.Controls.Add(this.m_toolStrip);
		base.Name = "DataStorePreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).EndInit();
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
	}
}
