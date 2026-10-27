using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class StackListView<T> : UserControl
{
	private IContainer components;

	private ToolStrip toolStrip1;

	private DataGridView dataGridView1;

	private Panel panel1;

	private ToolStripLabel m_slvLabel;

	private ToolStripButton m_slvRefeshButton;

	private ToolStripButton m_slvClearButton;

	private ToolStripComboBox m_slvComboBox;

	public StackListView(string[,] headers, BindingList<T> data)
	{
		InitializeComponent();
		base.Enabled = false;
		SuspendLayout();
		dataGridView1 = new AutomatedDataGridView<T>(headers, data, emptyEndColumn: false);
		dataGridView1.AutoSize = true;
		panel1.Controls.Add(dataGridView1);
		m_slvComboBox.KeyPress += OnPush;
		ResumeLayout();
		EnvironmentHelper.SelectedJobChanged += delegate
		{
			JobHelper.StackList.Clear();
		};
		ProfileManager.Instance.Connected += delegate
		{
			base.Enabled = true;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			base.Enabled = false;
			JobHelper.StackList.Clear();
		};
	}

	internal void RefreshData()
	{
		JobHelper.RefreshStackList();
	}

	private void OnPush(object sender, KeyPressEventArgs args)
	{
		if (args.KeyChar == '\r')
		{
			ComboBox comboBox = (sender as ToolStripComboBox).ComboBox;
			if (comboBox != null && !string.IsNullOrEmpty(comboBox.Text) && JobHelper.PushStackItem(comboBox.Text))
			{
				comboBox.Text = string.Empty;
			}
		}
	}

	private void OnClear(object sender, EventArgs args)
	{
		JobHelper.ClearJobStack();
	}

	private void OnRefresh(object sender, EventArgs args)
	{
		JobHelper.StackList.Clear();
		RefreshData();
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
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.m_slvRefeshButton = new System.Windows.Forms.ToolStripButton();
		this.m_slvClearButton = new System.Windows.Forms.ToolStripButton();
		this.m_slvLabel = new System.Windows.Forms.ToolStripLabel();
		this.m_slvComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.m_slvRefeshButton, this.m_slvClearButton, this.m_slvLabel, this.m_slvComboBox });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(410, 25);
		this.toolStrip1.TabIndex = 0;
		this.m_slvRefeshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_slvRefeshButton.Image = Redbox.HAL.Management.Console.Properties.Resources.refresh;
		this.m_slvRefeshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_slvRefeshButton.Name = "m_slvRefeshButton";
		this.m_slvRefeshButton.Size = new System.Drawing.Size(23, 22);
		this.m_slvRefeshButton.Text = "Refresh";
		this.m_slvRefeshButton.Click += new System.EventHandler(OnRefresh);
		this.m_slvClearButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_slvClearButton.Image = Redbox.HAL.Management.Console.Properties.Resources.clear;
		this.m_slvClearButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_slvClearButton.Name = "m_slvClearButton";
		this.m_slvClearButton.Size = new System.Drawing.Size(23, 22);
		this.m_slvClearButton.Text = "Clear Stack";
		this.m_slvClearButton.Click += new System.EventHandler(OnClear);
		this.m_slvLabel.Name = "m_slvLabel";
		this.m_slvLabel.Size = new System.Drawing.Size(79, 22);
		this.m_slvLabel.Text = "Push Values:  ";
		this.m_slvComboBox.Name = "m_slvComboBox";
		this.m_slvComboBox.Size = new System.Drawing.Size(121, 25);
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(409, 382);
		this.panel1.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.toolStrip1);
		base.Controls.Add(this.panel1);
		base.Name = "SLV";
		base.Size = new System.Drawing.Size(410, 411);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
