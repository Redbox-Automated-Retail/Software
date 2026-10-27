using System;

namespace Redbox.Macros;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class FunctionAttribute : Attribute
{
	public string Name { get; set; }

	public FunctionAttribute(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Trim().Length == 0)
		{
			throw new ArgumentOutOfRangeException("name", name, "A zero-length string is not an allowed value.");
		}
		Name = name;
	}
}
