using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class LocationNumberPad : Form
{
	private IContainer components;

	private Button button1;

	private Button button2;

	private Button button3;

	private Button button4;

	private Button button5;

	private Button button7;

	private Button button8;

	private Button button9;

	private Button button6;

	private TextBox m_displayTextBox;

	private Button button11;

	private Button button12;

	private Button button10;

	private Button button13;

	public int Number
	{
		get
		{
			if (int.TryParse(m_displayTextBox.Text, out var result))
			{
				return result;
			}
			return -1;
		}
	}

	public LocationNumberPad()
	{
		InitializeComponent();
	}

	private void OnNumberButtonPush(object sender, EventArgs e)
	{
		string text = m_displayTextBox.Text;
		string number = GetNumber(sender);
		m_displayTextBox.Text = text + number;
	}

	private string GetNumber(object sender)
	{
		if (!(sender is Button button))
		{
			return string.Empty;
		}
		if (button.Tag is string result)
		{
			return result;
		}
		return string.Empty;
	}

	private void button11_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(m_displayTextBox.Text))
		{
			base.DialogResult = DialogResult.Cancel;
		}
		else
		{
			base.DialogResult = DialogResult.OK;
		}
		Close();
	}

	private void button12_Click(object sender, EventArgs e)
	{
		m_displayTextBox.Text = string.Empty;
	}

	private void button10_Click(object sender, EventArgs e)
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
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.button5 = new System.Windows.Forms.Button();
		this.button7 = new System.Windows.Forms.Button();
		this.button8 = new System.Windows.Forms.Button();
		this.button9 = new System.Windows.Forms.Button();
		this.button6 = new System.Windows.Forms.Button();
		this.m_displayTextBox = new System.Windows.Forms.TextBox();
		this.button11 = new System.Windows.Forms.Button();
		this.button12 = new System.Windows.Forms.Button();
		this.button10 = new System.Windows.Forms.Button();
		this.button13 = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Location = new System.Drawing.Point(12, 62);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 49);
		this.button1.TabIndex = 0;
		this.button1.Tag = "1";
		this.button1.Text = "1";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(OnNumberButtonPush);
		this.button2.BackColor = System.Drawing.Color.LightGray;
		this.button2.Location = new System.Drawing.Point(105, 62);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 49);
		this.button2.TabIndex = 1;
		this.button2.Tag = "2";
		this.button2.Text = "2";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(OnNumberButtonPush);
		this.button3.BackColor = System.Drawing.Color.LightGray;
		this.button3.Location = new System.Drawing.Point(197, 62);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 49);
		this.button3.TabIndex = 2;
		this.button3.Tag = "3";
		this.button3.Text = "3";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(OnNumberButtonPush);
		this.button4.BackColor = System.Drawing.Color.LightGray;
		this.button4.Location = new System.Drawing.Point(12, 130);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(75, 49);
		this.button4.TabIndex = 3;
		this.button4.Tag = "4";
		this.button4.Text = "4";
		this.button4.UseVisualStyleBackColor = false;
		this.button4.Click += new System.EventHandler(OnNumberButtonPush);
		this.button5.BackColor = System.Drawing.Color.LightGray;
		this.button5.Location = new System.Drawing.Point(105, 130);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(75, 49);
		this.button5.TabIndex = 4;
		this.button5.Tag = "5";
		this.button5.Text = "5";
		this.button5.UseVisualStyleBackColor = false;
		this.button5.Click += new System.EventHandler(OnNumberButtonPush);
		this.button7.BackColor = System.Drawing.Color.LightGray;
		this.button7.Location = new System.Drawing.Point(12, 204);
		this.button7.Name = "button7";
		this.button7.Size = new System.Drawing.Size(75, 49);
		this.button7.TabIndex = 6;
		this.button7.Tag = "7";
		this.button7.Text = "7";
		this.button7.UseVisualStyleBackColor = false;
		this.button7.Click += new System.EventHandler(OnNumberButtonPush);
		this.button8.BackColor = System.Drawing.Color.LightGray;
		this.button8.Location = new System.Drawing.Point(105, 204);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(75, 49);
		this.button8.TabIndex = 7;
		this.button8.Tag = "8";
		this.button8.Text = "8";
		this.button8.UseVisualStyleBackColor = false;
		this.button8.Click += new System.EventHandler(OnNumberButtonPush);
		this.button9.BackColor = System.Drawing.Color.LightGray;
		this.button9.Location = new System.Drawing.Point(197, 204);
		this.button9.Name = "button9";
		this.button9.Size = new System.Drawing.Size(75, 49);
		this.button9.TabIndex = 8;
		this.button9.Tag = "9";
		this.button9.Text = "9";
		this.button9.UseVisualStyleBackColor = false;
		this.button9.Click += new System.EventHandler(OnNumberButtonPush);
		this.button6.BackColor = System.Drawing.Color.LightGray;
		this.button6.Location = new System.Drawing.Point(197, 130);
		this.button6.Name = "button6";
		this.button6.Size = new System.Drawing.Size(75, 49);
		this.button6.TabIndex = 9;
		this.button6.Tag = "6";
		this.button6.Text = "6";
		this.button6.UseVisualStyleBackColor = false;
		this.button6.Click += new System.EventHandler(OnNumberButtonPush);
		this.m_displayTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
		this.m_displayTextBox.Location = new System.Drawing.Point(12, 21);
		this.m_displayTextBox.Name = "m_displayTextBox";
		this.m_displayTextBox.ReadOnly = true;
		this.m_displayTextBox.Size = new System.Drawing.Size(270, 20);
		this.m_displayTextBox.TabIndex = 11;
		this.button11.BackColor = System.Drawing.Color.LightGray;
		this.button11.Location = new System.Drawing.Point(314, 62);
		this.button11.Name = "button11";
		this.button11.Size = new System.Drawing.Size(75, 49);
		this.button11.TabIndex = 12;
		this.button11.Text = "Ok";
		this.button11.UseVisualStyleBackColor = false;
		this.button11.Click += new System.EventHandler(button11_Click);
		this.button12.BackColor = System.Drawing.Color.LightGray;
		this.button12.Location = new System.Drawing.Point(314, 204);
		this.button12.Name = "button12";
		this.button12.Size = new System.Drawing.Size(75, 49);
		this.button12.TabIndex = 13;
		this.button12.Text = "Clear";
		this.button12.UseVisualStyleBackColor = false;
		this.button12.Click += new System.EventHandler(button12_Click);
		this.button10.BackColor = System.Drawing.Color.LightGray;
		this.button10.Location = new System.Drawing.Point(314, 130);
		this.button10.Name = "button10";
		this.button10.Size = new System.Drawing.Size(75, 49);
		this.button10.TabIndex = 14;
		this.button10.Text = "Cancel";
		this.button10.UseVisualStyleBackColor = false;
		this.button10.Click += new System.EventHandler(button10_Click);
		this.button13.BackColor = System.Drawing.Color.LightGray;
		this.button13.Location = new System.Drawing.Point(105, 271);
		this.button13.Name = "button13";
		this.button13.Size = new System.Drawing.Size(75, 49);
		this.button13.TabIndex = 15;
		this.button13.Tag = "0";
		this.button13.Text = "0";
		this.button13.UseVisualStyleBackColor = false;
		this.button13.Click += new System.EventHandler(OnNumberButtonPush);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(419, 333);
		base.Controls.Add(this.button13);
		base.Controls.Add(this.button10);
		base.Controls.Add(this.button12);
		base.Controls.Add(this.button11);
		base.Controls.Add(this.m_displayTextBox);
		base.Controls.Add(this.button6);
		base.Controls.Add(this.button9);
		base.Controls.Add(this.button8);
		base.Controls.Add(this.button7);
		base.Controls.Add(this.button5);
		base.Controls.Add(this.button4);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Name = "NumberPad";
		this.Text = "NumberPad";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
