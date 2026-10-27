using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class Instruction : ICloneable<Instruction>
{
	public enum Branch
	{
		None,
		Unconditional,
		Conditional
	}

	protected enum ExpectedOperands
	{
		None,
		Optional,
		AtLeastOne
	}

	protected const string FailureMessage = "FAILURE";

	private TokenList m_operands;

	private InstructionList m_instructions;

	private int m_trueBranch = ProgramInstruction.EndLineNumber;

	private int m_falseBranch = ProgramInstruction.InvalidLineNumber;

	private const string ErrorMessageFormat = "The {0} instruction failed (line {1} of script {2}): {3}";

	public Instruction FirstInstruction => Instructions[0];

	public virtual string EndContextMnemonic => null;

	public virtual bool BeginsContext => false;

	public virtual bool IsCooperativeMultitask => false;

	public Instruction ParentContext { get; set; }

	public virtual string LabelName { get; set; }

	public virtual bool EndsContext => false;

	public virtual string Mnemonic => string.Empty;

	public virtual bool ExitsContext => false;

	public TokenList Operands
	{
		get
		{
			if (m_operands == null)
			{
				m_operands = new TokenList();
			}
			return m_operands;
		}
		private set
		{
			m_operands = value;
		}
	}

	public int LineNumber { get; set; }

	public int X { get; internal set; }

	public int Y { get; internal set; }

	protected virtual ExpectedOperands Expected => ExpectedOperands.AtLeastOne;

	protected string SuccessMessage => ErrorCodes.Success.ToString().ToUpper();

	protected string TimeoutMessage => ErrorCodes.Timeout.ToString().ToUpper();

	internal virtual Branch BranchType => Branch.None;

	internal virtual bool IsSynthetic => false;

	internal InstructionList Instructions
	{
		get
		{
			if (m_instructions == null)
			{
				m_instructions = new InstructionList(this);
			}
			return m_instructions;
		}
		set
		{
			m_instructions = value;
		}
	}

	internal bool IsLabel { get; set; }

	internal virtual bool CreatesVariableDef => false;

	internal virtual int TrueBranchTarget
	{
		get
		{
			return m_trueBranch;
		}
		set
		{
			m_trueBranch = value;
		}
	}

	internal virtual int FalseBranchTarget
	{
		get
		{
			return m_falseBranch;
		}
		set
		{
			m_falseBranch = value;
		}
	}

	internal virtual string CounterSymbol { get; set; }

	internal virtual string TargetLabel => string.Empty;

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Instruction instruction))
		{
			return false;
		}
		if (instruction.Mnemonic.ToUpper().Equals(Mnemonic))
		{
			return instruction.LineNumber == LineNumber;
		}
		return false;
	}

	public static bool MnemonicImpliesConstSymbol(string mnemonic)
	{
		return mnemonic.ToUpper().Equals("CONST");
	}

	public static Instruction Parse(TokenList tokens, int lineNumber, ExecutionResult result)
	{
		if (tokens.Count == 0)
		{
			return null;
		}
		Type[] types = typeof(Instruction).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.IsSubclassOf(typeof(Instruction)) && Activator.CreateInstance(type) is Instruction instruction && string.Compare(instruction.Mnemonic, tokens.GetMnemonic().Value, ignoreCase: true) == 0)
			{
				instruction.LineNumber = lineNumber;
				Token label = tokens.GetLabel();
				if (label != null)
				{
					instruction.LabelName = label.Value;
				}
				instruction.OnParse(tokens, result);
				return instruction;
			}
		}
		return null;
	}

	public Instruction GetByLineNumber(int lineNumber)
	{
		return GetByLineNumber(this, lineNumber);
	}

	public void Execute(ExecutionResult result, ExecutionContext context)
	{
		try
		{
			OnExecute(result, context);
		}
		catch (Exception ex)
		{
			string text = string.Format("Program '{0}', Line {1}: An unhandled exception was raised by the execution engine.", (context != null) ? context.Registers.ActiveFrame.ProgramName : "Idle", LineNumber);
			result.Errors.Add(Error.NewError("E001", text, ex.ToString()));
			LogHelper.Instance.Log(text, ex);
		}
	}

	public ProgramInstruction GetProgram()
	{
		Instruction instruction = ParentContext ?? this;
		while (instruction != null && !(instruction is ProgramInstruction))
		{
			instruction = instruction.ParentContext;
		}
		return instruction as ProgramInstruction;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (LabelName != null)
		{
			stringBuilder.Append(LabelName + ":");
		}
		stringBuilder.AppendFormat(" {0,-16}", Mnemonic);
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (Token operand in Operands)
		{
			if (stringBuilder2.Length > 0)
			{
				stringBuilder2.Append(" ");
			}
			if (operand.Type == TokenType.StringLiteral)
			{
				stringBuilder2.AppendFormat("\"{0}\"", operand.Value.ToUpper());
			}
			else
			{
				stringBuilder2.Append(operand.Value.ToUpper());
			}
		}
		if (stringBuilder2.Length > 0)
		{
			stringBuilder.Append(stringBuilder2.ToString());
		}
		return stringBuilder.ToString();
	}

	public Instruction Clone(params object[] parms)
	{
		Instruction instruction = (Instruction)Activator.CreateInstance(GetType());
		instruction.LabelName = LabelName;
		instruction.LineNumber = LineNumber;
		instruction.Operands = Operands.Clone();
		instruction.Instructions = Instructions.Clone(instruction);
		instruction.TrueBranchTarget = TrueBranchTarget;
		instruction.FalseBranchTarget = FalseBranchTarget;
		instruction.CounterSymbol = CounterSymbol;
		return instruction;
	}

	internal bool OperandRecognized(string operand)
	{
		if (operand.Equals("TRUE") || operand.Equals("FALSE"))
		{
			return true;
		}
		return IsOperandRecognized(operand);
	}

	protected void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (Expected == ExpectedOperands.None)
		{
			if (tokensAfterMnemonic.Count > 0)
			{
				result.AddInvalidOperandError($"The {Mnemonic} instruction doesn't expect operands.");
			}
		}
		else if (ExpectedOperands.AtLeastOne == Expected && tokensAfterMnemonic.Count == 0)
		{
			result.AddMissingOperandError($"The {Mnemonic} instruction expects at least one operand.");
		}
		else if (ExpectedOperands.Optional != Expected || tokensAfterMnemonic.Count != 0)
		{
			ParseOperands(tokensAfterMnemonic, result);
			if (!result.Errors.ContainsError())
			{
				Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected virtual void OnExecute(ExecutionResult result, ExecutionContext context)
	{
	}

	protected virtual bool IsOperandRecognized(string op)
	{
		return false;
	}

	protected virtual void ParseOperands(TokenList operands, ExecutionResult result)
	{
	}

	protected void LogIncorrectBranch(string targetMnemonic, ExecutionResult result)
	{
	}

	protected bool CheckAssignment(TokenType type, ExecutionResult result)
	{
		if (type == TokenType.ConstSymbol)
		{
			result.AddError(ExecutionErrors.InvalidAssignment, "Illegal attempt to " + Mnemonic + " value to CONST symbol.");
			return false;
		}
		return true;
	}

	protected bool GetLocation(ExecutionResult result, ExecutionContext context, out ILocation location)
	{
		int deck = -1;
		int slot = -1;
		location = null;
		if (!GetLocation(result, context, out deck, out slot))
		{
			return false;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		location = service.Get(deck, slot);
		return true;
	}

	protected bool GetLocation(ExecutionResult result, ExecutionContext context, out int deck, out int slot)
	{
		deck = -1;
		slot = -1;
		int? deckOperand = GetDeckOperand(result.Errors, context);
		if (!deckOperand.HasValue)
		{
			return false;
		}
		deck = deckOperand.Value;
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("SLOT");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return false;
		}
		if (keyValuePairValue.HasValue)
		{
			slot = keyValuePairValue.Value;
		}
		return true;
	}

	protected string GetID(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("ID");
		if (keyValuePair == null)
		{
			result.Errors.Add(Error.NewError("E099", "Missing operand.", "The ID operand is missing."));
			return null;
		}
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
		if (result.Errors.Count != 0)
		{
			return string.Empty;
		}
		return keyValuePairValue;
	}

	protected void HandleError(IControlResponse response, ExecutionContext context)
	{
		if (response.CommError)
		{
			context.PushTop(response.Diagnostic);
		}
		else
		{
			context.PushTop(response.TimedOut ? TimeoutMessage : SuccessMessage);
		}
	}

	protected int? GetDeckOperand(ErrorList errors, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("DECK");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, errors, context);
		if (errors.Count > 0)
		{
			return null;
		}
		return keyValuePairValue;
	}

	internal object[] ConvertSymbolsToParameterArray(ErrorList errors, ExecutionContext context)
	{
		List<object> list = new List<object>();
		for (int i = 1; i < Operands.Count; i++)
		{
			Token token = Operands[i];
			if (!token.IsKeyValuePair)
			{
				object obj = token.ConvertValue();
				if ((token.Type == TokenType.Symbol || token.Type == TokenType.ConstSymbol) && token.Value.Equals(obj))
				{
					obj = context.GetSymbolValue(token.Value, errors);
				}
				list.Add(obj);
			}
		}
		return list.ToArray();
	}

	internal bool GetKeyValuePairValue<T>(string key, out T val, ErrorList errors, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(key);
		if (keyValuePair == null)
		{
			val = default(T);
			return false;
		}
		val = GetKeyValuePairValue<T>(keyValuePair, errors, context);
		return true;
	}

	internal T GetKeyValuePairValue<T>(KeyValuePair keyValuePair, ErrorList errors, ExecutionContext context)
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
			object symbolValue = context.GetSymbolValue(keyValuePair.Value, errors);
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
		catch (Exception ex)
		{
			AddError("E002", "Type conversion error: " + ex, errors);
		}
		return result;
	}

	internal int? GetNullableNumericOperand(ExecutionResult result, string name, ExecutionContext context)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(name);
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors, context);
		if (result.Errors.Count > 0)
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

	internal int? GetTimeOutValue(ErrorList errors, ExecutionContext context)
	{
		int? result = null;
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(TimeoutMessage);
		if (keyValuePair == null)
		{
			return null;
		}
		if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
		{
			return (int?)context.GetSymbolValue(keyValuePair.Value, errors);
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
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("WAIT");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			result = result2;
		}
		return result;
	}

	internal void AddError(string code, string message, ErrorList errors)
	{
		errors.Add(Error.NewError(code, message, $"Line {LineNumber}: {this}"));
	}

	internal void AddWarning(string code, string message, ErrorList errors)
	{
		errors.Add(Error.NewWarning(code, message, $"Line {LineNumber}: {this}"));
	}

	internal Instruction GetByLineNumber(Instruction instruction, int lineNumber)
	{
		if (instruction.LineNumber == lineNumber)
		{
			return instruction;
		}
		foreach (Instruction instruction2 in instruction.Instructions)
		{
			Instruction byLineNumber = GetByLineNumber(instruction2, lineNumber);
			if (byLineNumber != null)
			{
				return byLineNumber;
			}
		}
		return null;
	}

	internal bool IsSymbolic(Token tok)
	{
		if (tok.Type != TokenType.ConstSymbol)
		{
			return tok.Type == TokenType.Symbol;
		}
		return true;
	}

	internal bool IsBranch()
	{
		if (BranchType != Branch.Unconditional)
		{
			return BranchType == Branch.Conditional;
		}
		return true;
	}

	internal bool IsConditional()
	{
		return BranchType == Branch.Conditional;
	}

	internal virtual bool ValidateConditionalBranch(Instruction branchTarget, ExecutionResult result)
	{
		return true;
	}
}
