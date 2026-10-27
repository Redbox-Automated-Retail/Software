using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class CallStackToolWindow : ToolWindow
{
	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_refreshToolStripButton;

	private ListView m_listView;

	private ColumnHeader m_functionNameColumnHeader;

	private ColumnHeader m_scriptNameColumnHeader;

	private ColumnHeader m_lineNumberColumnHeader;

	public CallStackToolWindow()
	{
		InitializeComponent();
	}

	public void RefreshCallStack()
	{
		InnerRefreshCallStack();
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		InnerRefreshCallStack();
	}

	private void InnerRefreshCallStack()
	{
		CallStackEntry[] callStack = ((LuaDebugger)ServiceLocator.Instance.GetService<IDebugService>().DebuggerInstance).GetCallStack();
		m_listView.BeginUpdate();
		m_listView.Items.Clear();
		CallStackEntry[] array = callStack;
		foreach (CallStackEntry callStackEntry in array)
		{
			ListViewItem listViewItem = new ListViewItem(callStackEntry.FunctionName);
			listViewItem.SubItems.Add(callStackEntry.FileName);
			listViewItem.SubItems.Add(callStackEntry.Line.ToString());
			m_listView.Items.Add(listViewItem);
		}
		m_listView.EndUpdate();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.CallStackToolWindow));
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_listView = new System.Windows.Forms.ListView();
		this.m_functionNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_scriptNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_lineNumberColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_refreshToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(610, 23);
		this.m_toolStrip.TabIndex = 0;
		this.m_toolStrip.Text = "toolStrip1";
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_functionNameColumnHeader, this.m_scriptNameColumnHeader, this.m_lineNumberColumnHeader });
		this.m_listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_listView.Location = new System.Drawing.Point(0, 23);
		this.m_listView.Name = "m_listView";
		this.m_listView.Size = new System.Drawing.Size(610, 252);
		this.m_listView.TabIndex = 1;
		this.m_listView.UseCompatibleStateImageBehavior = false;
		this.m_listView.View = System.Windows.Forms.View.Details;
		this.m_functionNameColumnHeader.Text = "Function Name";
		this.m_functionNameColumnHeader.Width = 200;
		this.m_scriptNameColumnHeader.Text = "Script";
		this.m_scriptNameColumnHeader.Width = 200;
		this.m_lineNumberColumnHeader.Text = "Line #";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(610, 275);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "CallStackToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Call Stack";
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
