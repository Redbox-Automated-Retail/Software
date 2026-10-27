using System;
using System.Collections.Generic;

namespace Redbox.REDS.Framework;

public interface IResourceBundle : IDisposable
{
	IDictionary<string, IResourceType> ResourceTypes { get; }

	IResourceBundleStorage Storage { get; }

	ErrorList Activate(IResourceBundleFilter filter, out IManifestInfo manifestInfo);

	IResource GetResource(string name, IResourceBundleFilter filter);

	IResource GetManifest(IResourceBundleFilter filter, out IManifestInfo manifestInfo);

	ErrorList StoreResource(string name, string data);

	List<IResource> GetResources(string typeName, IResourceBundleFilter filter);

	List<IResource> GetAllResources();

	IResourceBundleFilter CreateFilter();

	ICollection<string> GetLoadedResources();

	ICollection<IResource> GetResources(string name);

	List<IResource> GetAllResources(string typeName);

	IResourceBundleStorageEntry GetEntry(string name);
}
