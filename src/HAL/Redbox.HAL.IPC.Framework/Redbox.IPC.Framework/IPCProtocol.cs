using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.IPC.Framework;

public class IPCProtocol : IIpcProtocol
{
	private enum ProtocolParserState
	{
		Start = 1,
		Scheme,
		Host,
		Port,
		Channel,
		Whitespace,
		Error
	}

	private class ProtocolTokenizer : Tokenizer<ProtocolParserState>
	{
		private int m_slashesSeen;

		private int m_semiColonsSeen;

		internal ChannelType Channel { get; private set; }

		internal bool IsSecure { get; private set; }

		internal string Host { get; private set; }

		internal string Port { get; private set; }

		internal string Scheme { get; private set; }

		public ProtocolTokenizer(string statement)
			: base(0, statement)
		{
		}

		protected override void OnReset()
		{
			base.CurrentState = ProtocolParserState.Start;
		}

		[StateHandler(State = ProtocolParserState.Error)]
		internal StateResult ProcessErrorState()
		{
			ResetAccumulator();
			base.Errors.Add(Error.NewError("T006", FormatError("An invalid token was detected."), "Correct your protocol syntax and resubmit."));
			return StateResult.Terminal;
		}

		[StateHandler(State = ProtocolParserState.Start)]
		internal StateResult ProcessStartState()
		{
			if (char.IsWhiteSpace(GetCurrentToken()))
			{
				return StateResult.Continue;
			}
			if (char.IsLetter(GetCurrentToken()))
			{
				base.CurrentState = ProtocolParserState.Scheme;
				return StateResult.Restart;
			}
			base.CurrentState = ProtocolParserState.Error;
			return StateResult.Restart;
		}

		[StateHandler(State = ProtocolParserState.Host)]
		internal StateResult ProcessHostState()
		{
			char currentToken = GetCurrentToken();
			if (char.IsWhiteSpace(currentToken))
			{
				return StateResult.Continue;
			}
			if (char.IsLetterOrDigit(currentToken) || currentToken == '.')
			{
				AppendToAccumulator();
				return StateResult.Continue;
			}
			if (':' == currentToken)
			{
				Host = base.Accumulator.ToString();
				AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
				base.CurrentState = ProtocolParserState.Whitespace;
				return StateResult.Restart;
			}
			base.CurrentState = ProtocolParserState.Error;
			return StateResult.Restart;
		}

		[StateHandler(State = ProtocolParserState.Scheme)]
		internal StateResult ProcessProtocolState()
		{
			char currentToken = GetCurrentToken();
			if (char.IsLetter(currentToken))
			{
				if (base.Accumulator.Length == 0 && ('s' == currentToken || 'S' == currentToken))
				{
					IsSecure = true;
				}
				AppendToAccumulator();
				return StateResult.Continue;
			}
			if ('-' == currentToken)
			{
				MoveToNextToken();
				Scheme = base.Accumulator.ToString();
				if (ValidateProtocol())
				{
					AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
					base.CurrentState = ProtocolParserState.Channel;
					return StateResult.Restart;
				}
				base.CurrentState = ProtocolParserState.Error;
				return StateResult.Restart;
			}
			if (':' == currentToken || char.IsWhiteSpace(currentToken))
			{
				Scheme = base.Accumulator.ToString();
				if (ValidateProtocol())
				{
					AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
					base.CurrentState = ProtocolParserState.Whitespace;
					return StateResult.Restart;
				}
			}
			base.CurrentState = ProtocolParserState.Error;
			return StateResult.Restart;
		}

		[StateHandler(State = ProtocolParserState.Port)]
		internal StateResult ProcessPortState()
		{
			char currentToken = GetCurrentToken();
			if (char.IsWhiteSpace(currentToken))
			{
				if (base.Accumulator.Length > 0)
				{
					Port = base.Accumulator.ToString();
					AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
					return StateResult.Terminal;
				}
				return StateResult.Continue;
			}
			if (char.IsLetterOrDigit(currentToken))
			{
				AppendToAccumulator();
				if (!PeekNextToken().HasValue)
				{
					Port = base.Accumulator.ToString();
					AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
					MoveToNextToken();
					return StateResult.Terminal;
				}
				return StateResult.Continue;
			}
			base.CurrentState = ProtocolParserState.Error;
			return StateResult.Restart;
		}

