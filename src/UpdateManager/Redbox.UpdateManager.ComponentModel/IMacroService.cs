namespace Redbox.UpdateManager.ComponentModel;

public interface IMacroService
{
	string this[string name] { get; set; }

	string ExpandProperties(string input);
}
