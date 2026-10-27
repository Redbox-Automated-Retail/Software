using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Xml;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework;

public class ResourceBundle : IResourceBundle, IDisposable
{
	private MultiDictionary<string, IResource> m_resources;

	private IDictionary<string, IResourceType> m_resourceTypes;

	public IDictionary<string, IResourceType> ResourceTypes
	{
		get
		{
			if (m_resourceTypes == null)
			{
				m_resourceTypes = new Dictionary<string, IResourceType>();
			}
			return m_resourceTypes;
		}
	}

	public IResourceBundleStorage Storage { get; internal set; }

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

	public ResourceBundle()
	{
	}

	public ResourceBundle(string fileName, ResourceBundleStorageType storageType)
	{
		switch (storageType)
		{
		case ResourceBundleStorageType.Zipped:
			Storage = new ZippedResourceBundleStorage();
			break;
		case ResourceBundleStorageType.FileSystem:
			Storage = new FileSystemResourceBundleStorage();
			break;
		}
		Storage.Open(fileName);
	}

	public static ResourceBundle Create(string fileName, string trimPrefix, ResourceBundleStorageType storageType)
	{
		try
		{
			ResourceBundle resourceBundle = new ResourceBundle();
			resourceBundle.Storage = new ZippedResourceBundleStorage();
			ResourceBundle resourceBundle2 = resourceBundle;
			resourceBundle2.Storage.Create(fileName, trimPrefix);
			return resourceBundle2;
		}
		catch
		{
			return null;
		}
	}

	public void Dispose()
	{
		Storage.Dispose();
	}

	public ErrorList Activate(IResourceBundleFilter filter, out IManifestInfo manifestInfo)
	{
		ErrorList errorList = new ErrorList();
		IResource manifest = GetManifest(filter, out manifestInfo);
		if (manifest == null)
		{
			errorList.Add(Error.NewError("E003", $"Resource bundle '{Storage.BundlePath}' does not contain: manifest.resource.", "The Redbox Engine for Distributed Stores requires one valid bundle to execute."));
			return errorList;
		}
		if (manifestInfo.BundleType == BundleType.Application)
		{
			if (string.IsNullOrEmpty(manifestInfo.StartupScript))
			{
				errorList.Add(Error.NewError("E004", $"Application resource bundle '{Storage.BundlePath}' must specify a valid startup_script in the manifest.resource.", "The Redbox Engine for Distributed Stores requires one valid startup script to execute."));
				return errorList;
			}
			IResource resource = GetResource(manifestInfo.StartupScript, filter);
			if (resource == null)
			{
				errorList.Add(Error.NewError("E004", $"Resource bundle '{Storage.BundlePath}' does not contain: {manifestInfo.StartupScript}.resource.", "The resource specified in the startup_script property does not exist within the bundle.  Specify a valid script resource."));
			}
			return errorList;
		}
		if (manifestInfo.BundleType == BundleType.HTMLApplication)
		{
			if (string.IsNullOrEmpty(manifestInfo.ControlAssembly))
			{
				errorList.Add(Error.NewError("E005", $"HTML Application resource bundle '{Storage.BundlePath}' must specify a valid control_assembly in the manifest.resource.", "The Redbox Engine for Distributed Stores requires a controller assembly to execute."));
				return errorList;
			}
			IResource resource2 = GetResource(manifestInfo.ControlAssembly, filter);
			if (resource2 == null)
			{
				errorList.Add(Error.NewError("E005", $"Resource bundle '{Storage.BundlePath}' does not contain: {manifestInfo.ControlAssembly}.resource.", "The resource specified in the control_assembly property does not exist within the bundle.  Specify a valid control assembly resource."));
			}
			return errorList;
		}
		return errorList;
	}

	public void AddEntry(string path)
	{
		Storage.AddEntry(path, null);
	}

	public void AddEntry(string path, string trimPrefix)
	{
		Storage.AddEntry(path, trimPrefix);
	}

	public void BeginUpdate()
	{
		Storage.BeginUpdate();
	}

	public void CommitUpdate()
	{
		Storage.CommitUpdate();
	}

	public IResourceBundleStorageEntry GetEntry(string path)
	{
		return Storage.GetEntry(path);
	}

