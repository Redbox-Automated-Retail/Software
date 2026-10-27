using System;

namespace Redbox.REDS.Framework;

public interface IPropertyType
{
	Type Type { get; }

	string Name { get; }

	string Label { get; }

	bool IsFilter { get; }

	bool IsRequired { get; }

	int FilterScore { get; }

	string Category { get; }

	string Description { get; }

	object DefaultValue { get; }

	void SetDefaultValue(object value);

	object GetTypedValue(object value);
}
