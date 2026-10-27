namespace Redbox.UpdateService.DeltaCompression.Instruction;

public enum InstructionType : byte
{
	Insert,
	Update,
	Delete,
	Literal,
	CacheUpdateInsert,
	CacheDelete
}
