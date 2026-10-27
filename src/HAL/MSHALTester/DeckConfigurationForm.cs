using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public class DeckConfigurationForm : Form
{
	private readonly HardwareService m_hardwareService;

	private IContainer components;

	private Button m_okButton;

	private DataGridView m_sandGrid;

	private Button m_propertiesButton;

	private Button m_fixSlotAlignment;

	private DataGridViewTextBoxColumn m_deckTextColumn;

	private DataGridViewTextBoxColumn m_offsetTextColumn;

	private DataGridViewTextBoxColumn m_slotWidthTextColumn;

	private DataGridViewTextBoxColumn m_numSlotsTextColumn;

	private DataGridViewTextBoxColumn m_isQlmTextColumn;

	private DataGridViewTextBoxColumn m_fillerColumn;

	private Button m_exitButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DecksConfigurationManager Manager { get; private set; }

	public DeckConfigurationForm(HardwareService service)
	{
		Manager = new DecksConfigurationManager(service);
		m_hardwareService = service;
		InitializeComponent();
		RefreshList();
	}

	private void OnOK(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnProperties(object sender, EventArgs e)
	{
		DataGridViewRow dataGridViewRow = m_sandGrid.SelectedRows[0];
		if (dataGridViewRow != null && dataGridViewRow.Tag is XmlNode node)
		{
			bool attributeValue = node.GetAttributeValue<bool>("IsQlm");
			if (new DeckConfigurationDetailForm(allowExcludeUpdates: true, Manager)
			{
				Number = node.GetAttributeValue<int?>("Number"),
				YOffset = node.GetAttributeValue<int?>("Offset"),
				NumberOfSlots = node.GetAttributeValue<int?>("NumberOfSlots"),
				SlotWidth = node.GetAttributeValue<decimal?>("SlotWidth"),
				SellThruSlots = node.GetAttributeValue<int?>("SellThruSlots"),
				SellThruOffset = node.GetAttributeValue<int?>("SellThruOffset"),
				ApproachOffset = node.GetAttributeValue<int?>("ApproachOffset"),
				SlotsPerQuadrant = node.GetAttributeValue<int?>("SlotsPerQuadrant"),
				IsQlmDeck = attributeValue,
				Service = m_hardwareService,
				ShowDumpSlot = false
			}.ShowDialog() == DialogResult.OK)
			{
				RefreshList();
				m_okButton.Enabled = true;
			}
		}
	}

	private void OnSelectedRowChanged(object sender, EventArgs e)
	{
		m_propertiesButton.Enabled = m_sandGrid.SelectedRows.Count > 0;
	}

	private void RefreshList()
	{
		m_sandGrid.SuspendLayout();
		m_sandGrid.Rows.Clear();
		foreach (XmlNode item in Manager.FindAllDeckNodes())
		{
			int attributeValue = item.GetAttributeValue<int>("Offset");
			int attributeValue2 = item.GetAttributeValue<int>("Number");
			bool attributeValue3 = item.GetAttributeValue<bool>("IsQlm");
			int attributeValue4 = item.GetAttributeValue<int>("NumberOfSlots");
			decimal attributeValue5 = item.GetAttributeValue<decimal>("SlotWidth");
			DataGridViewRow dataGridViewRow = new DataGridViewRow
			{
				Tag = item
			};
			DataGridViewCellCollection cells = dataGridViewRow.Cells;
			DataGridViewCell[] dataGridViewCells = new DataGridViewTextBoxCell[5]
			{
				new DataGridViewTextBoxCell(),
				new DataGridViewTextBoxCell(),
				new DataGridViewTextBoxCell(),
				new DataGridViewTextBoxCell(),
				new DataGridViewTextBoxCell()
			};
			cells.AddRange(dataGridViewCells);
			int index = m_sandGrid.Rows.Add(dataGridViewRow);
			m_sandGrid.Rows[index].Cells[0].Value = attributeValue2;
			m_sandGrid.Rows[index].Cells[1].Value = attributeValue;
			m_sandGrid.Rows[index].Cells[2].Value = attributeValue5;
			m_sandGrid.Rows[index].Cells[3].Value = attributeValue4;
			m_sandGrid.Rows[index].Cells[4].Value = attributeValue3;
		}
		m_sandGrid.ResumeLayout();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		if (new EditSlotData(m_hardwareService, Manager).ShowDialog() == DialogResult.OK)
		{
			RefreshList();
		}
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_sandGrid = new System.Windows.Forms.DataGridView();
		this.m_deckTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_offsetTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_slotWidthTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_numSlotsTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_isQlmTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_fillerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.m_propertiesButton = new System.Windows.Forms.Button();
		this.m_fixSlotAlignment = new System.Windows.Forms.Button();
		this.m_exitButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_sandGrid).BeginInit();
		base.SuspendLayout();
		this.m_okButton.BackColor = System.Drawing.Color.LightGray;
		this.m_okButton.Enabled = false;
		this.m_okButton.Location = new System.Drawing.Point(434, 12);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 5;
		this.m_okButton.Text = "Save";
		this.m_okButton.UseVisualStyleBackColor = false;
		this.m_okButton.Visible = false;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_sandGrid.AllowUserToAddRows = false;
		this.m_sandGrid.AllowUserToDeleteRows = false;
		this.m_sandGrid.BackgroundColor = System.Drawing.SystemColors.Window;
		this.m_sandGrid.Columns.AddRange(this.m_deckTextColumn, this.m_offsetTextColumn, this.m_slotWidthTextColumn, this.m_numSlotsTextColumn, this.m_isQlmTextColumn, this.m_fillerColumn);
		this.m_sandGrid.Location = new System.Drawing.Point(12, 12);
		this.m_sandGrid.Name = "m_sandGrid";
		this.m_sandGrid.ReadOnly = true;
		this.m_sandGrid.RowHeadersWidth = 24;
		this.m_sandGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.m_sandGrid.Size = new System.Drawing.Size(416, 289);
		this.m_sandGrid.TabIndex = 0;
		this.m_sandGrid.SelectionChanged += new System.EventHandler(OnSelectedRowChanged);
		this.m_deckTextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.m_deckTextColumn.Frozen = true;
		this.m_deckTextColumn.HeaderText = "Deck #";
		this.m_deckTextColumn.Name = "m_deckTextColumn";
		this.m_deckTextColumn.ReadOnly = true;
		this.m_deckTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.m_deckTextColumn.Width = 49;
		this.m_offsetTextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.m_offsetTextColumn.Frozen = true;
		this.m_offsetTextColumn.HeaderText = "Y Axis Offset";
		this.m_offsetTextColumn.Name = "m_offsetTextColumn";
		this.m_offsetTextColumn.ReadOnly = true;
		this.m_offsetTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.m_offsetTextColumn.Width = 73;
		this.m_slotWidthTextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.m_slotWidthTextColumn.Frozen = true;
		this.m_slotWidthTextColumn.HeaderText = "Slot Width";
		this.m_slotWidthTextColumn.Name = "m_slotWidthTextColumn";
		this.m_slotWidthTextColumn.ReadOnly = true;
		this.m_slotWidthTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.m_slotWidthTextColumn.Width = 62;
		this.m_numSlotsTextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.m_numSlotsTextColumn.Frozen = true;
		this.m_numSlotsTextColumn.HeaderText = "# Slots";
		this.m_numSlotsTextColumn.Name = "m_numSlotsTextColumn";
		this.m_numSlotsTextColumn.ReadOnly = true;
		this.m_numSlotsTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.m_numSlotsTextColumn.Width = 46;
		this.m_isQlmTextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.m_isQlmTextColumn.Frozen = true;
		this.m_isQlmTextColumn.HeaderText = "QLM Deck?";
		this.m_isQlmTextColumn.Name = "m_isQlmTextColumn";
		this.m_isQlmTextColumn.ReadOnly = true;
		this.m_isQlmTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.m_isQlmTextColumn.Width = 71;
		this.m_fillerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.m_fillerColumn.HeaderText = "";
		this.m_fillerColumn.Name = "m_fillerColumn";
		this.m_fillerColumn.ReadOnly = true;
		this.m_propertiesButton.BackColor = System.Drawing.Color.LightGray;
		this.m_propertiesButton.Enabled = false;
		this.m_propertiesButton.Location = new System.Drawing.Point(179, 307);
		this.m_propertiesButton.Name = "m_propertiesButton";
		this.m_propertiesButton.Size = new System.Drawing.Size(75, 23);
		this.m_propertiesButton.TabIndex = 3;
		this.m_propertiesButton.Text = "Properties...";
		this.m_propertiesButton.UseVisualStyleBackColor = false;
		this.m_propertiesButton.Click += new System.EventHandler(OnProperties);
		this.m_fixSlotAlignment.BackColor = System.Drawing.Color.LightGray;
		this.m_fixSlotAlignment.Location = new System.Drawing.Point(274, 307);
		this.m_fixSlotAlignment.Name = "m_fixSlotAlignment";
		this.m_fixSlotAlignment.Size = new System.Drawing.Size(100, 23);
		this.m_fixSlotAlignment.TabIndex = 7;
		this.m_fixSlotAlignment.Text = "Fix Slot Alignment";
		this.m_fixSlotAlignment.UseVisualStyleBackColor = false;
		this.m_fixSlotAlignment.Click += new System.EventHandler(button1_Click);
		this.m_exitButton.BackColor = System.Drawing.Color.LightGray;
		this.m_exitButton.Location = new System.Drawing.Point(434, 302);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(75, 32);
		this.m_exitButton.TabIndex = 8;
		this.m_exitButton.Text = "Close";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(521, 342);
		base.ControlBox = false;
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.m_fixSlotAlignment);
		base.Controls.Add(this.m_propertiesButton);
		base.Controls.Add(this.m_sandGrid);
		base.Controls.Add(this.m_okButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "DeckConfigurationForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Deck Configuration Editor";
		((System.ComponentModel.ISupportInitialize)this.m_sandGrid).EndInit();
		base.ResumeLayout(false);
	}
}
