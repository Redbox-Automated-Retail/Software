using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework;

public class ResourceBundleSet : IResourceBundleSet
{
	private readonly string m_name;

	private BundleCollection? m_bundles;

	private readonly IResourceBundle m_applicationBundle;

	private MultiDictionary<string, IResource>? m_resources;

	internal BundleCollection Bundles
	{
		get
		{
			if (m_bundles == null)
			{
				m_bundles = new BundleCollection();
			}
			return m_bundles;
		}
	}

	internal MultiDictionary<string, IResource> Resources
	{
		get
		{
			if (m_resources == null)
			{
				m_resources = new MultiDictionary<string, IResource>(allowDuplicateValues: true);
			}
			return m_resources;
		}
	}

	public ResourceBundleSet(string name, IResourceBundle applicationBundle, IEnumerable<IResourceBundle> otherBundles)
	{
		m_name = name;
		m_applicationBundle = applicationBundle;
		List<IResourceBundle> list = new List<IResourceBundle>();
		list.Add(applicationBundle);
		list.AddRange(otherBundles);
		foreach (IResourceBundle item in list)
		{
			Bundles.AddBundleVersion(item);
			foreach (KeyValuePair<string, ICollection<IResource>> resource in ((ResourceBundle)item).Resources)
			{
				Resources.Add(resource);
			}
		}
	}

	public ReadOnlyCollection<IResource> GetResources(string typeName, IResourceBundleFilter? filter)
	{
		List<IResource> resources = new List<IResource>();
		foreach (string bundle in Bundles.GetBundles())
		{
			foreach (IBundleSpecifier bundleVersion in Bundles.GetBundleVersions(bundle))
			{
				resources.AddRange(bundleVersion.Instance!.GetResources(typeName, filter)); // added with their bundle by AddBundleVersion
			}
		}
		return resources.AsReadOnly();
	}

	public ReadOnlyCollection<IBundleSpecifier> GetBundles()
	{
		List<IBundleSpecifier> bundleSpecifiers = new List<IBundleSpecifier>();
		foreach (IBundleSpecifier allBundleVersion in Bundles.GetAllBundleVersions())
		{
			bundleSpecifiers.Add(allBundleVersion);
		}
		return bundleSpecifiers.AsReadOnly();
	}

	public ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName)
	{
		List<IResource> resources = new List<IResource>();
		foreach (string bundle in Bundles.GetBundles())
		{
			foreach (IBundleSpecifier bundleVersion in Bundles.GetBundleVersions(bundle))
			{
				resources.AddRange(bundleVersion.Instance!.GetAllResources(typeName));
			}
		}
		return resources.AsReadOnly();
	}

	public IResource? GetResource(string name, IResourceBundleFilter? filter)
	{
		return ResourceLookup.GetResource2(Resources, name, filter);
	}

	public IDictionary<string, IResourceType> GetResourceTypes()
	{
		Dictionary<string, IResourceType> dictionary = new Dictionary<string, IResourceType>();
		foreach (string bundle in Bundles.GetBundles())
		{
			foreach (IBundleSpecifier bundleVersion in Bundles.GetBundleVersions(bundle))
			{
				foreach (KeyValuePair<string, IResourceType> resourceType in bundleVersion.Instance!.ResourceTypes)
				{
					dictionary[resourceType.Key] = resourceType.Value;
				}
			}
		}
		return dictionary;
	}

	public IResourceBundleFilter? CreateFilter()
	{
		if (m_applicationBundle != null)
		{
			return m_applicationBundle.CreateFilter();
		}
		return null;
	}

	public override string ToString()
	{
		return m_name;
	}
}
