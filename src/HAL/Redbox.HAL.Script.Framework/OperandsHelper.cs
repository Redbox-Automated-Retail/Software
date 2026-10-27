using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal struct OperandsHelper : IDisposable
{
	internal const string TimeoutMessage = "TIMEOUT";

	internal const string FailureMessage = "FAILURE";

	internal const string SuccessMessage = "SUCCESS";

	private TokenList Tokens;

	private ErrorList Errors;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}

	internal int? GetNullableNumericOperand(string name, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair(name);
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, context);
		if (Errors.Count <= 0)
		{
			return keyValuePairValue;
		}
		return null;
	}

	internal static object[] ConvertSymbolsToParameterArray(TokenList tl, ErrorList el, ExecutionContext context)
	{
		using OperandsHelper operandsHelper = new OperandsHelper(tl, el);
		return operandsHelper.ConvertSymbolsToParameterArray(context);
	}

	internal object[] ConvertSymbolsToParameterArray(ExecutionContext context)
	{
		List<object> list = new List<object>();
		for (int i = 1; i < Tokens.Count; i++)
		{
			Token token = Tokens[i];
			if (!token.IsKeyValuePair)
			{
				object obj = token.ConvertValue();
				if ((token.Type == TokenType.Symbol || token.Type == TokenType.ConstSymbol) && token.Value.Equals(obj))
				{
					obj = context.GetSymbolValue(token.Value, Errors);
				}
				list.Add(obj);
			}
		}
		return list.ToArray();
	}

	internal bool GetKeyValuePairValue<T>(string key, out T val, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair(key);
		if (keyValuePair == null)
		{
			val = default(T);
			return false;
		}
		val = GetKeyValuePairValue<T>(keyValuePair, context);
		return true;
	}

	internal T GetKeyValuePairValue<T>(string key, ExecutionContext ctx)
	{
		return GetKeyValuePairValue<T>(Tokens.GetKeyValuePair(key), ctx);
	}

	internal T GetKeyValuePairValue<T>(KeyValuePair keyValuePair, ExecutionContext context)
	{
		if (keyValuePair == null)
		{
			return default(T);
		}
		object value = null;
		if (keyValuePair.Type == TokenType.NumericLiteral)
		{
			value = int.Parse(keyValuePair.Value);
		}
		else if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
		{
			object symbolValue = context.GetSymbolValue(keyValuePair.Value, Errors);
			if (symbolValue != null)
			{
				value = symbolValue;
			}
		}
		else
		{
			value = keyValuePair.Value;
		}
		T result = default(T);
		try
		{
			result = ConversionHelper.ChangeType<T>(value);
			return result;
		}
		catch (Exception e)
		{
			Errors.Add(Error.NewError("E002", "Type conversion error.", e));
		}
		return result;
	}

	internal bool GetLocation(ExecutionContext context, out int deck, out int slot)
	{
		return GetLocationInner(context, out deck, out slot);
	}

	internal string GetID(ExecutionContext context)
	{
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair("ID");
		if (keyValuePair == null)
		{
			Errors.Add(Error.NewError("E099", "Missing operand.", "The ID operand is missing."));
			return null;
		}
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, context);
		if (Errors.Count != 0)
		{
			return string.Empty;
		}
		return keyValuePairValue;
	}

	internal int? GetDeckOperand(ExecutionContext context)
	{
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair("DECK");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, context);
		if (Errors.Count > 0)
		{
			return null;
		}
		return keyValuePairValue;
	}

	internal string GetDateRepresentation(DateTime? dateTime)
	{
		if (dateTime.HasValue)
		{
			return dateTime.Value.ToString();
		}
		return "NONE";
	}

	internal int? GetTimeOutValue(ExecutionContext context)
	{
		int? result = null;
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair("TIMEOUT");
		if (keyValuePair == null)
		{
			return null;
		}
		if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
		{
			return (int?)context.GetSymbolValue(keyValuePair.Value, Errors);
		}
		if (keyValuePair.Type == TokenType.NumericLiteral && int.TryParse(keyValuePair.Value, out var result2))
		{
			result = result2;
		}
		return result;
	}

	internal bool GetWaitFlag()
	{
		bool result = false;
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair("WAIT");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			result = result2;
		}
		return result;
	}

	private bool GetLocationInner(ExecutionContext context, out int deck, out int slot)
	{
		deck = -1;
		slot = -1;
		int? deckOperand = GetDeckOperand(context);
		if (!deckOperand.HasValue)
		{
			return false;
		}
		deck = deckOperand.Value;
		KeyValuePair keyValuePair = Tokens.GetKeyValuePair("SLOT");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, context);
		if (Errors.Count > 0)
		{
			return false;
		}
		if (keyValuePairValue.HasValue)
		{
			slot = keyValuePairValue.Value;
		}
		return true;
	}

	internal OperandsHelper(TokenList tl, ErrorList el)
	{
		Tokens = tl;
		Errors = el;
		Disposed = false;
	}
}
