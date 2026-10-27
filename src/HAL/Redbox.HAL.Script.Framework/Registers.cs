using System.Collections.Generic;

namespace Redbox.HAL.Script.Framework;

internal sealed class Registers
{
	internal readonly Stack<CallFrame> CallStack = new Stack<CallFrame>();

	internal CallFrame ActiveFrame
	{
		get
		{
			if (CallStack.Count != 0)
			{
				return CallStack.Peek();
			}
			return null;
		}
	}

	internal Instruction PopCallFrame()
	{
		if (CallStack.Count != 0)
		{
			return CallStack.Pop().ParentContext;
		}
		return null;
	}

	internal void PushCallFrame(string programName, Instruction parentContext)
	{
		CallStack.Push(new CallFrame
		{
			ProgramName = programName,
			ParentContext = parentContext,
			X = parentContext.X,
			Y = parentContext.Y
		});
	}

	internal bool JumpToLabel(string labelName)
	{
		while (ActiveFrame != null)
		{
			int num = ActiveFrame.ParentContext.Instructions.FindInstructionIndexAtLabel(labelName);
			if (num != -1)
			{
				ActiveFrame.PC = num - 1;
				return true;
			}
			PopCallFrame();
		}
		return false;
	}

	internal void Reset()
	{
		CallStack.Clear();
	}
}
