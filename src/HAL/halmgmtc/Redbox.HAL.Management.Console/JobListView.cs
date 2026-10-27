using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class JobListView<T> : UserControl
{
	private IContainer components;

	private Timer m_timer;

	private ToolStrip toolStrip1;

	private DataGridView dataGridView1;

	private Panel panel1;

	public JobListView(string[,] headers, BindingList<T> data)
	{
		InitializeComponent();
		base.Enabled = false;
		SuspendLayout();
		dataGridView1 = new AutomatedDataGridView<T>(headers, data, emptyEndColumn: true);
		dataGridView1.AutoSize = true;
		panel1.Controls.Add(dataGridView1);
		Dock = DockStyle.Fill;
		dataGridView1.SelectionChanged += (toolStrip1 as JobToolStrip).UpdateButtons;
		dataGridView1.DataBindingComplete += (toolStrip1 as JobToolStrip).UpdateButtons;
		CreatePriorityDropDown();
		ResumeLayout();
		bool justConnected = false;
		ProfileManager.Instance.Connected += delegate
		{
			base.Enabled = true;
			m_timer.Enabled = true;
			justConnected = true;
		};
		ProfileManager.Instance.Disconnected += delegate
		{
			base.Enabled = false;
			m_timer.Enabled = false;
			JobHelper.JobList.Clear();
		};
		dataGridView1.DataBindingComplete += delegate
		{
			if (justConnected && dataGridView1.Rows.Count > 1)
			{
				dataGridView1.Rows[0].Selected = false;
				justConnected = false;
			}
		};
	}

	public List<HardwareJobWrapper> GetSelected()
	{
		List<HardwareJobWrapper> list = new List<HardwareJobWrapper>();
		foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
		{
			list.Add(selectedRow.DataBoundItem as HardwareJobWrapper);
		}
		return list;
	}

	internal void RefreshData()
	{
		JobHelper.RefreshJobList();
		JobHelper.UpdateButtons(toolStrip1.Items, GetSelected());
	}

	private void OnTimer(object sender, EventArgs e)
	{
		RefreshData();
	}

	private void CreatePriorityDropDown()
	{
		ToolStripDropDownButton toolStripDropDownButton = new ToolStripDropDownButton();
		foreach (KeyValuePair<HardwareJobPriority, string> item in new Dictionary<HardwareJobPriority, string>
		{
			{
				HardwareJobPriority.Lowest,
				"Lowest"
			},
			{
				HardwareJobPriority.Low,
				"Low"
			},
			{
				HardwareJobPriority.BelowNormal,
				"Below Normal"
			},
			{
				HardwareJobPriority.Normal,
				"Normal"
			},
			{
				HardwareJobPriority.AboveNormal,
				"Above Normal"
			},
			{
				HardwareJobPriority.High,
				"High"
			},
			{
				HardwareJobPriority.Highest,
				"Highest"
			}
		})
		{
			ToolStripButton toolStripButton = new ToolStripButton();
			toolStripButton.Tag = item.Key;
			toolStripButton.Text = item.Value;
			toolStripButton.Click += OnPriorityChanged;
			toolStripDropDownButton.DropDownItems.Add(toolStripButton);
		}
		toolStripDropDownButton.Name = JobItems.Priority.ToString();
		toolStripDropDownButton.ToolTipText = "Priority";
		toolStripDropDownButton.Image = Resources.flag;
		toolStripDropDownButton.Tag = HardwareJobPriority.Normal;
		toolStripDropDownButton.Text = "Priority";
		toolStripDropDownButton.DropDown.Width = 150;
		toolStrip1.Items.Add(toolStripDropDownButton);
	}

	private void OnPriorityChanged(object sender, EventArgs e)
	{
		if (!(sender is ToolStripItem { Tag: not null } toolStripItem))
		{
			return;
		}
		foreach (HardwareJobWrapper item in GetSelected())
		{
			JobHelper.SetPriority(item, (HardwareJobPriority)toolStripItem.Tag);
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
		this.toolStrip1 = new Redbox.HAL.Management.Console.JobToolStrip(new System.Func<System.Collections.Generic.List<Redbox.HAL.Management.Console.HardwareJobWrapper>>(GetSelected), Redbox.HAL.Management.Console.UpdateSource.JobListView, false);
		this.m_timer = new System.Windows.Forms.Timer();
		this.panel1 = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.m_timer.Interval = 1000;
		this.m_timer.Tick += new System.EventHandler(OnTimer);
		this.m_timer.Enabled = false;
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(409, 382);
		this.panel1.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.toolStrip1);
		base.Controls.Add(this.panel1);
		base.Name = "JLV";
		base.Size = new System.Drawing.Size(410, 411);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
