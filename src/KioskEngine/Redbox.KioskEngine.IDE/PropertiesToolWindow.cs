using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class PropertiesToolWindow : ToolWindow
{
	internal class PropertyHandler
	{
		public IDictionary<string, object> Properties = new Dictionary<string, object>();

		public virtual void UpdatePropertyHost(Property property)
		{
			if (Properties.ContainsKey("resource") && Properties.ContainsKey("propertyType"))
			{
				IResource resource = Properties["resource"] as IResource;
				IPropertyType propertyType = Properties["propertyType"] as IPropertyType;
				if (resource != null && propertyType != null)
				{
					resource[propertyType.Name] = property.Value.GetValue();
				}
			}
		}
	}

	private IContainer components;

	private DynamicPropertyGrid m_dynamicPropertyGrid;

	internal DynamicPropertyGrid Properties => m_dynamicPropertyGrid;

	public PropertiesToolWindow()
	{
		InitializeComponent();
		m_dynamicPropertyGrid.PropertyChanged += delegate(object sender, VisualHint.SmartPropertyGrid.PropertyChangedEventArgs e)
		{
			if (e.PropertyEnum.Property.Tag is PropertyHandler propertyHandler)
			{
				propertyHandler.UpdatePropertyHost(e.PropertyEnum.Property);
			}
		};
	}

	public void SetObject(object instance)
	{
		m_dynamicPropertyGrid.BeginUpdate();
		m_dynamicPropertyGrid.Clear();
		int num = 2;
		_ = instance is IResourceBundle;
		if (instance is IResource resource)
		{
			Dictionary<string, PropertyEnumerator> dictionary = new Dictionary<string, PropertyEnumerator>();
			IResourceType resourceType = resource.Type;
			PropertyEnumerator propertyEnumerator = null;
			while (resourceType != null)
			{
				foreach (IPropertyType property in resourceType.Properties)
				{
					try
					{
						PropertyEnumerator propertyEnumerator2 = null;
						if (!string.IsNullOrEmpty(property.Category))
						{
							if (!dictionary.ContainsKey(property.Category))
							{
								dictionary[property.Category] = m_dynamicPropertyGrid.AppendRootCategory(num++, property.Category);
							}
							propertyEnumerator2 = dictionary[property.Category];
						}
						else if (propertyEnumerator == null)
						{
							propertyEnumerator = m_dynamicPropertyGrid.AppendRootCategory(num++, "Misc");
						}
						string propName = property.Name;
						if (!string.IsNullOrEmpty(property.Label))
						{
							propName = $"{property.Label} ({property.Name})";
						}
						object value = resource[property.Name];
						object typedValue = property.GetTypedValue(value);
						object typedValue2 = property.GetTypedValue(property.DefaultValue);
						if (typedValue is List<object>)
						{
							List<string> initialValue = convertValueArray(typedValue as List<object>);
							m_dynamicPropertyGrid.AppendManagedProperty(propertyEnumerator2 ?? propertyEnumerator, num++, propName, typeof(List<string>), initialValue, property.Description ?? string.Empty, new DefaultValueAttribute(typedValue2));
						}
						else
						{
							m_dynamicPropertyGrid.AppendManagedProperty(propertyEnumerator2 ?? propertyEnumerator, num++, propName, property.Type, typedValue, property.Description ?? string.Empty, new DefaultValueAttribute(typedValue2)).Property.Tag = new PropertyHandler
							{
								Properties = 
								{
									{
										"resource",
										(object)resource
									},
									{
										"propertyType",
										(object)property
									}
								}
							};
						}
					}
					catch (Exception)
					{
					}
				}
				resourceType = resourceType.SuperType;
			}
		}
		_ = instance is IResourceType;
		if (instance is KernelFunctionInfo kernelFunctionInfo)
		{
			Dictionary<string, PropertyEnumerator> dictionary2 = new Dictionary<string, PropertyEnumerator>();
			dictionary2["Library"] = m_dynamicPropertyGrid.AppendRootCategory(num++, "Library");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Library"], num++, "Extension", typeof(string), kernelFunctionInfo.Extension, "The Kiosk Extension name or Core name into which the function is grouped.");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Library"], num++, "Namespace", typeof(string), kernelFunctionInfo.Method.DeclaringType.FullName, "The namespace in which this function is defined");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Library"], num++, "Module", typeof(string), kernelFunctionInfo.Method.Module.Name, "The DLL into which this method is compiled and distributed.");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Library"], num++, "C# Function", typeof(string), kernelFunctionInfo.Method.Name, "The C# function which implements the API when it is called from Lua.");
			dictionary2["Function"] = m_dynamicPropertyGrid.AppendRootCategory(num++, "Function");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Function"], num++, "API Name", typeof(string), kernelFunctionInfo.Attributes[0].Name, "The API name which is accessible from the Lua runtime.");
			m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Function"], num++, "Return Type", typeof(string), kernelFunctionInfo.Method.ReturnType.Name, "The object type of the return value.");
			dictionary2["Parameters"] = m_dynamicPropertyGrid.AppendRootCategory(num++, "Parameters");
			ParameterInfo[] parameters = kernelFunctionInfo.Method.GetParameters();
			if (parameters != null)
			{
				ParameterInfo[] array = parameters;
				foreach (ParameterInfo parameterInfo in array)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("(" + (parameterInfo.IsOut ? "out" : "in") + ") ");
					stringBuilder.Append(parameterInfo.ParameterType.Name + " ");
					m_dynamicPropertyGrid.AppendManagedProperty(dictionary2["Parameters"], num++, parameterInfo.Name, typeof(string), stringBuilder.ToString(), string.Empty);
				}
			}
		}
		m_dynamicPropertyGrid.EndUpdate();
	}

	private List<string> convertValueArray(List<object> input)
	{
		List<string> list = new List<string>();
		foreach (object item in input)
		{
			if (item is string)
			{
				list.Add(item as string);
			}
		}
		return list;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.PropertiesToolWindow));
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
		this.m_dynamicPropertyGrid.Size = new System.Drawing.Size(342, 303);
		this.m_dynamicPropertyGrid.TabIndex = 0;
		this.m_dynamicPropertyGrid.Text = "dynamicPropertyGrid1";
		this.m_dynamicPropertyGrid.ToolbarVisibility = true;
		this.m_dynamicPropertyGrid.ToolTipMode = VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(342, 303);
		base.Controls.Add(this.m_dynamicPropertyGrid);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "PropertiesToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide;
		this.Text = "Properties";
		((System.ComponentModel.ISupportInitialize)this.m_dynamicPropertyGrid).EndInit();
		base.ResumeLayout(false);
	}
}
