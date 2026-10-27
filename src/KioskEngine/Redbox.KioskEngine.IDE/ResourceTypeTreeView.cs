using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.IDE;

public class ResourceTypeTreeView : UserControl
{
	private IContainer components;

	private BaseTreeView m_treeView;

	private ImageList m_imageList;

	public IResourceBundle Bundle { get; set; }

	public IResourceType SelectedType { get; set; }

	public ResourceTypeTreeView()
	{
		InitializeComponent();
	}

	public void RefreshTree()
	{
	}

	private void OnAfterSelect(object sender, TreeViewEventArgs e)
	{
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
		this.m_treeView = new Redbox.KioskEngine.IDE.BaseTreeView();
		this.m_imageList = new System.Windows.Forms.ImageList(this.components);
		base.SuspendLayout();
		this.m_treeView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_treeView.Location = new System.Drawing.Point(0, 0);
		this.m_treeView.Name = "m_treeView";
		this.m_treeView.Size = new System.Drawing.Size(281, 357);
		this.m_treeView.TabIndex = 0;
		this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnAfterSelect);
		this.m_imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this.m_imageList.ImageSize = new System.Drawing.Size(16, 16);
		this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_treeView);
		base.Name = "ResourceTypeTreeView";
		base.Size = new System.Drawing.Size(281, 357);
		base.ResumeLayout(false);
	}
}
