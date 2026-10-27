using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class BundleFilterToolWindow : ToolWindow
{
	private IContainer components;

	private DynamicPropertyGrid m_dynamicPropertyGrid;

	public BundleFilterToolWindow()
	{
		InitializeComponent();
		m_dynamicPropertyGrid.PropertyChanged += delegate(object sender, VisualHint.SmartPropertyGrid.PropertyChangedEventArgs e)
		{
			if (e.PropertyEnum.Property.Tag is IPropertyType propertyType)
			{
				base.BundleService.Filter[propertyType.Name] = e.PropertyEnum.Property.Value.GetValue() as string;
			}
		};
	}

	public void RefreshFilters()
	{
		InnerRefreshFilters();
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		InnerRefreshFilters();
	}

	private void InnerRefreshFilters()
	{
		m_dynamicPropertyGrid.SuspendLayout();
		m_dynamicPropertyGrid.BeginUpdate();
		m_dynamicPropertyGrid.Clear();
		m_dynamicPropertyGrid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
		int num = 2;
		List<string> list = new List<string>();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		foreach (KeyValuePair<string, IResourceType> resourceType in service.GetResourceTypes())
		{
			ReadOnlyCollection<IPropertyType> filterProperties = resourceType.Value.GetFilterProperties();
			if (filterProperties.Count == 0)
			{
				continue;
			}
			List<IPropertyType> list2 = new List<IPropertyType>();
			foreach (IPropertyType each in filterProperties)
			{
				if (list.Find((string eachName) => each.Name == eachName) == null)
				{
					list2.Add(each);
					list.Add(each.Name);
				}
			}
			if (list2.Count == 0)
			{
				continue;
			}
			PropertyEnumerator underCategory = m_dynamicPropertyGrid.AppendRootCategory(num++, resourceType.Key);
			foreach (IPropertyType item in list2)
			{
				m_dynamicPropertyGrid.AppendManagedProperty(underCategory, num++, $"{item.Label} ({item.Name})", item.Type, service.Filter[item.Name], item.Description ?? string.Empty, new DefaultValueAttribute(item.DefaultValue)).Property.Tag = item;
			}
		}
		m_dynamicPropertyGrid.EndUpdate();
		m_dynamicPropertyGrid.ResumeLayout();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.BundleFilterToolWindow));
		this.m_dynamicPropertyGrid = new Redbox.KioskEngine.IDE.DynamicPropertyGrid();
		((System.ComponentModel.ISupportInitialize)this.m_dynamicPropertyGrid).BeginInit();
		base.SuspendLayout();
		this.m_dynamicPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_dynamicPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_dynamicPropertyGrid.Location = new System.Drawing.Point(0, 0);
		this.m_dynamicPropertyGrid.Name = "m_dynamicPropertyGrid";
		this.m_dynamicPropertyGrid.PropertyLabelBackColor = System.Drawing.SystemColors.Window;
		this.m_dynamicPropertyGrid.PropertyValueBackColor = System.Drawing.SystemColors.Window;
		this.m_dynamicPropertyGrid.ShowAdditionalIndentation = true;
		this.m_dynamicPropertyGrid.ShowDefaultValues = true;
		this.m_dynamicPropertyGrid.Size = new System.Drawing.Size(460, 260);
		this.m_dynamicPropertyGrid.TabIndex = 0;
		this.m_dynamicPropertyGrid.ToolbarVisibility = true;
		this.m_dynamicPropertyGrid.ToolTipMode = VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
		this.m_dynamicPropertyGrid.RefreshGrid += new System.EventHandler(OnRefresh);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(460, 260);
		base.Controls.Add(this.m_dynamicPropertyGrid);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "BundleFilterToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Bundle Filters";
		((System.ComponentModel.ISupportInitialize)this.m_dynamicPropertyGrid).EndInit();
		base.ResumeLayout(false);
	}
}
