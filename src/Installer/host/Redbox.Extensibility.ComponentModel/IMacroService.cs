namespace Redbox.Extensibility.ComponentModel;

public interface IMacroService
{
	void SetMacro(string name, string value);

	string GetMacro(string name);

	string ExpandRuntimeMacros(string value);

	string ExpandConstantMacros(string value);
}
