using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework;

public class ResourceType : IResourceType
{
	private List<IAspectType> m_aspects;

	private List<IResourceType> m_subTypes;

	private List<IPropertyType> m_properties;

	public string Name { get; internal set; }

	public IResourceType SuperType { get; internal set; }

	public ReadOnlyCollection<IResourceType> SubTypes => InnerSubTypes.AsReadOnly();

	public ReadOnlyCollection<IAspectType> AspectTypes => InnerAspects.AsReadOnly();

	public ReadOnlyCollection<IPropertyType> Properties => InnerProperties.AsReadOnly();

	internal List<IAspectType> InnerAspects
	{
		get
		{
			if (m_aspects == null)
			{
				m_aspects = new List<IAspectType>();
			}
			return m_aspects;
		}
	}

	internal List<IResourceType> InnerSubTypes
	{
		get
		{
			if (m_subTypes == null)
			{
				m_subTypes = new List<IResourceType>();
			}
			return m_subTypes;
		}
	}

	internal List<IPropertyType> InnerProperties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new List<IPropertyType>();
			}
			return m_properties;
		}
	}

	public IResourceType NewType(string name, Type runtimeType)
	{
		ResourceType resourceType = (ResourceType)Activator.CreateInstance(runtimeType ?? typeof(ResourceType));
		resourceType.SuperType = this;
		resourceType.Name = name;
		InnerSubTypes.Add(resourceType);
		return resourceType;
	}

	public IAspectType AddAspectType(string name, Type aspectType)
	{
		AspectType aspectType2 = (AspectType)Activator.CreateInstance(aspectType ?? typeof(AspectType));
		aspectType2.Name = name;
		InnerAspects.Add(aspectType2);
		return aspectType2;
	}

	public IPropertyType AddPropertyType(string name, Type type, string description, bool isRequired, bool isFilter, int? filterScore, string label, string category)
	{
		PropertyType propertyType = new PropertyType();
		propertyType.Name = name;
		propertyType.Type = type;
		propertyType.Label = label;
		propertyType.IsFilter = isFilter;
		propertyType.Category = category;
		propertyType.IsRequired = isRequired;
		propertyType.Description = description;
		propertyType.FilterScore = filterScore ?? 1;
		PropertyType propertyType2 = propertyType;
		InnerProperties.Add(propertyType2);
		return propertyType2;
	}

	public void RemoveAspectType(string name)
	{
		IAspectType aspectType = GetAspectType(name);
		if (aspectType != null)
		{
			InnerAspects.Remove(aspectType);
		}
	}

	public void RemovePropertyType(string name)
	{
		IPropertyType propertyType = GetPropertyType(name);
		if (propertyType != null)
		{
			InnerProperties.Remove(propertyType);
		}
	}

	public IResource NewResourceInstance(string name)
	{
		Resource resource = new Resource();
		resource.Type = this;
		Resource resource2 = resource;
		resource2.Properties["name"] = name;
		resource2.Properties["created_on"] = DateTime.UtcNow;
		resource2.Properties["modified_on"] = DateTime.UtcNow;
		return resource2;
	}

	public IAspectType GetAspectType(string name)
	{
		for (ResourceType resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
		{
			IAspectType aspectType = resourceType.InnerAspects.Find((IAspectType each) => each.Name == name);
			if (aspectType != null)
			{
				return aspectType;
			}
		}
		return null;
	}

	public IPropertyType GetPropertyType(string name)
	{
		for (ResourceType resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
		{
			IPropertyType propertyType = resourceType.InnerProperties.Find((IPropertyType each) => each.Name == name);
			if (propertyType != null)
			{
				return propertyType;
			}
		}
		return null;
	}

	public ReadOnlyCollection<IPropertyType> GetFilterProperties()
	{
		List<IPropertyType> list = new List<IPropertyType>();
		for (ResourceType resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
		{
			list.AddRange(resourceType.InnerProperties.FindAll((IPropertyType each) => each.IsFilter));
		}
		return list.AsReadOnly();
	}
}
