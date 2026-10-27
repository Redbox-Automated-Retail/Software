namespace Redbox.KioskEngine.ComponentModel;

public interface IStyleSheetState
{
	string Name { get; }

	object this[string key] { get; }
}
