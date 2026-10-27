using System;

namespace Redbox.HAL.Configuration;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class XmlConfigurationAttribute : Attribute
{
	public string DefaultValue { get; set; }
}
