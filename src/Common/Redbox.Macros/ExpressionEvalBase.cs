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

	private ExpressionTokenizer _tokenizer;

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
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object obj = ParseBooleanAnd();
		EvalMode evalMode = _evalMode;
		try
		{
			while (_tokenizer.IsKeyword("or"))
			{
				bool flag = true;
				if (!SyntaxCheckOnly())
				{
					flag = (bool)SafeConvert(typeof(bool), obj, "the left hand side of the 'or' operator", currentPosition, _tokenizer.CurrentPosition);
					if (flag)
					{
						_evalMode = EvalMode.ParseOnly;
					}
				}
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
				object source = ParseBooleanAnd();
				ExpressionTokenizer.Position currentPosition3 = _tokenizer.CurrentPosition;
				if (!SyntaxCheckOnly())
				{
					bool flag2 = (bool)SafeConvert(typeof(bool), source, "the right hand side of the 'or' operator", currentPosition2, currentPosition3);
					obj = flag | flag2;
				}
			}
			return obj;
		}
		finally
		{
			_evalMode = evalMode;
		}
	}

	private object ParseBooleanAnd()
	{
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object obj = ParseRelationalExpression();
		EvalMode evalMode = _evalMode;
		try
		{
			while (_tokenizer.IsKeyword("and"))
			{
				bool flag = true;
				if (!SyntaxCheckOnly())
				{
					flag = (bool)SafeConvert(typeof(bool), obj, "the left hand side of the 'and' operator", currentPosition, _tokenizer.CurrentPosition);
					if (!flag)
					{
						_evalMode = EvalMode.ParseOnly;
					}
				}
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
				object source = ParseRelationalExpression();
				ExpressionTokenizer.Position currentPosition3 = _tokenizer.CurrentPosition;
				if (!SyntaxCheckOnly())
				{
					bool flag2 = (bool)SafeConvert(typeof(bool), source, "the right hand side of the 'and' operator", currentPosition2, currentPosition3);
					obj = flag & flag2;
				}
			}
			return obj;
		}
		finally
		{
			_evalMode = evalMode;
		}
	}

	private object ParseRelationalExpression()
	{
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object obj = ParseAddSubtract();
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.EQ || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.NE || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LT || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GT || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LE || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.GE)
		{
			ExpressionTokenizer.TokenType currentToken = _tokenizer.CurrentToken;
			_tokenizer.GetNextToken();
			object obj2 = ParseAddSubtract();
			ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				return null;
			}
			switch (currentToken)
			{
			case ExpressionTokenizer.TokenType.EQ:
				if (obj is string && obj2 is string)
				{
					return obj.Equals(obj2);
				}
				if (obj is bool && obj2 is bool)
				{
					return obj.Equals(obj2);
				}
				if (obj is int && obj2 is int)
				{
					return obj.Equals(obj2);
				}
				if (obj is int && obj2 is long)
				{
					return Convert.ToInt64(obj).Equals(obj2);
				}
				if (obj is int && obj2 is double)
				{
					return Convert.ToDouble(obj).Equals(obj2);
				}
				if (obj is long && obj2 is long)
				{
					return obj.Equals(obj2);
				}
				if (obj is long && obj2 is int)
				{
					return obj.Equals(Convert.ToInt64(obj2));
				}
				if (obj is long && obj2 is double)
				{
					return Convert.ToDouble(obj).Equals(obj2);
				}
				if (obj is double && obj2 is double)
				{
					return obj.Equals(obj2);
				}
				if (obj is double && obj2 is int)
				{
					return obj.Equals(Convert.ToDouble(obj2));
				}
				if (obj is double && obj2 is long)
				{
					return obj.Equals(Convert.ToDouble(obj2));
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return obj.Equals(obj2);
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return obj.Equals(obj2);
				}
				if (obj is Version && obj2 is Version)
				{
					return obj.Equals(obj2);
				}
				if (obj.GetType().IsEnum)
				{
					if (obj2 is string)
					{
						return obj.Equals(Enum.Parse(obj.GetType(), (string)obj2, ignoreCase: false));
					}
					return obj.Equals(Enum.ToObject(obj.GetType(), obj2));
				}
				if (obj2.GetType().IsEnum)
				{
					if (obj is string)
					{
						return obj2.Equals(Enum.Parse(obj2.GetType(), (string)obj, ignoreCase: false));
					}
					return obj2.Equals(Enum.ToObject(obj2.GetType(), obj));
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1038"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			case ExpressionTokenizer.TokenType.NE:
				if (obj is string && obj2 is string)
				{
					return !obj.Equals(obj2);
				}
				if (obj is bool && obj2 is bool)
				{
					return !obj.Equals(obj2);
				}
				if (obj is int && obj2 is int)
				{
					return !obj.Equals(obj2);
				}
				if (obj is int && obj2 is long)
				{
					return !Convert.ToInt64(obj).Equals(obj2);
				}
				if (obj is int && obj2 is double)
				{
					return !Convert.ToDouble(obj).Equals(obj2);
				}
				if (obj is long && obj2 is long)
				{
					return !obj.Equals(obj2);
				}
				if (obj is long && obj2 is int)
				{
					return !obj.Equals(Convert.ToInt64(obj2));
				}
				if (obj is long && obj2 is double)
				{
					return !Convert.ToDouble(obj).Equals(obj2);
				}
				if (obj is double && obj2 is double)
				{
					return !obj.Equals(obj2);
				}
				if (obj is double && obj2 is int)
				{
					return !obj.Equals(Convert.ToDouble(obj2));
				}
				if (obj is double && obj2 is long)
				{
					return !obj.Equals(Convert.ToDouble(obj2));
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return !obj.Equals(obj2);
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return !obj.Equals(obj2);
				}
				if (obj is Version && obj2 is Version)
				{
					return !obj.Equals(obj2);
				}
				if (obj.GetType().IsEnum)
				{
					if (obj2 is string)
					{
						return !obj.Equals(Enum.Parse(obj.GetType(), (string)obj2, ignoreCase: false));
					}
					return !obj.Equals(Enum.ToObject(obj.GetType(), obj2));
				}
				if (obj2.GetType().IsEnum)
				{
					if (obj is string)
					{
						return !obj2.Equals(Enum.Parse(obj2.GetType(), (string)obj, ignoreCase: false));
					}
					return !obj2.Equals(Enum.ToObject(obj2.GetType(), obj));
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1042"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			case ExpressionTokenizer.TokenType.LT:
				if (obj is string && obj2 is string)
				{
					return string.Compare((string)obj, (string)obj2, ignoreCase: false, CultureInfo.InvariantCulture) < 0;
				}
				if (obj is bool && obj2 is bool)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is int && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is int && obj2 is long)
				{
					return ((IComparable)Convert.ToInt64(obj)).CompareTo(obj2) < 0;
				}
				if (obj is int && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) < 0;
				}
				if (obj is long && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is long && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToInt64(obj2)) < 0;
				}
				if (obj is long && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) < 0;
				}
				if (obj is double && obj2 is double)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is double && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) < 0;
				}
				if (obj is double && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) < 0;
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				if (obj is Version && obj2 is Version)
				{
					return ((IComparable)obj).CompareTo(obj2) < 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1051"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			case ExpressionTokenizer.TokenType.GT:
				if (obj is string && obj2 is string)
				{
					return string.Compare((string)obj, (string)obj2, ignoreCase: false, CultureInfo.InvariantCulture) > 0;
				}
				if (obj is bool && obj2 is bool)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is int && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is int && obj2 is long)
				{
					return ((IComparable)Convert.ToInt64(obj)).CompareTo(obj2) > 0;
				}
				if (obj is int && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) > 0;
				}
				if (obj is long && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is long && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToInt64(obj2)) > 0;
				}
				if (obj is long && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) > 0;
				}
				if (obj is double && obj2 is double)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is double && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) > 0;
				}
				if (obj is double && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) > 0;
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				if (obj is Version && obj2 is Version)
				{
					return ((IComparable)obj).CompareTo(obj2) > 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1037"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			case ExpressionTokenizer.TokenType.LE:
				if (obj is string && obj2 is string)
				{
					return string.Compare((string)obj, (string)obj2, ignoreCase: false, CultureInfo.InvariantCulture) <= 0;
				}
				if (obj is bool && obj2 is bool)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is int && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is int && obj2 is long)
				{
					return ((IComparable)Convert.ToInt64(obj)).CompareTo(obj2) <= 0;
				}
				if (obj is int && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) <= 0;
				}
				if (obj is long && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is long && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToInt64(obj2)) <= 0;
				}
				if (obj is long && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) <= 0;
				}
				if (obj is double && obj2 is double)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is double && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) <= 0;
				}
				if (obj is double && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) <= 0;
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				if (obj is Version && obj2 is Version)
				{
					return ((IComparable)obj).CompareTo(obj2) <= 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1049"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			case ExpressionTokenizer.TokenType.GE:
				if (obj is string && obj2 is string)
				{
					return string.Compare((string)obj, (string)obj2, ignoreCase: false, CultureInfo.InvariantCulture) >= 0;
				}
				if (obj is bool && obj2 is bool)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is int && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is int && obj2 is long)
				{
					return ((IComparable)Convert.ToInt64(obj)).CompareTo(obj2) >= 0;
				}
				if (obj is int && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) >= 0;
				}
				if (obj is long && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is long && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToInt64(obj2)) >= 0;
				}
				if (obj is long && obj2 is double)
				{
					return ((IComparable)Convert.ToDouble(obj)).CompareTo(obj2) >= 0;
				}
				if (obj is double && obj2 is double)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is double && obj2 is int)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) >= 0;
				}
				if (obj is double && obj2 is long)
				{
					return ((IComparable)obj).CompareTo(Convert.ToDouble(obj2)) >= 0;
				}
				if (obj is DateTime && obj2 is DateTime)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				if (obj is Version && obj2 is Version)
				{
					return ((IComparable)obj).CompareTo(obj2) >= 0;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1050"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			}
		}
		return obj;
	}

	private object ParseAddSubtract()
	{
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object obj = ParseMulDiv();
		while (true)
		{
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Plus)
			{
				_tokenizer.GetNextToken();
				object obj2 = ParseMulDiv();
				ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (obj is string && obj2 is string)
				{
					obj = (string)obj + (string)obj2;
					continue;
				}
				if (obj is int && obj2 is int)
				{
					obj = (int)obj + (int)obj2;
					continue;
				}
				if (obj is int && obj2 is long)
				{
					obj = (int)obj + (long)obj2;
					continue;
				}
				if (obj is int && obj2 is double)
				{
					obj = (double)(int)obj + (double)obj2;
					continue;
				}
				if (obj is long && obj2 is long)
				{
					obj = (long)obj + (long)obj2;
					continue;
				}
				if (obj is long && obj2 is int)
				{
					obj = (long)obj + (int)obj2;
					continue;
				}
				if (obj is long && obj2 is double)
				{
					obj = (double)(long)obj + (double)obj2;
					continue;
				}
				if (obj is double && obj2 is double)
				{
					obj = (double)obj + (double)obj2;
					continue;
				}
				if (obj is double && obj2 is int)
				{
					obj = (double)obj + (double)(int)obj2;
					continue;
				}
				if (obj is double && obj2 is long)
				{
					obj = (double)obj + (double)(long)obj2;
					continue;
				}
				if (obj is DateTime && obj2 is TimeSpan)
				{
					obj = (DateTime)obj + (TimeSpan)obj2;
					continue;
				}
				if (obj is TimeSpan && obj2 is TimeSpan)
				{
					obj = (TimeSpan)obj + (TimeSpan)obj2;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1041"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			}
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Minus)
			{
				break;
			}
			_tokenizer.GetNextToken();
			object obj3 = ParseMulDiv();
			ExpressionTokenizer.Position currentPosition3 = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				continue;
			}
			if (obj is int && obj3 is int)
			{
				obj = (int)obj - (int)obj3;
				continue;
			}
			if (obj is int && obj3 is long)
			{
				obj = (int)obj - (long)obj3;
				continue;
			}
			if (obj is int && obj3 is double)
			{
				obj = (double)(int)obj - (double)obj3;
				continue;
			}
			if (obj is long && obj3 is long)
			{
				obj = (long)obj - (long)obj3;
				continue;
			}
			if (obj is long && obj3 is int)
			{
				obj = (long)obj - (int)obj3;
				continue;
			}
			if (obj is long && obj3 is double)
			{
				obj = (double)(long)obj - (double)obj3;
				continue;
			}
			if (obj is double && obj3 is double)
			{
				obj = (double)obj - (double)obj3;
				continue;
			}
			if (obj is double && obj3 is int)
			{
				obj = (double)obj - (double)(int)obj3;
				continue;
			}
			if (obj is double && obj3 is long)
			{
				obj = (double)obj - (double)(long)obj3;
				continue;
			}
			if (obj is DateTime && obj3 is DateTime)
			{
				obj = (DateTime)obj - (DateTime)obj3;
				continue;
			}
			if (obj is DateTime && obj3 is TimeSpan)
			{
				obj = (DateTime)obj - (TimeSpan)obj3;
				continue;
			}
			if (obj is TimeSpan && obj3 is TimeSpan)
			{
				obj = (TimeSpan)obj - (TimeSpan)obj3;
				continue;
			}
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1048"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj3.GetType())), currentPosition, currentPosition3);
		}
		return obj;
	}

	private object ParseMulDiv()
	{
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object obj = ParseValue();
		while (true)
		{
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Mul)
			{
				_tokenizer.GetNextToken();
				object obj2 = ParseValue();
				ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (obj is int && obj2 is int)
				{
					obj = (int)obj * (int)obj2;
					continue;
				}
				if (obj is int && obj2 is long)
				{
					obj = (int)obj * (long)obj2;
					continue;
				}
				if (obj is int && obj2 is double)
				{
					obj = (double)(int)obj * (double)obj2;
					continue;
				}
				if (obj is long && obj2 is long)
				{
					obj = (long)obj * (long)obj2;
					continue;
				}
				if (obj is long && obj2 is int)
				{
					obj = (long)obj * (int)obj2;
					continue;
				}
				if (obj is long && obj2 is double)
				{
					obj = (double)(long)obj * (double)obj2;
					continue;
				}
				if (obj is double && obj2 is double)
				{
					obj = (double)obj * (double)obj2;
					continue;
				}
				if (obj is double && obj2 is int)
				{
					obj = (double)obj * (double)(int)obj2;
					continue;
				}
				if (obj is double && obj2 is long)
				{
					obj = (double)obj * (double)(long)obj2;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1036"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj2.GetType())), currentPosition, currentPosition2);
			}
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Div)
			{
				_tokenizer.GetNextToken();
				ExpressionTokenizer.Position currentPosition3 = _tokenizer.CurrentPosition;
				object obj3 = ParseValue();
				ExpressionTokenizer.Position currentPosition4 = _tokenizer.CurrentPosition;
				if (SyntaxCheckOnly())
				{
					continue;
				}
				if (obj is int && obj3 is int)
				{
					if ((int)obj3 == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (int)obj / (int)obj3;
					continue;
				}
				if (obj is int && obj3 is long)
				{
					if ((long)obj3 == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (int)obj / (long)obj3;
					continue;
				}
				if (obj is int && obj3 is double)
				{
					if ((double)obj3 == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (double)(int)obj / (double)obj3;
					continue;
				}
				if (obj is long && obj3 is long)
				{
					if ((long)obj3 == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (long)obj / (long)obj3;
					continue;
				}
				if (obj is long && obj3 is int)
				{
					if ((int)obj3 == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (long)obj / (int)obj3;
					continue;
				}
				if (obj is long && obj3 is double)
				{
					if ((double)obj3 == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (double)(long)obj / (double)obj3;
					continue;
				}
				if (obj is double && obj3 is double)
				{
					if ((double)obj3 == 0.0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (double)obj / (double)obj3;
					continue;
				}
				if (obj is double && obj3 is int)
				{
					if ((int)obj3 == 0)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (double)obj / (double)(int)obj3;
					continue;
				}
				if (obj is double && obj3 is long)
				{
					if ((long)obj3 == 0L)
					{
						throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition3, currentPosition4);
					}
					obj = (double)obj / (double)(long)obj3;
					continue;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1039"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj3.GetType())), currentPosition, currentPosition4);
			}
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Mod)
			{
				break;
			}
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position currentPosition5 = _tokenizer.CurrentPosition;
			object obj4 = ParseValue();
			ExpressionTokenizer.Position currentPosition6 = _tokenizer.CurrentPosition;
			if (SyntaxCheckOnly())
			{
				continue;
			}
			if (obj is int && obj4 is int)
			{
				if ((int)obj4 == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (int)obj % (int)obj4;
				continue;
			}
			if (obj is int && obj4 is long)
			{
				if ((long)obj4 == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (int)obj % (long)obj4;
				continue;
			}
			if (obj is int && obj4 is double)
			{
				if ((double)obj4 == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (double)(int)obj % (double)obj4;
				continue;
			}
			if (obj is long && obj4 is long)
			{
				if ((long)obj4 == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (long)obj % (long)obj4;
				continue;
			}
			if (obj is long && obj4 is int)
			{
				if ((int)obj4 == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (long)obj % (int)obj4;
				continue;
			}
			if (obj is long && obj4 is double)
			{
				if ((double)obj4 == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (double)(long)obj % (double)obj4;
				continue;
			}
			if (obj is double && obj4 is double)
			{
				if ((double)obj4 == 0.0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (double)obj % (double)obj4;
				continue;
			}
			if (obj is double && obj4 is int)
			{
				if ((int)obj4 == 0)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (double)obj % (double)(int)obj4;
				continue;
			}
			if (obj is double && obj4 is long)
			{
				if ((long)obj4 == 0L)
				{
					throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1043")), currentPosition5, currentPosition6);
				}
				obj = (double)obj % (double)(long)obj4;
				continue;
			}
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1047"), GetSimpleTypeName(obj.GetType()), GetSimpleTypeName(obj4.GetType())), currentPosition, currentPosition6);
		}
		return obj;
	}

	private object ParseConditional()
	{
		_tokenizer.GetNextToken();
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
		{
			throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
		}
		_tokenizer.GetNextToken();
		ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
		object source = ParseExpression();
		ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
		bool flag = false;
		if (!SyntaxCheckOnly())
		{
			flag = (bool)SafeConvert(typeof(bool), source, "the conditional expression", currentPosition, currentPosition2);
		}
		if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
		{
			throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
		}
		_tokenizer.GetNextToken();
		EvalMode evalMode = _evalMode;
		try
		{
			if (!flag)
			{
				_evalMode = EvalMode.ParseOnly;
			}
			else
			{
				_evalMode = evalMode;
			}
			object obj = ParseExpression();
			_evalMode = evalMode;
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Comma)
			{
				throw BuildParseError("',' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			if (flag)
			{
				_evalMode = EvalMode.ParseOnly;
			}
			else
			{
				_evalMode = evalMode;
			}
			object obj2 = ParseExpression();
			_evalMode = evalMode;
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
			{
				throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			return flag ? obj : obj2;
		}
		finally
		{
			_evalMode = evalMode;
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
			string tokenText2 = _tokenizer.TokenText;
			ExpressionTokenizer.Position currentPosition = _tokenizer.CurrentPosition;
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position p = new ExpressionTokenizer.Position(_tokenizer.CurrentPosition.CharIndex - 1);
			if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot)
			{
				tokenText2 += ".";
				_tokenizer.GetNextToken();
				if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Number)
				{
					throw BuildParseError("Fractional part expected.", _tokenizer.CurrentPosition);
				}
				tokenText2 += _tokenizer.TokenText;
				_tokenizer.GetNextToken();
				p = _tokenizer.CurrentPosition;
				try
				{
					return double.Parse(tokenText2, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					throw BuildParseError("Value was either too large or too small for type 'double'.", currentPosition, p);
				}
			}
			try
			{
				return int.Parse(tokenText2, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				try
				{
					return long.Parse(tokenText2, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					throw BuildParseError("Value was either too large or too small for type 'long'.", currentPosition, p);
				}
			}
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus)
		{
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position currentPosition2 = _tokenizer.CurrentPosition;
			object obj = ParseValue();
			ExpressionTokenizer.Position currentPosition3 = _tokenizer.CurrentPosition;
			if (!SyntaxCheckOnly())
			{
				if (obj is int)
				{
					return -(int)obj;
				}
				if (obj is long)
				{
					return -(long)obj;
				}
				if (obj is double)
				{
					return 0.0 - (double)obj;
				}
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1040"), GetSimpleTypeName(obj.GetType())), currentPosition2, currentPosition3);
			}
			return null;
		}
		if (_tokenizer.IsKeyword("not"))
		{
			_tokenizer.GetNextToken();
			ExpressionTokenizer.Position currentPosition4 = _tokenizer.CurrentPosition;
			object source = ParseValue();
			ExpressionTokenizer.Position currentPosition5 = _tokenizer.CurrentPosition;
			if (!SyntaxCheckOnly())
			{
				return !(bool)SafeConvert(typeof(bool), source, "the argument of 'not' operator", currentPosition4, currentPosition5);
			}
			return null;
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftParen)
		{
			_tokenizer.GetNextToken();
			object result = ParseExpression();
			if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen)
			{
				throw BuildParseError("')' expected.", _tokenizer.CurrentPosition);
			}
			_tokenizer.GetNextToken();
			return result;
		}
		if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword)
		{
			ExpressionTokenizer.Position currentPosition6 = _tokenizer.CurrentPosition;
			string text = _tokenizer.TokenText;
			switch (text)
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
				ArrayList arrayList = new ArrayList();
				bool flag = false;
				if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.DoubleColon)
				{
					flag = true;
					text += "::";
					_tokenizer.GetNextToken();
					if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.Keyword)
					{
						throw BuildParseError("Function name expected.", currentPosition6, _tokenizer.CurrentPosition);
					}
					text += _tokenizer.TokenText;
					_tokenizer.GetNextToken();
				}
				else
				{
					while (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dot || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Minus || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Keyword || _tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Number)
					{
						text += _tokenizer.TokenText;
						_tokenizer.GetNextToken();
					}
				}
				_tokenizer.IgnoreWhitespace = true;
				if (_tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Whitespace)
				{
					_tokenizer.GetNextToken();
				}
				MethodInfo methodInfo = null;
				if (flag)
				{
					if (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.LeftParen)
					{
						throw BuildParseError("'(' expected.", _tokenizer.CurrentPosition);
					}
					_tokenizer.GetNextToken();
					int num = 0;
					while (_tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightParen && _tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
					{
						ExpressionTokenizer.Position currentPosition7 = _tokenizer.CurrentPosition;
						object value = ParseExpression();
						ExpressionTokenizer.Position currentPosition8 = _tokenizer.CurrentPosition;
						arrayList.Add(new FunctionArgument(text, num, value, currentPosition7, currentPosition8));
						num++;
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
						FunctionArgument[] array = new FunctionArgument[arrayList.Count];
						arrayList.CopyTo(0, array, 0, arrayList.Count);
						try
						{
							methodInfo = FunctionFactory.LookupFunction(text, array);
						}
						catch (EvaluatorException ex4)
						{
							throw BuildParseError(ex4.Message, currentPosition6, _tokenizer.CurrentPosition);
						}
						ParameterInfo[] parameters = methodInfo.GetParameters();
						arrayList.Clear();
						for (int i = 0; i < array.Length; i++)
						{
							FunctionArgument functionArgument = array[i];
							ParameterInfo parameterInfo = parameters[i];
							object value2 = SafeConvert(parameterInfo.ParameterType, functionArgument.Value, string.Format(CultureInfo.InvariantCulture, "argument {1} ({0}) of {2}()", parameterInfo.Name, functionArgument.Index, functionArgument.Name), functionArgument.BeforeArgument, functionArgument.AfterArgument);
							arrayList.Add(value2);
						}
					}
				}
				try
				{
					if (!SyntaxCheckOnly())
					{
						if (flag)
						{
							return EvaluateFunction(methodInfo, arrayList.ToArray());
						}
						return EvaluateProperty(text);
					}
					return null;
				}
				catch (Exception ex5)
				{
					if (flag)
					{
						throw BuildParseError("Function call failed.", currentPosition6, _tokenizer.CurrentPosition, ex5);
					}
					throw BuildParseError("Property evaluation failed.", currentPosition6, _tokenizer.CurrentPosition, ex5);
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
			bool flag = false;
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
				flag = true;
			}
			if (returnType == typeof(bool) && !(source is string) && !(source is bool))
			{
				flag = true;
			}
			if (source is DateTime && returnType != typeof(string) && returnType != typeof(DateTime))
			{
				flag = true;
			}
			if (returnType == typeof(DateTime) && !(source is DateTime) && !(source is string))
			{
				flag = true;
			}
			if (source is TimeSpan && returnType != typeof(TimeSpan))
			{
				flag = true;
			}
			if (returnType == typeof(TimeSpan) && !(source is TimeSpan))
			{
				flag = true;
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
				if (source is string text)
				{
					string[] array = text.Split(' ', ',');
					StringBuilder stringBuilder = new StringBuilder(text.Length);
					for (int i = 0; i < array.Length; i++)
					{
						string text2 = array[i].Trim();
						if (text2.Length != 0)
						{
							if (stringBuilder.Length > 0)
							{
								stringBuilder.Append(',');
							}
							stringBuilder.Append(text2);
						}
					}
					return Enum.Parse(returnType, stringBuilder.ToString(), ignoreCase: true);
				}
				return Enum.ToObject(returnType, source);
			}
			if (flag)
			{
				throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", description, GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1);
			}
			return Convert.ChangeType(source, returnType, CultureInfo.InvariantCulture);
		}
		catch (ExpressionParseException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Cannot convert {0} to '{1}' (actual type was '{2}').", description, GetSimpleTypeName(returnType), GetSimpleTypeName(source.GetType())), p0, p1, ex2);
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
		return t.FullName;
	}

	protected abstract object EvaluateFunction(MethodInfo method, object[] args);

	protected abstract object EvaluateProperty(string propertyName);

	protected virtual object UnexpectedToken()
	{
		throw BuildParseError(string.Format(CultureInfo.InvariantCulture, "Unexpected token '{0}'.", _tokenizer.CurrentToken), _tokenizer.CurrentPosition);
	}
}
