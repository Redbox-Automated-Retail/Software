using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class EditSlotData : Form
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}

	private class ESDOutput : OutputBox
	{
		internal ESDOutput(ListBox box)
			: base(box)
		{
		}
	}

	private bool RinglightActive;

	private bool SensorbarActive;

	private int StartXPosition;

	private int m_changes;

	private int? m_preferredDeck;

	private int? m_qlmDeckNumber;

	private readonly XmlNode DeckNode;

	private readonly HardwareService Service;

	private readonly OutputBox OutputBox;

	private IContainer components;

	private Button m_moveLeftButton;

	private TextBox m_encoderUnitsTextBox;

	private Button m_moveRightButton;

	private Button m_okButton;

	private Button m_cancelButton;

	private Button button1;

	private Button button2;

	private ErrorProvider m_errorProvider;

	private TextBox m_deckTextBox;

	private Label label1;

	private Button m_moveButton;

	private Panel panel1;

	private Label label2;

	private Button m_gripperFingerClose;

	private Button m_gripperFingerOpen;

	private Button m_gripperRetractButton;

	private Button m_gripperExtendButton;

	private TextBox textBox1;

	private ListBox m_OutputListBox;

	private Label lbl_IncMoves;

	private Button m_saveChangesButton;

	private GroupBox groupBox1;

	private Button m_ringlightOnButton;

	private Button m_sensorBarOffButton;

	private Button m_sensorBarOnButton;

	private Button m_ringlightOffButton;

	private CheckBox m_applyToAllDecksCheckBox;

	private TextBox m_moveStatusTB;

	private Label label3;

	internal int Deck
	{
		get
		{
			if (m_preferredDeck.HasValue)
			{
				return m_preferredDeck.Value;
			}
			string text = m_deckTextBox.Text;
			if (string.IsNullOrEmpty(text))
			{
				return -1;
			}
			if (int.TryParse(text, out var result))
			{
				m_applyToAllDecksCheckBox.Enabled = result == 1;
				m_applyToAllDecksCheckBox.Visible = result == 1;
				return result;
			}
			return -1;
		}
	}

	internal bool ApplyToAllDecks
	{
		get
		{
			if (m_preferredDeck.HasValue)
			{
				return false;
			}
			return m_applyToAllDecksCheckBox.Checked;
		}
	}

	private int CurrentEY { get; set; }

	private int CurrentEX { get; set; }

	private int OriginalEY { get; set; }

	private int OriginalEX { get; set; }

	private bool PositionsRecorded { get; set; }

	private bool MoveFailed { get; set; }

	private DecksConfigurationManager Manager { get; set; }

	public EditSlotData(HardwareService service, DecksConfigurationManager mgr)
	{
		InitializeComponent();
		OutputBox = new ESDOutput(m_OutputListBox);
		Service = service;
		Manager = mgr;
		XmlNode xmlNode = Manager.FindDeckNode(8);
		if (xmlNode != null && xmlNode.GetAttributeValue<bool>("IsQlm"))
		{
			m_qlmDeckNumber = 8;
		}
	}

	public EditSlotData(HardwareService service, DecksConfigurationManager mgr, int deck, int measureSlot)
		: this(service, mgr)
	{
		DeckNode = mgr.FindDeckNode(deck);
		m_preferredDeck = deck;
		m_deckTextBox.Text = deck.ToString();
		m_deckTextBox.ReadOnly = true;
	}

	public EditSlotData(HardwareService service, int deck, DecksConfigurationManager mgr)
		: this(service, mgr)
	{
		ToggleApplyCheckBox(deck == 1);
		m_deckTextBox.Text = deck.ToString();
	}

	private void moveRightButton_Click(object sender, EventArgs e)
	{
		moveEncoderUnits(Direction.Right);
	}

	private void cancelButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		CleanupHardware();
		Close();
	}

	private bool moveEncoderUnits(Direction direction)
	{
		if (-1 == Deck)
		{
			m_errorProvider.SetError(m_deckTextBox, "Please select a deck.");
			return false;
		}
		if (!PositionsRecorded || MoveFailed)
		{
			return false;
		}
		string text = m_encoderUnitsTextBox.Text;
		int result = 0;
		if (int.TryParse(text, out result))
		{
			if ((direction == Direction.Up || direction == Direction.Down) && Math.Abs(result) > 1000)
			{
				m_errorProvider.SetError(m_encoderUnitsTextBox, "Units exceeds threshold for axis.");
				return false;
			}
			if ((direction == Direction.Left || direction == Direction.Right) && Math.Abs(result) > 100)
			{
				m_errorProvider.SetError(m_encoderUnitsTextBox, "Units exceeds threshold for axis.");
				return false;
			}
			if (direction == Direction.Left || direction == Direction.Down)
			{
				result = -result;
			}
			bool flag = direction == Direction.Up || direction == Direction.Down;
			int num = (flag ? (CurrentEY + result) : (CurrentEX + result));
			MoveHelper moveHelper = new MoveHelper(Service);
			MoveHelperResult moveHelperResult = moveHelper.MoveAbs(flag ? MoveAxis.Y : MoveAxis.X, num);
			OutputBox.Write("MOVE {0} {1} UNITS - New Location {2} ...", direction.ToString().ToUpper(), text, num);
			if (!moveHelperResult.MoveOk)
			{
				OutputBox.Write(moveHelperResult.Status);
				return false;
			}
			if (flag)
			{
				CurrentEY = num;
			}
			else
			{
				CurrentEX = num;
			}
			OutputBox.Write(moveHelperResult.Status);
			MotorPosition position = new MotorPosition(MoveAxis.X);
			MotorPosition position2 = new MotorPosition(MoveAxis.Y);
			moveHelper.GetPosition(ref position);
			moveHelper.GetPosition(ref position2);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RinglightActive ? "RINGLIGHT ON" : ("RINGLIGHT OFF" + Environment.NewLine));
			stringBuilder.Append(SensorbarActive ? "SENSOR PICKER-ON" : ("SENSOR PICKER-OFF" + Environment.NewLine));
			Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			IncrementChange();
			return moveHelperResult.MoveOk;
		}
		m_errorProvider.SetError(m_encoderUnitsTextBox, "Must specify an integer!");
		return false;
	}

	private void m_moveLeftButton_Click(object sender, EventArgs e)
	{
		moveEncoderUnits(Direction.Left);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		moveEncoderUnits(Direction.Up);
	}

	private void button2_Click(object sender, EventArgs e)
	{
		moveEncoderUnits(Direction.Down);
	}

	private void SavePositionalState()
	{
		int num = 0;
		MotorPosition position = new MotorPosition(MoveAxis.Y);
		MoveHelper moveHelper = new MoveHelper(Service);
		moveHelper.GetPosition(ref position);
		if (position.ReadOk)
		{
			int originalEY = (CurrentEY = position.Position);
			OriginalEY = originalEY;
			num++;
		}
		MotorPosition position3 = new MotorPosition(MoveAxis.X);
		moveHelper.GetPosition(ref position3);
		if (position3.ReadOk)
		{
			int originalEY = (CurrentEX = position3.Position);
			OriginalEX = originalEY;
			num++;
		}
		PositionsRecorded = num == 2;
	}

	private void m_moveButton_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		if (m_preferredDeck.HasValue)
		{
			MoveToStart(m_preferredDeck.Value, BaseSlot(m_preferredDeck.Value));
			m_moveButton.Enabled = false;
			return;
		}
		int deck = Deck;
		if (deck == -1)
		{
			m_errorProvider.SetError(m_deckTextBox, "The deck value must be valid and not excluded.");
		}
		else if (m_qlmDeckNumber.HasValue && m_qlmDeckNumber.Value == Deck)
		{
			m_errorProvider.SetError(m_deckTextBox, "The deck value must be valid and not excluded.");
		}
		else
		{
			MoveToStart(Deck, BaseSlot(deck));
		}
	}

	private void MoveToStart(int deck, int slot)
	{
		try
		{
			MoveHelperResult moveHelperResult = new MoveHelper(Service).MoveTo(deck, slot, readLimits: true);
			if (moveHelperResult.MoveOk)
			{
				m_moveStatusTB.Text = moveHelperResult.Status;
				SavePositionalState();
				StartXPosition = CurrentEX;
				OutputBox.Write("Start positions: X = {0} Y = {1}", CurrentEX, CurrentEY);
				ClearChanges();
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(moveHelperResult.Status);
			string[] limits = moveHelperResult.Limits;
			foreach (string text in limits)
			{
				if (text.Contains("BLOCKED"))
				{
					stringBuilder.Append(" ");
					stringBuilder.Append(text);
				}
			}
			m_moveStatusTB.Text = stringBuilder.ToString();
			MessageBox.Show("Move to position failed!");
			MoveFailed = true;
		}
		finally
		{
			Application.DoEvents();
		}
	}

	private void RunSimpleCommand(object sender, EventArgs e)
	{
		if (!(sender is Button button))
		{
			return;
		}
		if (button == m_ringlightOnButton)
		{
			RinglightActive = true;
		}
		else if (button == m_ringlightOffButton)
		{
			RinglightActive = false;
		}
		if (button == m_sensorBarOnButton)
		{
			SensorbarActive = true;
		}
		else if (button == m_sensorBarOffButton)
		{
			SensorbarActive = false;
		}
		if (!(button.Tag is string statement))
		{
			return;
		}
		if (Deck == -1)
		{
			m_errorProvider.SetError(m_deckTextBox, "Please select a deck.");
			return;
		}
		if (Service == null)
		{
			NotifyCommunicationProblem();
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Concat(button.Tag, " ... "));
		if (!Service.ExecuteImmediate(statement, out var job).Success)
		{
			NotifyCommunicationProblem();
			return;
		}
		if (!job.GetStack(out var stack).Success)
		{
			NotifyCommunicationProblem();
			return;
		}
		stringBuilder.Append(stack.Pop());
		OutputBox.Write(stringBuilder.ToString());
		stringBuilder = null;
		Application.DoEvents();
	}

	private void NotifyCommunicationProblem()
	{
		OutputBox.Write("Unable to communicate with HAL. Might be on a lunch break.");
		m_errorProvider.SetError(m_deckTextBox, "Unable to communicate with HAL.");
	}

	private void ToggleApplyCheckBox(bool enabled)
	{
		m_applyToAllDecksCheckBox.Visible = enabled;
		m_applyToAllDecksCheckBox.Enabled = enabled;
	}

	private void saveChangesButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		if (m_changes > 0 && ApplyChanges())
		{
			Manager.FlushChanges(refresh: false);
		}
		CleanupHardware();
		Close();
	}

	private void button3_Click(object sender, EventArgs e)
	{
		if (ApplyChanges())
		{
			Manager.FlushChanges(refresh: false);
		}
		ClearChanges();
	}

	private bool ApplyChanges()
	{
		int deltaY = CurrentEY - OriginalEY;
		int deltaX = CurrentEX - OriginalEX;
		if (m_preferredDeck.HasValue)
		{
			return UpdateDeckNode(DeckNode, deltaX, deltaY, applyBlind: false);
		}
		if (Deck == 1 && m_applyToAllDecksCheckBox.Checked)
		{
			int num = 0;
			foreach (XmlNode item in Manager.FindAllDeckNodes())
			{
				if (UpdateDeckNode(item, deltaX, deltaY, applyBlind: true))
				{
					num++;
				}
			}
			return num > 0;
		}
		XmlNode node2 = Manager.FindDeckNode(Deck);
		return UpdateDeckNode(node2, deltaX, deltaY, applyBlind: false);
	}

	private bool UpdateDeckNode(XmlNode node, int deltaX, int deltaY, bool applyBlind)
	{
		if (deltaY == 0 && deltaX == 0)
		{
			return false;
		}
		if (node.GetAttributeValue<bool>("IsQlm"))
		{
			return false;
		}
		int attributeValue = node.GetAttributeValue<int>("Number");
		if (deltaY != 0)
		{
			int attributeValue2 = node.GetAttributeValue<int>("Offset");
			attributeValue2 += deltaY;
			node.SetAttributeValue("Offset", attributeValue2);
		}
		if (deltaX != 0)
		{
			XmlNode node2 = node.ChildNodes[0];
			decimal? attributeValue3 = node.GetAttributeValue<decimal?>("SlotWidth");
			decimal num = 0m;
			if (attributeValue == 8 && !applyBlind)
			{
				num = (decimal)StartXPosition + (decimal)deltaX - attributeValue3.Value;
			}
			else
			{
				num = node2.GetAttributeValue<int>("Offset");
				num += (decimal)deltaX;
			}
			int attributeValue4 = node.GetAttributeValue<int>("Offset");
			int? attributeValue5 = node.GetAttributeValue<int?>("NumberOfSlots");
			int? attributeValue6 = node.GetAttributeValue<int?>("SellThruSlots");
			int? attributeValue7 = node.GetAttributeValue<int?>("SellThruOffset");
			int? attributeValue8 = node.GetAttributeValue<int?>("ApproachOffset");
			bool attributeValue9 = node.GetAttributeValue<bool>("IsQlm");
			int? num2 = (attributeValue6.HasValue ? 6 : 15);
			int count = node.ChildNodes.Count;
			node.Attributes.RemoveAll();
			CommonFunctions.ComputeQuadrants(num, count, attributeValue6, attributeValue7, num2, attributeValue3, node);
			node.SetAttributeValue("Number", attributeValue);
			node.SetAttributeValue("Offset", attributeValue4);
			node.SetAttributeValue("NumberOfSlots", attributeValue5);
			node.SetAttributeValue("SlotWidth", attributeValue3);
			node.SetAttributeValue("IsQlm", attributeValue9);
			node.SetAttributeValue("SellThruSlots", attributeValue6);
			node.SetAttributeValue("SellThruOffset", attributeValue7);
			node.SetAttributeValue("ApproachOffset", attributeValue8);
			node.SetAttributeValue("SlotsPerQuadrant", num2);
		}
		return true;
	}

	private void ClearChanges()
	{
		m_changes = 0;
		m_saveChangesButton.Enabled = false;
	}

	private void IncrementChange()
	{
		m_changes++;
		m_saveChangesButton.Enabled = m_changes > 0;
	}

	private void CleanupHardware()
	{
		if (Service != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
			stringBuilder.Append("SENSOR PICKER-OFF" + Environment.NewLine);
			Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
		}
	}

	private int BaseSlot(int targetDeck)
	{
		if (targetDeck != 8)
		{
			return 1;
		}
		return 2;
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
		this.m_moveLeftButton = new System.Windows.Forms.Button();
		this.m_encoderUnitsTextBox = new System.Windows.Forms.TextBox();
		this.m_moveRightButton = new System.Windows.Forms.Button();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.label1 = new System.Windows.Forms.Label();
		this.m_deckTextBox = new System.Windows.Forms.TextBox();
		this.m_moveButton = new System.Windows.Forms.Button();
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_gripperFingerClose = new System.Windows.Forms.Button();
		this.m_gripperFingerOpen = new System.Windows.Forms.Button();
		this.m_gripperRetractButton = new System.Windows.Forms.Button();
		this.m_gripperExtendButton = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.m_OutputListBox = new System.Windows.Forms.ListBox();
		this.lbl_IncMoves = new System.Windows.Forms.Label();
		this.m_saveChangesButton = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_sensorBarOffButton = new System.Windows.Forms.Button();
		this.m_sensorBarOnButton = new System.Windows.Forms.Button();
		this.m_ringlightOffButton = new System.Windows.Forms.Button();
		this.m_ringlightOnButton = new System.Windows.Forms.Button();
		this.m_applyToAllDecksCheckBox = new System.Windows.Forms.CheckBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_moveStatusTB = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		this.panel1.SuspendLayout();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.m_moveLeftButton.BackColor = System.Drawing.Color.LightGray;
		this.m_moveLeftButton.Location = new System.Drawing.Point(529, 174);
		this.m_moveLeftButton.Name = "m_moveLeftButton";
		this.m_moveLeftButton.Size = new System.Drawing.Size(92, 40);
		this.m_moveLeftButton.TabIndex = 0;
		this.m_moveLeftButton.Text = "< Move &Left";
		this.m_moveLeftButton.UseVisualStyleBackColor = false;
		this.m_moveLeftButton.Click += new System.EventHandler(m_moveLeftButton_Click);
		this.m_encoderUnitsTextBox.Location = new System.Drawing.Point(627, 185);
		this.m_encoderUnitsTextBox.Name = "m_encoderUnitsTextBox";
		this.m_encoderUnitsTextBox.Size = new System.Drawing.Size(74, 20);
		this.m_encoderUnitsTextBox.TabIndex = 1;
		this.m_encoderUnitsTextBox.Text = "10";
		this.m_moveRightButton.BackColor = System.Drawing.Color.LightGray;
		this.m_moveRightButton.Location = new System.Drawing.Point(707, 167);
		this.m_moveRightButton.Name = "m_moveRightButton";
		this.m_moveRightButton.Size = new System.Drawing.Size(92, 40);
		this.m_moveRightButton.TabIndex = 2;
		this.m_moveRightButton.Text = "Move &Right >";
		this.m_moveRightButton.UseVisualStyleBackColor = false;
		this.m_moveRightButton.Click += new System.EventHandler(moveRightButton_Click);
		this.m_okButton.BackColor = System.Drawing.Color.LightGray;
		this.m_okButton.Location = new System.Drawing.Point(327, 425);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(90, 40);
		this.m_okButton.TabIndex = 3;
		this.m_okButton.Text = "&Ok";
		this.m_okButton.UseVisualStyleBackColor = false;
		this.m_okButton.Click += new System.EventHandler(saveChangesButton_Click);
		this.m_cancelButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cancelButton.Location = new System.Drawing.Point(423, 425);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(90, 40);
		this.m_cancelButton.TabIndex = 4;
		this.m_cancelButton.Text = "&Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = false;
		this.m_cancelButton.Click += new System.EventHandler(cancelButton_Click);
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Location = new System.Drawing.Point(626, 124);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 55);
		this.button1.TabIndex = 5;
		this.button1.Text = "&Up";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.button2.BackColor = System.Drawing.Color.LightGray;
		this.button2.Location = new System.Drawing.Point(627, 211);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 53);
		this.button2.TabIndex = 6;
		this.button2.Text = "&Down";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.m_errorProvider.ContainerControl = this;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(14, 55);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(73, 13);
		this.label1.TabIndex = 7;
		this.label1.Text = "Current Deck:";
		this.m_deckTextBox.Location = new System.Drawing.Point(23, 71);
		this.m_deckTextBox.Name = "m_deckTextBox";
		this.m_deckTextBox.Size = new System.Drawing.Size(64, 20);
		this.m_deckTextBox.TabIndex = 8;
		this.m_moveButton.BackColor = System.Drawing.Color.LightGray;
		this.m_moveButton.Location = new System.Drawing.Point(108, 50);
		this.m_moveButton.Name = "m_moveButton";
		this.m_moveButton.Size = new System.Drawing.Size(95, 47);
		this.m_moveButton.TabIndex = 13;
		this.m_moveButton.Text = "Move";
		this.m_moveButton.UseVisualStyleBackColor = false;
		this.m_moveButton.Click += new System.EventHandler(m_moveButton_Click);
		this.panel1.Controls.Add(this.m_gripperFingerClose);
		this.panel1.Controls.Add(this.m_gripperFingerOpen);
		this.panel1.Controls.Add(this.m_gripperRetractButton);
		this.panel1.Controls.Add(this.m_gripperExtendButton);
		this.panel1.Location = new System.Drawing.Point(15, 164);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(213, 115);
		this.panel1.TabIndex = 14;
		this.m_gripperFingerClose.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperFingerClose.Location = new System.Drawing.Point(117, 72);
		this.m_gripperFingerClose.Name = "m_gripperFingerClose";
		this.m_gripperFingerClose.Size = new System.Drawing.Size(92, 40);
		this.m_gripperFingerClose.TabIndex = 3;
		this.m_gripperFingerClose.Tag = "GRIPPER CLOSE";
		this.m_gripperFingerClose.Text = "Close";
		this.m_gripperFingerClose.UseVisualStyleBackColor = false;
		this.m_gripperFingerClose.Click += new System.EventHandler(RunSimpleCommand);
		this.m_gripperFingerOpen.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperFingerOpen.Location = new System.Drawing.Point(117, 3);
		this.m_gripperFingerOpen.Name = "m_gripperFingerOpen";
		this.m_gripperFingerOpen.Size = new System.Drawing.Size(92, 40);
		this.m_gripperFingerOpen.TabIndex = 2;
		this.m_gripperFingerOpen.Tag = "GRIPPER RENT";
		this.m_gripperFingerOpen.Text = "Rent";
		this.m_gripperFingerOpen.UseVisualStyleBackColor = false;
		this.m_gripperFingerOpen.Click += new System.EventHandler(RunSimpleCommand);
		this.m_gripperRetractButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperRetractButton.Location = new System.Drawing.Point(3, 72);
		this.m_gripperRetractButton.Name = "m_gripperRetractButton";
		this.m_gripperRetractButton.Size = new System.Drawing.Size(92, 40);
		this.m_gripperRetractButton.TabIndex = 1;
		this.m_gripperRetractButton.Tag = "GRIPPER RETRACT";
		this.m_gripperRetractButton.Text = "Retract";
		this.m_gripperRetractButton.UseVisualStyleBackColor = false;
		this.m_gripperRetractButton.Click += new System.EventHandler(RunSimpleCommand);
		this.m_gripperExtendButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperExtendButton.Location = new System.Drawing.Point(3, 3);
		this.m_gripperExtendButton.Name = "m_gripperExtendButton";
		this.m_gripperExtendButton.Size = new System.Drawing.Size(92, 40);
		this.m_gripperExtendButton.TabIndex = 0;
		this.m_gripperExtendButton.Tag = "GRIPPER EXTEND";
		this.m_gripperExtendButton.Text = "Extend";
		this.m_gripperExtendButton.UseVisualStyleBackColor = false;
		this.m_gripperExtendButton.Click += new System.EventHandler(RunSimpleCommand);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 148);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(114, 13);
		this.label2.TabIndex = 15;
		this.label2.Text = "Gripper Test Functions";
		this.textBox1.BackColor = System.Drawing.Color.Yellow;
		this.textBox1.Location = new System.Drawing.Point(12, 12);
		this.textBox1.Name = "textBox1";
		this.textBox1.ReadOnly = true;
		this.textBox1.Size = new System.Drawing.Size(230, 20);
		this.textBox1.TabIndex = 16;
		this.textBox1.Text = "** This tool does not work on the QLM deck. **";
		this.m_OutputListBox.FormattingEnabled = true;
		this.m_OutputListBox.Location = new System.Drawing.Point(15, 285);
		this.m_OutputListBox.Name = "m_OutputListBox";
		this.m_OutputListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this.m_OutputListBox.Size = new System.Drawing.Size(570, 134);
		this.m_OutputListBox.TabIndex = 17;
		this.lbl_IncMoves.AutoSize = true;
		this.lbl_IncMoves.Location = new System.Drawing.Point(510, 124);
		this.lbl_IncMoves.Name = "lbl_IncMoves";
		this.lbl_IncMoves.Size = new System.Drawing.Size(97, 13);
		this.lbl_IncMoves.TabIndex = 18;
		this.lbl_IncMoves.Text = "Incremental Moves";
		this.m_saveChangesButton.BackColor = System.Drawing.Color.LightGray;
		this.m_saveChangesButton.Enabled = false;
		this.m_saveChangesButton.Location = new System.Drawing.Point(231, 425);
		this.m_saveChangesButton.Name = "m_saveChangesButton";
		this.m_saveChangesButton.Size = new System.Drawing.Size(90, 40);
		this.m_saveChangesButton.TabIndex = 19;
		this.m_saveChangesButton.Text = "Save Current Changes";
		this.m_saveChangesButton.UseVisualStyleBackColor = false;
		this.m_saveChangesButton.Click += new System.EventHandler(button3_Click);
		this.groupBox1.Controls.Add(this.m_sensorBarOffButton);
		this.groupBox1.Controls.Add(this.m_sensorBarOnButton);
		this.groupBox1.Controls.Add(this.m_ringlightOffButton);
		this.groupBox1.Controls.Add(this.m_ringlightOnButton);
		this.groupBox1.Location = new System.Drawing.Point(243, 148);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(216, 131);
		this.groupBox1.TabIndex = 20;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Lighting";
		this.m_sensorBarOffButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorBarOffButton.Location = new System.Drawing.Point(118, 85);
		this.m_sensorBarOffButton.Name = "m_sensorBarOffButton";
		this.m_sensorBarOffButton.Size = new System.Drawing.Size(92, 40);
		this.m_sensorBarOffButton.TabIndex = 3;
		this.m_sensorBarOffButton.Tag = "SENSOR PICKER-OFF";
		this.m_sensorBarOffButton.Text = "Sensor Bar Off";
		this.m_sensorBarOffButton.UseVisualStyleBackColor = false;
		this.m_sensorBarOffButton.Click += new System.EventHandler(RunSimpleCommand);
		this.m_sensorBarOnButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorBarOnButton.Location = new System.Drawing.Point(118, 17);
		this.m_sensorBarOnButton.Name = "m_sensorBarOnButton";
		this.m_sensorBarOnButton.Size = new System.Drawing.Size(92, 40);
		this.m_sensorBarOnButton.TabIndex = 2;
		this.m_sensorBarOnButton.Tag = "SENSOR PICKER-ON";
		this.m_sensorBarOnButton.Text = "Sensor Bar On";
		this.m_sensorBarOnButton.UseVisualStyleBackColor = false;
		this.m_sensorBarOnButton.Click += new System.EventHandler(RunSimpleCommand);
		this.m_ringlightOffButton.BackColor = System.Drawing.Color.LightGray;
		this.m_ringlightOffButton.Location = new System.Drawing.Point(6, 85);
		this.m_ringlightOffButton.Name = "m_ringlightOffButton";
		this.m_ringlightOffButton.Size = new System.Drawing.Size(92, 40);
		this.m_ringlightOffButton.TabIndex = 1;
		this.m_ringlightOffButton.Tag = "RINGLIGHT OFF";
		this.m_ringlightOffButton.Text = "Ringlight Off";
		this.m_ringlightOffButton.UseVisualStyleBackColor = false;
		this.m_ringlightOffButton.Click += new System.EventHandler(RunSimpleCommand);
		this.m_ringlightOnButton.BackColor = System.Drawing.Color.LightGray;
		this.m_ringlightOnButton.Location = new System.Drawing.Point(6, 17);
		this.m_ringlightOnButton.Name = "m_ringlightOnButton";
		this.m_ringlightOnButton.Size = new System.Drawing.Size(92, 40);
		this.m_ringlightOnButton.TabIndex = 0;
		this.m_ringlightOnButton.Tag = "RINGLIGHT ON";
		this.m_ringlightOnButton.Text = "Ringlight On";
		this.m_ringlightOnButton.UseVisualStyleBackColor = false;
		this.m_ringlightOnButton.Click += new System.EventHandler(RunSimpleCommand);
		this.m_applyToAllDecksCheckBox.AutoSize = true;
		this.m_applyToAllDecksCheckBox.Enabled = false;
		this.m_applyToAllDecksCheckBox.Location = new System.Drawing.Point(23, 107);
		this.m_applyToAllDecksCheckBox.Name = "m_applyToAllDecksCheckBox";
		this.m_applyToAllDecksCheckBox.Size = new System.Drawing.Size(115, 17);
		this.m_applyToAllDecksCheckBox.TabIndex = 21;
		this.m_applyToAllDecksCheckBox.Text = "Apply to all decks?";
		this.m_applyToAllDecksCheckBox.UseVisualStyleBackColor = true;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(228, 50);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(67, 13);
		this.label3.TabIndex = 22;
		this.label3.Text = "Move Status";
		this.m_moveStatusTB.Location = new System.Drawing.Point(231, 71);
		this.m_moveStatusTB.Name = "m_moveStatusTB";
		this.m_moveStatusTB.Size = new System.Drawing.Size(186, 20);
		this.m_moveStatusTB.TabIndex = 23;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(832, 488);
		base.ControlBox = false;
		base.Controls.Add(this.m_moveStatusTB);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_applyToAllDecksCheckBox);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_saveChangesButton);
		base.Controls.Add(this.lbl_IncMoves);
		base.Controls.Add(this.m_OutputListBox);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.m_moveButton);
		base.Controls.Add(this.m_deckTextBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_moveRightButton);
		base.Controls.Add(this.m_encoderUnitsTextBox);
		base.Controls.Add(this.m_moveLeftButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "EditSlotData";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Edit Slot Data";
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		this.panel1.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
