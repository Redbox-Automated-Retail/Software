using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.REDS.Framework;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ViewStackToolWindow : ToolWindow
{
	private IContainer components;

	private ListView m_stackListView;

	private ColumnHeader m_stackViewNameColumnHeader;

	private ColumnHeader m_clearColumnHeader;

	private ColumnHeader m_sceneNameColumnHeader;

	private ColumnHeader m_widthColumnHeader;

	private ColumnHeader m_heightColumnHeader;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripComboBox m_viewResourceToolStripComboBox;

	private ToolStripButton m_pushToolStripButton;

	private ToolStripButton m_popToolStripButton;

	private ToolStripButton m_popDiscardToolStripButton;

	private ToolStripButton m_showToolStripButton;

	private ToolStripLabel m_viewToolStripLabel;

	public ViewStackToolWindow()
	{
		InitializeComponent();
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.ViewStateChanged += delegate
			{
				RefreshStackList();
			};
		}
	}

	private void OnLoad(object sender, EventArgs e)
	{
		RefreshViewResourceList();
		RefreshStackList();
	}

	private void OnPush(object sender, EventArgs e)
	{
		ComboBox comboBox = (ComboBox)m_viewResourceToolStripComboBox.Control;
		if (comboBox.SelectedItem == null)
		{
			return;
		}
		try
		{
			Cursor = Cursors.WaitCursor;
			ServiceLocator.Instance.GetService<IViewService>().Push(((IResource)comboBox.SelectedItem).Name);
			RefreshStackList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnPop(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ServiceLocator.Instance.GetService<IViewService>().Pop();
			RefreshStackList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnPopDiscard(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ServiceLocator.Instance.GetService<IViewService>().PopDiscard();
			RefreshStackList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnShow(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ServiceLocator.Instance.GetService<IViewService>().Show();
			RefreshStackList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshViewResourceList();
		RefreshStackList();
	}

	private void RefreshViewResourceList()
	{
		ComboBox comboBox = (ComboBox)m_viewResourceToolStripComboBox.Control;
		comboBox.BeginUpdate();
		comboBox.Items.Clear();
		foreach (IResource resource in ServiceLocator.Instance.GetService<IResourceBundleService>().GetResources("view"))
		{
			comboBox.Items.Add(resource);
		}
		comboBox.EndUpdate();
	}

	private void RefreshStackList()
	{
		m_stackListView.BeginUpdate();
		m_stackListView.Items.Clear();
		foreach (IViewFrameInstance item in ServiceLocator.Instance.GetService<IViewService>().Stack)
		{
			ListViewItem listViewItem = new ListViewItem(item?.ViewFrame.ViewName)
			{
				Tag = item
			};
			IViewFrame viewFrame = item.ViewFrame as IViewFrame;
			listViewItem.SubItems.Add((viewFrame != null) ? viewFrame.SceneName : item?.ViewFrame?.Scene?.Name);
			listViewItem.SubItems.Add(item?.ViewFrame?.Clear.ToString());
			listViewItem.SubItems.Add(viewFrame?.Width.ToString());
			listViewItem.SubItems.Add(viewFrame?.Height.ToString());
			m_stackListView.Items.Add(listViewItem);
		}
		m_stackListView.EndUpdate();
		m_popToolStripButton.Enabled = m_stackListView.Items.Count > 0;
		m_popDiscardToolStripButton.Enabled = m_stackListView.Items.Count > 0;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.ViewStackToolWindow));
		this.m_stackListView = new System.Windows.Forms.ListView();
		this.m_stackViewNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_sceneNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_clearColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_widthColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_heightColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_viewToolStripLabel = new System.Windows.Forms.ToolStripLabel();
		this.m_viewResourceToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_pushToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_popToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_popDiscardToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_showToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_stackListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5] { this.m_stackViewNameColumnHeader, this.m_sceneNameColumnHeader, this.m_clearColumnHeader, this.m_widthColumnHeader, this.m_heightColumnHeader });
		this.m_stackListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_stackListView.FullRowSelect = true;
		this.m_stackListView.Location = new System.Drawing.Point(0, 25);
		this.m_stackListView.MultiSelect = false;
		this.m_stackListView.Name = "m_stackListView";
		this.m_stackListView.Size = new System.Drawing.Size(787, 294);
		this.m_stackListView.TabIndex = 4;
		this.m_stackListView.UseCompatibleStateImageBehavior = false;
		this.m_stackListView.View = System.Windows.Forms.View.Details;
		this.m_stackViewNameColumnHeader.Text = "View Name";
		this.m_stackViewNameColumnHeader.Width = 125;
		this.m_sceneNameColumnHeader.Text = "Scene Name";
		this.m_sceneNameColumnHeader.Width = 100;
		this.m_clearColumnHeader.Text = "Clear?";
		this.m_clearColumnHeader.Width = 45;
		this.m_widthColumnHeader.Text = "Width";
		this.m_widthColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_heightColumnHeader.Text = "Height";
		this.m_heightColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.m_refreshToolStripButton, this.toolStripSeparator1, this.m_viewToolStripLabel, this.m_viewResourceToolStripComboBox, this.m_pushToolStripButton, this.m_popToolStripButton, this.m_popDiscardToolStripButton, this.m_showToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(787, 25);
		this.m_toolStrip.TabIndex = 5;
		this.m_toolStrip.Text = "toolStrip1";
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.m_viewToolStripLabel.AutoSize = false;
		this.m_viewToolStripLabel.Name = "m_viewToolStripLabel";
		this.m_viewToolStripLabel.Size = new System.Drawing.Size(85, 20);
		this.m_viewToolStripLabel.Text = "View Resource:";
		this.m_viewResourceToolStripComboBox.AutoSize = false;
		this.m_viewResourceToolStripComboBox.DropDownHeight = 250;
		this.m_viewResourceToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_viewResourceToolStripComboBox.DropDownWidth = 160;
		this.m_viewResourceToolStripComboBox.IntegralHeight = false;
		this.m_viewResourceToolStripComboBox.MaxDropDownItems = 25;
		this.m_viewResourceToolStripComboBox.Name = "m_viewResourceToolStripComboBox";
		this.m_viewResourceToolStripComboBox.Size = new System.Drawing.Size(200, 21);
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.m_pushToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_pushToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Plus;
		this.m_pushToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_pushToolStripButton.Name = "m_pushToolStripButton";
		this.m_pushToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_pushToolStripButton.Text = "toolStripButton1";
		this.m_pushToolStripButton.ToolTipText = "Push View Resource";
		this.m_pushToolStripButton.Click += new System.EventHandler(OnPush);
		this.m_popToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_popToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Minus;
		this.m_popToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_popToolStripButton.Name = "m_popToolStripButton";
		this.m_popToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_popToolStripButton.Text = "toolStripButton1";
		this.m_popToolStripButton.ToolTipText = "Pop TOS";
		this.m_popToolStripButton.Click += new System.EventHandler(OnPop);
		this.m_popDiscardToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_popDiscardToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Cross;
		this.m_popDiscardToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_popDiscardToolStripButton.Name = "m_popDiscardToolStripButton";
		this.m_popDiscardToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_popDiscardToolStripButton.Text = "toolStripButton1";
		this.m_popDiscardToolStripButton.ToolTipText = "Pop and Discard TOS";
		this.m_popDiscardToolStripButton.Click += new System.EventHandler(OnPopDiscard);
		this.m_showToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_showToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Views;
		this.m_showToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_showToolStripButton.Name = "m_showToolStripButton";
		this.m_showToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.m_showToolStripButton.Text = "toolStripButton1";
		this.m_showToolStripButton.ToolTipText = "Show TOS";
		this.m_showToolStripButton.Click += new System.EventHandler(OnShow);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(787, 319);
		base.Controls.Add(this.m_stackListView);
		base.Controls.Add(this.m_toolStrip);
		base.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom;
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "ViewStackToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "View Stack";
		base.Load += new System.EventHandler(OnLoad);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
