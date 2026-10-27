using System;

namespace Redbox.Tokenizer.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class StateHandlerAttribute : Attribute
{
	public object State { get; set; }
}
