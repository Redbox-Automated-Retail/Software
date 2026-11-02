using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.Macros;

public abstract class ExpressionEvalBase
{
	public enum EvalMode
	{
		Evaluate,
		ParseOnly
	}

	// Set by Evaluate/CheckSyntax before parsing starts. The Parse* methods return null only in ParseOnly mode, where
	// the value is not used.
	private ExpressionTokenizer _tokenizer = null!;

	private EvalMode _evalMode;

	public object Evaluate(ExpressionTokenizer tokenizer)
	{
		_evalMode = EvalMode.Evaluate;
		_tokenizer = tokenizer;
		return ParseExpression();
	}

	public object Evaluate(string s)
	{
		_tokenizer = new ExpressionTokenizer();
		_evalMode = EvalMode.Evaluate;
		_tokenizer.InitTokenizer(s);
		object result = ParseExpression();
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
		{
			throw BuildParseError("Unexpected token at the end of expression", _tokenizer.CurrentPosition);
		}
		return result;
	}

	public void CheckSyntax(string s)
	{
		_tokenizer = new ExpressionTokenizer();
		_evalMode = EvalMode.ParseOnly;
		_tokenizer.InitTokenizer(s);
		ParseExpression();
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
		{
			throw BuildParseError("Unexpected token at the end of expression", _tokenizer.CurrentPosition);
		}
	}

	private bool SyntaxCheckOnly()
	{
		return _evalMode == EvalMode.ParseOnly;
	}

	private object ParseExpression()
	{
		return ParseBooleanOr();
	}

	private object ParseBooleanOr()
	{
		ExpressionTokenizer.Position leftStart = _tokenizer.CurrentPosition;
		object left = ParseBooleanAnd();
		EvalMode savedEvalMode = _evalMode;
		try
		{
			while (_tokenizer.IsKeyword("or"))
			{
				bool leftValue = true;
				if (!SyntaxCheckOnly())
				{
					leftValue = (bool)SafeConvert(typeof(bool), left, "the left hand side of the 'or' operator", leftStart, _tokenizer.CurrentPosition);
					if (leftValue)
					{
						_evalMode = EvalMode.ParseOnly;
					}
				}
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position rightStart = _tokenizer.CurrentPosition;
				object right = ParseBooleanAnd();
				ExpressionTokenizer.Position rightEnd = _tokenizer.CurrentPosition;
				if (!SyntaxCheckOnly())
				{
					bool rightValue = (bool)SafeConvert(typeof(bool), right, "the right hand side of the 'or' operator", rightStart, rightEnd);
					left = leftValue | rightValue;
				}
			}
			return left;
		}
		finally
		{
			_evalMode = savedEvalMode;
		}
	}

	private object ParseBooleanAnd()
	{
		ExpressionTokenizer.Position leftStart = _tokenizer.CurrentPosition;
		object left = ParseRelationalExpression();
		EvalMode savedEvalMode = _evalMode;
		try
		{
			while (_tokenizer.IsKeyword("and"))
			{
				bool leftValue = true;
				if (!SyntaxCheckOnly())
				{
					leftValue = (bool)SafeConvert(typeof(bool), left, "the left hand side of the 'and' operator", leftStart, _tokenizer.CurrentPosition);
					if (!leftValue)
					{
						_evalMode = EvalMode.ParseOnly;
					}
				}
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position rightStart = _tokenizer.CurrentPosition;
				object right = ParseRelationalExpression();
				ExpressionTokenizer.Position rightEnd = _tokenizer.CurrentPosition;
				if (!SyntaxCheckOnly())
				{
					bool rightValue = (bool)SafeConvert(typeof(bool), right, "the right hand side of the 'and' operator", rightStart, rightEnd);
					left = leftValue & rightValue;
				}
			}
			return left;
		}
		finally
		{
			_evalMode = savedEvalMode;
		}
	}

