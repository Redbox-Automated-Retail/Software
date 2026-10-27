using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public class IncrementalMoveForm : Form
{
	private readonly ILogger Log;

	private IContainer components;

	private Label label1;

	private TextBox m_unitsTextBox;

	private Button button1;

	private Button button2;

	private ErrorProvider m_errorProvider;

	private RadioButton m_yAxisRadio;

	private RadioButton m_xAxisRadio;

	private Button button3;

	private TextBox textBox1;

	public int Units
	{
		get
		{
			if (int.TryParse(m_unitsTextBox.Text, out var result))
			{
				return result;
			}
			m_errorProvider.SetError(m_unitsTextBox, "Please type a number.");
			return 0;
		}
	}

	public string Axis
	{
		get
		{
			if (m_xAxisRadio.Checked)
			{
				return "X";
			}
			return "Y";
		}
	}

	private HardwareService Service { get; set; }

	public IncrementalMoveForm()
	{
		InitializeComponent();
	}

	public IncrementalMoveForm(HardwareService service)
		: this()
	{
		Service = service;
		Log = ServiceLocator.Instance.GetService<ILogger>();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void IncrementalMoveForm_Load(object sender, EventArgs e)
	{
	}

	private void m_xAxisRadio_CheckedChanged_1(object s, EventArgs e)
	{
	}

	private void button3_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		MoveAxis moveAxis = Enum<MoveAxis>.ParseIgnoringCase(Axis, MoveAxis.X);
		try
		{
			if (MoveAxis.Y == moveAxis)
			{
				if (Math.Abs(Units) > 200)
				{
					m_errorProvider.SetError(m_unitsTextBox, "Move cannot be more than 200 units.");
					return;
				}
			}
			else if (moveAxis == MoveAxis.X && Math.Abs(Units) > 50)
			{
				m_errorProvider.SetError(m_unitsTextBox, "Move cannot be more than 50 units.");
				return;
			}
			MotorPosition position = new MotorPosition(moveAxis);
			MoveHelper moveHelper = new MoveHelper(Service);
			moveHelper.GetPosition(ref position);
			if (position.ReadOk)
			{
				int units = Units + position.Position;
				if (!moveHelper.MoveAbs(moveAxis, units, checkSensors: false).MoveOk)
				{
					Log.Log("Execute immediate failed ... ", LogEntryType.Error);
				}
			}
		}
		catch (Exception e2)
		{
			Log.Log("*** exception ***", e2);
		}
	}

	private void IncrementalMoveForm_Load_1(object sender, EventArgs e)
	{
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_unitsTextBox = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_xAxisRadio = new System.Windows.Forms.RadioButton();
		this.m_yAxisRadio = new System.Windows.Forms.RadioButton();
		this.button3 = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(16, 85);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(31, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Units";
		this.m_unitsTextBox.Location = new System.Drawing.Point(53, 78);
		this.m_unitsTextBox.Name = "m_unitsTextBox";
		this.m_unitsTextBox.Size = new System.Drawing.Size(87, 20);
		this.m_unitsTextBox.TabIndex = 1;
		this.button1.Location = new System.Drawing.Point(19, 136);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 5;
		this.button1.Text = "Done";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.button2.Location = new System.Drawing.Point(111, 136);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 23);
		this.button2.TabIndex = 6;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.m_errorProvider.ContainerControl = this;
		this.m_xAxisRadio.AutoSize = true;
		this.m_xAxisRadio.Checked = true;
		this.m_xAxisRadio.Location = new System.Drawing.Point(19, 101);
		this.m_xAxisRadio.Name = "m_xAxisRadio";
		this.m_xAxisRadio.Size = new System.Drawing.Size(53, 17);
		this.m_xAxisRadio.TabIndex = 7;
		this.m_xAxisRadio.TabStop = true;
		this.m_xAxisRadio.Text = "X axis";
		this.m_xAxisRadio.UseVisualStyleBackColor = true;
		this.m_yAxisRadio.AutoSize = true;
		this.m_yAxisRadio.Location = new System.Drawing.Point(87, 101);
		this.m_yAxisRadio.Name = "m_yAxisRadio";
		this.m_yAxisRadio.Size = new System.Drawing.Size(53, 17);
		this.m_yAxisRadio.TabIndex = 8;
		this.m_yAxisRadio.TabStop = true;
		this.m_yAxisRadio.Text = "Y axis";
		this.m_yAxisRadio.UseVisualStyleBackColor = true;
		this.button3.Location = new System.Drawing.Point(146, 76);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 23);
		this.button3.TabIndex = 9;
		this.button3.Text = "Move";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.textBox1.BackColor = System.Drawing.Color.Yellow;
		this.textBox1.Location = new System.Drawing.Point(19, 12);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.ReadOnly = true;
		this.textBox1.Size = new System.Drawing.Size(212, 48);
		this.textBox1.TabIndex = 10;
		this.textBox1.Text = "WARNING: clicking 'MOVE' will move the drum or picker *WITHOUT PERFORMING A SENSOR CHECK*!";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(275, 171);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.m_yAxisRadio);
		base.Controls.Add(this.m_xAxisRadio);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.m_unitsTextBox);
		base.Controls.Add(this.label1);
		base.Name = "IncrementalMoveForm";
		this.Text = "Move Units Form";
		base.Load += new System.EventHandler(IncrementalMoveForm_Load_1);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
