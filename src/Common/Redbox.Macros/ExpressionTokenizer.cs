using System.Globalization;
using System.Text;

namespace Redbox.Macros;

public class ExpressionTokenizer
{
	public struct Position(int charIndex)
	{
		private readonly int _charIndex = charIndex;

		public int CharIndex => _charIndex;
	}

	public enum TokenType
	{
		BOF,
		EOF,
		Number,
		String,
		Keyword,
		EQ,
		NE,
		LT,
		GT,
		LE,
		GE,
		Plus,
		Minus,
		Mul,
		Div,
		Mod,
		LeftParen,
		RightParen,
		LeftCurlyBrace,
		RightCurlyBrace,
		Not,
		Punctuation,
		Whitespace,
		Dollar,
		Comma,
		Dot,
		DoubleColon
	}

	private struct CharToTokenType(char ch, TokenType tokenType)
	{
		public readonly char ch = ch;

		public readonly TokenType tokenType = tokenType;
	}

	private static readonly CharToTokenType[] charToTokenType;

	private string _text;

	private int _position;

	private bool _ignoreWhiteSpace = true;

	private static readonly TokenType[] charIndexToTokenType;

	public bool IgnoreWhitespace
	{
		get
		{
			return _ignoreWhiteSpace;
		}
		set
		{
			_ignoreWhiteSpace = value;
		}
	}

	public bool SingleCharacterMode { get; set; }

	public TokenType CurrentToken { get; private set; }

	public string TokenText { get; private set; }

	public Position CurrentPosition { get; private set; }

	public void InitTokenizer(string s)
	{
		_text = s;
		_position = 0;
		CurrentToken = TokenType.BOF;
		GetNextToken();
	}

	public void GetNextToken()
	{
		if (CurrentToken == TokenType.EOF)
		{
			throw new ExpressionParseException(ResourceUtils.GetString("String_CannotReadPastStream"), -1, -1);
		}
		if (IgnoreWhitespace)
		{
			SkipWhitespace();
		}
		CurrentPosition = new Position(_position);
		int num = PeekChar();
		if (num == -1)
		{
			CurrentToken = TokenType.EOF;
			return;
		}
		char c = (char)num;
		if (!SingleCharacterMode)
		{
			if (!IgnoreWhitespace && char.IsWhiteSpace(c))
			{
				StringBuilder stringBuilder = new StringBuilder();
				while ((num = PeekChar()) != -1 && char.IsWhiteSpace((char)num))
				{
					stringBuilder.Append((char)num);
					ReadChar();
				}
				CurrentToken = TokenType.Whitespace;
				TokenText = stringBuilder.ToString();
				return;
			}
			if (char.IsDigit(c))
			{
				CurrentToken = TokenType.Number;
				string empty = string.Empty;
				empty += c;
				ReadChar();
				while ((num = PeekChar()) != -1)
				{
					c = (char)num;
					if (!char.IsDigit(c))
					{
						break;
					}
					empty += (char)ReadChar();
				}
				TokenText = empty;
				return;
			}
			if (c == '\'')
			{
				CurrentToken = TokenType.String;
				string text = "";
				ReadChar();
				while ((num = ReadChar()) != -1)
				{
					c = (char)num;
					if (c == '\'')
					{
						if (PeekChar() != 39)
						{
							break;
						}
						ReadChar();
					}
					text += c;
				}
				TokenText = text;
				return;
			}
			if (c == '_' || char.IsLetter(c))
			{
				CurrentToken = TokenType.Keyword;
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append(c);
				ReadChar();
				while ((num = PeekChar()) != -1)
				{
					char c2 = (char)num;
					if (c2 == '_' || c2 == '-' || c2 == '.' || c2 == '\\' || char.IsLetterOrDigit(c2))
					{
						ReadChar();
						stringBuilder2.Append(c2);
						continue;
					}
					if (c2 != ':' || PeekChar2() == 58)
					{
						break;
					}
					ReadChar();
					stringBuilder2.Append(':');
				}
				TokenText = stringBuilder2.ToString();
				if (TokenText.EndsWith("-") || TokenText.EndsWith(".") || TokenText.EndsWith(":"))
				{
					throw new ExpressionParseException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1182"), TokenText), CurrentPosition.CharIndex);
				}
				return;
			}
			ReadChar();
			num = PeekChar();
			if (c == ':' && num == 58)
			{
				CurrentToken = TokenType.DoubleColon;
				TokenText = "::";
				ReadChar();
				return;
			}
			if (c == '!' && num == 61)
			{
				CurrentToken = TokenType.NE;
				TokenText = "!=";
				ReadChar();
				return;
			}
			if (c == '=' && num == 61)
			{
				CurrentToken = TokenType.EQ;
				TokenText = "==";
				ReadChar();
				return;
			}
			if (c == '<' && num == 61)
			{
				CurrentToken = TokenType.LE;
				TokenText = "<=";
				ReadChar();
				return;
			}
			if (c == '>' && num == 61)
			{
				CurrentToken = TokenType.GE;
				TokenText = ">=";
				ReadChar();
				return;
			}
		}
		else
		{
			ReadChar();
		}
		TokenText = new string(c, 1);
		if (c >= ' ' && c < '\u0080')
		{
			CurrentToken = charIndexToTokenType[(uint)c];
		}
		else
		{
			CurrentToken = TokenType.Punctuation;
		}
	}

	public bool IsKeyword(string k)
	{
		if (CurrentToken == TokenType.Keyword)
		{
			return TokenText == k;
		}
		return false;
	}

	static ExpressionTokenizer()
	{
		ExpressionTokenizer.charToTokenType = new CharToTokenType[15]
		{
			new CharToTokenType('+', TokenType.Plus),
			new CharToTokenType('-', TokenType.Minus),
			new CharToTokenType('*', TokenType.Mul),
			new CharToTokenType('/', TokenType.Div),
			new CharToTokenType('%', TokenType.Mod),
			new CharToTokenType('<', TokenType.LT),
			new CharToTokenType('>', TokenType.GT),
			new CharToTokenType('(', TokenType.LeftParen),
			new CharToTokenType(')', TokenType.RightParen),
			new CharToTokenType('{', TokenType.LeftCurlyBrace),
			new CharToTokenType('}', TokenType.RightCurlyBrace),
			new CharToTokenType('!', TokenType.Not),
			new CharToTokenType('$', TokenType.Dollar),
			new CharToTokenType(',', TokenType.Comma),
			new CharToTokenType('.', TokenType.Dot)
		};
		charIndexToTokenType = new TokenType[128];
		for (int i = 0; i < 128; i++)
		{
			charIndexToTokenType[i] = TokenType.Punctuation;
		}
		CharToTokenType[] array = ExpressionTokenizer.charToTokenType;
		for (int j = 0; j < array.Length; j++)
		{
			CharToTokenType charToTokenType = array[j];
			charIndexToTokenType[(uint)charToTokenType.ch] = charToTokenType.tokenType;
		}
	}

	private int ReadChar()
	{
		if (_position < _text.Length)
		{
			return _text[_position++];
		}
		return -1;
	}

	private int PeekChar()
	{
		if (_position < _text.Length)
		{
			return _text[_position];
		}
		return -1;
	}

	private int PeekChar2()
	{
		if (_position + 1 < _text.Length)
		{
			return _text[_position + 1];
		}
		return -1;
	}

	private void SkipWhitespace()
	{
		int num;
		while ((num = PeekChar()) != -1 && char.IsWhiteSpace((char)num))
		{
			ReadChar();
		}
	}
}
