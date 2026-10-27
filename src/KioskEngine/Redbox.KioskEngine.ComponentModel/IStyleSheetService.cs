using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IStyleSheetService
{
	ReadOnlyCollection<IStyleSheet> StyleSheets { get; }

	void Reset();

	IStyleSheet New(string name);

	IStyleSheet GetStyleSheet(string name);
}
