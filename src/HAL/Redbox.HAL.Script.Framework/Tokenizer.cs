using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class Tokenizer : Tokenizer<TokenizerState>
{
	private bool m_mnemonicDefined;

	private bool m_keyValuePairOpen;

	private bool m_stringLiteralOpen;

	internal Dictionary<string, bool> ConstTable { get; set; }

	public Tokenizer(int lineNumber, string statement)
		: base(lineNumber, statement)
	{
		ConstTable = new Dictionary<string, bool>();
	}

	public Tokenizer(int lineNumber, string statement, Dictionary<string, bool> table)
		: base(lineNumber, statement)
	{
		ConstTable = table;
	}

	protected override void OnReset()
	{
		base.Tokens.Clear();
		base.CurrentState = TokenizerState.Start;
		m_mnemonicDefined = false;
		m_keyValuePairOpen = false;
		m_stringLiteralOpen = false;
	}

	[StateHandler(State = TokenizerState.Error)]
	internal StateResult ProcessErrorState()
	{
		ResetAccumulator();
		base.Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."), "Correct your source and recompile."));
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.Start)]
	internal StateResult ProcessStartState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		if (GetCurrentToken() == '#' || GetCurrentToken() == ';')
		{
			base.CurrentState = TokenizerState.Comment;
		}
		else
		{
			if (char.IsLetter(GetCurrentToken()) || GetCurrentToken() == '_')
			{
				base.CurrentState = TokenizerState.Symbol;
				return StateResult.Restart;
			}
			if (char.IsPunctuation(GetCurrentToken()))
			{
				base.CurrentState = TokenizerState.Error;
				return StateResult.Restart;
			}
		}
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.Symbol)]
	internal StateResult ProcessSymbolState()
	{
		if (char.IsLetterOrDigit(GetCurrentToken()) || GetCurrentToken() == '_' || GetCurrentToken() == '-')
		{
			AppendToAccumulator();
		}
		else if (GetCurrentToken() == '=')
		{
			m_keyValuePairOpen = true;
			base.CurrentState = TokenizerState.KeyValuePair;
			AppendToAccumulator();
		}
		else
		{
			if (char.IsWhiteSpace(GetCurrentToken()))
			{
				if (!m_mnemonicDefined)
				{
					base.CurrentState = TokenizerState.Mnemonic;
				}
				else
				{
					CloseSymbolState();
				}
				return StateResult.Restart;
			}
			if (GetCurrentToken() == ':')
			{
				base.CurrentState = TokenizerState.Label;
				return StateResult.Restart;
			}
		}
		if (StreamTerminated())
		{
			if (!m_mnemonicDefined)
			{
				DefineMnemonic();
			}
			else
			{
				CloseSymbolState();
			}
		}
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.KeyValuePair)]
	internal StateResult ProcessKeyValuePairState()
	{
		if (GetCurrentToken() == '-' || char.IsDigit(GetCurrentToken()))
		{
			base.CurrentState = TokenizerState.NumberLiteral;
			return StateResult.Restart;
		}
		if (char.IsLetter(GetCurrentToken()) || GetCurrentToken() == '_')
		{
			base.CurrentState = TokenizerState.Symbol;
			return StateResult.Restart;
		}
		if (GetCurrentToken() == '"')
		{
			m_stringLiteralOpen = true;
			base.CurrentState = TokenizerState.StringLiteral;
		}
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.NumberLiteral)]
	internal StateResult ProcessNumericLiteralState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			AddTokenAndReset(TokenType.NumericLiteral, m_keyValuePairOpen);
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		if (char.IsDigit(GetCurrentToken()) || GetCurrentToken() == '.' || GetCurrentToken() == '-')
		{
			AppendToAccumulator();
			if (StreamTerminated())
			{
				AddTokenAndReset(TokenType.NumericLiteral, m_keyValuePairOpen);
				base.CurrentState = TokenizerState.WhitespaceIgnore;
				return StateResult.Continue;
			}
			return StateResult.Continue;
		}
		base.Errors.Add(Error.NewError("T002", FormatError("Numeric literals can only contain digits 0 - 9, an embedded decimal place and a front sign."), "Correct your source and recompile"));
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.StringLiteral)]
	internal StateResult ProcessStringLiteralState()
	{
		if (GetCurrentToken() == '"')
		{
			m_stringLiteralOpen = false;
		}
		else
		{
			if (!m_stringLiteralOpen)
			{
				AddTokenAndReset(TokenType.StringLiteral, m_keyValuePairOpen);
				base.CurrentState = TokenizerState.WhitespaceIgnore;
				if (m_keyValuePairOpen)
				{
					m_keyValuePairOpen = false;
				}
				return StateResult.Restart;
			}
			AppendToAccumulator();
		}
		if (GetCurrentToken() == '"' && StreamTerminated())
		{
			AddTokenAndReset(TokenType.StringLiteral, m_keyValuePairOpen);
			if (m_keyValuePairOpen)
			{
				m_keyValuePairOpen = false;
			}
		}
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.WhitespaceIgnore)]
	internal StateResult ProcessWhitespaceState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			return StateResult.Continue;
		}
		if (m_mnemonicDefined && GetCurrentToken() == '"')
		{
			m_stringLiteralOpen = true;
			base.CurrentState = TokenizerState.StringLiteral;
		}
		else
		{
			if (char.IsLetter(GetCurrentToken()) || GetCurrentToken() == '_')
			{
				base.CurrentState = TokenizerState.Symbol;
				return StateResult.Restart;
			}
			if (m_mnemonicDefined && (GetCurrentToken() == '+' || GetCurrentToken() == '-' || char.IsDigit(GetCurrentToken())))
			{
				base.CurrentState = TokenizerState.NumberLiteral;
				return StateResult.Restart;
			}
			if (m_mnemonicDefined && (GetCurrentToken() == '<' || GetCurrentToken() == '>'))
			{
				AppendToAccumulator();
				base.CurrentState = TokenizerState.RelationalOperator;
			}
			else if (m_mnemonicDefined && GetCurrentToken() == '~')
			{
				AppendToAccumulator();
				base.CurrentState = TokenizerState.PatternOperator;
			}
			else if (m_mnemonicDefined && (GetCurrentToken() == '=' || GetCurrentToken() == '!'))
			{
				AppendToAccumulator();
				base.CurrentState = TokenizerState.EqualityOperator;
			}
			else if (m_mnemonicDefined && GetCurrentToken() == '.')
			{
				AppendToAccumulator();
				base.CurrentState = TokenizerState.RangeOperator;
			}
			else
			{
				if (GetCurrentToken() != ';' && GetCurrentToken() != '#')
				{
					base.Errors.Add(Error.NewError("T003", FormatError("Expected comment or mnemonic."), "Correct your source and recompile."));
					return StateResult.Terminal;
				}
				base.CurrentState = TokenizerState.Comment;
			}
		}
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.Mnemonic)]
	internal StateResult ProcessMnemonicState()
	{
		DefineMnemonic();
		base.CurrentState = TokenizerState.WhitespaceIgnore;
		return StateResult.Restart;
	}

	[StateHandler(State = TokenizerState.Comment)]
	internal StateResult ProcessCommentState()
	{
		AddTokenAndReset(TokenType.Comment, isKeyValuePair: false);
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.Label)]
	internal StateResult ProcessLabelState()
	{
		AddTokenAndReset(TokenType.Label, isKeyValuePair: false);
		base.CurrentState = TokenizerState.WhitespaceIgnore;
		return StateResult.Continue;
	}

	[StateHandler(State = TokenizerState.RelationalOperator)]
	internal StateResult ProcessRelationalOperatorState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			AddTokenAndReset(TokenType.Operator, isKeyValuePair: false);
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		if (GetCurrentToken() == '=' && base.Accumulator.Length < 2)
		{
			AppendToAccumulator();
			return StateResult.Continue;
		}
		base.Errors.Add(Error.NewError("T004", FormatError("Expected a valid relational operator: >, <, >=, <=."), "Correct your source and recompile."));
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.EqualityOperator)]
	internal StateResult ProcessEqualityOperatorState()
	{
		if (GetCurrentToken() == '=' && base.Accumulator.Length < 2)
		{
			AppendToAccumulator();
			return StateResult.Continue;
		}
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			AddTokenAndReset(TokenType.Operator, isKeyValuePair: false);
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		base.Errors.Add(Error.NewError("T005", FormatError("Expected a valid equality operator: ==, !=."), "Correct your source and recompile."));
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.PatternOperator)]
	internal StateResult ProcessPatternOperatorState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			AddTokenAndReset(TokenType.Operator, isKeyValuePair: false);
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		if (GetCurrentToken() == '=' && base.Accumulator.Length < 2)
		{
			AppendToAccumulator();
			return StateResult.Continue;
		}
		base.Errors.Add(Error.NewError("T004", FormatError("Expected a valid pattern operator: ~=."), "Correct your source and recompile."));
		return StateResult.Terminal;
	}

	[StateHandler(State = TokenizerState.RangeOperator)]
	internal StateResult ProcessRangeOperatorState()
	{
		if (GetCurrentToken() == '.')
		{
			AppendToAccumulator();
			return StateResult.Continue;
		}
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			base.CurrentState = TokenizerState.WhitespaceIgnore;
			return StateResult.Restart;
		}
		base.Errors.Add(Error.NewError("T005", FormatError("Expected a range operator: .."), "Correct your source and recompile."));
		return StateResult.Terminal;
	}

	private bool StreamTerminated()
	{
		if (PeekNextToken().HasValue)
		{
			return $"{GetCurrentToken()}{PeekNextToken()}" == Environment.NewLine;
		}
		return true;
	}

	private void CloseSymbolState()
	{
		string value = base.Tokens[0].Value;
		string accumulatedToken = GetAccumulatedToken();
		if (ConstTable.ContainsKey(accumulatedToken) || Instruction.MnemonicImpliesConstSymbol(value))
		{
			ConstTable[accumulatedToken] = true;
			AddTokenAndReset(TokenType.ConstSymbol, m_keyValuePairOpen);
		}
		else
		{
			AddTokenAndReset(TokenType.Symbol, m_keyValuePairOpen);
		}
		base.CurrentState = TokenizerState.WhitespaceIgnore;
		if (m_keyValuePairOpen)
		{
			m_keyValuePairOpen = false;
		}
	}

	private void DefineMnemonic()
	{
		AddTokenAndReset(TokenType.Mnemonic, isKeyValuePair: false);
		m_mnemonicDefined = true;
	}
}
