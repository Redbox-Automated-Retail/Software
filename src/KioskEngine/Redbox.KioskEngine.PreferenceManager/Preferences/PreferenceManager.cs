using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.KioskEngine.PreferenceManager.Preferences;

public class PreferenceManager
{
	public List<IPreferencePage> PreferencePages = new List<IPreferencePage>();

	public void RegisterPreferencePages()
	{
		Type preferencePageHostInterfaceType = typeof(IPreferencePageHost);
		foreach (Type item in from x in Assembly.GetExecutingAssembly().GetTypes()
			where preferencePageHostInterfaceType.IsAssignableFrom(x) && x.IsClass
			select x)
		{
			PreferencePageAttribute preferencePageAttribute = (PreferencePageAttribute)Attribute.GetCustomAttribute(item, typeof(PreferencePageAttribute));
			if (preferencePageAttribute != null)
			{
				PreferencePages.Add(new PreferencePage
				{
					Path = preferencePageAttribute.Path,
					Name = preferencePageAttribute.Name,
					PreferencePageHostType = item,
					TreeOrder = preferencePageAttribute.TreeOrder
				});
			}
		}
		PreferencePages.Sort((IPreferencePage a, IPreferencePage b) => a.TreeOrder.CompareTo(b.TreeOrder));
	}
}
