using Redbox.Tokenizer.Framework;

namespace Redbox.Macros;

internal class ScriptFunctionTokenizer : SimpleTokenizer<FunctionTokenizerState>
{
	private bool m_inQuotedValue;

	private bool m_haveParameter;

	public ScriptFunctionTokenizer(int lineNumber, string statement)
		: base(lineNumber, statement)
	{
	}

	protected override void OnReset()
	{
		base.CurrentState = FunctionTokenizerState.Start;
	}

	[StateHandler(State = FunctionTokenizerState.Error)]
	internal StateResult ProcessErrorState()
	{
		ResetAccumulator();
		base.Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."), "Correct your command syntax and resubmit."));
		return StateResult.Terminal;
	}

	[StateHandler(State = FunctionTokenizerState.Start)]
	internal StateResult ProcessStartState()
	{
		if (char.IsWhiteSpace(GetCurrentToken()))
		{
			base.CurrentState = FunctionTokenizerState.Whitespace;
			return StateResult.Restart;
		}
		if (char.IsLetter(GetCurrentToken()))
		{
			base.CurrentState = FunctionTokenizerState.Function;
			return StateResult.Restart;
		}
		if (char.IsPunctuation(GetCurrentToken()))
		{
			base.CurrentState = FunctionTokenizerState.Error;
			return StateResult.Restart;
		}
		return StateResult.Continue;
	}

	[StateHandler(State = FunctionTokenizerState.Function)]
	internal StateResult ProcessFunctionState()
	{
		char currentToken = GetCurrentToken();
		if (!char.IsLetter(currentToken))
		{
			switch (currentToken)
			{
			case '-':
			case ':':
			case '_':
				break;
			case '(':
				AddTokenAndReset(SimpleTokenType.Mnemonic, isKeyValuePair: false);
				base.CurrentState = FunctionTokenizerState.Whitespace;
				return StateResult.Restart;
			default:
				base.CurrentState = FunctionTokenizerState.Error;
				return StateResult.Restart;
			}
		}
		AppendToAccumulator();
		if (!((int?)PeekNextToken()).HasValue)
		{
			base.CurrentState = FunctionTokenizerState.Error;
			return StateResult.Restart;
		}
		return StateResult.Continue;
	}

	[StateHandler(State = FunctionTokenizerState.Parameter)]
	internal StateResult ProcessParameterState()
	{
		char currentToken = GetCurrentToken();
		if (currentToken == ',')
		{
			m_haveParameter = false;
			AddTokenAndReset(SimpleTokenType.Symbol, isKeyValuePair: false);
			base.CurrentState = FunctionTokenizerState.Whitespace;
			return StateResult.Restart;
		}
		if (char.IsWhiteSpace(currentToken) && !m_inQuotedValue)
		{
			base.CurrentState = FunctionTokenizerState.Whitespace;
			return StateResult.Restart;
		}
		if (currentToken == '\\' && m_inQuotedValue)
		{
			MoveToNextToken();
			AppendToAccumulator();
		}
		else if (currentToken == '\'')
		{
			m_inQuotedValue = !m_inQuotedValue;
		}
		else
		{
			m_haveParameter = true;
			AppendToAccumulator();
		}
		return StateResult.Continue;
	}

	[StateHandler(State = FunctionTokenizerState.Whitespace)]
	internal StateResult ProcessWhitespaceState()
	{
		char currentToken = GetCurrentToken();
		if ((char.IsLetter(currentToken) || char.IsDigit(currentToken) || currentToken == ':') && base.Tokens.GetMnemonic() == null)
		{
			base.CurrentState = FunctionTokenizerState.Function;
			return StateResult.Restart;
		}
		if (char.IsLetter(currentToken) || char.IsDigit(currentToken) || currentToken == '$' || currentToken == '{' || currentToken == '}' || currentToken == '_')
		{
			base.CurrentState = FunctionTokenizerState.Parameter;
			return StateResult.Restart;
		}
		if (currentToken == '\\' && m_inQuotedValue)
		{
			MoveToNextToken();
			AppendToAccumulator();
		}
		else if (currentToken == '\'' && !m_inQuotedValue)
		{
			if (!m_inQuotedValue)
			{
				base.CurrentState = FunctionTokenizerState.Parameter;
				m_inQuotedValue = true;
			}
			else
			{
				m_inQuotedValue = false;
			}
			base.CurrentState = FunctionTokenizerState.Parameter;
		}
		else if (currentToken == ')' && m_haveParameter)
		{
			AddTokenAndReset(SimpleTokenType.Symbol, isKeyValuePair: false);
		}
		else if (m_inQuotedValue)
		{
			AppendToAccumulator();
		}
		return StateResult.Continue;
	}
}
