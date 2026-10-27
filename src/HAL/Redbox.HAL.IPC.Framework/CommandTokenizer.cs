using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class CommandTokenizer : Tokenizer<CommandParserState>
{
	private bool m_isEscaped;

	private bool m_inQuotedValue;

	private bool m_expectingValue;

	private char m_expectedClosingQuote;

	private bool m_treatAsLiteral;

	public CommandTokenizer(int lineNumber, string statement)
		: base(lineNumber, statement)
	{
	}

	protected override void OnReset()
	{
		base.CurrentState = CommandParserState.Start;
	}

	[StateHandler(State = CommandParserState.Error)]
	internal StateResult ProcessErrorState()
	{
		ResetAccumulator();
		base.Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."), "Correct your command syntax and resubmit."));
		return StateResult.Terminal;
	}

	[StateHandler(State = CommandParserState.Start)]
	internal StateResult ProcessStartState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			base.CurrentState = CommandParserState.Whitespace;
			return StateResult.Restart;
		}
		if (GetCurrentToken() == '-' && PeekNextToken() == '-')
		{
			base.CurrentState = CommandParserState.Comment;
			return StateResult.Restart;
		}
		if (char.IsLetter(GetCurrentToken()))
		{
			base.CurrentState = CommandParserState.Command;
			return StateResult.Restart;
		}
		if (char.IsPunctuation(GetCurrentToken()))
		{
			base.CurrentState = CommandParserState.Error;
			return StateResult.Restart;
		}
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.Comment)]
	internal StateResult ProcessCommentState()
	{
		AddTokenAndReset(TokenType.Comment, isKeyValuePair: false);
		return StateResult.Terminal;
	}

	[StateHandler(State = CommandParserState.Command)]
	internal StateResult ProcessCommandState()
	{
		char currentToken = GetCurrentToken();
		if (char.IsLetter(currentToken) || currentToken == '-')
		{
			AppendToAccumulator();
		}
		if (char.IsWhiteSpace(currentToken))
		{
			AddTokenAndReset(TokenType.Mnemonic, isKeyValuePair: false);
			base.CurrentState = CommandParserState.Whitespace;
			return StateResult.Restart;
		}
		if (!PeekNextToken().HasValue)
		{
			AddTokenAndReset(TokenType.Mnemonic, isKeyValuePair: false);
			return StateResult.Continue;
		}
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.Symbol)]
	internal StateResult ProcessSymbolState()
	{
		char currentToken = GetCurrentToken();
		if (currentToken == ':')
		{
			AppendToAccumulator();
			base.CurrentState = CommandParserState.Key;
			return StateResult.Restart;
		}
		if (char.IsWhiteSpace(currentToken))
		{
			AddTokenAndReset(TokenType.Symbol, isKeyValuePair: false);
			base.CurrentState = CommandParserState.Whitespace;
			return StateResult.Restart;
		}
		if (!PeekNextToken().HasValue)
		{
			AppendToAccumulator();
			AddTokenAndReset(TokenType.Symbol, isKeyValuePair: false);
			base.CurrentState = CommandParserState.Start;
			return StateResult.Continue;
		}
		AppendToAccumulator();
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.Key)]
	internal StateResult ProcessKeyState()
	{
		m_expectingValue = true;
		base.CurrentState = CommandParserState.Whitespace;
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.StringLiteral)]
	internal StateResult ProcessStringLiteralState()
	{
		char currentToken = GetCurrentToken();
		if (m_inQuotedValue && currentToken == m_expectedClosingQuote)
		{
			m_inQuotedValue = false;
			m_expectingValue = false;
			m_treatAsLiteral = false;
			AddTokenAndReset(TokenType.StringLiteral, ":");
			base.CurrentState = CommandParserState.Whitespace;
			return StateResult.Continue;
		}
		AppendToAccumulator();
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.Value)]
	internal StateResult ProcessValueState()
	{
		char currentToken = GetCurrentToken();
		if (currentToken == '\\' && m_inQuotedValue && !m_isEscaped)
		{
			char? c = PeekNextToken();
			if (c != '\'' && c != '"' && c != '\\')
			{
				base.CurrentState = CommandParserState.Error;
				return StateResult.Restart;
			}
			m_isEscaped = true;
		}
		else if ((currentToken == m_expectedClosingQuote && !m_isEscaped) || (char.IsWhiteSpace(currentToken) && !m_inQuotedValue))
		{
			m_inQuotedValue = false;
			m_expectingValue = false;
			AddTokenAndReset(TokenType.StringLiteral, ":");
			base.CurrentState = CommandParserState.Whitespace;
		}
		else if (!PeekNextToken().HasValue)
		{
			if (m_inQuotedValue)
			{
				base.CurrentState = CommandParserState.Error;
				return StateResult.Restart;
			}
			m_expectingValue = false;
			AppendToAccumulator();
			AddTokenAndReset(TokenType.StringLiteral, ":");
			base.CurrentState = CommandParserState.Whitespace;
		}
		else
		{
			AppendToAccumulator();
			if (m_isEscaped)
			{
				m_isEscaped = false;
			}
		}
		return StateResult.Continue;
	}

	[StateHandler(State = CommandParserState.Whitespace)]
	internal StateResult ProcessWhitespaceState()
	{
		char currentToken = GetCurrentToken();
		if (char.IsLetter(currentToken) || char.IsDigit(currentToken))
		{
			base.CurrentState = ((!m_expectingValue) ? CommandParserState.Symbol : (m_treatAsLiteral ? CommandParserState.StringLiteral : CommandParserState.Value));
			return StateResult.Restart;
		}
		switch (currentToken)
		{
		case '"':
		case '\'':
			m_expectedClosingQuote = currentToken;
			base.CurrentState = (m_treatAsLiteral ? CommandParserState.StringLiteral : CommandParserState.Value);
			m_inQuotedValue = true;
			break;
		case '@':
			m_treatAsLiteral = true;
			break;
		default:
			if (m_inQuotedValue)
			{
				AppendToAccumulator();
			}
			break;
		}
		return StateResult.Continue;
	}
}
