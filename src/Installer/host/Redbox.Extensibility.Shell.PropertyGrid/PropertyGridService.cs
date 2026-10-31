using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.Extensibility.ComponentModel;
using VisualHint.SmartPropertyGrid;

namespace Redbox.Extensibility.Shell.PropertyGrid;

public class PropertyGridService : IPropertyGridService
{
	private static int m_globalId;

	private DynamicPropertyGrid m_propertyGrid;

	private readonly object m_syncObject = new object();

	private static IShellConfiguration m_shellConfiguration;

	private IDictionary<string, IPropertyGridCategory> m_rootCategories;

	public static IPropertyGridService Instance => Singleton<PropertyGridService>.Instance;

	public Control Host => PropertyGrid;

	internal DynamicPropertyGrid PropertyGrid
	{
		get
		{
			if (m_propertyGrid == null)
			{
				m_propertyGrid = new DynamicPropertyGrid();
			}
			return m_propertyGrid;
		}
	}

	internal static IShellConfiguration ShellConfiguration
	{
		get
		{
			if (m_shellConfiguration == null)
			{
				m_shellConfiguration = ServiceLocator.Instance.GetService(typeof(IShellConfiguration)) as IShellConfiguration;
			}
			return m_shellConfiguration;
		}
	}

	internal IDictionary<string, IPropertyGridCategory> RootCategories
	{
		get
		{
			if (m_rootCategories == null)
			{
				m_rootCategories = new Dictionary<string, IPropertyGridCategory>();
			}
			return m_rootCategories;
		}
	}

	public IPropertyGridCategory GetRootCategory(string name)
	{
		return RootCategories.ContainsKey(name) ? RootCategories[name] : null;
	}

	public IPropertyGridCategory AddRootCategory(string name)
	{
		lock (m_syncObject)
		{
			IPropertyGridCategory propertyGridCategory = GetRootCategory(name);
			if (propertyGridCategory == null)
			{
				PropertyEnumerator enumerator = null;
				MethodInvoker method = delegate
				{
					enumerator = PropertyGrid.AppendRootCategory(m_globalId, name);
				};
				ThreadSafeHostUpdate(method);
				propertyGridCategory = new PropertyGridCategory(m_globalId, name, isRoot: true, enumerator, PropertyGrid);
				RootCategories[name] = propertyGridCategory;
				m_globalId++;
			}
			return propertyGridCategory;
		}
	}

	public IPropertyGridItem[] GetProperty(string name)
	{
		List<IPropertyGridItem> list = new List<IPropertyGridItem>();
		foreach (IPropertyGridCategory value in RootCategories.Values)
		{
			IPropertyGridItem property = value.GetProperty(name);
			if (property != null)
			{
				list.Add(property);
			}
		}
		return list.ToArray();
	}

	public void Clear()
	{
		lock (m_syncObject)
		{
			RootCategories.Clear();
			ThreadSafeHostUpdate(delegate
			{
				PropertyGrid.Clear();
			});
		}
	}

	internal static void ThreadSafeHostUpdate(MethodInvoker method)
	{
		ShellConfiguration.ThreadSafeHostUpdate(method);
	}

	private PropertyGridService()
	{
	}
}
