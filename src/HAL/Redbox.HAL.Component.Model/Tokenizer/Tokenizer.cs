namespace Redbox.HAL.Component.Model.Tokenizer;

public class Tokenizer<T> : TokenizerBase<T>
{
	public TokenList Tokens { get; protected set; }

	public Tokenizer(int lineNumber, string statement)
		: base(lineNumber, statement)
	{
		Tokens = new TokenList();
	}

	protected internal void AddTokenAndReset(TokenType type, bool isKeyValuePair)
	{
		Tokens.Add(new Token(type, (type == TokenType.Comment) ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(), isKeyValuePair));
		ResetAccumulator();
	}

	protected internal void AddTokenAndReset(TokenType type, string pairSeparator)
	{
		Tokens.Add(new Token(type, (type == TokenType.Comment) ? m_tokenReader.RemainingTokens.Trim() : GetAccumulatedToken(), pairSeparator));
		ResetAccumulator();
	}
}
