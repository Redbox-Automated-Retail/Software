using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.DeltaCompression.Instruction;

public class Encoder
{
	private BinaryWriter m_writer;

	public Stream Strm { get; set; }

	private BinaryWriter Writer
	{
		get
		{
			if (m_writer == null)
			{
				m_writer = new BinaryWriter(Strm);
			}
			return m_writer;
		}
	}

	public void Encode(string tableName, List<Instruction> instructions)
	{
		Writer.Write(tableName.Length);
		Writer.Write(Encoding.ASCII.GetBytes(tableName));
		Writer.Write(instructions.Count);
		foreach (Instruction instruction in instructions)
		{
			switch (instruction.Type)
			{
			case InstructionType.Insert:
				EncodeInsert(instruction, Writer);
				break;
			case InstructionType.Update:
				EncodeUpdate(instruction, Writer);
				break;
			case InstructionType.Delete:
				EncodeDelete(instruction, Writer);
				break;
			case InstructionType.CacheDelete:
				EncodeCacheDelete(instruction, Writer);
				break;
			case InstructionType.CacheUpdateInsert:
				EncodeCacheUpdateInsert(instruction, Writer);
				break;
			}
		}
	}

	public void BeginBlock(string tableName, int totalInstructions)
	{
		BinaryWriter binaryWriter = new BinaryWriter(Strm);
		binaryWriter.Write(tableName.Length);
		binaryWriter.Write(Encoding.ASCII.GetBytes(tableName));
		binaryWriter.Write(totalInstructions);
	}

	public void Encode(Instruction i)
	{
		switch (i.Type)
		{
		case InstructionType.Insert:
			EncodeInsert(i, Writer);
			break;
		case InstructionType.Update:
			EncodeUpdate(i, Writer);
			break;
		case InstructionType.Delete:
			EncodeDelete(i, Writer);
			break;
		case InstructionType.CacheDelete:
			EncodeCacheDelete(i, Writer);
			break;
		case InstructionType.CacheUpdateInsert:
			EncodeCacheUpdateInsert(i, Writer);
			break;
		case InstructionType.Literal:
			break;
		}
	}

	private static void EncodeInsert(Instruction i, BinaryWriter writer)
	{
		writer.Write((byte)i.Type);
		if (i.Key.Length > 65535)
		{
			throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write((ushort)i.Key.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Key));
		if (i.Value.Length > int.MaxValue)
		{
			throw new InvalidOperationException("Values greater than int.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write(i.Value.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Value));
	}

	private static void EncodeDelete(Instruction i, BinaryWriter writer)
	{
		writer.Write((byte)i.Type);
		if (i.Key.Length > 65535)
		{
			throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write((ushort)i.Key.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Key));
	}

	private static void EncodeUpdate(Instruction i, BinaryWriter writer)
	{
		writer.Write((byte)i.Type);
		if (i.Key.Length > 65535)
		{
			throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write((ushort)i.Key.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Key));
		writer.Write(i.StartIndex);
		writer.Write(i.Value.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Value));
	}

	private static void EncodeCacheUpdateInsert(Instruction i, BinaryWriter writer)
	{
		writer.Write((byte)i.Type);
		if (i.Key.Length > 65535)
		{
			throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write((ushort)i.Key.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Key));
		writer.Write(i.CacheValue.Length);
		writer.Write(i.CacheValue);
	}

	private static void EncodeCacheDelete(Instruction i, BinaryWriter writer)
	{
		writer.Write((byte)i.Type);
		if (i.Key.Length > 65535)
		{
			throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
		}
		writer.Write((ushort)i.Key.Length);
		writer.Write(Encoding.ASCII.GetBytes(i.Key));
	}
}
