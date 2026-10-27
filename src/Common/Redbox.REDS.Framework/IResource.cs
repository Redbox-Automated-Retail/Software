using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public interface IResource
{
	string Name { get; }

	IResourceType Type { get; }

	object this[string name] { get; set; }

	ReadOnlyCollection<string> Dependencies { get; }

	IResourceBundleStorageEntry StorageEntry { get; }

	bool IsOverriden(ReadOnlyCollection<string> exclusions);

	IAspect NewAspect(string name, IAspectType type);

	string GetFullName();

	IAspect GetAspect(string name);

	void AddDependency(string name);

	void RemoveDependency(string name);

	ErrorList Save(IResourceBundle bundle);

	bool PropertyEquals(string name, object value);

	bool PropertyEquals2(string name, object value);
}
