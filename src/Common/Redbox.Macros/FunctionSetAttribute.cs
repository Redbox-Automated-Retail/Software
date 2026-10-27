using System;

namespace Redbox.Macros;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class FunctionSetAttribute : Attribute
{
	public string Category { get; set; }

	public string Prefix { get; set; }

	public FunctionSetAttribute(string prefix, string category)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (category == null)
		{
			throw new ArgumentNullException("category");
		}
		if (prefix.Trim().Length == 0)
		{
			throw new ArgumentOutOfRangeException("prefix", prefix, "A zero-length string is not an allowed value.");
		}
		if (category.Trim().Length == 0)
		{
			throw new ArgumentOutOfRangeException("category", category, "A zero-length string is not an allowed value.");
		}
		Prefix = prefix;
		Category = category;
	}
}
