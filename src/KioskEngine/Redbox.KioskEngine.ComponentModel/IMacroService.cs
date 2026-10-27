using System.Collections;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IMacroService
{
	string this[string name] { get; set; }

	ICollection Keys { get; }

	ICollection Values { get; }

	void Clear(ICollection<string> exclusions);

	string ExpandProperties(string input);
}
