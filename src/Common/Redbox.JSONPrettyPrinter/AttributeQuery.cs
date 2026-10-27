using System;
using System.Reflection;

namespace Redbox.JSONPrettyPrinter;

internal static class AttributeQuery<T> where T : Attribute
{
	public static T Get<P>(P provider) where P : ICustomAttributeProvider
	{
		return Get(provider, inherit: false);
	}

	public static T Get<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		return Find(provider, inherit) ?? throw new ObjectNotFoundException($"The attribute {typeof(T).FullName} was not found.");
	}

	public static T Find<P>(P provider) where P : ICustomAttributeProvider
	{
		return Find(provider, inherit: false);
	}

	public static T Find<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		T[] array = FindAll(provider, inherit);
		if (array.Length == 0)
		{
			return null;
		}
		return array[0];
	}

	public static T FindAll<P>(P provider) where P : ICustomAttributeProvider
	{
		return Find(provider, inherit: false);
	}

	public static T[] FindAll<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		return (T[])provider.GetCustomAttributes(typeof(T), inherit);
	}
}
