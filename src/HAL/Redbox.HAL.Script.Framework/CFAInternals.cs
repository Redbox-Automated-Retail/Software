using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class CFAInternals
{
	internal virtual List<Instruction> Instructions { get; private set; }

	internal bool Finalized { get; set; }

	protected Dictionary<string, Instruction> Labels { get; private set; }

	protected List<Instruction> UnconditionalBranches { get; private set; }

	internal void Dump(string fileName)
	{
		if (CFA.Debug)
		{
			InternalsDump(fileName);
		}
	}

	internal abstract void AddLabel(string labelName, Instruction instruction, ExecutionResult result);

	internal abstract void Initialize(ExecutionResult result);

	internal abstract void FindReferencedUndefinedVariables(ExecutionResult result);

	internal abstract void ProcessInstruction(Instruction instruction, TokenList symbols, ExecutionResult result);

	internal abstract void ProcessControlBlock(Instruction begin, Instruction end, ExecutionResult r);

	internal abstract void Finalize(ExecutionResult er);

	internal abstract void InternalsDump(string fileName);

	internal abstract void FixupUnconditionalBranches(ExecutionResult r);

	internal abstract void RemoveDeadBlocks(ExecutionResult result);

	internal abstract void AddVariableUse(string name, int line, ExecutionResult result);

	internal abstract void AddVariableDef(string name, int line, ExecutionResult result);

	internal CFAInternals()
	{
		Labels = new Dictionary<string, Instruction>();
		UnconditionalBranches = new List<Instruction>();
		Instructions = new List<Instruction>();
	}

	protected void AddUnconditionalBranch(Instruction instruction)
	{
		UnconditionalBranches.Add(instruction);
	}
}
