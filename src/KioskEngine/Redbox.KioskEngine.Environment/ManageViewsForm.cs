using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment;

public class ManageViewsForm : Form
{
	private bool m_isActivated;

	private IContainer components;

	private ListView m_viewListView;

	private Button m_popButton;

	private Button m_pushButton;

	private Button m_showButton;

	private Button m_closeButton;

	private ColumnHeader m_viewNameColumnHeader;

	private ListView m_stackListView;

	private Button m_popDiscardButton;

	private ColumnHeader m_stackViewNameColumnHeader;

	private Label label1;

	private Label label2;

	private Button m_refreshButton;

	private ColumnHeader m_clearColumnHeader;

	private ColumnHeader m_sceneNameColumnHeader;

	private ColumnHeader m_widthColumnHeader;

	private ColumnHeader m_heightColumnHeader;

	public ManageViewsForm()
	{
		InitializeComponent();
	}

	private void OnActivated(object sender, EventArgs e)
	{
		if (!m_isActivated)
		{
			Cursor.Show();
			RefreshViewResourceList();
			RefreshStackList();
			m_isActivated = true;
		}
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		Cursor.Hide();
	}

	private void OnPush(object sender, EventArgs e)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			ViewService.Instance.Push(m_viewListView.SelectedItems[0].Text);
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
			ViewService.Instance.Pop();
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
			ViewService.Instance.PopDiscard();
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
			ViewService.Instance.Show();
			RefreshStackList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnClose(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshStackList();
	}

	private void OnSelectedViewResourceChanged(object sender, EventArgs e)
	{
		m_pushButton.Enabled = m_viewListView.SelectedItems.Count > 0;
	}

	private void RefreshViewResourceList()
	{
		m_viewListView.BeginUpdate();
		m_viewListView.Items.Clear();
		IResourceBundleSet activeBundleSet = ResourceBundleService.Instance.ActiveBundleSet;
		if (activeBundleSet == null)
		{
			return;
		}
		foreach (IResource resource in activeBundleSet.GetResources("view", ResourceBundleService.Instance.Filter))
		{
			ListViewItem value = new ListViewItem(resource.Name)
			{
				Tag = resource
			};
			m_viewListView.Items.Add(value);
		}
		m_viewListView.EndUpdate();
	}

