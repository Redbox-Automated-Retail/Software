using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.IDE;

public class BundleDependencyForm : Form
{
	private IContainer components;

	private ListView m_bundlesListView;

	private ColumnHeader m_productNameColumn;

	private ColumnHeader m_productVersionColumn;

	private ColumnHeader m_pathColumn;

	public IBundleSpecifier BundleSpecifier { get; set; }

	public BundleDependencyForm()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		if (BundleSpecifier == null || BundleSpecifier.Instance.GetManifest(BundleSpecifier.Instance.CreateFilter(), out var manifestInfo) == null || manifestInfo == null)
		{
			return;
		}
		foreach (IBundleSpecifier require in manifestInfo.Requires)
		{
			ListViewItem listViewItem = new ListViewItem(require.Name);
			string text = require.Version.ToString();
			if (text.StartsWith("9"))
			{
				listViewItem.SubItems.Add("*");
			}
			else
			{
				listViewItem.SubItems.Add(text);
			}
			if (require.Instance != null)
			{
				listViewItem.SubItems.Add(require.Instance.Storage.BundlePath);
			}
			else
			{
				listViewItem.SubItems.Add("N/A");
			}
			listViewItem.Tag = require.Instance;
			m_bundlesListView.Items.Add(listViewItem);
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
		this.m_bundlesListView = new System.Windows.Forms.ListView();
		this.m_productNameColumn = new System.Windows.Forms.ColumnHeader();
		this.m_productVersionColumn = new System.Windows.Forms.ColumnHeader();
		this.m_pathColumn = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.m_bundlesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_productNameColumn, this.m_productVersionColumn, this.m_pathColumn });
		this.m_bundlesListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_bundlesListView.FullRowSelect = true;
		this.m_bundlesListView.HideSelection = false;
		this.m_bundlesListView.Location = new System.Drawing.Point(0, 0);
		this.m_bundlesListView.MultiSelect = false;
		this.m_bundlesListView.Name = "m_bundlesListView";
		this.m_bundlesListView.Size = new System.Drawing.Size(748, 287);
		this.m_bundlesListView.TabIndex = 5;
		this.m_bundlesListView.UseCompatibleStateImageBehavior = false;
		this.m_bundlesListView.View = System.Windows.Forms.View.Details;
		this.m_productNameColumn.Text = "Product Name";
		this.m_productNameColumn.Width = 175;
		this.m_productVersionColumn.Text = "Version";
		this.m_pathColumn.Text = "Path";
		this.m_pathColumn.Width = 410;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(748, 287);
		base.Controls.Add(this.m_bundlesListView);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "BundleDependencyForm";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Bundle Dependencies";
		base.Load += new System.EventHandler(OnLoad);
		base.ResumeLayout(false);
	}
}
