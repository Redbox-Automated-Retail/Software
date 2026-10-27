using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class TimersToolWindow : ToolWindow
{
	private IContainer components;

	private ListView m_listView;

	private ColumnHeader m_nameColumnHeader;

	private ColumnHeader m_dueTimeColumnHeader;

	private ColumnHeader m_periodTimeColumnHeader;

	private ColumnHeader m_statusColumnHeader;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripButton m_clearToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton m_startToolStripButton;

	private ToolStripButton m_stopToolStripButton;

	private ToolStripButton m_executeToolStripButton;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_removeToolStripButton;

	public TimersToolWindow()
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
			if (selectedItem.Tag is ITimer timer)
			{
				timer.Enabled = true;
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
			if (selectedItem.Tag is ITimer timer)
			{
				timer.Enabled = false;
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
			if (selectedItem.Tag is ITimer timer)
			{
				ServiceLocator.Instance.GetService<ITimerService>().RemoveTimer(timer.Name);
			}
		}
		RefreshList();
	}

	private void OnClear(object sender, EventArgs e)
	{
		ServiceLocator.Instance.GetService<ITimerService>().Reset();
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
			if (selectedItem.Tag is ITimer timer)
			{
				timer.RaiseFire();
			}
		}
	}

	private void RefreshList()
	{
		m_listView.BeginUpdate();
		m_listView.Items.Clear();
		foreach (ITimer timer in ServiceLocator.Instance.GetService<ITimerService>().Timers)
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
		m_startToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_stopToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_executeToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_removeToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_clearToolStripButton.Enabled = m_listView.Items.Count > 0;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.TimersToolWindow));
		this.m_listView = new System.Windows.Forms.ListView();
		this.m_nameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_dueTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_periodTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_statusColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_startToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_stopToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_executeToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_clearToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_removeToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_nameColumnHeader, this.m_dueTimeColumnHeader, this.m_periodTimeColumnHeader, this.m_statusColumnHeader });
		this.m_listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_listView.FullRowSelect = true;
		this.m_listView.HideSelection = false;
		this.m_listView.Location = new System.Drawing.Point(0, 23);
		this.m_listView.Name = "m_listView";
		this.m_listView.Size = new System.Drawing.Size(630, 263);
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
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.m_refreshToolStripButton, this.toolStripSeparator1, this.m_startToolStripButton, this.m_stopToolStripButton, this.m_executeToolStripButton, this.toolStripSeparator2, this.m_clearToolStripButton, this.m_removeToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(630, 23);
		this.m_toolStrip.TabIndex = 8;
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.ToolTipText = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.m_startToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_startToolStripButton.Enabled = false;
		this.m_startToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugPlay;
		this.m_startToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_startToolStripButton.Name = "m_startToolStripButton";
		this.m_startToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_startToolStripButton.ToolTipText = "Start";
		this.m_startToolStripButton.Click += new System.EventHandler(OnStart);
		this.m_stopToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_stopToolStripButton.Enabled = false;
		this.m_stopToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.DebugStop;
		this.m_stopToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_stopToolStripButton.Name = "m_stopToolStripButton";
		this.m_stopToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_stopToolStripButton.ToolTipText = "Stop";
		this.m_stopToolStripButton.Click += new System.EventHandler(OnStop);
		this.m_executeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_executeToolStripButton.Enabled = false;
		this.m_executeToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Execute;
		this.m_executeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_executeToolStripButton.Name = "m_executeToolStripButton";
		this.m_executeToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_executeToolStripButton.ToolTipText = "Execute";
		this.m_executeToolStripButton.Click += new System.EventHandler(OnExecute);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_clearToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_clearToolStripButton.Enabled = false;
		this.m_clearToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Clean;
		this.m_clearToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_clearToolStripButton.Name = "m_clearToolStripButton";
		this.m_clearToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_clearToolStripButton.ToolTipText = "Clear";
		this.m_clearToolStripButton.Click += new System.EventHandler(OnClear);
		this.m_removeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_removeToolStripButton.Enabled = false;
		this.m_removeToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Delete;
		this.m_removeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_removeToolStripButton.Name = "m_removeToolStripButton";
		this.m_removeToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_removeToolStripButton.ToolTipText = "Remove";
		this.m_removeToolStripButton.Click += new System.EventHandler(OnRemove);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(630, 286);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.m_toolStrip);
		base.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom;
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "TimersToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Timers";
		base.Load += new System.EventHandler(OnLoad);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