	public ErrorList LoadResources()
	{
		ErrorList errorList = new ErrorList();
		InitializeTypes();
		IResourceBundleStorageEntry entry = Storage.GetEntry("manifest.resource");
		if (entry == null)
		{
			throw new MissingManifestResourceException("The manifest.resource is required.");
		}
		errorList.AddRange(ParseResource(entry));
		foreach (IResourceBundleStorageEntry item in Storage.GetResourceManifest())
		{
			if (item.Name.EndsWith(".resource", StringComparison.InvariantCulture) && string.Compare("manifest.resource", item.Name, ignoreCase: true) != 0)
			{
				errorList.AddRange(ParseResource(item));
			}
		}
		return errorList;
	}

	public IResource GetManifest(IResourceBundleFilter filter, out IManifestInfo manifestInfo)
	{
		manifestInfo = null;
		IResource resource = GetResource("manifest", filter);
		if (resource == null)
		{
			return null;
		}
		manifestInfo = new ManifestInfo(resource);
		return resource;
	}

	public IResource GetResource(string name, IResourceBundleFilter filter)
	{
		return ResourceLookup.GetResource2(Resources, name, filter);
	}

	public ICollection<IResource> GetResources(string name)
	{
		return Resources[name];
	}

	public List<IResource> GetResources(string typeName, IResourceBundleFilter filter)
	{
		List<IResource> list = new List<IResource>();
		if (string.IsNullOrEmpty(typeName) || !ResourceTypes.ContainsKey(typeName))
		{
			return list;
		}
		IResourceType resourceType = ResourceTypes[typeName];
		foreach (string key in Resources.Keys)
		{
			IResource resource = GetResource(key, filter);
			if (resource != null && resource.Type == resourceType)
			{
				list.Add(resource);
			}
		}
		return list;
	}

	public List<IResource> GetAllResources()
	{
		List<IResource> list = new List<IResource>();
		foreach (string key in Resources.Keys)
		{
			list.AddRange(Resources[key]);
		}
		return list;
	}

	public List<IResource> GetAllResources(string typeName)
	{
		List<IResource> list = new List<IResource>();
		if (string.IsNullOrEmpty(typeName) || !ResourceTypes.ContainsKey(typeName))
		{
			return list;
		}
		IResourceType resourceType = ResourceTypes[typeName];
		foreach (string key in Resources.Keys)
		{
			foreach (IResource item in Resources[key])
			{
				if (item.Type == resourceType)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public ErrorList StoreResource(string name, string data)
	{
		ErrorList errorList = new ErrorList();
		if (!Storage.SupportsSave)
		{
			errorList.Add(Error.NewError("B003", "The current bundle storage provider does not support saving.", "Access the bundle using a storage provider that supports saving."));
			return errorList;
		}
		return errorList;
	}

	public IResourceBundleFilter CreateFilter()
	{
		return new ResourceBundleFilter();
	}

	public ICollection<string> GetLoadedResources()
	{
		return Resources.Keys;
	}

	internal ErrorList ParseResource(IResourceBundleStorageEntry storageEntry)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			using MemoryStream inStream = new MemoryStream(Storage.GetEntryContent(storageEntry));
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(inStream);
			errorList.AddRange(Resource.FromXml(this, xmlDocument, out var resource));
			if (!errorList.ContainsError())
			{
				((Resource)resource).StorageEntry = storageEntry;
				Resources.Add(resource.Name, resource);
			}
			return errorList;
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("R999", "An unhandled exception was raised in ResourceBundle.ParseResource.", e));
			return errorList;
		}
	}

	private void InitializeTypes()
	{
		ResourceType resourceType = new ResourceType();
		resourceType.Name = "root";
		ResourceType resourceType2 = resourceType;
		resourceType2.InnerAspects.Add(new AspectType
		{
			Name = "content"
		});
		ResourceTypes["root"] = resourceType2;
		ResourceType resourceType3 = new ResourceType();
		resourceType3.Name = "manifest";
		ResourceType resourceType4 = resourceType3;
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "requires",
			Type = typeof(string)
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "public_key",
			Type = typeof(string)
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "startup_script",
			Type = typeof(string),
			IsRequired = false
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "name",
			Type = typeof(string),
			IsRequired = true
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "product_name",
			Type = typeof(string),
			IsRequired = true
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "product_version",
			Type = typeof(string),
			IsRequired = true
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "bundle_type",
			Type = typeof(BundleType?),
			DefaultValue = BundleType.Application
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "test_control",
			Type = typeof(string),
			IsRequired = false
		});
		resourceType4.InnerProperties.Add(new PropertyType
		{
			Name = "control_assembly",
			Type = typeof(string),
			IsRequired = false
		});
		resourceType4.InnerAspects.Add(new ManifestAspectType
		{
			Name = "schema"
		});
		ResourceTypes["manifest"] = resourceType4;
		Resources.Clear();
	}
}
