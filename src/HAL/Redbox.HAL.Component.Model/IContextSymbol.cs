namespace Redbox.HAL.Component.Model;

public interface IContextSymbol
{
	string Key { get; }

	object Value { get; set; }

	bool IsReserved { get; }
}
