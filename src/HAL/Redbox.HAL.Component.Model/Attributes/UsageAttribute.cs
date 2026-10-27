using System;

namespace Redbox.HAL.Component.Model.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class UsageAttribute : Attribute
{
	public string Template { get; private set; }

	public UsageAttribute(string template)
	{
		Template = template;
	}
}
