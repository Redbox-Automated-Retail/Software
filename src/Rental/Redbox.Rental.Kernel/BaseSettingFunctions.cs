using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Rental.Kernel;

public class BaseSettingFunctions
{
	public object? ConvertValueToPropertyType(PropertyInfo propertyInfo, object? value)
	{
		object? result = value;
		if (value != null)
		{
			Type type = propertyInfo.PropertyType;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				type = type.GetGenericArguments()[0];
			}
			if (type == typeof(int))
			{
				result = Convert.ToInt32(value);
			}
			else if (type == typeof(decimal))
			{
				result = Convert.ToDecimal(value);
			}
			else if (type.IsEnum)
			{
				result = Enum.Parse(type, value.ToString()!);
			}
		}
		return result;
	}

	public void GetProperty(object? obj, string name, out object? resultObject, out PropertyInfo? resultPropertyInfo)
	{
		resultObject = null;
		resultPropertyInfo = null;
		List<string> list = name.Split('.').ToList();
		for (int i = 0; i < list.Count; i++)
		{
			string propertyName = list[i];
			if (obj == null)
			{
				continue;
			}
			PropertyInfo? property = obj.GetType().GetProperty(propertyName);
			if (property != null)
			{
				if (i != list.Count - 1)
				{
					obj = property.GetValue(obj, null);
					continue;
				}
				resultPropertyInfo = property;
				resultObject = obj;
			}
		}
	}

	public object? GetPropertyValue(object? obj, string name)
	{
		if (obj != null)
		{
			bool found = false;
			foreach (string propertyName in name.Split('.'))
			{
				if (obj == null)
				{
					break;
				}
				found = false;
				PropertyInfo? property = obj.GetType().GetProperty(propertyName);
				if (property == null)
				{
					break;
				}
				obj = property.GetValue(obj, null);
				found = true;
			}
			if (!found)
			{
				throw new ArgumentException($"Error in GetPropertyValue.  Unable to locate property {name}");
			}
			return obj;
		}
		return null;
	}
}
