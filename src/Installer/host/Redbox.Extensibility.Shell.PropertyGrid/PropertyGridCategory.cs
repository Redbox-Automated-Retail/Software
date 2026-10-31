using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Redbox.Extensibility.ComponentModel;
using VisualHint.SmartPropertyGrid;

namespace Redbox.Extensibility.Shell.PropertyGrid;

public class PropertyGridCategory : IPropertyGridCategory
{
	private static int m_globalId;

	private readonly PropertyEnumerator m_enumerator;

	private readonly DynamicPropertyGrid m_propertyGrid;

	private readonly object m_syncObject = new object();

	private IDictionary<string, IPropertyGridItem> m_items;

	private IDictionary<string, IPropertyGridCategory> m_subCategories;

	public int Id { get; private set; }

	public string Name { get; private set; }

	public bool IsRoot { get; private set; }

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

	internal IDictionary<string, IPropertyGridCategory> SubCategories
	{
		get
		{
			if (m_subCategories == null)
			{
				m_subCategories = new Dictionary<string, IPropertyGridCategory>();
			}
			return m_subCategories;
		}
	}

	public PropertyGridCategory(int id, string name, bool isRoot, PropertyEnumerator enumerator, DynamicPropertyGrid propertyGrid)
	{
		Id = id;
		Name = name;
		IsRoot = isRoot;
		m_enumerator = enumerator;
		m_propertyGrid = propertyGrid;
	}

	public void Clear()
	{
		lock (m_syncObject)
		{
			PropertyGridService.ThreadSafeHostUpdate(delegate
			{
				m_propertyGrid.ClearUnderProperty(m_enumerator);
			});
			SubCategories.Clear();
		}
	}

	public IPropertyGridCategory GetSubCategory(string name)
	{
		return (!SubCategories.ContainsKey(name)) ? null : SubCategories[name];
	}

	public IPropertyGridCategory AddSubCategory(string name)
	{
		lock (m_syncObject)
		{
			IPropertyGridCategory propertyGridCategory = GetSubCategory(name);
			if (propertyGridCategory == null)
			{
				PropertyEnumerator enumerator = null;
				MethodInvoker method = delegate
				{
					enumerator = m_propertyGrid.AppendSubCategory(m_enumerator, m_globalId, name);
				};
				PropertyGridService.ThreadSafeHostUpdate(method);
				propertyGridCategory = new PropertyGridCategory(m_globalId, name, isRoot: false, enumerator, m_propertyGrid);
				SubCategories[name] = propertyGridCategory;
				m_globalId++;
			}
			return propertyGridCategory;
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
