using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Macros;

[Serializable]
public class PropertyDictionary : DictionaryBase
{
	internal const string Visiting = "VISITING";

	internal const string Visited = "VISITED";

	private readonly StringCollection m_dynamicProperties = new StringCollection();

	private readonly StringCollection m_readOnlyProperties = new StringCollection();

	public virtual string this[string name]
	{
		get
		{
			string text = (string)base.Dictionary[name];
			CheckDeprecation(name);
			if (!IsDynamicProperty(name))
			{
				return text;
			}
			return ExpandProperties(text, Location.UnknownLocation);
		}
		set
		{
			base.Dictionary[name] = value;
		}
	}

	public ICollection Keys => base.Dictionary.Keys;

	public ICollection Values => base.Dictionary.Values;

	public bool TreatUndefinedPropertyAsError { get; set; }

	public virtual void AddReadOnly(string name, string value)
	{
		if (!IsReadOnlyProperty(name))
		{
			base.Dictionary.Add(name, value);
			m_readOnlyProperties.Add(name);
		}
	}

	public virtual void MarkDynamic(string name)
	{
		if (!IsDynamicProperty(name))
		{
			if (!Contains(name))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1067")));
			}
			m_dynamicProperties.Add(name);
		}
	}

	public virtual void Add(string name, string value)
	{
		base.Dictionary.Add(name, value);
	}

	public virtual bool IsReadOnlyProperty(string name)
	{
		return m_readOnlyProperties.Contains(name);
	}

	public virtual bool IsDynamicProperty(string name)
	{
		return m_dynamicProperties.Contains(name);
	}

	public virtual void Inherit(PropertyDictionary source, StringCollection excludes)
	{
		foreach (DictionaryEntry item in source.Dictionary)
		{
			string text = (string)item.Key;
			if ((excludes == null || !excludes.Contains(text)) && !IsReadOnlyProperty(text))
			{
				ValidatePropertyName(text, Location.UnknownLocation);
				base.Dictionary[text] = item.Value;
				if (source.IsReadOnlyProperty(text))
				{
					m_readOnlyProperties.Add(text);
				}
				if (source.IsDynamicProperty(text) && !IsDynamicProperty(text))
				{
					m_dynamicProperties.Add(text);
				}
			}
		}
	}

	public string ExpandProperties(string input, Location location)
	{
		Hashtable state = new Hashtable();
		Stack visiting = new Stack();
		return ExpandProperties(input, location, state, visiting);
	}

	public bool Contains(string name)
	{
		return base.Dictionary.Contains(name);
	}

	public void Remove(string name)
	{
		base.Dictionary.Remove(name);
	}

	internal string GetPropertyValue(string propertyName)
	{
		CheckDeprecation(propertyName);
		return (string)base.Dictionary[propertyName];
	}

	internal string ExpandProperties(string input, Location location, Hashtable state, Stack visiting)
	{
		return EvaluateEmbeddedExpressions(input, location, state, visiting);
	}

	protected override void OnClear()
	{
		m_readOnlyProperties.Clear();
		m_dynamicProperties.Clear();
	}

	protected override void OnSet(object key, object oldValue, object newValue)
	{
		string text = (string)key;
		if (IsReadOnlyProperty(text))
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1068"), text), Location.UnknownLocation);
		}
		base.OnSet(key, oldValue, newValue);
	}

	protected override void OnInsert(object key, object value)
	{
		string text = (string)key;
		if (Contains(text))
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1065"), text), Location.UnknownLocation);
		}
	}

	protected override void OnRemove(object key, object value)
	{
		if (key is string value2 && m_readOnlyProperties.Contains(value2))
		{
			m_readOnlyProperties.Remove(value2);
		}
	}

	protected override void OnValidate(object key, object value)
	{
		ValidatePropertyName((key as string) ?? throw new ArgumentException("Property name must be a string.", "key"), Location.UnknownLocation);
		ValidatePropertyValue(value, Location.UnknownLocation);
		base.OnValidate(key, value);
	}

	private string EvaluateEmbeddedExpressions(string input, Location location, Hashtable state, Stack visiting)
	{
		if (input == null)
		{
			return null;
		}
		if (input.IndexOf('$') < 0)
		{
			return input;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(input.Length);
			ExpressionTokenizer expressionTokenizer = new ExpressionTokenizer();
			ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator(this, state, visiting);
			expressionTokenizer.IgnoreWhitespace = false;
			expressionTokenizer.SingleCharacterMode = true;
			expressionTokenizer.InitTokenizer(input);
			while (expressionTokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
			{
				if (expressionTokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dollar)
				{
					expressionTokenizer.GetNextToken();
					if (expressionTokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftCurlyBrace)
					{
						expressionTokenizer.IgnoreWhitespace = true;
						expressionTokenizer.SingleCharacterMode = false;
						expressionTokenizer.GetNextToken();
						string value = Convert.ToString(expressionEvaluator.Evaluate(expressionTokenizer), CultureInfo.InvariantCulture);
						stringBuilder.Append(value);
						expressionTokenizer.IgnoreWhitespace = false;
						if (expressionTokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightCurlyBrace)
						{
							throw new ExpressionParseException("'}' expected", expressionTokenizer.CurrentPosition.CharIndex);
						}
						expressionTokenizer.SingleCharacterMode = true;
						expressionTokenizer.GetNextToken();
					}
					else
					{
						stringBuilder.Append('$');
						if (expressionTokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
						{
							stringBuilder.Append(expressionTokenizer.TokenText);
							expressionTokenizer.GetNextToken();
						}
					}
				}
				else
				{
					stringBuilder.Append(expressionTokenizer.TokenText);
					expressionTokenizer.GetNextToken();
				}
			}
			return stringBuilder.ToString();
		}
		catch (ExpressionParseException ex)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			string text = input;
			text = text.Replace('\n', ' ');
			text = text.Replace('\r', ' ');
			text = text.Replace('\t', ' ');
			stringBuilder2.Append(ex.Message);
			stringBuilder2.Append(Environment.NewLine);
			stringBuilder2.Append("Expression: ");
			stringBuilder2.Append(text);
			int startPos = ex.StartPos;
			int num = ex.EndPos;
			if (startPos != -1 || num != -1)
			{
				stringBuilder2.Append(Environment.NewLine);
				if (num == -1)
				{
					num = startPos + 1;
				}
				for (int i = 0; i < startPos + "Expression: ".Length; i++)
				{
					stringBuilder2.Append(' ');
				}
				for (int j = startPos; j < num; j++)
				{
					stringBuilder2.Append('^');
				}
			}
			throw new EvaluatorException(stringBuilder2.ToString(), location, ex.InnerException);
		}
	}

	private static void CheckDeprecation(string name)
	{
	}

	private static void ValidatePropertyName(string propertyName, Location location)
	{
		if (!Regex.IsMatch(propertyName, "^[_A-Za-z0-9][_A-Za-z0-9\\-:.()]*$"))
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), propertyName), location);
		}
		if (propertyName.EndsWith("-") || propertyName.EndsWith("."))
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), propertyName), location);
		}
	}

	private static void ValidatePropertyValue(object value, Location location)
	{
		if (value != null && !(value is string))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1066"), value.GetType()), "value");
		}
	}

	internal static EvaluatorException CreateCircularException(string end, Stack stack)
	{
		StringBuilder stringBuilder = new StringBuilder("Circular property reference: ");
		stringBuilder.Append(end);
		string text;
		do
		{
			text = (string)stack.Pop();
			stringBuilder.Append(" <- ");
			stringBuilder.Append(text);
		}
		while (!text.Equals(end));
		return new EvaluatorException(stringBuilder.ToString());
	}
}
