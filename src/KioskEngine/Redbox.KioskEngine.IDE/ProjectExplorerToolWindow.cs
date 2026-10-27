using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.REDS.Framework;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ProjectExplorerToolWindow : ToolWindow
{
	private enum ResourceTypeIcons
	{
		invalid = 4,
		bitmap,
		movie,
		font,
		sound,
		template,
		script,
		table,
		view,
		publickey,
		manifest,
		stylesheet
	}

	private enum KernelExtensionIcons
	{
		function = 16,
		extension,
		namespaces
	}

	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_applyFilterToolStripButton;

	private BaseTreeView m_treeView;

	private ImageList m_imageList;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton m_exploreBundle;

	private ToolStripButton m_exploreKernel;

	private ContextMenuStrip m_contextMenu;

	private ToolStripMenuItem m_contextGenerateDoc;

	private ToolStripMenuItem asHTMLToFileToolStripMenuItem;

	private ToolStripMenuItem asHTMLToWindowToolStripMenuItem;

	private ToolStripMenuItem asXMLToFileToolStripMenuItem;

	private ToolStripMenuItem asXMLToWindowToolStripMenuItem;

	public ProjectExplorerToolWindow()
	{
		InitializeComponent();
	}

	public void RefreshTree()
	{
		if (m_exploreBundle.Checked)
		{
			InnerRefreshTree2();
		}
		else if (m_exploreKernel.Checked)
		{
			InnerRefreshTree3();
		}
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshTree();
	}

	private void OnAfterSelect(object sender, TreeViewEventArgs e)
	{
		IIdeService service = ServiceLocator.Instance.GetService<IIdeService>();
		if (service != null)
		{
			((IdeMainForm)service.MainForm).PropertiesWindow.SetObject(e.Node.Tag);
		}
	}

	private void OnDoubleClick(object sender, EventArgs e)
	{
		if (m_treeView.SelectedNode == null)
		{
			return;
		}
		IIdeService service = ServiceLocator.Instance.GetService<IIdeService>();
		if (service != null)
		{
			IdeMainForm ideMainForm = (IdeMainForm)service.MainForm;
			if (m_exploreBundle.Checked && m_treeView.SelectedNode.Tag is IResource resource)
			{
				ideMainForm.OpenResourceEditor(resource);
			}
		}
	}

	private void OnApplyFilter(object sender, EventArgs e)
	{
		m_applyFilterToolStripButton.Checked = !m_applyFilterToolStripButton.Checked;
	}

	private void InnerRefreshTree2()
	{
		m_treeView.BeginUpdate();
		m_treeView.Nodes.Clear();
		if (!base.BundleService.HasActiveBundle())
		{
			return;
		}
		IDictionary<string, IResourceType> resourceTypes = base.BundleService.GetResourceTypes();
		foreach (IBundleSpecifier bundle2 in base.BundleService.ActiveBundleSet.GetBundles())
		{
			TreeNode treeNode = new TreeNode($"{bundle2.Name} ( {bundle2.Version} )", (int)bundle2.Type, (int)bundle2.Type);
			IResourceBundle bundle = base.BundleService.GetBundle(bundle2);
			foreach (KeyValuePair<string, IResourceType> item in resourceTypes)
			{
				TreeNode treeNode2 = new TreeNode(item.Key, 3, 3);
				foreach (IResource allResource in bundle.GetAllResources(item.Key))
				{
					string text = allResource["program_name"] as string;
					string obj = (string.IsNullOrEmpty(text) ? allResource.Name : $"{allResource.Name} '{text}'");
					ResourceTypeIcons resourceTypeIcons = Enum<ResourceTypeIcons>.ParseIgnoringCase(item.Key, ResourceTypeIcons.invalid);
					TreeNode node = new TreeNode(obj, (int)resourceTypeIcons, (int)resourceTypeIcons)
					{
						Tag = allResource
					};
					treeNode2.Nodes.Add(node);
				}
				if (treeNode2.Nodes.Count > 0)
				{
					treeNode.Nodes.Add(treeNode2);
				}
			}
			if (bundle2.Type == BundleType.Application)
			{
				m_treeView.Nodes.Insert(0, treeNode);
			}
			else
			{
				m_treeView.Nodes.Add(treeNode);
			}
		}
		m_treeView.EndUpdate();
		m_treeView.ExpandAll();
	}

	private void InnerRefreshTree3()
	{
		m_treeView.BeginUpdate();
		m_treeView.Nodes.Clear();
		foreach (KeyValuePair<string, object> item in GetAPIDictionary())
		{
			TreeNode treeNode = new TreeNode(item.Key, 17, 17);
			if (item.Value is SortedDictionary<string, object> sortedDictionary)
			{
				foreach (KeyValuePair<string, object> item2 in sortedDictionary)
				{
					TreeNode treeNode2 = new TreeNode(item2.Key, 18, 18);
					if (item2.Value is SortedDictionary<string, object> sortedDictionary2)
					{
						foreach (KeyValuePair<string, object> item3 in sortedDictionary2)
						{
							TreeNode treeNode3 = new TreeNode(item3.Key, 16, 16);
							treeNode3.Tag = item3.Value;
							treeNode2.Nodes.Add(treeNode3);
						}
					}
					treeNode.Nodes.Add(treeNode2);
				}
			}
			m_treeView.Nodes.Add(treeNode);
		}
		m_treeView.EndUpdate();
	}

	private SortedDictionary<string, object> GetAPIDictionary()
	{
		SortedDictionary<string, object> sortedDictionary = new SortedDictionary<string, object>();
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			foreach (KernelFunctionInfo kernelFunction in service.GetKernelFunctions())
			{
				if (!sortedDictionary.ContainsKey(kernelFunction.Extension))
				{
					sortedDictionary.Add(kernelFunction.Extension, new SortedDictionary<string, object>());
				}
				if (sortedDictionary[kernelFunction.Extension] is SortedDictionary<string, object> sortedDictionary2)
				{
					if (!sortedDictionary2.ContainsKey(kernelFunction.Method.DeclaringType.FullName))
					{
						sortedDictionary2.Add(kernelFunction.Method.DeclaringType.FullName, new SortedDictionary<string, object>());
					}
					if (sortedDictionary2[kernelFunction.Method.DeclaringType.FullName] is SortedDictionary<string, object> sortedDictionary3 && !sortedDictionary3.ContainsKey(kernelFunction.Attributes[0].Name))
					{
						sortedDictionary3.Add(kernelFunction.Attributes[0].Name, kernelFunction);
					}
				}
			}
		}
		return sortedDictionary;
	}

	private void m_exploreBundle_Click(object sender, EventArgs e)
	{
		m_exploreKernel.Checked = false;
		m_contextGenerateDoc.Visible = false;
		InnerRefreshTree2();
	}

	private void m_exploreKernel_Click(object sender, EventArgs e)
	{
		m_exploreBundle.Checked = false;
		m_contextGenerateDoc.Visible = true;
		InnerRefreshTree3();
	}

	private void asHTMLToFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string directoryName = Path.GetDirectoryName(Application.ExecutablePath);
		SortedDictionary<string, object> aPIDictionary = GetAPIDictionary();
		using StreamWriter streamWriter = new StreamWriter(directoryName + "\\APIOutput.html");
		streamWriter.WriteLine("<HTML>");
		streamWriter.WriteLine("<BODY>");
		foreach (KeyValuePair<string, object> item in aPIDictionary)
		{
			streamWriter.WriteLine($"<h1 style=\"font-family:Segoe UI\">{item.Key}</h1>");
			if (!(item.Value is SortedDictionary<string, object> sortedDictionary))
			{
				continue;
			}
			foreach (KeyValuePair<string, object> item2 in sortedDictionary)
			{
				streamWriter.WriteLine($"<p style=\"font-style:bold;font-family:Segoe UI;color:blue;font-size:14;\">{item2.Key}</p>");
				if (!(item2.Value is SortedDictionary<string, object> sortedDictionary2))
				{
					continue;
				}
				streamWriter.WriteLine("<table border=\"0\" width=\"100%\" style=\"font-family:Courier;font-size:10\">");
				streamWriter.WriteLine("<col valign=\"top\">");
				streamWriter.WriteLine("<col valign=\"top\">");
				foreach (KeyValuePair<string, object> item3 in sortedDictionary2)
				{
					if (!(item3.Value is KernelFunctionInfo kernelFunctionInfo))
					{
						continue;
					}
					streamWriter.WriteLine("<tr>");
					streamWriter.WriteLine("<td width=\"15%\">" + kernelFunctionInfo.Attributes[0].Name + "</td>");
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("<font color=\"blue\">internal " + kernelFunctionInfo.Method.ReturnType.Name + " ");
					stringBuilder.Append("<font color=\"black\">" + kernelFunctionInfo.Method.Name + "(");
					ParameterInfo[] parameters = kernelFunctionInfo.Method.GetParameters();
					int num = 0;
					if (parameters != null)
					{
						ParameterInfo[] array = parameters;
						foreach (ParameterInfo parameterInfo in array)
						{
							num++;
							stringBuilder.Append("<font color=\"blue\">" + parameterInfo.ParameterType.Name + " <font color=\"black\">" + parameterInfo.Name);
							if (num < parameters.Count())
							{
								stringBuilder.Append(", ");
							}
						}
					}
					stringBuilder.Append(")");
					streamWriter.WriteLine("<td width=\"85%\">" + stringBuilder.ToString() + "</td>");
					streamWriter.WriteLine("</tr>");
				}
				streamWriter.WriteLine("</table>");
			}
		}
		streamWriter.WriteLine("</BODY>");
		streamWriter.WriteLine("</HTML>");
		streamWriter.Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.ProjectExplorerToolWindow));
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_exploreBundle = new System.Windows.Forms.ToolStripButton();
		this.m_exploreKernel = new System.Windows.Forms.ToolStripButton();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_applyFilterToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_treeView = new Redbox.KioskEngine.IDE.BaseTreeView();
		this.m_contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.m_contextGenerateDoc = new System.Windows.Forms.ToolStripMenuItem();
		this.m_imageList = new System.Windows.Forms.ImageList(this.components);
		this.asHTMLToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.asHTMLToWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.asXMLToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.asXMLToWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.m_toolStrip.SuspendLayout();
		this.m_contextMenu.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.m_exploreBundle, this.m_exploreKernel, this.m_refreshToolStripButton, this.toolStripSeparator1, this.m_applyFilterToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(349, 23);
		this.m_toolStrip.TabIndex = 0;
		this.m_toolStrip.Text = "toolStrip1";
		this.m_exploreBundle.Checked = true;
		this.m_exploreBundle.CheckOnClick = true;
		this.m_exploreBundle.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_exploreBundle.Image = Redbox.KioskEngine.IDE.Properties.Resources.Cube;
		this.m_exploreBundle.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_exploreBundle.Name = "m_exploreBundle";
		this.m_exploreBundle.Size = new System.Drawing.Size(64, 20);
		this.m_exploreBundle.Text = "Bundle";
		this.m_exploreBundle.Click += new System.EventHandler(m_exploreBundle_Click);
		this.m_exploreKernel.CheckOnClick = true;
		this.m_exploreKernel.Image = Redbox.KioskEngine.IDE.Properties.Resources.Sitemap;
		this.m_exploreKernel.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_exploreKernel.Name = "m_exploreKernel";
		this.m_exploreKernel.Size = new System.Drawing.Size(60, 20);
		this.m_exploreKernel.Text = "Kernel";
		this.m_exploreKernel.Click += new System.EventHandler(m_exploreKernel_Click);
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.ToolTipText = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.m_applyFilterToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_applyFilterToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Filter;
		this.m_applyFilterToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_applyFilterToolStripButton.Name = "m_applyFilterToolStripButton";
		this.m_applyFilterToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_applyFilterToolStripButton.ToolTipText = "Apply Bundle Filter";
		this.m_applyFilterToolStripButton.Click += new System.EventHandler(OnApplyFilter);
		this.m_treeView.ContextMenuStrip = this.m_contextMenu;
		this.m_treeView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_treeView.FullRowSelect = true;
		this.m_treeView.HideSelection = false;
		this.m_treeView.ImageIndex = 0;
		this.m_treeView.ImageList = this.m_imageList;
		this.m_treeView.Location = new System.Drawing.Point(0, 23);
		this.m_treeView.Name = "m_treeView";
		this.m_treeView.PathSeparator = "/";
		this.m_treeView.SelectedImageIndex = 0;
		this.m_treeView.Size = new System.Drawing.Size(349, 597);
		this.m_treeView.TabIndex = 1;
		this.m_treeView.DoubleClick += new System.EventHandler(OnDoubleClick);
		this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnAfterSelect);
		this.m_contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.m_contextGenerateDoc });
		this.m_contextMenu.Name = "m_contextMenu";
		this.m_contextMenu.Size = new System.Drawing.Size(208, 48);
		this.m_contextGenerateDoc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.asHTMLToFileToolStripMenuItem, this.asHTMLToWindowToolStripMenuItem, this.asXMLToFileToolStripMenuItem, this.asXMLToWindowToolStripMenuItem });
		this.m_contextGenerateDoc.Name = "m_contextGenerateDoc";
		this.m_contextGenerateDoc.Size = new System.Drawing.Size(207, 22);
		this.m_contextGenerateDoc.Text = "Generate Documentation";
		this.m_contextGenerateDoc.Visible = false;
		this.m_imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_imageList.ImageStream");
		this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_imageList.Images.SetKeyName(0, "Application-red.png");
		this.m_imageList.Images.SetKeyName(1, "Gear-red.png");
		this.m_imageList.Images.SetKeyName(2, "Dots-red.png");
		this.m_imageList.Images.SetKeyName(3, "Arrow3 Right.png");
		this.m_imageList.Images.SetKeyName(4, "Cancel.png");
		this.m_imageList.Images.SetKeyName(5, "Picture.png");
		this.m_imageList.Images.SetKeyName(6, "Movie.png");
		this.m_imageList.Images.SetKeyName(7, "Fbook.png");
		this.m_imageList.Images.SetKeyName(8, "Music.png");
		this.m_imageList.Images.SetKeyName(9, "Clipboard Copy.png");
		this.m_imageList.Images.SetKeyName(10, "Document.png");
		this.m_imageList.Images.SetKeyName(11, "Table.png");
		this.m_imageList.Images.SetKeyName(12, "Screen.png");
		this.m_imageList.Images.SetKeyName(13, "Key.png");
		this.m_imageList.Images.SetKeyName(14, "Cube.png");
		this.m_imageList.Images.SetKeyName(15, "Write.png");
		this.m_imageList.Images.SetKeyName(16, "Document2.png");
		this.m_imageList.Images.SetKeyName(17, "Dots Down.png");
		this.m_imageList.Images.SetKeyName(18, "Folder3.png");
		this.asHTMLToFileToolStripMenuItem.Name = "asHTMLToFileToolStripMenuItem";
		this.asHTMLToFileToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
		this.asHTMLToFileToolStripMenuItem.Text = "As HTML to file";
		this.asHTMLToFileToolStripMenuItem.Click += new System.EventHandler(asHTMLToFileToolStripMenuItem_Click);
		this.asHTMLToWindowToolStripMenuItem.Name = "asHTMLToWindowToolStripMenuItem";
		this.asHTMLToWindowToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
		this.asHTMLToWindowToolStripMenuItem.Text = "As HTML to window";
		this.asXMLToFileToolStripMenuItem.Name = "asXMLToFileToolStripMenuItem";
		this.asXMLToFileToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
		this.asXMLToFileToolStripMenuItem.Text = "As XML to file";
		this.asXMLToWindowToolStripMenuItem.Name = "asXMLToWindowToolStripMenuItem";
		this.asXMLToWindowToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
		this.asXMLToWindowToolStripMenuItem.Text = "As XML to window";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(349, 620);
		base.ControlBox = false;
		base.Controls.Add(this.m_treeView);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Name = "ProjectExplorerToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
		this.Text = "Project Explorer";
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		this.m_contextMenu.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
