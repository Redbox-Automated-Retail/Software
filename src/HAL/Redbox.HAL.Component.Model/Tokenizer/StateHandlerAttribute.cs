using System;

namespace Redbox.HAL.Component.Model.Tokenizer;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class StateHandlerAttribute : Attribute
{
	public object State { get; set; }
}