	private object ParseRelationalExpression()
	{
		ExpressionTokenizer.Position start = _tokenizer.CurrentPosition;
		object left = ParseAddSubtract();
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EQ || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.NE || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LT || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GT || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LE || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GE)
		{
			ExpressionTokenizer.TokenType op = _tokenizer.CurrentToken;
			_tokenizer.GetNextToken();
			object right = ParseAddSubtract();
			ExpressionTokenizer.Position end = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				return null!;
			}
			switch (op)
			{
			case ExpressionTokenizer.TokenType.EQ:
				if (left is string && right is string)
				{
					return left.Equals(right);
				}
				if (left is bool && right is bool)
				{
					return left.Equals(right);
				}
				if (left is int && right is int)
				{
					return left.Equals(right);
				}
				if (left is int && right is long)
				{
					return Convert.ToInt64(left).Equals(right);
				}
				if (left is int && right is double)
				{
					return Convert.ToDouble(left).Equals(right);
				}
				if (left is long && right is long)
				{
					return left.Equals(right);
				}
				if (left is long && right is int)
				{
					return left.Equals(Convert.ToInt64(right));
				}
				if (left is long && right is double)
				{
					return Convert.ToDouble(left).Equals(right);
				}
				if (left is double && right is double)
				{
					return left.Equals(right);
				}
				if (left is double && right is int)
				{
					return left.Equals(Convert.ToDouble(right));
				}
				if (left is double && right is long)
				{
					return left.Equals(Convert.ToDouble(right));
				}
				if (left is DateTime && right is DateTime)
				{
					return left.Equals(right);
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return left.Equals(right);
				}
				if (left is Version && right is Version)
				{
					return left.Equals(right);
				}
				if (left.GetType().IsEnum)
				{
					if (right is string)
					{
						return left.Equals(Enum.Parse(left.GetType(), (string)right, ignoreCase: false));
					}
					return left.Equals(Enum.ToObject(left.GetType(), right));
				}
				if (right.GetType().IsEnum)
				{
					if (left is string)
					{
						return right.Equals(Enum.Parse(right.GetType(), (string)left, ignoreCase: false));
					}
					return right.Equals(Enum.ToObject(right.GetType(), left));
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1038"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			case ExpressionTokenizer.TokenType.NE:
				if (left is string && right is string)
				{
					return !left.Equals(right);
				}
				if (left is bool && right is bool)
				{
					return !left.Equals(right);
				}
				if (left is int && right is int)
				{
					return !left.Equals(right);
				}
				if (left is int && right is long)
				{
					return !Convert.ToInt64(left).Equals(right);
				}
				if (left is int && right is double)
				{
					return !Convert.ToDouble(left).Equals(right);
				}
				if (left is long && right is long)
				{
					return !left.Equals(right);
				}
				if (left is long && right is int)
				{
					return !left.Equals(Convert.ToInt64(right));
				}
				if (left is long && right is double)
				{
					return !Convert.ToDouble(left).Equals(right);
				}
				if (left is double && right is double)
				{
					return !left.Equals(right);
				}
				if (left is double && right is int)
				{
					return !left.Equals(Convert.ToDouble(right));
				}
				if (left is double && right is long)
				{
					return !left.Equals(Convert.ToDouble(right));
				}
				if (left is DateTime && right is DateTime)
				{
					return !left.Equals(right);
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return !left.Equals(right);
				}
				if (left is Version && right is Version)
				{
					return !left.Equals(right);
				}
				if (left.GetType().IsEnum)
				{
					if (right is string)
					{
						return !left.Equals(Enum.Parse(left.GetType(), (string)right, ignoreCase: false));
					}
					return !left.Equals(Enum.ToObject(left.GetType(), right));
				}
				if (right.GetType().IsEnum)
				{
					if (left is string)
					{
						return !right.Equals(Enum.Parse(right.GetType(), (string)left, ignoreCase: false));
					}
					return !right.Equals(Enum.ToObject(right.GetType(), left));
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1042"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			case ExpressionTokenizer.TokenType.LT:
				if (left is string && right is string)
				{
					return string.Compare((string)left, (string)right, ignoreCase: false, CultureInfo.InvariantCulture) < 0;
				}
				if (left is bool && right is bool)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is int && right is int)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is int && right is long)
				{
					return ((IComparable)Convert.ToInt64(left)).CompareTo(right) < 0;
				}
				if (left is int && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) < 0;
				}
				if (left is long && right is long)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is long && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToInt64(right)) < 0;
				}
				if (left is long && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) < 0;
				}
				if (left is double && right is double)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is double && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) < 0;
				}
				if (left is double && right is long)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) < 0;
				}
				if (left is DateTime && right is DateTime)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				if (left is Version && right is Version)
				{
					return ((IComparable)left).CompareTo(right) < 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1051"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			case ExpressionTokenizer.TokenType.GT:
				if (left is string && right is string)
				{
					return string.Compare((string)left, (string)right, ignoreCase: false, CultureInfo.InvariantCulture) > 0;
				}
				if (left is bool && right is bool)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is int && right is int)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is int && right is long)
				{
					return ((IComparable)Convert.ToInt64(left)).CompareTo(right) > 0;
				}
				if (left is int && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) > 0;
				}
				if (left is long && right is long)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is long && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToInt64(right)) > 0;
				}
				if (left is long && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) > 0;
				}
				if (left is double && right is double)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is double && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) > 0;
				}
				if (left is double && right is long)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) > 0;
				}
				if (left is DateTime && right is DateTime)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				if (left is Version && right is Version)
				{
					return ((IComparable)left).CompareTo(right) > 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1037"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			case ExpressionTokenizer.TokenType.LE:
				if (left is string && right is string)
				{
					return string.Compare((string)left, (string)right, ignoreCase: false, CultureInfo.InvariantCulture) <= 0;
				}
				if (left is bool && right is bool)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is int && right is int)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is int && right is long)
				{
					return ((IComparable)Convert.ToInt64(left)).CompareTo(right) <= 0;
				}
				if (left is int && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) <= 0;
				}
				if (left is long && right is long)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is long && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToInt64(right)) <= 0;
				}
				if (left is long && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) <= 0;
				}
				if (left is double && right is double)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is double && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) <= 0;
				}
				if (left is double && right is long)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) <= 0;
				}
				if (left is DateTime && right is DateTime)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				if (left is Version && right is Version)
				{
					return ((IComparable)left).CompareTo(right) <= 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1049"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			case ExpressionTokenizer.TokenType.GE:
				if (left is string && right is string)
				{
					return string.Compare((string)left, (string)right, ignoreCase: false, CultureInfo.InvariantCulture) >= 0;
				}
				if (left is bool && right is bool)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is int && right is int)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is int && right is long)
				{
					return ((IComparable)Convert.ToInt64(left)).CompareTo(right) >= 0;
				}
				if (left is int && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) >= 0;
				}
				if (left is long && right is long)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is long && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToInt64(right)) >= 0;
				}
				if (left is long && right is double)
				{
					return ((IComparable)Convert.ToDouble(left)).CompareTo(right) >= 0;
				}
				if (left is double && right is double)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is double && right is int)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) >= 0;
				}
				if (left is double && right is long)
				{
					return ((IComparable)left).CompareTo(Convert.ToDouble(right)) >= 0;
				}
				if (left is DateTime && right is DateTime)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				if (left is Version && right is Version)
				{
					return ((IComparable)left).CompareTo(right) >= 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1050"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			}
		}
		return left;
	}

	private object ParseAddSubtract()
	{
		ExpressionTokenizer.Position start = _tokenizer.CurrentPosition;
		object left = ParseMulDiv();
		while (true)
		{
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Plus)
			{
				_tokenizer.GetNextToken();
				object right = ParseMulDiv();
				ExpressionTokenizer.Position end = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (left is string && right is string)
				{
					left = (string)left + (string)right;
					continue;
				}
				if (left is int && right is int)
				{
					left = (int)left + (int)right;
					continue;
				}
				if (left is int && right is long)
				{
					left = (int)left + (long)right;
					continue;
				}
				if (left is int && right is double)
				{
					left = (double)(int)left + (double)right;
					continue;
				}
				if (left is long && right is long)
				{
					left = (long)left + (long)right;
					continue;
				}
				if (left is long && right is int)
				{
					left = (long)left + (int)right;
					continue;
				}
				if (left is long && right is double)
				{
					left = (double)(long)left + (double)right;
					continue;
				}
				if (left is double && right is double)
				{
					left = (double)left + (double)right;
					continue;
				}
				if (left is double && right is int)
				{
					left = (double)left + (double)(int)right;
					continue;
				}
				if (left is double && right is long)
				{
					left = (double)left + (double)(long)right;
					continue;
				}
				if (left is DateTime && right is TimeSpan)
				{
					left = (DateTime)left + (TimeSpan)right;
					continue;
				}
				if (left is TimeSpan && right is TimeSpan)
				{
					left = (TimeSpan)left + (TimeSpan)right;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1041"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			}
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Minus)
			{
				break;
			}
			_tokenizer.GetNextToken();
			object subtrahend = ParseMulDiv();
			ExpressionTokenizer.Position subtrahendEnd = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				continue;
			}
			if (left is int && subtrahend is int)
			{
				left = (int)left - (int)subtrahend;
				continue;
			}
			if (left is int && subtrahend is long)
			{
				left = (int)left - (long)subtrahend;
				continue;
			}
			if (left is int && subtrahend is double)
			{
				left = (double)(int)left - (double)subtrahend;
				continue;
			}
			if (left is long && subtrahend is long)
			{
				left = (long)left - (long)subtrahend;
				continue;
			}
			if (left is long && subtrahend is int)
			{
				left = (long)left - (int)subtrahend;
				continue;
			}
			if (left is long && subtrahend is double)
			{
				left = (double)(long)left - (double)subtrahend;
				continue;
			}
			if (left is double && subtrahend is double)
			{
				left = (double)left - (double)subtrahend;
				continue;
			}
			if (left is double && subtrahend is int)
			{
				left = (double)left - (double)(int)subtrahend;
				continue;
			}
			if (left is double && subtrahend is long)
			{
				left = (double)left - (double)(long)subtrahend;
				continue;
			}
			if (left is DateTime && subtrahend is DateTime)
			{
				left = (DateTime)left - (DateTime)subtrahend;
				continue;
			}
			if (left is DateTime && subtrahend is TimeSpan)
			{
				left = (DateTime)left - (TimeSpan)subtrahend;
				continue;
			}
			if (left is TimeSpan && subtrahend is TimeSpan)
			{
				left = (TimeSpan)left - (TimeSpan)subtrahend;
				continue;
			}
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1048"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(subtrahend.GetType())), start, subtrahendEnd);
		}
		return left;
	}

	private object ParseMulDiv()
	{
		ExpressionTokenizer.Position start = _tokenizer.CurrentPosition;
		object left = ParseValue();
		while (true)
		{
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Mul)
			{
				_tokenizer.GetNextToken();
				object right = ParseValue();
				ExpressionTokenizer.Position end = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (left is int && right is int)
				{
					left = (int)left * (int)right;
					continue;
				}
				if (left is int && right is long)
				{
					left = (int)left * (long)right;
					continue;
				}
				if (left is int && right is double)
				{
					left = (double)(int)left * (double)right;
					continue;
				}
				if (left is long && right is long)
				{
					left = (long)left * (long)right;
					continue;
				}
				if (left is long && right is int)
				{
					left = (long)left * (int)right;
					continue;
				}
				if (left is long && right is double)
				{
					left = (double)(long)left * (double)right;
					continue;
				}
				if (left is double && right is double)
				{
					left = (double)left * (double)right;
					continue;
				}
				if (left is double && right is int)
				{
					left = (double)left * (double)(int)right;
					continue;
				}
				if (left is double && right is long)
				{
					left = (double)left * (double)(long)right;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1036"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			}
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Div)
			{
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position rightStart = _tokenizer.CurrentPosition;
				object right = ParseValue();
				ExpressionTokenizer.Position end = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (left is int && right is int)
				{
					if ((int)right == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (int)left / (int)right;
					continue;
				}
				if (left is int && right is long)
				{
					if ((long)right == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (int)left / (long)right;
					continue;
				}
				if (left is int && right is double)
				{
					if ((double)right == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (double)(int)left / (double)right;
					continue;
				}
				if (left is long && right is long)
				{
					if ((long)right == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (long)left / (long)right;
					continue;
				}
				if (left is long && right is int)
				{
					if ((int)right == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (long)left / (int)right;
					continue;
				}
				if (left is long && right is double)
				{
					if ((double)right == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (double)(long)left / (double)right;
					continue;
				}
				if (left is double && right is double)
				{
					if ((double)right == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (double)left / (double)right;
					continue;
				}
				if (left is double && right is int)
				{
					if ((int)right == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (double)left / (double)(int)right;
					continue;
				}
				if (left is double && right is long)
				{
					if ((long)right == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), rightStart, end);
					}
					left = (double)left / (double)(long)right;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1039"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(right.GetType())), start, end);
			}
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mod)
			{
				break;
			}
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position modulusStart = _tokenizer.CurrentPosition;
			object modulus = ParseValue();
			ExpressionTokenizer.Position modulusEnd = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				continue;
			}
			if (left is int && modulus is int)
			{
				if ((int)modulus == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (int)left % (int)modulus;
				continue;
			}
			if (left is int && modulus is long)
			{
				if ((long)modulus == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (int)left % (long)modulus;
				continue;
			}
			if (left is int && modulus is double)
			{
				if ((double)modulus == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (double)(int)left % (double)modulus;
				continue;
			}
			if (left is long && modulus is long)
			{
				if ((long)modulus == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (long)left % (long)modulus;
				continue;
			}
			if (left is long && modulus is int)
			{
				if ((int)modulus == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (long)left % (int)modulus;
				continue;
			}
			if (left is long && modulus is double)
			{
				if ((double)modulus == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (double)(long)left % (double)modulus;
				continue;
			}
			if (left is double && modulus is double)
			{
				if ((double)modulus == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (double)left % (double)modulus;
				continue;
			}
			if (left is double && modulus is int)
			{
				if ((int)modulus == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (double)left % (double)(int)modulus;
				continue;
			}
			if (left is double && modulus is long)
			{
				if ((long)modulus == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), modulusStart, modulusEnd);
				}
				left = (double)left % (double)(long)modulus;
				continue;
			}
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1047"), GetSimpleTypeName(left.GetType()), GetSimpleTypeName(modulus.GetType())), start, modulusEnd);
		}
		return left;
	}

	private object ParseConditional()
	{
		_tokenizer.GetNextToken();
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
		{
			throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
		}
		_tokenizer.GetNextToken();
		ExpressionTokenizer.Position conditionStart = _tokenizer.CurrentPosition;
		object condition = ParseExpression();
		ExpressionTokenizer.Position conditionEnd = _tokenizer.CurrentPosition;
		bool conditionValue = false;
		if (!SyntaxCheckOnly())
		{
			conditionValue = (bool)SafeConvert(typeof(bool), condition, "the conditional expression", conditionStart, conditionEnd);
		}
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
		{
			throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
		}
		_tokenizer.GetNextToken();
		EvalMode savedEvalMode = _evalMode;
		try
		{
			if (!conditionValue)
			{
				_evalMode = EvalMode.ParseOnly;
			}
			else
			{
				_evalMode = savedEvalMode;
			}
			object whenTrue = ParseExpression();
			_evalMode = savedEvalMode;
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
			{
				throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			if (conditionValue)
			{
				_evalMode = EvalMode.ParseOnly;
			}
			else
			{
				_evalMode = savedEvalMode;
			}
			object whenFalse = ParseExpression();
			_evalMode = savedEvalMode;
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
			{
				throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			return conditionValue ? whenTrue : whenFalse;
		}
		finally
		{
			_evalMode = savedEvalMode;
		}
	}

	private object ParseValue()
	{
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.String)
		{
			string tokenText = _tokenizer.TokenText;
			_tokenizer.GetNextToken();
			return tokenText;
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
		{
			string number = _tokenizer.TokenText;
			ExpressionTokenizer.Position numberStart = _tokenizer.CurrentPosition;
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position numberEnd = new ExpressionTokenizer.Position(_tokenizer.CurrentPosition.CharIndex - 1);
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot)
			{
				number += ".";
				_tokenizer.GetNextToken();
				if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Number)
				{
					throw BuildParseError("Fractional part expected.", _tokenizer.CurrentPosition);
				}
				number += _tokenizer.TokenText;
				_tokenizer.GetNextToken();
				numberEnd = _tokenizer.CurrentPosition;
				try
				{
					return double.Parse(number, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					throw BuildParseError("Value was either too large or too small for type 'double'.", numberStart, numberEnd);
				}
			}
			try
			{
				return int.Parse(number, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				try
				{
					return long.Parse(number, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					throw BuildParseError("Value was either too large or too small for type 'long'.", numberStart, numberEnd);
				}
			}
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus)
		{
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position operandStart = _tokenizer.CurrentPosition;
			object operand = ParseValue();
			ExpressionTokenizer.Position operandEnd = _tokenizer.CurrentPosition;
			if (!SyntaxCheckOnly())
			{
				if (operand is int)
				{
					return -(int)operand;
				}
				if (operand is long)
				{
					return -(long)operand;
				}
				if (operand is double)
				{
					return 0.0 - (double)operand;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1040"), GetSimpleTypeName(operand.GetType())), operandStart, operandEnd);
			}
			return null!;
		}
		if (_tokenizer.IsKeyword("not"))
		{
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position operandStart = _tokenizer.CurrentPosition;
			object operand = ParseValue();
			ExpressionTokenizer.Position operandEnd = _tokenizer.CurrentPosition;
			if (!SyntaxCheckOnly())
			{
				return !(bool)SafeConvert(typeof(bool), operand, "the argument of 'not' operator", operandStart, operandEnd);
			}
			return null!;
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftParen)
		{
			_tokenizer.GetNextToken();
			object inner = ParseExpression();
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
			{
				throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			return inner;
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword)
		{
			ExpressionTokenizer.Position nameStart = _tokenizer.CurrentPosition;
			string name = _tokenizer.TokenText;
			switch (name)
			{
			case "if":
				return ParseConditional();
			case "true":
				_tokenizer.GetNextToken();
				return true;
			case "false":
				_tokenizer.GetNextToken();
				return false;
			default:
			{
				_tokenizer.IgnoreWhitespace = false;
				_tokenizer.GetNextToken();
				ArrayList args = new ArrayList();
				bool isFunction = false;
				if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.DoubleColon)
				{
					isFunction = true;
					name += "::";
					_tokenizer.GetNextToken();
					if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
					{
						throw BuildParseError("Function name expected.", nameStart, _tokenizer.CurrentPosition);
					}
					name += _tokenizer.TokenText;
					_tokenizer.GetNextToken();
				}
				else
				{
					while (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
					{
						name += _tokenizer.TokenText;
						_tokenizer.GetNextToken();
					}
				}
				_tokenizer.IgnoreWhitespace = true;
				if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Whitespace)
				{
					_tokenizer.GetNextToken();
				}
				MethodInfo? function = null;
				if (isFunction)
				{
					if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
					{
						throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
					}
					_tokenizer.GetNextToken();
					int argumentIndex = 0;
					while (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen && _tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
					{
						ExpressionTokenizer.Position argStart = _tokenizer.CurrentPosition;
						object argValue = ParseExpression();
						ExpressionTokenizer.Position argEnd = _tokenizer.CurrentPosition;
						args.Add(new FunctionArgument(name, argumentIndex, argValue, argStart, argEnd));
						argumentIndex++;
						if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.RightParen)
						{
							break;
						}
						if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
						{
							throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
						}
						_tokenizer.GetNextToken();
					}
					if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
					{
						throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
					}
					_tokenizer.GetNextToken();
					if (!SyntaxCheckOnly())
					{
						FunctionArgument[] functionArgs = new FunctionArgument[args.Count];
						args.CopyTo(0, functionArgs, 0, args.Count);
						try
						{
							function = FunctionFactory.LookupFunction(name, functionArgs);
						}
						catch (EvaluatorException ex)
						{
							throw BuildParseError(ex.Message, nameStart, _tokenizer.CurrentPosition);
						}
						ParameterInfo[] parameters = function.GetParameters();
						args.Clear();
						for (int i = 0; i < functionArgs.Length; i++)
						{
							FunctionArgument functionArgument = functionArgs[i];
							ParameterInfo parameterInfo = parameters[i];
							object convertedValue = SafeConvert(parameterInfo.ParameterType, functionArgument.Value, string.Format(CultureInfo.InvariantCulture, "argument {1} ({0}) of {2}()", parameterInfo.Name, functionArgument.Index, functionArgument.Name), functionArgument.BeforeArgument, functionArgument.AfterArgument);
							args.Add(convertedValue);
						}
					}
				}
				try
				{
					if (!SyntaxCheckOnly())
					{
						if (isFunction)
						{
							// function was looked up above (not ParseOnly); a void or null result flows on as before.
							return EvaluateFunction(function!, args.ToArray())!;
						}
						return EvaluateProperty(name);
					}
					return null!;
				}
				catch (Exception ex)
				{
					if (isFunction)
					{
						throw BuildParseError("Function call failed.", nameStart, _tokenizer.CurrentPosition, ex);
					}
					throw BuildParseError("Property evaluation failed.", nameStart, _tokenizer.CurrentPosition, ex);
				}
			}
			}
		}
		return UnexpectedToken();
	}

	protected ExpressionParseException BuildParseError(string desc, ExpressionTokenizer.Position p0)
	{
		return new ExpressionParseException(desc, p0.CharIndex);
	}

	protected ExpressionParseException BuildParseError(string desc, ExpressionTokenizer.Position p0, ExpressionTokenizer.Position p1)
	{
		return new ExpressionParseException(desc, p0.CharIndex, p1.CharIndex);
	}

	protected ExpressionParseException BuildParseError(string desc, ExpressionTokenizer.Position p0, ExpressionTokenizer.Position p1, Exception ex)
	{
		return new ExpressionParseException(desc, p0.CharIndex, p1.CharIndex, ex);
	}

	protected object SafeConvert(Type returnType, object source, string description, ExpressionTokenizer.Position p0, ExpressionTokenizer.Position p1)
	{
		try
		{
			bool unsupported = false;
			if (source == null)
			{
				if (returnType == typeof(string))
				{
					return string.Empty;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1045"), description, GetSimpleTypeName(returnType)), p0, p1);
			}
			if (source is bool && returnType != typeof(string) && returnType != typeof(bool))
			{
				unsupported = true;
			}
			if (returnType == typeof(bool) && !(source is string) && !(source is bool))
			{
				unsupported = true;
			}
			if (source is DateTime && returnType != typeof(string) && returnType != typeof(DateTime))
			{
				unsupported = true;
			}
			if (returnType == typeof(DateTime) && !(source is DateTime) && !(source is string))
			{
				unsupported = true;
			}
			if (source is TimeSpan && returnType != typeof(TimeSpan))
			{
				unsupported = true;
			}
			if (returnType == typeof(TimeSpan) && !(source is TimeSpan))
			{
				unsupported = true;
			}
			if (returnType == typeof(string))
			{
				if (source is DirectoryInfo)
				{
					return ((DirectoryInfo)source).FullName;
				}
				if (source is FileInfo)
				{
					return ((FileInfo)source).FullName;
				}
			}
			if (returnType.IsEnum)
			{
				if (source is string flagNames)
				{
					string[] names = flagNames.Split(' ', ',');
					StringBuilder normalized = new StringBuilder(flagNames.Length);
					for (int i = 0; i < names.Length; i++)
					{
						string flagName = names[i].Trim();
						if (flagName.Length != 0)
						{
							if (normalized.Length > 0)
							{
								normalized.Append(',');
							}
							normalized.Append(flagName);
						}
					}
					return Enum.Parse(returnType, normalized.ToString(), ignoreCase: true);
				}
				return Enum.ToObject(returnType, source);
			}
			if (unsupported)
			{
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", description, GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1);
			}
			return Convert.ChangeType(source, returnType, CultureInfo.InvariantCulture);
		}
		catch (ExpressionParseException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", description, GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1, ex);
		}
	}

	protected string GetSimpleTypeName(Type t)
	{
		if (t == typeof(int))
		{
			return "int";
		}
		if (t == typeof(long))
		{
			return "long";
		}
		if (t == typeof(double))
		{
			return "double";
		}
		if (t == typeof(string))
		{
			return "string";
		}
		if (t == typeof(bool))
		{
			return "bool";
		}
		if (t == typeof(DateTime))
		{
			return "datetime";
		}
		if (t == typeof(TimeSpan))
		{
			return "timespan";
		}
		return t.FullName!;
	}

	protected abstract object? EvaluateFunction(MethodInfo method, object?[] args);

	protected abstract object EvaluateProperty(string propertyName);

	protected virtual object UnexpectedToken()
	{
		throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Unexpected token '{0}'.", _tokenizer.CurrentToken), _tokenizer.CurrentPosition);
	}
}
