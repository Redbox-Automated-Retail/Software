using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class ProgramEventListView<T> : UserControl
{
	private DataGridView dataGridView1;

	private IContainer components;

	private Panel panel1;

	private ToolStrip toolStrip1;

	private ToolStripButton m_clearButton;

	private Timer m_timer;

	private ToolStripButton m_refreshButton;

	public ProgramEventListView(string[,] headers, BindingList<T> data)
	{
		InitializeComponent();
		SuspendLayout();
		dataGridView1 = new AutomatedDataGridView<T>(headers, data, emptyEndColumn: false);
		dataGridView1.Columns[0].DefaultCellStyle.Format = "MM/dd/yyyy hh:mm:ss";
		panel1.Controls.Add(dataGridView1);
		ResumeLayout();
		EnvironmentHelper.SelectedJobChanged += delegate
		{
			JobHelper.ProgramEventList.Clear();
			dataGridView1.DataSource = (JobComboBox.Instance.SelectedItem as HardwareJobWrapper).Events;
		};
		ProfileManager.Instance.Connected += delegate
		{
			base.Enabled = true;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			base.Enabled = false;
			JobHelper.ProgramEventList.Clear();
		};
	}

	internal void RefreshData()
	{
		JobHelper.RefreshProgramEventList();
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshData();
	}

	private void OnClear(object sender, EventArgs e)
	{
		dataGridView1.Rows.Clear();
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
		this.panel1 = new System.Windows.Forms.Panel();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.m_refreshButton = new System.Windows.Forms.ToolStripButton();
		this.m_clearButton = new System.Windows.Forms.ToolStripButton();
		this.m_timer = new System.Windows.Forms.Timer(this.components);
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 4);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(150, 121);
		this.panel1.TabIndex = 0;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.m_refreshButton, this.m_clearButton });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
		this.toolStrip1.Size = new System.Drawing.Size(150, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.m_refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshButton.Image = Redbox.HAL.Management.Console.Properties.Resources.refresh;
		this.m_refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshButton.Name = "m_refreshButton";
		this.m_refreshButton.Size = new System.Drawing.Size(23, 22);
		this.m_refreshButton.Text = "Refresh";
		this.m_clearButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_clearButton.Image = Redbox.HAL.Management.Console.Properties.Resources.clear;
		this.m_clearButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_clearButton.Name = "m_clearButton";
		this.m_clearButton.Size = new System.Drawing.Size(23, 22);
		this.m_clearButton.Text = "Clear";
		this.m_timer.Interval = 1000;
		this.m_timer.Tick += new System.EventHandler(OnRefresh);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.toolStrip1);
		base.Controls.Add(this.panel1);
		base.Name = "ProgramEventListView";
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
