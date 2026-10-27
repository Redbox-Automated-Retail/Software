using System;
using System.Collections;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class InstructionList : IEnumerable<Instruction>, IEnumerable, ICloneable<InstructionList>
{
	private readonly List<Instruction> m_instructions = new List<Instruction>();

	public Instruction this[int index] => m_instructions[index];

	public int Count => m_instructions.Count;

	internal Instruction Parent { get; set; }

	public InstructionList(Instruction parent)
	{
		Parent = parent;
	}

	public void Add(Instruction instruction)
	{
		if (Parent != null)
		{
			instruction.ParentContext = Parent;
		}
		m_instructions.Add(instruction);
	}

	public void AddRange(IEnumerable<Instruction> instructions)
	{
		foreach (Instruction instruction in instructions)
		{
			Add(instruction);
		}
	}

	public int FindInstructionIndexAtLabel(string labelName)
	{
		return m_instructions.FindIndex((Instruction each) => string.Compare(labelName, each.LabelName, ignoreCase: true) == 0);
	}

	public Instruction FindInstructionAtLabel(string labelName)
	{
		return m_instructions.Find((Instruction each) => string.Compare(labelName, each.LabelName, ignoreCase: true) == 0);
	}

	IEnumerator<Instruction> IEnumerable<Instruction>.GetEnumerator()
	{
		return m_instructions.GetEnumerator();
	}

	public IEnumerator GetEnumerator()
	{
		return ((IEnumerable<Instruction>)this).GetEnumerator();
	}

	public InstructionList Clone(params object[] parms)
	{
		InstructionList instructionList = new InstructionList((Instruction)parms[0]);
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Instruction instruction = (Instruction)enumerator.Current;
				instructionList.Add(instruction.Clone());
			}
			return instructionList;
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
