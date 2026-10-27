using System;

namespace Redbox.GetOpts;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class OptionAttribute : Attribute
{
	public object Min { get; set; }

	public object Max { get; set; }

	public bool Required { get; set; }

	public string LongName { get; set; }

	public string ShortName { get; set; }
}
