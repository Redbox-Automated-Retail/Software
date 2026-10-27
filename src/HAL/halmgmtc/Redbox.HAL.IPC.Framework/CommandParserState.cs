namespace Redbox.HAL.IPC.Framework;

public enum CommandParserState
{
	Start = 1,
	Command,
	Comment,
	Symbol,
	Key,
	Value,
	Whitespace,
	Error,
	StringLiteral
}
