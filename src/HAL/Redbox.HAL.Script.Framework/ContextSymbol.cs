using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class ContextSymbol : IContextSymbol
{
	public string Key { get; private set; }

	public object Value { get; set; }

	public bool IsReserved => Key.StartsWith("__");

	internal ContextSymbol(string s)
	{
		Key = s;
	}
}
