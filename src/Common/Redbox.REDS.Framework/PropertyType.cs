using System;
using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.REDS.Framework;

public class PropertyType : IPropertyType
{
	public Type Type { get; internal set; }

	public string Name { get; internal set; }

	public string Label { get; internal set; }

	public bool IsFilter { get; internal set; }

	public bool IsRequired { get; internal set; }

	public int FilterScore { get; internal set; }

	public string Category { get; internal set; }

	public string Description { get; internal set; }

	public object DefaultValue { get; internal set; }

	public void SetDefaultValue(object value)
	{
		if (value == null)
		{
			DefaultValue = null;
		}
		else
		{
			DefaultValue = GetTypedValue(value);
		}
	}

	public object GetTypedValue(object value)
	{
		if (value is List<object>)
		{
			return value;
		}
		if (!(Type == null))
		{
			return ConversionHelper.ChangeType(value, Type);
		}
		return value;
	}

	protected internal PropertyType()
	{
		FilterScore = 1;
	}
}
