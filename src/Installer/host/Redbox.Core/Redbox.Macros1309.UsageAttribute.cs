using System;

namespace Redbox.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
internal class Redbox_002EMacros1309_002EUsageAttribute : Attribute
{
	public string Template { get; private set; }

	public Redbox_002EMacros1309_002EUsageAttribute(string template)
	{
		Template = template;
	}
}
