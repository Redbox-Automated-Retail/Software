using System;
using System.Text.RegularExpressions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class IfInstruction : Instruction
{
	private static class Constants
	{
		internal static class Operands
		{
			public const string PatternMatch = "~=";

			public const string EqualTo = "==";

			public const string NotEqualTo = "!=";

			public const string GreaterThan = ">";

			public const string LessThan = "<";

			public const string GreaterThanEqualTo = ">=";

			public const string LessThanEqualTo = "<=";
		}
	}

	public override string EndContextMnemonic => "ENDIF";

	public override bool BeginsContext => true;

	public override string Mnemonic => "IF";

	internal override Branch BranchType => Branch.Conditional;

	internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
	{
		if (target.Mnemonic.Equals("ELSEIF") || target.Mnemonic.Equals("ELSE") || target.Mnemonic.Equals("ENDIF"))
		{
			FalseBranchTarget = target.LineNumber;
			return true;
		}
		LogIncorrectBranch(target.Mnemonic, result);
		return false;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 3)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "The IF mnemonic requires a symbol, followed by a relational or equality operator and a comparand.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol && operandTokens[1].Type != TokenType.Operator)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a symbol and relational or equality operator.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IComparable comparable = (IComparable)base.Operands[0].ConvertValue();
		if ((base.Operands[0].Type == TokenType.Symbol || base.Operands[0].Type == TokenType.ConstSymbol) && base.Operands[0].Value.Equals(comparable))
		{
			comparable = (IComparable)context.GetSymbolValue(base.Operands[0].Value, result.Errors);
		}
		IComparable comparable2 = (IComparable)base.Operands[2].ConvertValue();
		if ((base.Operands[2].Type == TokenType.Symbol || base.Operands[2].Type == TokenType.ConstSymbol) && base.Operands[2].Value.Equals(comparable2))
		{
			comparable2 = (IComparable)context.GetSymbolValue(base.Operands[2].Value, result.Errors);
		}
		if (comparable != null && comparable2 != null && comparable.GetType() != comparable2.GetType())
		{
			comparable2 = (IComparable)Convert.ChangeType(comparable2, comparable.GetType());
		}
		bool flag = false;
		switch (base.Operands[1].Value.Trim())
		{
		case "==":
			flag = object.Equals(comparable, comparable2);
			break;
		case "~=":
			if (comparable != null && comparable2 != null)
			{
				flag = Regex.Match(comparable.ToString(), comparable2.ToString())?.Success ?? false;
			}
			break;
		case "!=":
			flag = !object.Equals(comparable, comparable2);
			break;
		case ">":
			flag = comparable != null && comparable.CompareTo(comparable2) > 0;
			break;
		case "<":
			flag = comparable != null && comparable.CompareTo(comparable2) < 0;
			break;
		case ">=":
			flag = comparable != null && comparable.CompareTo(comparable2) >= 0;
			break;
		case "<=":
			flag = comparable != null && comparable.CompareTo(comparable2) <= 0;
			break;
		}
		if (flag)
		{
			result.SwitchToContext = this;
		}
	}
}
