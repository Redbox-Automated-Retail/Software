using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class CFAInternalsMock : CFAInternals
{
	internal override void Initialize(ExecutionResult result)
	{
	}

	internal override void AddLabel(string labelName, Instruction instruction, ExecutionResult result)
	{
	}

	internal override void FindReferencedUndefinedVariables(ExecutionResult result)
	{
	}

	internal override void ProcessInstruction(Instruction instruction, TokenList syms, ExecutionResult result)
	{
	}

	internal override void ProcessControlBlock(Instruction begin, Instruction end, ExecutionResult r)
	{
	}

	internal override void Finalize(ExecutionResult er)
	{
		base.Finalized = true;
	}

	internal override void FixupUnconditionalBranches(ExecutionResult r)
	{
	}

	internal override void InternalsDump(string fileName)
	{
	}

	internal override void RemoveDeadBlocks(ExecutionResult result)
	{
	}

	internal override void AddVariableDef(string name, int line, ExecutionResult result)
	{
	}

	internal override void AddVariableUse(string name, int line, ExecutionResult result)
	{
	}
}
