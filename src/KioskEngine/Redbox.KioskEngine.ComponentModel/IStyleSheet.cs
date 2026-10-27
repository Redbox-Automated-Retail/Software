namespace Redbox.KioskEngine.ComponentModel;

public interface IStyleSheet
{
	string Name { get; }

	void RemoveStyle(string name);

	IStyleSheetStyle GetStyle(string name);

	IStyleSheetStyle AddStyle(string name);
}
