using System;

namespace Redbox.Installer.Framework;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ElementPropertyAttribute : Attribute
{
	public string Name { get; set; }

	public bool IsRequired { get; set; }

	public bool UseNodeText { get; set; }
}
