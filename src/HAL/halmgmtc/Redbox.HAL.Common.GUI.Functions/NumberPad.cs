using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class NumberPad : UserControl
{
	private IContainer components;

	private TableLayoutPanel m_tableLayoutPanel;

	private Button m_sevenButton;

	private Button m_eightButton;

	private Button m_nineButton;

	private Button m_fourButton;

	private Button m_fiveButton;

	private Button m_sixButton;

	private Button m_oneButton;

	private Button m_twoButton;

	private Button m_threeButton;

	private Button m_zeroButton;

	private Button m_clearButton;

	private TextBox m_textBox;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Number
	{
		get
		{
			return m_textBox.Text;
		}
		set
		{
			m_textBox.Text = value;
		}
	}

	public int MaxLength
	{
		get
		{
			return m_textBox.MaxLength;
		}
		set
		{
			m_textBox.MaxLength = value;
		}
	}

	public event EventHandler NumberChanged;

	public NumberPad()
	{
		InitializeComponent();
		m_textBox.MaxLength = 40;
	}

	public void Clear()
	{
		m_textBox.Clear();
	}

	private void OnNumberPadButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			if (button.Text == "C")
			{
				Clear();
			}
			else
			{
				m_textBox.Text += button.Text;
			}
		}
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Control)
		{
			return;
		}
		switch (e.KeyCode)
		{
		case Keys.V:
			e.Handled = true;
			e.SuppressKeyPress = true;
			if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
			{
				m_textBox.Paste();
			}
			break;
		case Keys.C:
			e.Handled = true;
			e.SuppressKeyPress = true;
			if (m_textBox.SelectedText.Length > 0)
			{
				m_textBox.Copy();
			}
			break;
		case Keys.X:
			e.Handled = true;
			e.SuppressKeyPress = true;
			if (m_textBox.SelectedText.Length > 0)
			{
				m_textBox.Cut();
			}
			break;
		}
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private void OnTextChanged(object sender, EventArgs e)
	{
		if (NumberChanged != null)
		{
			NumberChanged(this, EventArgs.Empty);
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
		this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.m_sevenButton = new System.Windows.Forms.Button();
		this.m_eightButton = new System.Windows.Forms.Button();
		this.m_nineButton = new System.Windows.Forms.Button();
		this.m_fourButton = new System.Windows.Forms.Button();
		this.m_fiveButton = new System.Windows.Forms.Button();
		this.m_sixButton = new System.Windows.Forms.Button();
		this.m_oneButton = new System.Windows.Forms.Button();
		this.m_twoButton = new System.Windows.Forms.Button();
		this.m_threeButton = new System.Windows.Forms.Button();
		this.m_zeroButton = new System.Windows.Forms.Button();
		this.m_clearButton = new System.Windows.Forms.Button();
		this.m_textBox = new System.Windows.Forms.TextBox();
		this.m_tableLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_tableLayoutPanel.ColumnCount = 4;
		this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.m_tableLayoutPanel.Controls.Add(this.m_sevenButton, 0, 1);
		this.m_tableLayoutPanel.Controls.Add(this.m_eightButton, 1, 1);
		this.m_tableLayoutPanel.Controls.Add(this.m_nineButton, 2, 1);
		this.m_tableLayoutPanel.Controls.Add(this.m_fourButton, 0, 2);
		this.m_tableLayoutPanel.Controls.Add(this.m_fiveButton, 1, 2);
		this.m_tableLayoutPanel.Controls.Add(this.m_sixButton, 2, 2);
		this.m_tableLayoutPanel.Controls.Add(this.m_oneButton, 0, 3);
		this.m_tableLayoutPanel.Controls.Add(this.m_twoButton, 1, 3);
		this.m_tableLayoutPanel.Controls.Add(this.m_threeButton, 2, 3);
		this.m_tableLayoutPanel.Controls.Add(this.m_zeroButton, 0, 4);
		this.m_tableLayoutPanel.Controls.Add(this.m_clearButton, 3, 1);
		this.m_tableLayoutPanel.Controls.Add(this.m_textBox, 0, 0);
		this.m_tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
		this.m_tableLayoutPanel.RowCount = 5;
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.04348f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.73913f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.73913f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.73913f));
		this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.73913f));
		this.m_tableLayoutPanel.Size = new System.Drawing.Size(373, 222);
		this.m_tableLayoutPanel.TabIndex = 0;
		this.m_sevenButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_sevenButton.Location = new System.Drawing.Point(3, 31);
		this.m_sevenButton.Name = "m_sevenButton";
		this.m_sevenButton.Size = new System.Drawing.Size(87, 42);
		this.m_sevenButton.TabIndex = 1;
		this.m_sevenButton.Text = "7";
		this.m_sevenButton.UseVisualStyleBackColor = true;
		this.m_sevenButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_eightButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_eightButton.Location = new System.Drawing.Point(96, 31);
		this.m_eightButton.Name = "m_eightButton";
		this.m_eightButton.Size = new System.Drawing.Size(87, 42);
		this.m_eightButton.TabIndex = 2;
		this.m_eightButton.Text = "8";
		this.m_eightButton.UseVisualStyleBackColor = true;
		this.m_eightButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_nineButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_nineButton.Location = new System.Drawing.Point(189, 31);
		this.m_nineButton.Name = "m_nineButton";
		this.m_nineButton.Size = new System.Drawing.Size(87, 42);
		this.m_nineButton.TabIndex = 3;
		this.m_nineButton.Text = "9";
		this.m_nineButton.UseVisualStyleBackColor = true;
		this.m_nineButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_fourButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_fourButton.Location = new System.Drawing.Point(3, 79);
		this.m_fourButton.Name = "m_fourButton";
		this.m_fourButton.Size = new System.Drawing.Size(87, 42);
		this.m_fourButton.TabIndex = 5;
		this.m_fourButton.Text = "4";
		this.m_fourButton.UseVisualStyleBackColor = true;
		this.m_fourButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_fiveButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_fiveButton.Location = new System.Drawing.Point(96, 79);
		this.m_fiveButton.Name = "m_fiveButton";
		this.m_fiveButton.Size = new System.Drawing.Size(87, 42);
		this.m_fiveButton.TabIndex = 6;
		this.m_fiveButton.Text = "5";
		this.m_fiveButton.UseVisualStyleBackColor = true;
		this.m_fiveButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_sixButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_sixButton.Location = new System.Drawing.Point(189, 79);
		this.m_sixButton.Name = "m_sixButton";
		this.m_sixButton.Size = new System.Drawing.Size(87, 42);
		this.m_sixButton.TabIndex = 7;
		this.m_sixButton.Text = "6";
		this.m_sixButton.UseVisualStyleBackColor = true;
		this.m_sixButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_oneButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_oneButton.Location = new System.Drawing.Point(3, 127);
		this.m_oneButton.Name = "m_oneButton";
		this.m_oneButton.Size = new System.Drawing.Size(87, 42);
		this.m_oneButton.TabIndex = 8;
		this.m_oneButton.Text = "1";
		this.m_oneButton.UseVisualStyleBackColor = true;
		this.m_oneButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_twoButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_twoButton.Location = new System.Drawing.Point(96, 127);
		this.m_twoButton.Name = "m_twoButton";
		this.m_twoButton.Size = new System.Drawing.Size(87, 42);
		this.m_twoButton.TabIndex = 9;
		this.m_twoButton.Text = "2";
		this.m_twoButton.UseVisualStyleBackColor = true;
		this.m_twoButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_threeButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_threeButton.Location = new System.Drawing.Point(189, 127);
		this.m_threeButton.Name = "m_threeButton";
		this.m_threeButton.Size = new System.Drawing.Size(87, 42);
		this.m_threeButton.TabIndex = 10;
		this.m_threeButton.Text = "3";
		this.m_threeButton.UseVisualStyleBackColor = true;
		this.m_threeButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_tableLayoutPanel.SetColumnSpan(this.m_zeroButton, 3);
		this.m_zeroButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_zeroButton.Location = new System.Drawing.Point(3, 175);
		this.m_zeroButton.Name = "m_zeroButton";
		this.m_zeroButton.Size = new System.Drawing.Size(273, 44);
		this.m_zeroButton.TabIndex = 11;
		this.m_zeroButton.Text = "0";
		this.m_zeroButton.UseVisualStyleBackColor = true;
		this.m_zeroButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_clearButton.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_clearButton.Location = new System.Drawing.Point(282, 31);
		this.m_clearButton.Name = "m_clearButton";
		this.m_clearButton.Size = new System.Drawing.Size(88, 42);
		this.m_clearButton.TabIndex = 4;
		this.m_clearButton.Text = "C";
		this.m_clearButton.UseVisualStyleBackColor = true;
		this.m_clearButton.Click += new System.EventHandler(OnNumberPadButtonClicked);
		this.m_tableLayoutPanel.SetColumnSpan(this.m_textBox, 4);
		this.m_textBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_textBox.Location = new System.Drawing.Point(3, 3);
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.Size = new System.Drawing.Size(367, 20);
		this.m_textBox.TabIndex = 0;
		this.m_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_textBox.TextChanged += new System.EventHandler(OnTextChanged);
		this.m_textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDown);
		this.m_textBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_tableLayoutPanel);
		base.Name = "NumberPad";
		base.Size = new System.Drawing.Size(373, 222);
		this.m_tableLayoutPanel.ResumeLayout(false);
		this.m_tableLayoutPanel.PerformLayout();
		base.ResumeLayout(false);
	}
}
