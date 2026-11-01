using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Extensibility.ComponentModel;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

public class PropertyGridView : UserControl
{
	private string m_configurationXml;

	private IDictionary<string, NameValueProperty> m_properties;

	private readonly List<NameValueProperty> m_errors = new List<NameValueProperty>();

	private IContainer components;

	public string ConfigurationXml
	{
		get
		{
			return m_configurationXml;
		}
		set
		{
			m_configurationXml = value;
			m_properties = NameValueProperty.ParseConfigurationXml(value, new ErrorList());
			Configure();
		}
	}

	public event EventHandler PropertyChanged;

	public PropertyGridView()
	{
		InitializeComponent();
		if (ServiceLocator.Instance.GetService(typeof(IPropertyGridService)) is IPropertyGridService propertyGridService)
		{
			propertyGridService.Host.Dock = DockStyle.Fill;
			base.Controls.Add(propertyGridService.Host);
		}
	}

	public void ApplyProperties()
	{
		foreach (string key in m_properties.Keys)
		{
			NameValueProperty nameValueProperty = m_properties[key];
			if (!InstallerProject.Instance.Values.ContainsKey(nameValueProperty.TargetMacro))
			{
				object defaultValue = nameValueProperty.DefaultValue;
				if (defaultValue != null)
				{
					defaultValue = MacroContainer.Instance.ExpandRuntimeMacros(defaultValue.ToString());
				}
				if (defaultValue != null)
				{
					InstallerProject.Instance.Values[nameValueProperty.TargetMacro] = defaultValue.ToString();
				}
			}
		}
	}

	public void Configure()
	{
		if (m_properties == null || !(ServiceLocator.Instance.GetService(typeof(IPropertyGridService)) is IPropertyGridService propertyGridService))
		{
			return;
		}
		propertyGridService.Clear();
		foreach (string key in m_properties.Keys)
		{
			NameValueProperty nameValueProperty = m_properties[key];
			IPropertyGridCategory propertyGridCategory = propertyGridService.GetRootCategory(nameValueProperty.Category) ?? propertyGridService.AddRootCategory(nameValueProperty.Category);
			object typedDefaultValue = nameValueProperty.GetTypedDefaultValue();
			object typedValue = nameValueProperty.GetTypedValue();
			IPropertyGridItem propertyItem = propertyGridCategory.AddProperty(nameValueProperty.Name, typedValue, nameValueProperty.Comment ?? string.Empty);
			propertyItem.Tag = nameValueProperty;
			if (!string.IsNullOrEmpty(nameValueProperty.DisplayName))
			{
				propertyItem.DisplayName = nameValueProperty.DisplayName;
			}
			propertyItem.ReadOnly = nameValueProperty.ReadOnly;
			if (nameValueProperty.DisplayValues.Count > 0)
			{
				propertyItem.DisplayedValues = nameValueProperty.DisplayValues.ToArray();
			}
			propertyItem.DefaultValue = typedDefaultValue;
			propertyItem.PropertyChanged += delegate
			{
				if (nameValueProperty.IsRequired && (propertyItem.Value == null || string.IsNullOrEmpty(propertyItem.Value.ToString())))
				{
					MessageBox.Show($"The property '{nameValueProperty.DisplayName ?? nameValueProperty.Name}' requires a value.", $"{InstallerProject.Instance.ProductName} {InstallerProject.Instance.ProductVersion} Setup", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					if (!m_errors.Contains(nameValueProperty))
					{
						m_errors.Add(nameValueProperty);
					}
				}
				else
				{
					m_errors.Remove(nameValueProperty);
				}
				if (!string.IsNullOrEmpty(nameValueProperty.TargetMacro))
				{
					InstallerProject.Instance.Values[nameValueProperty.TargetMacro] = ((propertyItem.Value != null) ? propertyItem.Value.ToString() : string.Empty);
				}
				if (PropertyChanged != null)
				{
					PropertyChanged(propertyItem, EventArgs.Empty);
				}
			};
			if (nameValueProperty.Editor.HasValue)
			{
				try
				{
					propertyItem.Editor = nameValueProperty.Editor.Value;
				}
				catch
				{
				}
			}
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
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	}
}
