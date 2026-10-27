using System;

namespace Redbox.HAL.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class StateHandlerAttribute : Attribute
{
	public object State { get; set; }
}
