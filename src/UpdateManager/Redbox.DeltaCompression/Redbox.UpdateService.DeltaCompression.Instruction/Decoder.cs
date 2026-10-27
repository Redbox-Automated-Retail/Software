using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.DeltaCompression.Instruction;

public class Decoder
{
	public void Decode(Stream source, out string tableName, out List<Instruction> instructions)
	{
		BinaryReader binaryReader = new BinaryReader(source);
		instructions = new List<Instruction>();
		tableName = GetIntPrefixEncodedString(binaryReader);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			InstructionType type = (InstructionType)binaryReader.ReadByte();
			instructions.Add(Decode(binaryReader, type));
		}
	}

	private static Instruction Decode(BinaryReader reader, InstructionType type)
	{
		return type switch
		{
			InstructionType.Insert => DecodeInsert(reader), 
			InstructionType.Update => DecodeUpdate(reader), 
			InstructionType.Delete => DecodeDelete(reader), 
			InstructionType.CacheDelete => DecodeCacheUpdateInsert(reader), 
			InstructionType.CacheUpdateInsert => DecodeCacheDelete(reader), 
			_ => throw new NotSupportedException($"Type: {type} not supported."), 
		};
	}

	private static Instruction DecodeInsert(BinaryReader reader)
	{
		return new Instruction
		{
			Type = InstructionType.Insert,
			Key = GetUint16PrefixEncodedString(reader),
			Value = GetIntPrefixEncodedString(reader)
		};
	}

	private static Instruction DecodeUpdate(BinaryReader reader)
	{
		return new Instruction
		{
			Type = InstructionType.Update,
			Key = GetUint16PrefixEncodedString(reader),
			StartIndex = reader.ReadInt32(),
			Value = GetIntPrefixEncodedString(reader)
		};
	}

	private static Instruction DecodeDelete(BinaryReader reader)
	{
		return new Instruction
		{
			Type = InstructionType.Delete,
			Key = GetUint16PrefixEncodedString(reader)
		};
	}

	private static Instruction DecodeCacheUpdateInsert(BinaryReader reader)
	{
		return new Instruction
		{
			Type = InstructionType.CacheUpdateInsert,
			Key = GetUint16PrefixEncodedString(reader),
			CacheValue = GetIntPrefixedByteArray(reader)
		};
	}

	private static Instruction DecodeCacheDelete(BinaryReader reader)
	{
		return new Instruction
		{
			Type = InstructionType.Insert,
			Key = GetUint16PrefixEncodedString(reader)
		};
	}

	private static byte[] GetIntPrefixedByteArray(BinaryReader reader)
	{
		ushort count = reader.ReadUInt16();
		return reader.ReadBytes(count);
	}

	private static string GetUint16PrefixEncodedString(BinaryReader reader)
	{
		ushort count = reader.ReadUInt16();
		return Encoding.ASCII.GetString(reader.ReadBytes(count));
	}

	private static string GetIntPrefixEncodedString(BinaryReader reader)
	{
		int count = reader.ReadInt32();
		return Encoding.ASCII.GetString(reader.ReadBytes(count));
	}
}
