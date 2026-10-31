using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Extensibility.ComponentModel;
using VisualHint.SmartPropertyGrid;

namespace Redbox.Extensibility.Shell.PropertyGrid;

public class PropertyGridItem : IPropertyGridItem
{
	private static int m_globalId;

	private readonly Property m_property;

	private readonly PropertyEnumerator m_enumerator;

	private readonly object m_syncObject = new object();

	private IDictionary<string, IPropertyGridItem> m_items;

	private readonly VisualHint.SmartPropertyGrid.PropertyGrid m_propertyGrid;

	public object Tag { get; set; }

	public int Id { get; private set; }

	public string Name { get; private set; }

	public string Comment { get; private set; }

	public object Value
	{
		get
		{
			return m_property.Value.GetValue();
		}
		set
		{
			PropertyGridService.ThreadSafeHostUpdate(delegate
			{
				m_property.Value.SetValue(value);
			});
		}
	}

	public object PreviousValue => m_property.Value.PreviousValue;

	public bool ReadOnly
	{
		get
		{
			return m_property.Value.ReadOnly;
		}
		set
		{
			MethodInvoker method = delegate
			{
				m_property.Value.ReadOnly = value;
			};
			PropertyGridService.ThreadSafeHostUpdate(method);
		}
	}

	public Color BackColor
	{
		get
		{
			return m_property.BackColor;
		}
		set
		{
			MethodInvoker method = delegate
			{
				m_property.BackColor = value;
			};
			PropertyGridService.ThreadSafeHostUpdate(method);
		}
	}

	public string DisplayName
	{
		get
		{
			return m_property.DisplayName;
		}
		set
		{
			MethodInvoker method = delegate
			{
				m_property.DisplayName = value;
			};
			PropertyGridService.ThreadSafeHostUpdate(method);
		}
	}

	public object DefaultValue
	{
		get
		{
			return ((DefaultValueAttribute)m_property.Value.GetAttribute(typeof(DefaultValueAttribute)))?.Value;
		}
		set
		{
			m_property.Value.SetAttribute(new DefaultValueAttribute(value));
		}
	}

	public string[] DisplayedValues
	{
		get
		{
			return m_property.Value.GetDisplayedValues();
		}
		set
		{
			PropertyGridService.ThreadSafeHostUpdate(delegate
			{
				m_property.Value.ResetDisplayedValues(value);
			});
		}
	}

	public PropertyEditor Editor
	{
		get
		{
			return Enum<PropertyEditor>.ParseIgnoringCase(m_property.Feel.Name, PropertyEditor.None);
		}
		set
		{
			MethodInvoker method = delegate
			{
				if (value == PropertyEditor.EditPassword)
				{
					m_property.Feel = m_propertyGrid.GetRegisteredFeel("editpwd");
					m_property.Look = new PropertyPasswordLook();
				}
				else
				{
					m_property.Feel = m_propertyGrid.GetRegisteredFeel($"{value.ToString().ToLower()}");
				}
			};
			PropertyGridService.ThreadSafeHostUpdate(method);
		}
	}

	internal IDictionary<string, IPropertyGridItem> Items
	{
		get
		{
			if (m_items == null)
			{
				m_items = new Dictionary<string, IPropertyGridItem>();
			}
			return m_items;
		}
	}

	public event EventHandler Activated;

	public event EventHandler ValueValidate;

	public event EventHandler PropertyChanged;

	public event EventHandler PropertyChanging;

	public PropertyGridItem(int id, string name, Property property, PropertyEnumerator enumerator, VisualHint.SmartPropertyGrid.PropertyGrid propertyGrid)
	{
		Id = id;
		Name = name;
		m_property = property;
		m_enumerator = enumerator;
		m_propertyGrid = propertyGrid;
	}

	public void RaiseActivated()
	{
		if (Activated != null)
		{
			Activated(this, EventArgs.Empty);
		}
	}

	public void RaiseValueValidate()
	{
		if (ValueValidate != null)
		{
			ValueValidate(this, EventArgs.Empty);
		}
	}

	public void RaisePropertyChanged()
	{
		if (PropertyChanged != null)
		{
			PropertyChanged(this, EventArgs.Empty);
		}
	}

	public void RaisePropertyChanging()
	{
		if (PropertyChanging != null)
		{
			PropertyChanging(this, EventArgs.Empty);
		}
	}

	public IPropertyGridItem GetProperty(string name)
	{
		return (!Items.ContainsKey(name)) ? null : Items[name];
	}

	public IPropertyGridItem AddProperty(string name, object value, string comment)
	{
		lock (m_syncObject)
		{
			IPropertyGridItem propertyGridItem = GetProperty(name);
			if (propertyGridItem == null)
			{
				PropertyEnumerator enumerator = null;
				MethodInvoker method = delegate
				{
					Type valueType = typeof(string);
					if (value != null)
					{
						valueType = value.GetType();
					}
					enumerator = m_propertyGrid.AppendManagedProperty(m_enumerator, m_globalId, name, valueType, value, comment, new DefaultValueAttribute(value));
				};
				PropertyGridService.ThreadSafeHostUpdate(method);
				propertyGridItem = new PropertyGridItem(m_globalId, name, enumerator.Property, enumerator, m_propertyGrid);
				enumerator.Property.Tag = propertyGridItem;
			}
			Items[name] = propertyGridItem;
			m_globalId++;
			return propertyGridItem;
		}
	}

	public IPropertyGridItem AddHyperlinkProperty(string name, string comment)
	{
		lock (m_syncObject)
		{
			IPropertyGridItem propertyGridItem = GetProperty(name);
			if (propertyGridItem == null)
			{
				PropertyEnumerator enumerator = null;
				MethodInvoker method = delegate
				{
					enumerator = m_propertyGrid.AppendHyperLinkProperty(m_enumerator, m_globalId, name, comment);
				};
				PropertyGridService.ThreadSafeHostUpdate(method);
				propertyGridItem = new PropertyGridItem(m_globalId, name, enumerator.Property, enumerator, m_propertyGrid);
				enumerator.Property.Tag = propertyGridItem;
			}
			Items[name] = propertyGridItem;
			m_globalId++;
			return propertyGridItem;
		}
	}
}