	private void RefreshStackList()
	{
		m_stackListView.BeginUpdate();
		m_stackListView.Items.Clear();
		foreach (IViewFrameInstance item in ViewService.Instance.Stack)
		{
			ListViewItem listViewItem = new ListViewItem(item.ViewFrame.ViewName)
			{
				Tag = item
			};
			IViewFrame viewFrame = item?.ViewFrame as IViewFrame;
			listViewItem.SubItems.Add((viewFrame != null) ? viewFrame.SceneName : item?.ViewFrame?.Scene?.Name);
			listViewItem.SubItems.Add(item?.ViewFrame?.Clear.ToString());
			listViewItem.SubItems.Add(viewFrame?.Width.ToString());
			listViewItem.SubItems.Add(viewFrame?.Height.ToString());
			m_stackListView.Items.Add(listViewItem);
		}
		m_stackListView.EndUpdate();
		m_popButton.Enabled = m_stackListView.Items.Count > 0;
		m_popDiscardButton.Enabled = m_stackListView.Items.Count > 0;
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
		this.m_viewListView = new System.Windows.Forms.ListView();
		this.m_viewNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_popButton = new System.Windows.Forms.Button();
		this.m_pushButton = new System.Windows.Forms.Button();
		this.m_showButton = new System.Windows.Forms.Button();
		this.m_closeButton = new System.Windows.Forms.Button();
		this.m_stackListView = new System.Windows.Forms.ListView();
		this.m_stackViewNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_sceneNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_clearColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_widthColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_heightColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_popDiscardButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_refreshButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_viewListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.m_viewNameColumnHeader });
		this.m_viewListView.FullRowSelect = true;
		this.m_viewListView.Location = new System.Drawing.Point(12, 25);
		this.m_viewListView.MultiSelect = false;
		this.m_viewListView.Name = "m_viewListView";
		this.m_viewListView.Size = new System.Drawing.Size(190, 280);
		this.m_viewListView.TabIndex = 1;
		this.m_viewListView.UseCompatibleStateImageBehavior = false;
		this.m_viewListView.View = System.Windows.Forms.View.Details;
		this.m_viewListView.SelectedIndexChanged += new System.EventHandler(OnSelectedViewResourceChanged);
		this.m_viewNameColumnHeader.Text = "View Name";
		this.m_viewNameColumnHeader.Width = 185;
		this.m_popButton.Enabled = false;
		this.m_popButton.Location = new System.Drawing.Point(690, 25);
		this.m_popButton.Name = "m_popButton";
		this.m_popButton.Size = new System.Drawing.Size(86, 23);
		this.m_popButton.TabIndex = 5;
		this.m_popButton.Text = "Pop";
		this.m_popButton.UseVisualStyleBackColor = true;
		this.m_popButton.Click += new System.EventHandler(OnPop);
		this.m_pushButton.Enabled = false;
		this.m_pushButton.Location = new System.Drawing.Point(208, 25);
		this.m_pushButton.Name = "m_pushButton";
		this.m_pushButton.Size = new System.Drawing.Size(75, 23);
		this.m_pushButton.TabIndex = 2;
		this.m_pushButton.Text = "Push >>";
		this.m_pushButton.UseVisualStyleBackColor = true;
		this.m_pushButton.Click += new System.EventHandler(OnPush);
		this.m_showButton.Location = new System.Drawing.Point(690, 83);
		this.m_showButton.Name = "m_showButton";
		this.m_showButton.Size = new System.Drawing.Size(86, 23);
		this.m_showButton.TabIndex = 7;
		this.m_showButton.Text = "Show";
		this.m_showButton.UseVisualStyleBackColor = true;
		this.m_showButton.Click += new System.EventHandler(OnShow);
		this.m_closeButton.Location = new System.Drawing.Point(690, 282);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(86, 23);
		this.m_closeButton.TabIndex = 9;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		this.m_stackListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[5] { this.m_stackViewNameColumnHeader, this.m_sceneNameColumnHeader, this.m_clearColumnHeader, this.m_widthColumnHeader, this.m_heightColumnHeader });
		this.m_stackListView.FullRowSelect = true;
		this.m_stackListView.Location = new System.Drawing.Point(289, 25);
		this.m_stackListView.MultiSelect = false;
		this.m_stackListView.Name = "m_stackListView";
		this.m_stackListView.Size = new System.Drawing.Size(395, 280);
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
		this.m_popDiscardButton.Enabled = false;
		this.m_popDiscardButton.Location = new System.Drawing.Point(690, 54);
		this.m_popDiscardButton.Name = "m_popDiscardButton";
		this.m_popDiscardButton.Size = new System.Drawing.Size(86, 23);
		this.m_popDiscardButton.TabIndex = 6;
		this.m_popDiscardButton.Text = "Pop Discard";
		this.m_popDiscardButton.UseVisualStyleBackColor = true;
		this.m_popDiscardButton.Click += new System.EventHandler(OnPopDiscard);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "View Resources:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(286, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(64, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "View Stack:";
		this.m_refreshButton.Location = new System.Drawing.Point(690, 112);
		this.m_refreshButton.Name = "m_refreshButton";
		this.m_refreshButton.Size = new System.Drawing.Size(86, 23);
		this.m_refreshButton.TabIndex = 8;
		this.m_refreshButton.Text = "Refresh";
		this.m_refreshButton.UseVisualStyleBackColor = true;
		this.m_refreshButton.Click += new System.EventHandler(OnRefresh);
		base.AcceptButton = this.m_closeButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(787, 319);
		base.ControlBox = false;
		base.Controls.Add(this.m_refreshButton);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_popDiscardButton);
		base.Controls.Add(this.m_stackListView);
		base.Controls.Add(this.m_closeButton);
		base.Controls.Add(this.m_showButton);
		base.Controls.Add(this.m_pushButton);
		base.Controls.Add(this.m_popButton);
		base.Controls.Add(this.m_viewListView);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ManageViewsForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Manage Views";
		base.Activated += new System.EventHandler(OnActivated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnFormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
