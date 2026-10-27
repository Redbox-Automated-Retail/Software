using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public class DeckConfigurationDetailForm : Form
{
	private bool m_showDumpSlot;

	private bool m_allowSlotDataEdit;

	private IContainer components;

	private Button m_okButton;

	private Button m_cancelButton;

	private ErrorProvider m_errorProvider;

	private Label m_numberLabel;

	private CheckBox m_isQlmDeckCheckBox;

	private Label m_slotsPerQuadrantLabel;

	private Label m_approachOffsetLabel;

	private Label m_sellThruOffsetLabel;

	private Label m_sellThruSlotsLabel;

	private Label m_slotWidthLabel;

	private Label m_numberOfSlotsLabel;

	private Label m_yOffsetLabel;

	private Label label9;

	private TextBox m_slotsPerQuadrantTextBox;

	private TextBox m_approachOffsetTextBox;

	private TextBox m_sellThruOffsetTextBox;

	private TextBox m_sellThruSlotsTextBox;

	private TextBox m_slotWidthTextBox;

	private TextBox m_numberOfSlotsTextBox;

	private TextBox m_yoffsetTextBox;

	private TextBox m_numberTextBox;

	private Button m_computeQuadrantsButton;

	private CheckBox m_hasDumpSlotCheckBox;

	private Button m_editSlotDataButton;

	private DataGridView m_quadrantDataGridView;

	private DataGridViewTextBoxColumn Quadrant;

	private DataGridViewTextBoxColumn Offset;

	private DataGridViewTextBoxColumn Start;

	private DataGridViewTextBoxColumn End;

	private DataGridViewCheckBoxColumn Excluded;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool AllowSlotDataEdit
	{
		get
		{
			return m_allowSlotDataEdit;
		}
		set
		{
			m_allowSlotDataEdit = value;
			m_editSlotDataButton.Visible = m_allowSlotDataEdit;
			m_editSlotDataButton.Enabled = m_allowSlotDataEdit;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? Number
	{
		get
		{
			if (int.TryParse(m_numberTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_numberTextBox.Clear();
			if (value.HasValue)
			{
				m_numberTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? YOffset
	{
		get
		{
			if (int.TryParse(m_yoffsetTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				m_yoffsetTextBox.Clear();
				m_yoffsetTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? NumberOfSlots
	{
		get
		{
			if (int.TryParse(m_numberOfSlotsTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_numberOfSlotsTextBox.Clear();
			if (value.HasValue)
			{
				m_numberOfSlotsTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public decimal? SlotWidth
	{
		get
		{
			if (decimal.TryParse(m_slotWidthTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_slotWidthTextBox.Clear();
			if (value.HasValue)
			{
				m_slotWidthTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? SellThruSlots
	{
		get
		{
			if (int.TryParse(m_sellThruSlotsTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_sellThruSlotsTextBox.Clear();
			if (value.HasValue)
			{
				m_sellThruSlotsTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? SellThruOffset
	{
		get
		{
			if (int.TryParse(m_sellThruOffsetTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_sellThruOffsetTextBox.Clear();
			if (value.HasValue)
			{
				m_sellThruOffsetTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? ApproachOffset
	{
		get
		{
			if (int.TryParse(m_approachOffsetTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_approachOffsetTextBox.Clear();
			if (value.HasValue)
			{
				m_approachOffsetTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int? SlotsPerQuadrant
	{
		get
		{
			if (int.TryParse(m_slotsPerQuadrantTextBox.Text, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			m_slotsPerQuadrantTextBox.Clear();
			if (value.HasValue)
			{
				m_slotsPerQuadrantTextBox.Text = value.ToString();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsQlmDeck
	{
		get
		{
			return m_isQlmDeckCheckBox.Checked;
		}
		set
		{
			m_isQlmDeckCheckBox.Checked = value;
			m_computeQuadrantsButton.Enabled = !value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDumpSlot
	{
		get
		{
			return m_hasDumpSlotCheckBox.Checked;
		}
		set
		{
			m_hasDumpSlotCheckBox.Checked = value;
		}
	}

	public bool ShowDumpSlot
	{
		get
		{
			return m_showDumpSlot;
		}
		set
		{
			m_showDumpSlot = value;
			m_hasDumpSlotCheckBox.Visible = m_showDumpSlot;
			m_hasDumpSlotCheckBox.Checked = value;
		}
	}

	public HardwareService Service { get; set; }

	public DecksConfigurationManager Manager { get; private set; }

	private bool AllowExcludeUpdates { get; set; }

	private XmlNode WorkingNode { get; set; }

	public DeckConfigurationDetailForm(bool allowExcludeUpdates, DecksConfigurationManager manager)
	{
		InitializeComponent();
		AllowExcludeUpdates = allowExcludeUpdates;
		Manager = manager;
		m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].ReadOnly = !AllowExcludeUpdates;
	}

	public void EnableAddRemove()
	{
		m_isQlmDeckCheckBox.Enabled = false;
	}

	private void OnLoad(object sender, EventArgs e)
	{
		if (!AllowExcludeUpdates)
		{
			m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].DefaultCellStyle.BackColor = Color.LightGray;
			m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].DefaultCellStyle.ForeColor = Color.Gray;
			((DataGridViewCheckBoxColumn)m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded]).FlatStyle = FlatStyle.Flat;
		}
		if (m_isQlmDeckCheckBox.Checked)
		{
			m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].Visible = false;
			m_approachOffsetLabel.Visible = true;
			m_approachOffsetTextBox.Visible = true;
			m_editSlotDataButton.Enabled = false;
		}
		m_hasDumpSlotCheckBox.Enabled = ShowDumpSlot;
		WorkingNode = Manager.FindDeckNode(Number.Value).CloneNode(deep: true);
		RefreshView();
	}

	private void OnOK(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_numberTextBox, string.Empty);
		m_errorProvider.SetError(m_yoffsetTextBox, string.Empty);
		m_errorProvider.SetError(m_slotWidthTextBox, string.Empty);
		m_errorProvider.SetError(m_numberOfSlotsTextBox, string.Empty);
		if (!YOffset.HasValue)
		{
			m_errorProvider.SetError(m_yoffsetTextBox, "Y Offset is required field.");
			return;
		}
		if (!SlotWidth.HasValue)
		{
			m_errorProvider.SetError(m_slotWidthTextBox, "Slot Width is a required field.");
			return;
		}
		List<int> list = new List<int>();
		XmlNode xmlNode = Manager.FindDeckNode(Number.Value);
		for (int i = 0; i < WorkingNode.ChildNodes.Count; i++)
		{
			XmlNode node = xmlNode.ChildNodes[i];
			_ = WorkingNode.ChildNodes[i];
			if (!node.GetAttributeValue("IsExcluded", defaultValue: false) && IsQuadrantExcluded(i))
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			UpdateOriginalNode(xmlNode);
			Manager.FlushChanges(refresh: false);
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else if (DialogResult.Cancel == MessageBox.Show(this, "This will update the inventory for all slots in excluded quadrants to EMPTY.  You MUST remove ALL discs in the excluded quadrants prior to excluding the quadrant(s)." + Environment.NewLine + "To cancel this operation, click ‘Cancel.’", "You are about to exclude sections of a deck.", MessageBoxButtons.OKCancel))
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
		else
		{
			UpdateOriginalNode(xmlNode);
			Manager.FlushChanges(refresh: false);
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void OnCancel(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private XmlNode UpdateOriginalNode(XmlNode originalNode)
	{
		originalNode.SetAttributeValue("Number", Number.Value);
		originalNode.SetAttributeValue("Offset", YOffset.Value);
		originalNode.SetAttributeValue("NumberOfSlots", NumberOfSlots.Value);
		originalNode.SetAttributeValue("SlotWidth", SlotWidth.Value);
		originalNode.SetAttributeValue("IsQlm", IsQlmDeck);
		originalNode.SetAttributeValue("SellThruSlots", SellThruSlots);
		originalNode.SetAttributeValue("SellThruOffset", SellThruOffset);
		originalNode.SetAttributeValue("ApproachOffset", ApproachOffset);
		originalNode.SetAttributeValue("SlotsPerQuadrant", SlotsPerQuadrant);
		for (int i = 0; i < WorkingNode.ChildNodes.Count; i++)
		{
			XmlNode node = originalNode.ChildNodes[i];
			_ = WorkingNode.ChildNodes[i];
			bool value = IsQuadrantExcluded(i);
			node.SetAttributeValue("IsExcluded", value);
			node.SetAttributeValue("Offset", m_quadrantDataGridView[1, i].Value);
		}
		return originalNode;
	}

	private void RefreshView()
	{
		m_quadrantDataGridView.SuspendLayout();
		m_quadrantDataGridView.Rows.Clear();
		int num = 1;
		foreach (XmlNode childNode in WorkingNode.ChildNodes)
		{
			int attributeValue = childNode.GetAttributeValue<int>("Offset");
			int? attributeValue2 = childNode.GetAttributeValue<int?>("StartSlot");
			int? attributeValue3 = childNode.GetAttributeValue<int?>("EndSlot");
			bool attributeValue4 = childNode.GetAttributeValue("IsExcluded", defaultValue: false);
			m_quadrantDataGridView.Rows.Add(num++, attributeValue, attributeValue2, attributeValue3, attributeValue4);
		}
		m_quadrantDataGridView.ResumeLayout();
	}

	private void OnComputeQuadrants(object sender, EventArgs e)
	{
		if (SlotWidth.HasValue)
		{
			decimal? slotWidth = SlotWidth;
			if (!((slotWidth.GetValueOrDefault() <= 0m) & slotWidth.HasValue))
			{
				m_errorProvider.SetError(m_slotWidthTextBox, string.Empty);
				ComputeQuadrantsForm computeQuadrantsForm = new ComputeQuadrantsForm
				{
					NumberOfQuadrants = ((NumberOfSlots == 90) ? 6 : 12),
					SlotsPerQuadrant = ((NumberOfSlots == 90) ? 15 : 6)
				};
				if (DialogResult.Cancel != computeQuadrantsForm.ShowDialog())
				{
					CommonFunctions.ComputeQuadrants(computeQuadrantsForm.StartOffset, computeQuadrantsForm.NumberOfQuadrants, SellThruSlots, SellThruOffset, computeQuadrantsForm.SlotsPerQuadrant, SlotWidth, WorkingNode);
					RefreshView();
				}
				return;
			}
		}
		m_errorProvider.SetError(m_slotWidthTextBox, "Slot Width must be a positive value.");
	}

	private void m_editSlotDataButton_Click(object sender, EventArgs e)
	{
		if (m_allowSlotDataEdit)
		{
			new EditSlotData(Service, Number.Value, Manager).ShowDialog();
			WorkingNode = Manager.FindDeckNode(Number.Value).CloneNode(deep: true);
			RefreshView();
		}
	}

	private void m_quadrantDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.ColumnIndex == DeckConfigurationDetailColumns.Offset && e.RowIndex != -1)
		{
			NumberPadForm numberPadForm = new NumberPadForm
			{
				Text = "Offset"
			};
			if (numberPadForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(numberPadForm.Number))
			{
				m_quadrantDataGridView.EditingControl.Text = numberPadForm.Number;
			}
		}
	}

	private void m_quadrantDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
	{
		if (AllowExcludeUpdates && e.RowIndex != -1 && e.ColumnIndex == DeckConfigurationDetailColumns.Excluded)
		{
			if ((bool)m_quadrantDataGridView.CurrentCell.FormattedValue)
			{
				m_quadrantDataGridView.CurrentCell.Value = false;
			}
			else
			{
				m_quadrantDataGridView.CurrentCell.Value = true;
			}
		}
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private bool IsQuadrantExcluded(int quadrant)
	{
		DataGridViewCheckBoxCell dataGridViewCheckBoxCell = m_quadrantDataGridView.Rows[quadrant].Cells[DeckConfigurationDetailColumns.Excluded] as DataGridViewCheckBoxCell;
		return dataGridViewCheckBoxCell.Value.ToString().Equals((string)dataGridViewCheckBoxCell.TrueValue, StringComparison.CurrentCultureIgnoreCase);
	}

	private void OnEditControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
	{
		m_quadrantDataGridView.EditingControl.KeyPress += OnKeyPress;
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_numberLabel = new System.Windows.Forms.Label();
		this.m_yOffsetLabel = new System.Windows.Forms.Label();
		this.m_numberOfSlotsLabel = new System.Windows.Forms.Label();
		this.m_slotWidthLabel = new System.Windows.Forms.Label();
		this.m_sellThruSlotsLabel = new System.Windows.Forms.Label();
		this.m_sellThruOffsetLabel = new System.Windows.Forms.Label();
		this.m_approachOffsetLabel = new System.Windows.Forms.Label();
		this.m_slotsPerQuadrantLabel = new System.Windows.Forms.Label();
		this.m_isQlmDeckCheckBox = new System.Windows.Forms.CheckBox();
		this.label9 = new System.Windows.Forms.Label();
		this.m_numberTextBox = new System.Windows.Forms.TextBox();
		this.m_yoffsetTextBox = new System.Windows.Forms.TextBox();
		this.m_numberOfSlotsTextBox = new System.Windows.Forms.TextBox();
		this.m_slotWidthTextBox = new System.Windows.Forms.TextBox();
		this.m_sellThruSlotsTextBox = new System.Windows.Forms.TextBox();
		this.m_sellThruOffsetTextBox = new System.Windows.Forms.TextBox();
		this.m_approachOffsetTextBox = new System.Windows.Forms.TextBox();
		this.m_slotsPerQuadrantTextBox = new System.Windows.Forms.TextBox();
		this.m_computeQuadrantsButton = new System.Windows.Forms.Button();
		this.m_hasDumpSlotCheckBox = new System.Windows.Forms.CheckBox();
		this.m_editSlotDataButton = new System.Windows.Forms.Button();
		this.m_quadrantDataGridView = new System.Windows.Forms.DataGridView();
		this.Quadrant = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Offset = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Start = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.End = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Excluded = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.m_quadrantDataGridView).BeginInit();
		base.SuspendLayout();
		this.m_okButton.Location = new System.Drawing.Point(260, 456);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(75, 23);
		this.m_okButton.TabIndex = 19;
		this.m_okButton.Text = "OK";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(OnOK);
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Location = new System.Drawing.Point(341, 456);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_cancelButton.TabIndex = 20;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(OnCancel);
		this.m_errorProvider.ContainerControl = this;
		this.m_numberLabel.AutoSize = true;
		this.m_numberLabel.Location = new System.Drawing.Point(12, 12);
		this.m_numberLabel.Name = "m_numberLabel";
		this.m_numberLabel.Size = new System.Drawing.Size(47, 13);
		this.m_numberLabel.TabIndex = 0;
		this.m_numberLabel.Text = "Number:";
		this.m_yOffsetLabel.AutoSize = true;
		this.m_yOffsetLabel.Location = new System.Drawing.Point(12, 36);
		this.m_yOffsetLabel.Name = "m_yOffsetLabel";
		this.m_yOffsetLabel.Size = new System.Drawing.Size(48, 13);
		this.m_yOffsetLabel.TabIndex = 2;
		this.m_yOffsetLabel.Text = "Y Offset:";
		this.m_numberOfSlotsLabel.AutoSize = true;
		this.m_numberOfSlotsLabel.Location = new System.Drawing.Point(12, 62);
		this.m_numberOfSlotsLabel.Name = "m_numberOfSlotsLabel";
		this.m_numberOfSlotsLabel.Size = new System.Drawing.Size(87, 13);
		this.m_numberOfSlotsLabel.TabIndex = 4;
		this.m_numberOfSlotsLabel.Text = "Number Of Slots:";
		this.m_slotWidthLabel.AutoSize = true;
		this.m_slotWidthLabel.Location = new System.Drawing.Point(12, 88);
		this.m_slotWidthLabel.Name = "m_slotWidthLabel";
		this.m_slotWidthLabel.Size = new System.Drawing.Size(59, 13);
		this.m_slotWidthLabel.TabIndex = 6;
		this.m_slotWidthLabel.Text = "Slot Width:";
		this.m_sellThruSlotsLabel.AutoSize = true;
		this.m_sellThruSlotsLabel.Location = new System.Drawing.Point(12, 114);
		this.m_sellThruSlotsLabel.Name = "m_sellThruSlotsLabel";
		this.m_sellThruSlotsLabel.Size = new System.Drawing.Size(78, 13);
		this.m_sellThruSlotsLabel.TabIndex = 8;
		this.m_sellThruSlotsLabel.Text = "Sell Thru Slots:";
		this.m_sellThruOffsetLabel.AutoSize = true;
		this.m_sellThruOffsetLabel.Location = new System.Drawing.Point(12, 140);
		this.m_sellThruOffsetLabel.Name = "m_sellThruOffsetLabel";
		this.m_sellThruOffsetLabel.Size = new System.Drawing.Size(83, 13);
		this.m_sellThruOffsetLabel.TabIndex = 10;
		this.m_sellThruOffsetLabel.Text = "Sell Thru Offset:";
		this.m_approachOffsetLabel.AutoSize = true;
		this.m_approachOffsetLabel.Location = new System.Drawing.Point(12, 188);
		this.m_approachOffsetLabel.Name = "m_approachOffsetLabel";
		this.m_approachOffsetLabel.Size = new System.Drawing.Size(87, 13);
		this.m_approachOffsetLabel.TabIndex = 12;
		this.m_approachOffsetLabel.Text = "Approach Offset:";
		this.m_approachOffsetLabel.Visible = false;
		this.m_slotsPerQuadrantLabel.AutoSize = true;
		this.m_slotsPerQuadrantLabel.Location = new System.Drawing.Point(12, 164);
		this.m_slotsPerQuadrantLabel.Name = "m_slotsPerQuadrantLabel";
		this.m_slotsPerQuadrantLabel.Size = new System.Drawing.Size(99, 13);
		this.m_slotsPerQuadrantLabel.TabIndex = 14;
		this.m_slotsPerQuadrantLabel.Text = "Slots Per Quadrant:";
		this.m_isQlmDeckCheckBox.AutoSize = true;
		this.m_isQlmDeckCheckBox.Enabled = false;
		this.m_isQlmDeckCheckBox.Location = new System.Drawing.Point(117, 218);
		this.m_isQlmDeckCheckBox.Name = "m_isQlmDeckCheckBox";
		this.m_isQlmDeckCheckBox.Size = new System.Drawing.Size(95, 17);
		this.m_isQlmDeckCheckBox.TabIndex = 16;
		this.m_isQlmDeckCheckBox.Text = "Is QLM Deck?";
		this.m_isQlmDeckCheckBox.UseVisualStyleBackColor = true;
		this.label9.AutoSize = true;
		this.label9.BackColor = System.Drawing.Color.Yellow;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label9.Location = new System.Drawing.Point(12, 260);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(265, 15);
		this.label9.TabIndex = 17;
		this.label9.Text = "Double Click The Offset to Edit Slot Data";
		this.m_numberTextBox.Location = new System.Drawing.Point(117, 9);
		this.m_numberTextBox.Name = "m_numberTextBox";
		this.m_numberTextBox.ReadOnly = true;
		this.m_numberTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_numberTextBox.TabIndex = 1;
		this.m_yoffsetTextBox.Location = new System.Drawing.Point(117, 36);
		this.m_yoffsetTextBox.Name = "m_yoffsetTextBox";
		this.m_yoffsetTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_yoffsetTextBox.TabIndex = 3;
		this.m_numberOfSlotsTextBox.Location = new System.Drawing.Point(117, 62);
		this.m_numberOfSlotsTextBox.Name = "m_numberOfSlotsTextBox";
		this.m_numberOfSlotsTextBox.ReadOnly = true;
		this.m_numberOfSlotsTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_numberOfSlotsTextBox.TabIndex = 5;
		this.m_slotWidthTextBox.Location = new System.Drawing.Point(117, 88);
		this.m_slotWidthTextBox.Name = "m_slotWidthTextBox";
		this.m_slotWidthTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_slotWidthTextBox.TabIndex = 7;
		this.m_sellThruSlotsTextBox.Location = new System.Drawing.Point(117, 114);
		this.m_sellThruSlotsTextBox.Name = "m_sellThruSlotsTextBox";
		this.m_sellThruSlotsTextBox.ReadOnly = true;
		this.m_sellThruSlotsTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_sellThruSlotsTextBox.TabIndex = 9;
		this.m_sellThruOffsetTextBox.Location = new System.Drawing.Point(117, 140);
		this.m_sellThruOffsetTextBox.Name = "m_sellThruOffsetTextBox";
		this.m_sellThruOffsetTextBox.ReadOnly = true;
		this.m_sellThruOffsetTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_sellThruOffsetTextBox.TabIndex = 11;
		this.m_approachOffsetTextBox.Location = new System.Drawing.Point(117, 188);
		this.m_approachOffsetTextBox.Name = "m_approachOffsetTextBox";
		this.m_approachOffsetTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_approachOffsetTextBox.TabIndex = 13;
		this.m_approachOffsetTextBox.Visible = false;
		this.m_slotsPerQuadrantTextBox.Location = new System.Drawing.Point(117, 164);
		this.m_slotsPerQuadrantTextBox.Name = "m_slotsPerQuadrantTextBox";
		this.m_slotsPerQuadrantTextBox.ReadOnly = true;
		this.m_slotsPerQuadrantTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_slotsPerQuadrantTextBox.TabIndex = 15;
		this.m_computeQuadrantsButton.Location = new System.Drawing.Point(287, 250);
		this.m_computeQuadrantsButton.Name = "m_computeQuadrantsButton";
		this.m_computeQuadrantsButton.Size = new System.Drawing.Size(129, 23);
		this.m_computeQuadrantsButton.TabIndex = 24;
		this.m_computeQuadrantsButton.Text = "Compute Quadrants...";
		this.m_computeQuadrantsButton.UseVisualStyleBackColor = true;
		this.m_computeQuadrantsButton.Click += new System.EventHandler(OnComputeQuadrants);
		this.m_hasDumpSlotCheckBox.AutoSize = true;
		this.m_hasDumpSlotCheckBox.Location = new System.Drawing.Point(117, 241);
		this.m_hasDumpSlotCheckBox.Name = "m_hasDumpSlotCheckBox";
		this.m_hasDumpSlotCheckBox.Size = new System.Drawing.Size(103, 17);
		this.m_hasDumpSlotCheckBox.TabIndex = 25;
		this.m_hasDumpSlotCheckBox.Text = "Has Dump Slot?";
		this.m_hasDumpSlotCheckBox.UseVisualStyleBackColor = true;
		this.m_editSlotDataButton.Enabled = false;
		this.m_editSlotDataButton.Location = new System.Drawing.Point(287, 221);
		this.m_editSlotDataButton.Name = "m_editSlotDataButton";
		this.m_editSlotDataButton.Size = new System.Drawing.Size(129, 23);
		this.m_editSlotDataButton.TabIndex = 26;
		this.m_editSlotDataButton.Text = "Edit Slot Data";
		this.m_editSlotDataButton.UseVisualStyleBackColor = true;
		this.m_editSlotDataButton.Visible = false;
		this.m_editSlotDataButton.Click += new System.EventHandler(m_editSlotDataButton_Click);
		this.m_quadrantDataGridView.AllowUserToAddRows = false;
		this.m_quadrantDataGridView.AllowUserToDeleteRows = false;
		this.m_quadrantDataGridView.AllowUserToResizeRows = false;
		this.m_quadrantDataGridView.BackgroundColor = System.Drawing.Color.White;
		this.m_quadrantDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_quadrantDataGridView.Columns.AddRange(this.Quadrant, this.Offset, this.Start, this.End, this.Excluded);
		this.m_quadrantDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
		this.m_quadrantDataGridView.Location = new System.Drawing.Point(12, 281);
		this.m_quadrantDataGridView.Name = "m_quadrantDataGridView";
		this.m_quadrantDataGridView.RowHeadersVisible = false;
		this.m_quadrantDataGridView.RowHeadersWidth = 25;
		this.m_quadrantDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.m_quadrantDataGridView.Size = new System.Drawing.Size(404, 169);
		this.m_quadrantDataGridView.TabIndex = 27;
		this.m_quadrantDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(m_quadrantDataGridView_CellDoubleClick);
		this.m_quadrantDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(OnEditControlShowing);
		this.m_quadrantDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(m_quadrantDataGridView_CellContentClick);
		this.Quadrant.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Quadrant.FillWeight = 75f;
		this.Quadrant.HeaderText = "Quadrant";
		this.Quadrant.Name = "Quadrant";
		this.Quadrant.ReadOnly = true;
		this.Quadrant.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.Quadrant.Width = 57;
		this.Offset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Offset.FillWeight = 75f;
		this.Offset.HeaderText = "Offset";
		this.Offset.Name = "Offset";
		this.Offset.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.Offset.Width = 41;
		this.Start.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Start.FillWeight = 75f;
		this.Start.HeaderText = "Start Slot";
		this.Start.Name = "Start";
		this.Start.ReadOnly = true;
		this.Start.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.Start.Width = 56;
		this.End.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.End.FillWeight = 75f;
		this.End.HeaderText = "End Slot";
		this.End.Name = "End";
		this.End.ReadOnly = true;
		this.End.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.End.Width = 53;
		this.Excluded.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
		dataGridViewCellStyle.NullValue = false;
		this.Excluded.DefaultCellStyle = dataGridViewCellStyle;
		this.Excluded.FalseValue = "false";
		this.Excluded.FillWeight = 60f;
		this.Excluded.HeaderText = "Excluded?";
		this.Excluded.IndeterminateValue = "null";
		this.Excluded.Name = "Excluded";
		this.Excluded.TrueValue = "true";
		base.AcceptButton = this.m_okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.ClientSize = new System.Drawing.Size(428, 491);
		base.ControlBox = false;
		base.Controls.Add(this.m_quadrantDataGridView);
		base.Controls.Add(this.m_editSlotDataButton);
		base.Controls.Add(this.m_hasDumpSlotCheckBox);
		base.Controls.Add(this.m_computeQuadrantsButton);
		base.Controls.Add(this.m_slotsPerQuadrantTextBox);
		base.Controls.Add(this.m_approachOffsetTextBox);
		base.Controls.Add(this.m_sellThruOffsetTextBox);
		base.Controls.Add(this.m_sellThruSlotsTextBox);
		base.Controls.Add(this.m_slotWidthTextBox);
		base.Controls.Add(this.m_numberOfSlotsTextBox);
		base.Controls.Add(this.m_yoffsetTextBox);
		base.Controls.Add(this.m_numberTextBox);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.m_isQlmDeckCheckBox);
		base.Controls.Add(this.m_slotsPerQuadrantLabel);
		base.Controls.Add(this.m_approachOffsetLabel);
		base.Controls.Add(this.m_sellThruOffsetLabel);
		base.Controls.Add(this.m_sellThruSlotsLabel);
		base.Controls.Add(this.m_slotWidthLabel);
		base.Controls.Add(this.m_numberOfSlotsLabel);
		base.Controls.Add(this.m_yOffsetLabel);
		base.Controls.Add(this.m_numberLabel);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "DeckConfigurationDetailForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Edit Deck Configuration";
		base.Load += new System.EventHandler(OnLoad);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		((System.ComponentModel.ISupportInitialize)this.m_quadrantDataGridView).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
