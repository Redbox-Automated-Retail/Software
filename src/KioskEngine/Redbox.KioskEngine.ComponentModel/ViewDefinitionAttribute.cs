using System;

namespace Redbox.KioskEngine.ComponentModel;

[AttributeUsage(AttributeTargets.Class)]
public class ViewDefinitionAttribute : Attribute
{
	public string ViewName { get; set; }
}
