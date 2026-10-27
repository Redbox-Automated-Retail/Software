using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class PickerSensorsBar : UserControl
{
	public delegate void SensorOperationHandler(bool readError);

	public delegate void BarToggle(string msg, string response);

	private readonly Dictionary<int, Panel> m_sensors = new Dictionary<int, Panel>();

	private readonly ButtonAspectsManager ButtonManager;

	private IContainer components;

	private Panel m_sensor1Panel;

	private Panel m_sensor2Panel;

	private Panel m_sensor3Panel;

	private Panel m_sensor4Panel;

	private Panel m_sensor5Panel;

	private Panel m_sensor6Panel;

	private Button m_readButton;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private Button m_onButton;

	private Button m_offButton;

	public HardwareService Service { get; set; }

	public event SensorOperationHandler ReadEvents;

	public event BarToggle BarEvents;

	public PickerSensorsBar(HardwareService service)
	{
		InitializeComponent();
		m_sensors[0] = m_sensor1Panel;
		m_sensors[1] = m_sensor2Panel;
		m_sensors[2] = m_sensor3Panel;
		m_sensors[3] = m_sensor4Panel;
		m_sensors[4] = m_sensor5Panel;
		m_sensors[5] = m_sensor6Panel;
		Service = service;
		ButtonManager = new ButtonAspectsManager();
	}

	private void ExecuteBarInstruction(object sender, EventArgs e)
	{
		using (ButtonManager.MakeAspect(sender))
		{
			string text = ((Button)sender).Tag as string;
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, text);
			if (hardwareJob != null)
			{
				string topOfStack = hardwareJob.GetTopOfStack();
				if (BarEvents != null)
				{
					BarEvents(text, topOfStack);
				}
			}
		}
	}

	private void Read_Click(object sender, EventArgs e)
	{
		using (ButtonManager.MakeAspect(sender))
		{
			ReadSensorsInner();
		}
	}

	private void ReadSensorsInner()
	{
		bool readError = false;
		try
		{
			string instruction = "SENSOR READ PICKER-SENSOR=1..6";
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, instruction);
			if (hardwareJob == null)
			{
				return;
			}
			if (!hardwareJob.GetStack(out var stack).Success)
			{
				readError = true;
				return;
			}
			if (stack.Count == 0)
			{
				readError = true;
				return;
			}
			if (int.Parse(stack.Pop()) == 0)
			{
				readError = true;
				return;
			}
			bool[] array = new bool[6];
			for (int num = 5; num >= 0; num--)
			{
				string text = stack.Pop();
				array[num] = (text.Contains("BLOCKED") ? true : false);
			}
			DisplaySensors(array);
		}
		finally
		{
			if (ReadEvents != null)
			{
				ReadEvents(readError);
			}
		}
	}

	private void DisplaySensors(bool[] sensors)
	{
		for (int i = 0; i < sensors.Length; i++)
		{
			m_sensors[i].BackColor = (sensors[i] ? Color.Green : Color.WhiteSmoke);
		}
		Application.DoEvents();
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
		this.m_sensor1Panel = new System.Windows.Forms.Panel();
		this.m_sensor2Panel = new System.Windows.Forms.Panel();
		this.m_sensor3Panel = new System.Windows.Forms.Panel();
		this.m_sensor4Panel = new System.Windows.Forms.Panel();
		this.m_sensor5Panel = new System.Windows.Forms.Panel();
		this.m_sensor6Panel = new System.Windows.Forms.Panel();
		this.m_readButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.m_onButton = new System.Windows.Forms.Button();
		this.m_offButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_sensor1Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor1Panel.Enabled = false;
		this.m_sensor1Panel.Location = new System.Drawing.Point(14, 16);
		this.m_sensor1Panel.Name = "m_sensor1Panel";
		this.m_sensor1Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor1Panel.TabIndex = 0;
		this.m_sensor2Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor2Panel.Location = new System.Drawing.Point(70, 16);
		this.m_sensor2Panel.Name = "m_sensor2Panel";
		this.m_sensor2Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor2Panel.TabIndex = 1;
		this.m_sensor3Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor3Panel.Location = new System.Drawing.Point(127, 16);
		this.m_sensor3Panel.Name = "m_sensor3Panel";
		this.m_sensor3Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor3Panel.TabIndex = 2;
		this.m_sensor4Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor4Panel.Location = new System.Drawing.Point(184, 16);
		this.m_sensor4Panel.Name = "m_sensor4Panel";
		this.m_sensor4Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor4Panel.TabIndex = 3;
		this.m_sensor5Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor5Panel.Location = new System.Drawing.Point(240, 16);
		this.m_sensor5Panel.Name = "m_sensor5Panel";
		this.m_sensor5Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor5Panel.TabIndex = 4;
		this.m_sensor6Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_sensor6Panel.Location = new System.Drawing.Point(295, 16);
		this.m_sensor6Panel.Name = "m_sensor6Panel";
		this.m_sensor6Panel.Size = new System.Drawing.Size(40, 35);
		this.m_sensor6Panel.TabIndex = 5;
		this.m_readButton.BackColor = System.Drawing.Color.LightGray;
		this.m_readButton.Location = new System.Drawing.Point(14, 57);
		this.m_readButton.Name = "m_readButton";
		this.m_readButton.Size = new System.Drawing.Size(75, 45);
		this.m_readButton.TabIndex = 6;
		this.m_readButton.Text = "Read";
		this.m_readButton.UseVisualStyleBackColor = false;
		this.m_readButton.Click += new System.EventHandler(Read_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(30, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(13, 13);
		this.label1.TabIndex = 9;
		this.label1.Text = "1";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(86, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(13, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "2";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(140, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(13, 13);
		this.label3.TabIndex = 11;
		this.label3.Text = "3";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(198, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(13, 13);
		this.label4.TabIndex = 12;
		this.label4.Text = "4";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(255, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(13, 13);
		this.label5.TabIndex = 13;
		this.label5.Text = "5";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(311, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(13, 13);
		this.label6.TabIndex = 14;
		this.label6.Text = "6";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.m_onButton.BackColor = System.Drawing.Color.LightGray;
		this.m_onButton.Location = new System.Drawing.Point(127, 57);
		this.m_onButton.Name = "m_onButton";
		this.m_onButton.Size = new System.Drawing.Size(75, 45);
		this.m_onButton.TabIndex = 15;
		this.m_onButton.Tag = "SENSOR PICKER-ON";
		this.m_onButton.Text = "On";
		this.m_onButton.UseVisualStyleBackColor = false;
		this.m_onButton.Click += new System.EventHandler(ExecuteBarInstruction);
		this.m_offButton.BackColor = System.Drawing.Color.LightGray;
		this.m_offButton.Location = new System.Drawing.Point(240, 57);
		this.m_offButton.Name = "m_offButton";
		this.m_offButton.Size = new System.Drawing.Size(75, 45);
		this.m_offButton.TabIndex = 16;
		this.m_offButton.Tag = "SENSOR PICKER-OFF";
		this.m_offButton.Text = "Off";
		this.m_offButton.UseVisualStyleBackColor = false;
		this.m_offButton.Click += new System.EventHandler(ExecuteBarInstruction);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_offButton);
		base.Controls.Add(this.m_onButton);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_readButton);
		base.Controls.Add(this.m_sensor6Panel);
		base.Controls.Add(this.m_sensor5Panel);
		base.Controls.Add(this.m_sensor4Panel);
		base.Controls.Add(this.m_sensor3Panel);
		base.Controls.Add(this.m_sensor2Panel);
		base.Controls.Add(this.m_sensor1Panel);
		base.Name = "PickerSensorsBar";
		base.Size = new System.Drawing.Size(349, 113);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
