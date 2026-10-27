namespace Redbox.Macros;

internal class FunctionArgument
{
	private readonly int _index;

	private readonly string _name;

	private readonly object _value;

	private readonly ExpressionTokenizer.Position _beforeArgument;

	private readonly ExpressionTokenizer.Position _afterArgument;

	public int Index => _index;

	public string Name => _name;

	public object Value => _value;

	public ExpressionTokenizer.Position BeforeArgument => _beforeArgument;

	public ExpressionTokenizer.Position AfterArgument => _afterArgument;

	public FunctionArgument(string name, int index, object value, ExpressionTokenizer.Position beforeArgument, ExpressionTokenizer.Position afterArgument)
	{
		_name = name;
		_index = index;
		_value = value;
		_beforeArgument = beforeArgument;
		_afterArgument = afterArgument;
	}
}
