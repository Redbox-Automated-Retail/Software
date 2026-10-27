using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;

namespace Redbox.KioskEngine.IDE;

public class TaskManagerForm : Form
{
	public List<string> ProcessNamesToIgnore;

	private bool m_isRefreshing;

	private IContainer components;

	private ListView m_taskListView;

	private Panel m_buttonPanel;

	private ColumnHeader m_nameColumnHeader;

	private ColumnHeader m_pidColumnHeader;

	private ColumnHeader m_memoryColumnHeader;

	private Button m_killButton;

	private Button m_refreshButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IList<Process> SelectedProcesses
	{
		get
		{
			List<Process> list = new List<Process>();
			foreach (ListViewItem selectedItem in m_taskListView.SelectedItems)
			{
				list.Add((Process)selectedItem.Tag);
			}
			return list;
		}
	}

	public event EventHandler Kill;

	public TaskManagerForm()
	{
		InitializeComponent();
	}

	public void RefreshList()
	{
		if (m_isRefreshing)
		{
			return;
		}
		m_isRefreshing = true;
		int num = -1;
		if (m_taskListView.SelectedIndices.Count > 0)
		{
			num = m_taskListView.SelectedIndices[0];
		}
		m_taskListView.SuspendLayout();
		m_taskListView.BeginUpdate();
		m_taskListView.Items.Clear();
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			if (ProcessNamesToIgnore == null || (ProcessNamesToIgnore != null && !ProcessNamesToIgnore.Contains(process.ProcessName)))
			{
				ListViewItem listViewItem = new ListViewItem(process.ProcessName);
				listViewItem.SubItems.Add(process.Id.ToString());
				listViewItem.SubItems.Add($"{process.WorkingSet64 / 1000:#,###}K");
				listViewItem.Tag = process;
				m_taskListView.Items.Add(listViewItem);
			}
		}
		m_taskListView.EndUpdate();
		m_taskListView.ResumeLayout();
		if (num != -1)
		{
			m_taskListView.SelectedIndices.Add(num);
		}
		m_isRefreshing = false;
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Cursor.Show();
		RefreshList();
	}

	private void OnFormClosed(object sender, FormClosedEventArgs e)
	{
		Cursor.Hide();
	}

	private void OnSelectedTaskChanged(object sender, EventArgs e)
	{
		m_killButton.Enabled = m_taskListView.SelectedItems.Count > 0;
	}

	private void OnKill(object sender, EventArgs e)
	{
		if (Kill == null)
		{
			return;
		}
		try
		{
			Kill(sender, e);
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TaskManagerForm.OnKill.", e2);
		}
		finally
		{
			RefreshList();
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
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
		this.m_taskListView = new System.Windows.Forms.ListView();
		this.m_nameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_pidColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_memoryColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_buttonPanel = new System.Windows.Forms.Panel();
		this.m_refreshButton = new System.Windows.Forms.Button();
		this.m_killButton = new System.Windows.Forms.Button();
		this.m_buttonPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_taskListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_nameColumnHeader, this.m_pidColumnHeader, this.m_memoryColumnHeader });
		this.m_taskListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_taskListView.FullRowSelect = true;
		this.m_taskListView.Location = new System.Drawing.Point(0, 0);
		this.m_taskListView.Name = "m_taskListView";
		this.m_taskListView.Size = new System.Drawing.Size(431, 324);
		this.m_taskListView.TabIndex = 0;
		this.m_taskListView.UseCompatibleStateImageBehavior = false;
		this.m_taskListView.View = System.Windows.Forms.View.Details;
		this.m_taskListView.SelectedIndexChanged += new System.EventHandler(OnSelectedTaskChanged);
		this.m_nameColumnHeader.Text = "Name";
		this.m_nameColumnHeader.Width = 200;
		this.m_pidColumnHeader.Text = "PID";
		this.m_pidColumnHeader.Width = 75;
		this.m_memoryColumnHeader.Text = "Memory";
		this.m_memoryColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_memoryColumnHeader.Width = 95;
		this.m_buttonPanel.Controls.Add(this.m_refreshButton);
		this.m_buttonPanel.Controls.Add(this.m_killButton);
		this.m_buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.m_buttonPanel.Location = new System.Drawing.Point(0, 324);
		this.m_buttonPanel.Name = "m_buttonPanel";
		this.m_buttonPanel.Size = new System.Drawing.Size(431, 65);
		this.m_buttonPanel.TabIndex = 1;
		this.m_refreshButton.Location = new System.Drawing.Point(12, 13);
		this.m_refreshButton.Name = "m_refreshButton";
		this.m_refreshButton.Size = new System.Drawing.Size(193, 40);
		this.m_refreshButton.TabIndex = 0;
		this.m_refreshButton.Text = "Refresh";
		this.m_refreshButton.UseVisualStyleBackColor = true;
		this.m_refreshButton.Click += new System.EventHandler(OnRefresh);
		this.m_killButton.Enabled = false;
		this.m_killButton.Location = new System.Drawing.Point(226, 13);
		this.m_killButton.Name = "m_killButton";
		this.m_killButton.Size = new System.Drawing.Size(193, 40);
		this.m_killButton.TabIndex = 1;
		this.m_killButton.Text = "Kill";
		this.m_killButton.UseVisualStyleBackColor = true;
		this.m_killButton.Click += new System.EventHandler(OnKill);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(431, 389);
		base.Controls.Add(this.m_taskListView);
		base.Controls.Add(this.m_buttonPanel);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "TaskManagerForm";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Task Manager";
		base.TopMost = true;
		base.Load += new System.EventHandler(OnLoad);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnFormClosed);
		this.m_buttonPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
