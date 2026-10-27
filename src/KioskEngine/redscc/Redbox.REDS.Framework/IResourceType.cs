using System;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public interface IResourceType
{
	ReadOnlyCollection<IPropertyType> Properties { get; }

	ReadOnlyCollection<IAspectType> AspectTypes { get; }

	ReadOnlyCollection<IResourceType> SubTypes { get; }

	IResourceType SuperType { get; }

	string Name { get; }

	IAspectType AddAspectType(string name, Type runtimeType);

	IPropertyType AddPropertyType(string name, Type type, string description, bool isRequired, bool isFilter, int? filterScore, string label, string category);

	IResourceType NewType(string name, Type runtimeType);

	void RemoveAspectType(string name);

	void RemovePropertyType(string name);

	IAspectType GetAspectType(string name);

	IResource NewResourceInstance(string name);

	IPropertyType GetPropertyType(string name);

	ReadOnlyCollection<IPropertyType> GetFilterProperties();
}
