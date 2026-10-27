using System.Collections.Generic;
using Microsoft.Win32;

namespace Redbox.HAL.Script.Framework;

public class CallFrame
{
	public int PC { get; set; }

	public Instruction IP
	{
		get
		{
			if (ParentContext != null && ParentContext.Instructions.Count != 0)
			{
				return ParentContext.Instructions[PC];
			}
			return null;
		}
	}

	public int X { get; internal set; }

	public int Y { get; internal set; }

	public string ProgramName { get; set; }

	public Instruction ParentContext { get; set; }

	public void ResetPC()
	{
		PC = 0;
	}

	public bool FetchNextInstruction()
	{
		PC++;
		if (ParentContext == null)
		{
			return false;
		}
		return PC < ParentContext.Instructions.Count;
	}

	internal static CallFrame Load(RegistryKey parentKey, IDictionary<string, ProgramInstruction> programCache)
	{
		string text = (string)parentKey.GetValue("ProgramName");
		int num = (int)parentKey.GetValue("ParentContext");
		return new CallFrame
		{
			X = (int)parentKey.GetValue("X", 0),
			Y = (int)parentKey.GetValue("Y", 0),
			PC = (int)parentKey.GetValue("PC"),
			ProgramName = text,
			ParentContext = ((num != -1 && programCache.ContainsKey(text)) ? programCache[text].GetByLineNumber(num) : null)
		};
	}

	internal void Save(RegistryKey parentKey)
	{
		parentKey.SetValue("PC", PC);
		parentKey.SetValue("X", X);
		parentKey.SetValue("Y", Y);
		parentKey.SetValue("ProgramName", ProgramName);
		parentKey.SetValue("ParentContext", (ParentContext != null) ? ParentContext.LineNumber : (-1));
	}

	internal void LoadInstructionPointer(int parentContextLineNumber, ProgramInstruction program)
	{
		ParentContext = ((parentContextLineNumber != -1 && program != null) ? program.GetByLineNumber(parentContextLineNumber) : null);
	}
}
