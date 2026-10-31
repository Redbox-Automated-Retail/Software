using System;

namespace Redbox.Installer.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ElementAttribute : Attribute
{
	public string XPath { get; set; }
}
