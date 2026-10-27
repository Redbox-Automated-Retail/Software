namespace Redbox.HAL.Script.Framework;

public enum TokenizerState
{
	Start = 1,
	Label,
	Error,
	Symbol,
	Comment,
	Mnemonic,
	Operator,
	KeyValuePair,
	NumberLiteral,
	StringLiteral,
	RangeOperator,
	WhitespaceIgnore,
	EqualityOperator,
	RelationalOperator,
	PatternOperator
}
