namespace Redbox.Command.Tokenizer;

public enum CommandParserState
{
	Start = 1,
	Command,
	Comment,
	Symbol,
	Key,
	Value,
	Whitespace,
	Error
}
