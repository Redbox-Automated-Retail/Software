namespace Redbox.UpdateService.DeltaCompression.Instruction;

public class Instruction
{
	public InstructionType Type { get; set; }

	public string Key { get; set; }

	public string Value { get; set; }

	public byte[] CacheValue { get; set; }

	public int StartIndex { get; set; }

	public static Instruction CreateUpdate(string key, string value, int startIndex)
	{
		return new Instruction
		{
			Type = InstructionType.Update,
			Key = key,
			Value = value,
			StartIndex = startIndex
		};
	}

	public static Instruction CreateInsert(string key, string value)
	{
		return new Instruction
		{
			Type = InstructionType.Insert,
			Key = key,
			Value = value
		};
	}

	public static Instruction CreateDelete(string key)
	{
		return new Instruction
		{
			Type = InstructionType.Delete,
			Key = key
		};
	}

	public static Instruction CreateCacheUpdateInsert(string key, byte[] data)
	{
		return new Instruction
		{
			Type = InstructionType.CacheUpdateInsert,
			Key = key,
			CacheValue = data
		};
	}

	public static Instruction CreateCacheDelete(string key)
	{
		return new Instruction
		{
			Type = InstructionType.CacheDelete,
			Key = key
		};
	}
}
