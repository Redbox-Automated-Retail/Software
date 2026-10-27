namespace Redbox.KioskEngine.ComponentModel;

public interface IStyleSheetStyle
{
	string Name { get; }

	void RemoveState(string name);

	IStyleSheetState AddState(string name);

	IStyleSheetState GetState(string name);
}
