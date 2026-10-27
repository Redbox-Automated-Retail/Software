using System;

namespace Redbox.HAL.Component.Model.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class XmlConfigurationAttribute : Attribute
{
	public string DefaultValue { get; set; }
}
