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
		IComparable left = (IComparable)base.Operands[0].ConvertValue();
		if ((base.Operands[0].Type == TokenType.Symbol || base.Operands[0].Type == TokenType.ConstSymbol) && base.Operands[0].Value.Equals(left))
		{
			left = (IComparable)context.GetSymbolValue(base.Operands[0].Value, result.Errors);
		}
		IComparable right = (IComparable)base.Operands[2].ConvertValue();
		if ((base.Operands[2].Type == TokenType.Symbol || base.Operands[2].Type == TokenType.ConstSymbol) && base.Operands[2].Value.Equals(right))
		{
			right = (IComparable)context.GetSymbolValue(base.Operands[2].Value, result.Errors);
		}
		if (left != null && right != null && left.GetType() != right.GetType())
		{
			right = (IComparable)Convert.ChangeType(right, left.GetType());
		}
		bool conditionMet = false;
		switch (base.Operands[1].Value.Trim())
		{
		case "==":
			conditionMet = object.Equals(left, right);
			break;
		case "~=":
			if (left != null && right != null)
			{
				conditionMet = Regex.Match(left.ToString(), right.ToString())?.Success ?? false;
			}
			break;
		case "!=":
			conditionMet = !object.Equals(left, right);
			break;
		case ">":
			conditionMet = left != null && left.CompareTo(right) > 0;
			break;
		case "<":
			conditionMet = left != null && left.CompareTo(right) < 0;
			break;
		case ">=":
			conditionMet = left != null && left.CompareTo(right) >= 0;
			break;
		case "<=":
			conditionMet = left != null && left.CompareTo(right) <= 0;
			break;
		}
		if (conditionMet)
		{
			result.SwitchToContext = this;
		}
	}
}
