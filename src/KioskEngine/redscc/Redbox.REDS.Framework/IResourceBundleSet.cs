using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public interface IResourceBundleSet
{
	IResource GetResource(string name, IResourceBundleFilter filter);

	IResourceBundleFilter CreateFilter();

	ReadOnlyCollection<IResource> GetResources(string typeName, IResourceBundleFilter filter);

	ReadOnlyCollection<IBundleSpecifier> GetBundles();

	ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName);

	IDictionary<string, IResourceType> GetResourceTypes();
}
