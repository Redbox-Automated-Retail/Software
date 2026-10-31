namespace Redbox.Macros;

public enum FunctionTokenizerState
{
	Start,
	Function,
	Parameter,
	Whitespace,
	Error
}
