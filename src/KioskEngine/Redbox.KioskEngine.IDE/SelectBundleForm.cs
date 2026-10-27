using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.IDE;

public class SelectBundleForm : Form
{
	private IContainer components;

	private ListView m_appBundlesListView;

	private ColumnHeader m_productNameColumn;

	private ColumnHeader m_productVersionColumn;

	private ColumnHeader m_pathColumn;

	private ColumnHeader m_typeColumn;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripLabel toolStripLabel1;

	private ToolStripTextBox m_searchPathToolStripTextBox;

	private ToolStripButton m_editSearchPathToolStripButton;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_breakOnStartToolStripButton;

	private ToolStripButton m_activateToolStripButton;

	private ToolStripButton m_unloadActiveBundleToolStripButton;

	private Splitter m_splitter;

	private Panel m_panel;

	private ListView m_sharedBundlesListView;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private ColumnHeader columnHeader3;

	private ColumnHeader columnHeader4;

	private Label label1;

	private ToolStripButton m_dependenciesToolStripButton;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool ShowDebugger => m_breakOnStartToolStripButton.Checked;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IResourceBundle SelectedBundle { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SearchPath
	{
		get
		{
			return m_searchPathToolStripTextBox.Text;
		}
		set
		{
			if (!(m_searchPathToolStripTextBox.Text == value))
			{
				m_searchPathToolStripTextBox.Text = value;
				RefreshList();
			}
		}
	}

	public SelectBundleForm()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Cursor.Show();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service != null)
		{
			m_unloadActiveBundleToolStripButton.Enabled = service.HasActiveBundle();
		}
		if (m_appBundlesListView.Items.Count == 0)
		{
			RefreshList();
		}
	}

	private void OnItemActivate(object sender, EventArgs e)
	{
		if (m_appBundlesListView.SelectedItems.Count != 0)
		{
			if (string.Compare(m_appBundlesListView.SelectedItems[0].SubItems[0].Text, BundleType.Application.ToString(), ignoreCase: true) != 0)
			{
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				MessageBox.Show("Only bundles of type Application may be activated.", service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
	}

	private void OnSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_appBundlesListView.SelectedItems.Count > 0)
		{
			m_activateToolStripButton.Enabled = true;
			if (m_appBundlesListView.SelectedItems[0].Tag is IBundleSpecifier bundleSpecifier)
			{
				SelectedBundle = bundleSpecifier.Instance;
			}
		}
		else
		{
			m_activateToolStripButton.Enabled = false;
			SelectedBundle = null;
		}
		m_dependenciesToolStripButton.Enabled = m_appBundlesListView.SelectedItems.Count == 1;
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		if (m_appBundlesListView.SelectedItems.Count == 0)
		{
			SelectedBundle = null;
		}
		Cursor.Hide();
	}

	private void OnUnloadActiveBundle(object sender, EventArgs e)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service != null)
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				service.Deactivate();
			}
			finally
			{
				Cursor = Cursors.Default;
			}
			m_unloadActiveBundleToolStripButton.Enabled = service.HasActiveBundle();
		}
	}

	private void OnEditSearchPath(object sender, EventArgs e)
	{
		List<string> searchPath = new List<string>(m_searchPathToolStripTextBox.Text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
		EditSearchPathForm editSearchPathForm = new EditSearchPathForm
		{
			SearchPath = searchPath
		};
		if (editSearchPathForm.ShowDialog() == DialogResult.OK)
		{
			m_searchPathToolStripTextBox.Text = editSearchPathForm.SearchPath.Join(";");
			RefreshList();
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void OnBreakOnStartup(object sender, EventArgs e)
	{
		m_breakOnStartToolStripButton.Checked = !m_breakOnStartToolStripButton.Checked;
	}

	private void OnShowDependencies(object sender, EventArgs e)
	{
		BundleDependencyForm bundleDependencyForm = new BundleDependencyForm();
		bundleDependencyForm.BundleSpecifier = m_appBundlesListView.SelectedItems[0].Tag as IBundleSpecifier;
		bundleDependencyForm.ShowDialog();
	}

	private void RefreshList()
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			service.SearchPath = m_searchPathToolStripTextBox.Text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			service.LoadBundles();
			m_appBundlesListView.Items.Clear();
			m_sharedBundlesListView.Items.Clear();
			foreach (IBundleSpecifier bundle in service.Bundles)
			{
				ListViewItem listViewItem = new ListViewItem(bundle.Type.ToString());
				listViewItem.SubItems.Add(bundle.Name);
				listViewItem.SubItems.Add(bundle.Version.ToString());
				listViewItem.SubItems.Add(bundle.Instance.Storage.BundlePath);
				listViewItem.Tag = bundle;
				if (bundle.Type == BundleType.Application)
				{
					m_appBundlesListView.Items.Add(listViewItem);
				}
				else
				{
					m_sharedBundlesListView.Items.Add(listViewItem);
				}
			}
			IDebugService service2 = ServiceLocator.Instance.GetService<IDebugService>();
			if (service2 != null)
			{
				m_breakOnStartToolStripButton.Enabled = service2.IsEnabled;
			}
		}
		finally
		{
			Cursor = Cursors.Default;
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
		this.m_appBundlesListView = new System.Windows.Forms.ListView();
		this.m_typeColumn = new System.Windows.Forms.ColumnHeader();
		this.m_productNameColumn = new System.Windows.Forms.ColumnHeader();
		this.m_productVersionColumn = new System.Windows.Forms.ColumnHeader();
		this.m_pathColumn = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_activateToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_unloadActiveBundleToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_dependenciesToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.m_searchPathToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
		this.m_editSearchPathToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_breakOnStartToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_splitter = new System.Windows.Forms.Splitter();
		this.m_panel = new System.Windows.Forms.Panel();
		this.m_sharedBundlesListView = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
		this.label1 = new System.Windows.Forms.Label();
		this.m_toolStrip.SuspendLayout();
		this.m_panel.SuspendLayout();
		base.SuspendLayout();
		this.m_appBundlesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_typeColumn, this.m_productNameColumn, this.m_productVersionColumn, this.m_pathColumn });
		this.m_appBundlesListView.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_appBundlesListView.FullRowSelect = true;
		this.m_appBundlesListView.HideSelection = false;
		this.m_appBundlesListView.Location = new System.Drawing.Point(3, 26);
		this.m_appBundlesListView.MultiSelect = false;
		this.m_appBundlesListView.Name = "m_appBundlesListView";
		this.m_appBundlesListView.Size = new System.Drawing.Size(777, 208);
		this.m_appBundlesListView.TabIndex = 4;
		this.m_appBundlesListView.UseCompatibleStateImageBehavior = false;
		this.m_appBundlesListView.View = System.Windows.Forms.View.Details;
		this.m_appBundlesListView.ItemActivate += new System.EventHandler(OnItemActivate);
		this.m_appBundlesListView.SelectedIndexChanged += new System.EventHandler(OnSelectedIndexChanged);
		this.m_typeColumn.Text = "Bundle Type";
		this.m_typeColumn.Width = 95;
		this.m_productNameColumn.Text = "Product Name";
		this.m_productNameColumn.Width = 175;
		this.m_productVersionColumn.Text = "Version";
		this.m_pathColumn.Text = "Path";
		this.m_pathColumn.Width = 410;
		this.m_toolStrip.AllowMerge = false;
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.m_refreshToolStripButton, this.m_activateToolStripButton, this.m_unloadActiveBundleToolStripButton, this.m_dependenciesToolStripButton, this.toolStripSeparator1, this.toolStripLabel1, this.m_searchPathToolStripTextBox, this.m_editSearchPathToolStripButton, this.toolStripSeparator2, this.m_breakOnStartToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(3, 3);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(777, 23);
		this.m_toolStrip.TabIndex = 5;
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.Text = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.m_activateToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_activateToolStripButton.Enabled = false;
		this.m_activateToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Execute;
		this.m_activateToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_activateToolStripButton.Name = "m_activateToolStripButton";
		this.m_activateToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_activateToolStripButton.Text = "Activate";
		this.m_activateToolStripButton.Click += new System.EventHandler(OnItemActivate);
		this.m_unloadActiveBundleToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_unloadActiveBundleToolStripButton.Enabled = false;
		this.m_unloadActiveBundleToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Clean;
		this.m_unloadActiveBundleToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_unloadActiveBundleToolStripButton.Name = "m_unloadActiveBundleToolStripButton";
		this.m_unloadActiveBundleToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_unloadActiveBundleToolStripButton.Text = "Unload Active Bundle";
		this.m_unloadActiveBundleToolStripButton.Click += new System.EventHandler(OnUnloadActiveBundle);
		this.m_dependenciesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_dependenciesToolStripButton.Enabled = false;
		this.m_dependenciesToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Dependencies;
		this.m_dependenciesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_dependenciesToolStripButton.Name = "m_dependenciesToolStripButton";
		this.m_dependenciesToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_dependenciesToolStripButton.Text = "Dependencies";
		this.m_dependenciesToolStripButton.Click += new System.EventHandler(OnShowDependencies);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.toolStripLabel1.AutoSize = false;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(69, 20);
		this.toolStripLabel1.Text = "Search Path:";
		this.m_searchPathToolStripTextBox.MaxLength = 1024;
		this.m_searchPathToolStripTextBox.Name = "m_searchPathToolStripTextBox";
		this.m_searchPathToolStripTextBox.Size = new System.Drawing.Size(400, 21);
		this.m_editSearchPathToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_editSearchPathToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Properties;
		this.m_editSearchPathToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_editSearchPathToolStripButton.Name = "m_editSearchPathToolStripButton";
		this.m_editSearchPathToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_editSearchPathToolStripButton.Text = "Edit Search Path";
		this.m_editSearchPathToolStripButton.Click += new System.EventHandler(OnEditSearchPath);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_breakOnStartToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Debugger;
		this.m_breakOnStartToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_breakOnStartToolStripButton.Name = "m_breakOnStartToolStripButton";
		this.m_breakOnStartToolStripButton.Size = new System.Drawing.Size(114, 20);
		this.m_breakOnStartToolStripButton.Text = "Break On Activate";
		this.m_breakOnStartToolStripButton.Click += new System.EventHandler(OnBreakOnStartup);
		this.m_splitter.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_splitter.Location = new System.Drawing.Point(3, 234);
		this.m_splitter.Name = "m_splitter";
		this.m_splitter.Size = new System.Drawing.Size(777, 3);
		this.m_splitter.TabIndex = 6;
		this.m_splitter.TabStop = false;
		this.m_panel.Controls.Add(this.m_sharedBundlesListView);
		this.m_panel.Controls.Add(this.label1);
		this.m_panel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_panel.Location = new System.Drawing.Point(3, 237);
		this.m_panel.Name = "m_panel";
		this.m_panel.Size = new System.Drawing.Size(777, 234);
		this.m_panel.TabIndex = 7;
		this.m_sharedBundlesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.columnHeader1, this.columnHeader2, this.columnHeader3, this.columnHeader4 });
		this.m_sharedBundlesListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_sharedBundlesListView.FullRowSelect = true;
		this.m_sharedBundlesListView.HideSelection = false;
		this.m_sharedBundlesListView.Location = new System.Drawing.Point(0, 13);
		this.m_sharedBundlesListView.MultiSelect = false;
		this.m_sharedBundlesListView.Name = "m_sharedBundlesListView";
		this.m_sharedBundlesListView.Size = new System.Drawing.Size(777, 221);
		this.m_sharedBundlesListView.TabIndex = 5;
		this.m_sharedBundlesListView.UseCompatibleStateImageBehavior = false;
		this.m_sharedBundlesListView.View = System.Windows.Forms.View.Details;
		this.columnHeader1.Text = "Bundle Type";
		this.columnHeader1.Width = 95;
		this.columnHeader2.Text = "Product Name";
		this.columnHeader2.Width = 175;
		this.columnHeader3.Text = "Version";
		this.columnHeader4.Text = "Path";
		this.columnHeader4.Width = 410;
		this.label1.AutoSize = true;
		this.label1.Dock = System.Windows.Forms.DockStyle.Top;
		this.label1.Location = new System.Drawing.Point(0, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(85, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Shared Bundles:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(783, 474);
		base.Controls.Add(this.m_panel);
		base.Controls.Add(this.m_splitter);
		base.Controls.Add(this.m_appBundlesListView);
		base.Controls.Add(this.m_toolStrip);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(640, 480);
		base.Name = "SelectBundleForm";
		base.Padding = new System.Windows.Forms.Padding(3);
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Select Resource Bundle";
		base.Load += new System.EventHandler(OnLoad);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnFormClosing);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		this.m_panel.ResumeLayout(false);
		this.m_panel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
