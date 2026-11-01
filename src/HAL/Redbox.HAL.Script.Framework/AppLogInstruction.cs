using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AppLogInstruction : Instruction
{
	private enum OperandArgs
	{
		Operation,
		Name,
		Subdirectory,
		FlushOnWrite
	}

	public override string Mnemonic => "APPLOG";

	protected override bool IsOperandRecognized(string operand)
	{
		return operand.Equals("CONFIGURE");
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!operandTokens[0].Value.Equals("CONFIGURE") && operandTokens[0].Type != TokenType.StringLiteral && operandTokens[0].Type != TokenType.Symbol && operandTokens[0].Type != TokenType.ConstSymbol)
		{
			result.AddInvalidOperandError("Expected a string literal and any number of optional substitution parameters.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands[0].Value.Equals("CONFIGURE"))
		{
			Configure(result, context);
			return;
		}
		string text = base.Operands[0].Value;
		if (base.Operands[0].Type == TokenType.Symbol || base.Operands[0].Type == TokenType.ConstSymbol)
		{
			object symbolValue = context.GetSymbolValue(base.Operands[0].Value, result.Errors);
			if (symbolValue == null || result.Errors.Count > 0)
			{
				return;
			}
			text = symbolValue as string;
			if (text == null)
			{
				result.AddError(ExecutionErrors.InvalidSymbolValue, "The symbol specified for APPLOG is not a string type.");
				return;
			}
		}
		object[] args = ConvertSymbolsToParameterArray(result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string msg = string.Format(text, args);
			context.AppLog?.Write(msg);
		}
	}

	private void Configure(ExecutionResult result, ExecutionContext context)
	{
		ConvertSymbolsToParameterArray(result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return;
		}
		bool logAppends = false;
		KeyValuePair namePair = base.Operands.GetKeyValuePair(OperandArgs.Name.ToString().ToUpper());
		if (namePair != null)
		{
			_ = namePair.Value;
			if (namePair.Type == TokenType.ConstSymbol || namePair.Type == TokenType.Symbol)
			{
				_ = (string)context.GetSymbolValue(namePair.Value, result.Errors);
				if (result.Errors.ContainsError())
				{
					return;
				}
			}
			logAppends = true;
		}
		bool flushOnWrite = true;
		KeyValuePair flushPair = base.Operands.GetKeyValuePair(OperandArgs.FlushOnWrite.ToString().ToUpper());
		if (flushPair != null && bool.TryParse(flushPair.Value, out var parsedFlush))
		{
			flushOnWrite = parsedFlush;
		}
		string subDirectory = string.Empty;
		KeyValuePair subdirectoryPair = base.Operands.GetKeyValuePair(OperandArgs.Subdirectory.ToString().ToUpper());
		if (subdirectoryPair != null)
		{
			string text = subdirectoryPair.Value;
			if (subdirectoryPair.Type == TokenType.ConstSymbol || subdirectoryPair.Type == TokenType.Symbol)
			{
				text = (string)context.GetSymbolValue(subdirectoryPair.Value, result.Errors);
				if (result.Errors.ContainsError())
				{
					return;
				}
			}
			subDirectory = text;
		}
		ApplicationLog.ConfigureLog(context, logAppends, subDirectory, flushOnWrite, null);
	}
}
