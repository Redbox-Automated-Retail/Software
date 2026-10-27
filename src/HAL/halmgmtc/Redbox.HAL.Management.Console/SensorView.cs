using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class SensorView : UserControl
{
	private static SensorView m_instance;

	private bool[] m_sensors = new bool[6];

	private IContainer components;

	private Panel m_panel;

	private Button m_sensorReadButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool[] Sensors
	{
		get
		{
			return m_sensors;
		}
		set
		{
			m_sensors = value;
			RefreshSensors();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static SensorView Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new SensorView();
			}
			return m_instance;
		}
	}

	public void ResetSensors()
	{
		for (int i = 0; i < Sensors.Length; i++)
		{
			Sensors[i] = false;
		}
		RefreshSensors();
	}

	public void RefreshSensors()
	{
		m_panel.Invalidate();
	}

	private SensorView()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		m_sensorReadButton.Click += CommonFunctions.OnReadPickerSensors;
		m_sensorReadButton.EnabledChanged += OnSensorButtonEnableStateChanged;
		EnvironmentHelper.ExecutingImmediateStausChanged += OnExecuteImmediateStatusChanged;
		m_panel.BackColor = Color.WhiteSmoke;
	}

	private void OnExecuteImmediateStatusChanged(object sender, BoolEventArgs e)
	{
		m_sensorReadButton.Enabled = !e.State;
	}

	private void OnSensorButtonEnableStateChanged(object sender, EventArgs e)
	{
		if (m_sensorReadButton.Enabled)
		{
			m_sensorReadButton.FlatStyle = FlatStyle.Standard;
		}
		else
		{
			m_sensorReadButton.FlatStyle = FlatStyle.Flat;
		}
	}

	private void OnPaint(object sender, PaintEventArgs e)
	{
		e.Graphics.FillRectangle(SystemBrushes.ControlLightLight, 0, 0, m_panel.Width, m_panel.Height);
		int num = 1;
		int num2 = 28;
		int num3 = Math.Max(m_panel.Width, 180) / Sensors.Length;
		int num4 = (num3 - num2) / 2;
		float num5 = (float)m_panel.Height / 2f;
		if (m_panel.Width >= num2 * 6 + num3 * 6)
		{
		}
		bool[] sensors = Sensors;
		foreach (bool num6 in sensors)
		{
			e.Graphics.DrawString(num.ToString(), Font, Brushes.Black, num4 + 8, num5 - 19f);
			e.Graphics.FillRectangle(Brushes.DarkGray, num4 + 2, num5 + 2f, num2, num2);
			e.Graphics.FillRectangle(Brushes.Black, num4, num5, num2, num2);
			if (num6)
			{
				e.Graphics.FillRectangle(Brushes.Green, num4, num5, num2, num2);
			}
			else
			{
				e.Graphics.DrawRectangle(Pens.CornflowerBlue, num4, num5, num2, num2);
			}
			num4 += num3;
			num++;
		}
	}

	private void OnResize(object sender, EventArgs e)
	{
		m_panel.Invalidate();
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
		this.m_panel = new System.Windows.Forms.Panel();
		this.m_sensorReadButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_panel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_panel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_panel.Location = new System.Drawing.Point(0, 0);
		this.m_panel.Name = "m_panel";
		this.m_panel.Size = new System.Drawing.Size(150, 104);
		this.m_panel.TabIndex = 0;
		this.m_panel.Paint += new System.Windows.Forms.PaintEventHandler(OnPaint);
		this.m_sensorReadButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_sensorReadButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorReadButton.Location = new System.Drawing.Point(0, 110);
		this.m_sensorReadButton.Name = "m_sensorReadButton";
		this.m_sensorReadButton.Size = new System.Drawing.Size(147, 37);
		this.m_sensorReadButton.TabIndex = 1;
		this.m_sensorReadButton.Text = "Read Sensors";
		this.m_sensorReadButton.UseVisualStyleBackColor = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.Controls.Add(this.m_sensorReadButton);
		base.Controls.Add(this.m_panel);
		base.Name = "SensorView";
		base.Resize += new System.EventHandler(OnResize);
		base.ResumeLayout(false);
	}
}
