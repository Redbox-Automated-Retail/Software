using System;

namespace Redbox.UpdateManager.Kernel;

[AttributeUsage(AttributeTargets.Method)]
public class KernelFunctionAttribute : Attribute
{
	public string Name { get; set; }
}