		[StateHandler(State = ProtocolParserState.Channel)]
		internal StateResult ProcessChannelType()
		{
			char currentToken = GetCurrentToken();
			if (char.IsWhiteSpace(currentToken) || ':' == currentToken)
			{
				DecodeChannel();
				AddTokenAndReset(TokenType.StringLiteral, isKeyValuePair: false);
				base.CurrentState = ProtocolParserState.Whitespace;
				return StateResult.Restart;
			}
			if (char.IsLetter(currentToken))
			{
				AppendToAccumulator();
				return StateResult.Continue;
			}
			base.CurrentState = ProtocolParserState.Error;
			return StateResult.Restart;
		}

		[StateHandler(State = ProtocolParserState.Whitespace)]
		internal StateResult ProcessWhitespaceState()
		{
			char currentToken = GetCurrentToken();
			if (char.IsWhiteSpace(currentToken))
			{
				return StateResult.Continue;
			}
			switch (currentToken)
			{
			case ':':
				m_semiColonsSeen++;
				return StateResult.Continue;
			case '/':
				m_slashesSeen++;
				return StateResult.Continue;
			default:
				if (char.IsLetterOrDigit(currentToken))
				{
					if (m_slashesSeen == 0 && m_semiColonsSeen == 0)
					{
						base.CurrentState = ProtocolParserState.Scheme;
						return StateResult.Restart;
					}
					if (m_semiColonsSeen == 1 && m_slashesSeen == 2)
					{
						base.CurrentState = ProtocolParserState.Host;
						return StateResult.Restart;
					}
					if (m_semiColonsSeen == 2 && m_slashesSeen == 2)
					{
						base.CurrentState = ProtocolParserState.Port;
						return StateResult.Restart;
					}
				}
				base.CurrentState = ProtocolParserState.Error;
				return StateResult.Restart;
			}
		}

		private bool ValidateProtocol()
		{
			if (base.Accumulator.ToString().Equals("rcp", StringComparison.CurrentCultureIgnoreCase) || base.Accumulator.ToString().Equals("srcp", StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		private bool ChannelIsValid(char protocol)
		{
			if ('p' == protocol || 'P' == protocol)
			{
				Channel = ChannelType.NamedPipe;
				return true;
			}
			return false;
		}

		private bool DecodeChannel()
		{
			if (base.Accumulator.ToString().Equals("p", StringComparison.CurrentCultureIgnoreCase))
			{
				Channel = ChannelType.NamedPipe;
				return true;
			}
			Channel = ChannelType.Unknown;
			return false;
		}
	}

	private string PipeName;

	private readonly string RawUri;

	public bool IsSecure { get; private set; }

	public ChannelType Channel { get; private set; }

	public string Host { get; private set; }

	public string Port { get; private set; }

	public string Scheme { get; private set; }

	public static IPCProtocol Parse(string protocolURI)
	{
		IPCProtocol iPCProtocol = new IPCProtocol(protocolURI)
		{
			Host = string.Empty,
			Port = string.Empty,
			IsSecure = false,
			Channel = ChannelType.Unknown
		};
		ProtocolTokenizer protocolTokenizer = new ProtocolTokenizer(protocolURI);
		try
		{
			protocolTokenizer.Tokenize();
		}
		catch (Exception ex)
		{
			throw new UriFormatException(ex.Message);
		}
		if (protocolTokenizer.Errors.ContainsError())
		{
			foreach (Error error in protocolTokenizer.Errors)
			{
				LogHelper.Instance.Log(error.Description, LogEntryType.Error);
			}
			throw new UriFormatException("URI is malformed: see log for details.");
		}
		iPCProtocol.Host = protocolTokenizer.Host;
		iPCProtocol.Port = protocolTokenizer.Port;
		iPCProtocol.IsSecure = protocolTokenizer.IsSecure;
		iPCProtocol.Channel = protocolTokenizer.Channel;
		iPCProtocol.Scheme = protocolTokenizer.Scheme;
		if (string.IsNullOrEmpty(iPCProtocol.Host) || string.IsNullOrEmpty(iPCProtocol.Port))
		{
			throw new UriFormatException("Host or port isn't set.");
		}
		if (iPCProtocol.Channel == ChannelType.Unknown)
		{
			throw new UriFormatException("The channel type is unknown; please correct your URI.");
		}
		if (iPCProtocol.Channel == ChannelType.Socket && !int.TryParse(iPCProtocol.Port, out var _))
		{
			throw new UriFormatException("Protocol is set up for sockets, but port isn't a valid address.");
		}
		return iPCProtocol;
	}

	public string GetPipeName()
	{
		if (PipeName == null)
		{
			PipeName = $"{Host}{Port}";
		}
		return PipeName;
	}

	public override string ToString()
	{
		return RawUri;
	}

	private IPCProtocol(string rawUri)
	{
		RawUri = rawUri;
	}
}
