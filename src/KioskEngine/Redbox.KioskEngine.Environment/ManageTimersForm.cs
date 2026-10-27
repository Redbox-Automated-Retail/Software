using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ManageTimersForm : Form
{
	private IContainer components;

	private Button m_closeButton;

	private ListView m_listView;

	private ColumnHeader m_nameColumnHeader;

	private ColumnHeader m_dueTimeColumnHeader;

	private ColumnHeader m_periodTimeColumnHeader;

	private ColumnHeader m_statusColumnHeader;

	private Button m_startButton;

	private Button m_stopButton;

	private Button m_removeButton;

	private Button m_clearButton;

	private Button m_refreshButton;

	private Button m_executeButton;

	public ManageTimersForm()
	{
		InitializeComponent();
	}

	private void OnStart(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_listView.SelectedItems)
		{
			if (selectedItem.Tag is SoftTimer softTimer)
			{
				softTimer.Enabled = true;
			}
		}
		RefreshList();
	}

	private void OnStop(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_listView.SelectedItems)
		{
			if (selectedItem.Tag is SoftTimer softTimer)
			{
				softTimer.Enabled = false;
			}
		}
		RefreshList();
	}

	private void OnRemove(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_listView.SelectedItems)
		{
			if (selectedItem.Tag is SoftTimer softTimer)
			{
				TimerService.Instance.RemoveTimer(softTimer.Name);
			}
		}
		RefreshList();
	}

	private void OnClear(object sender, EventArgs e)
	{
		TimerService.Instance.Reset();
	}

	private void OnClose(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnSelectedTimerChanged(object sender, EventArgs e)
	{
		RefreshButtonState();
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

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void OnExecute(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count == 0)
		{
			return;
		}
		foreach (ListViewItem selectedItem in m_listView.SelectedItems)
		{
			if (selectedItem.Tag is SoftTimer softTimer)
			{
				softTimer.RaiseFire();
			}
		}
	}

	private void RefreshList()
	{
		m_listView.BeginUpdate();
		m_listView.Items.Clear();
		foreach (ITimer timer in TimerService.Instance.Timers)
		{
			ListViewItem listViewItem = new ListViewItem(timer.Name)
			{
				Tag = timer
			};
			listViewItem.SubItems.Add(timer.DueTime.ToString());
			listViewItem.SubItems.Add(timer.Period.ToString());
			listViewItem.SubItems.Add(timer.Enabled ? "Enabled" : "Disabled");
			m_listView.Items.Add(listViewItem);
		}
		m_listView.EndUpdate();
		RefreshButtonState();
	}

	private void RefreshButtonState()
	{
		m_startButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_stopButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_removeButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_clearButton.Enabled = m_listView.Items.Count > 0;
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
		this.m_closeButton = new System.Windows.Forms.Button();
		this.m_listView = new System.Windows.Forms.ListView();
		this.m_nameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_dueTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_periodTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_statusColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_startButton = new System.Windows.Forms.Button();
		this.m_stopButton = new System.Windows.Forms.Button();
		this.m_removeButton = new System.Windows.Forms.Button();
		this.m_clearButton = new System.Windows.Forms.Button();
		this.m_refreshButton = new System.Windows.Forms.Button();
		this.m_executeButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_closeButton.Location = new System.Drawing.Point(542, 251);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(75, 23);
		this.m_closeButton.TabIndex = 7;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_nameColumnHeader, this.m_dueTimeColumnHeader, this.m_periodTimeColumnHeader, this.m_statusColumnHeader });
		this.m_listView.FullRowSelect = true;
		this.m_listView.HideSelection = false;
		this.m_listView.Location = new System.Drawing.Point(12, 12);
		this.m_listView.Name = "m_listView";
		this.m_listView.Size = new System.Drawing.Size(605, 233);
		this.m_listView.TabIndex = 0;
		this.m_listView.UseCompatibleStateImageBehavior = false;
		this.m_listView.View = System.Windows.Forms.View.Details;
		this.m_listView.SelectedIndexChanged += new System.EventHandler(OnSelectedTimerChanged);
		this.m_nameColumnHeader.Text = "Name";
		this.m_nameColumnHeader.Width = 250;
		this.m_dueTimeColumnHeader.Text = "Due (ms)";
		this.m_dueTimeColumnHeader.Width = 105;
		this.m_periodTimeColumnHeader.Text = "Period (ms)";
		this.m_periodTimeColumnHeader.Width = 105;
		this.m_statusColumnHeader.Text = "Status";
		this.m_statusColumnHeader.Width = 95;
		this.m_startButton.Enabled = false;
		this.m_startButton.Location = new System.Drawing.Point(114, 251);
		this.m_startButton.Name = "m_startButton";
		this.m_startButton.Size = new System.Drawing.Size(75, 23);
		this.m_startButton.TabIndex = 2;
		this.m_startButton.Text = "Start";
		this.m_startButton.UseVisualStyleBackColor = true;
		this.m_startButton.Click += new System.EventHandler(OnStart);
		this.m_stopButton.Enabled = false;
		this.m_stopButton.Location = new System.Drawing.Point(195, 251);
		this.m_stopButton.Name = "m_stopButton";
		this.m_stopButton.Size = new System.Drawing.Size(75, 23);
		this.m_stopButton.TabIndex = 3;
		this.m_stopButton.Text = "Stop";
		this.m_stopButton.UseVisualStyleBackColor = true;
		this.m_stopButton.Click += new System.EventHandler(OnStop);
		this.m_removeButton.Enabled = false;
		this.m_removeButton.Location = new System.Drawing.Point(357, 251);
		this.m_removeButton.Name = "m_removeButton";
		this.m_removeButton.Size = new System.Drawing.Size(75, 23);
		this.m_removeButton.TabIndex = 5;
		this.m_removeButton.Text = "Remove";
		this.m_removeButton.UseVisualStyleBackColor = true;
		this.m_removeButton.Click += new System.EventHandler(OnRemove);
		this.m_clearButton.Enabled = false;
		this.m_clearButton.Location = new System.Drawing.Point(438, 251);
		this.m_clearButton.Name = "m_clearButton";
		this.m_clearButton.Size = new System.Drawing.Size(75, 23);
		this.m_clearButton.TabIndex = 6;
		this.m_clearButton.Text = "Clear";
		this.m_clearButton.UseVisualStyleBackColor = true;
		this.m_clearButton.Click += new System.EventHandler(OnClear);
		this.m_refreshButton.Location = new System.Drawing.Point(12, 251);
		this.m_refreshButton.Name = "m_refreshButton";
		this.m_refreshButton.Size = new System.Drawing.Size(75, 23);
		this.m_refreshButton.TabIndex = 1;
		this.m_refreshButton.Text = "Refresh";
		this.m_refreshButton.UseVisualStyleBackColor = true;
		this.m_refreshButton.Click += new System.EventHandler(OnRefresh);
		this.m_executeButton.Location = new System.Drawing.Point(276, 251);
		this.m_executeButton.Name = "m_executeButton";
		this.m_executeButton.Size = new System.Drawing.Size(75, 23);
		this.m_executeButton.TabIndex = 4;
		this.m_executeButton.Text = "Execute";
		this.m_executeButton.UseVisualStyleBackColor = true;
		this.m_executeButton.Click += new System.EventHandler(OnExecute);
		base.AcceptButton = this.m_closeButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(630, 286);
		base.Controls.Add(this.m_executeButton);
		base.Controls.Add(this.m_refreshButton);
		base.Controls.Add(this.m_clearButton);
		base.Controls.Add(this.m_removeButton);
		base.Controls.Add(this.m_stopButton);
		base.Controls.Add(this.m_startButton);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.m_closeButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ManageTimersForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Manage Timers";
		base.Load += new System.EventHandler(OnLoad);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnFormClosed);
		base.ResumeLayout(false);
	}
}
