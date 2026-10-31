namespace Redbox.HAL.Script.Framework;

internal sealed class CompiledProgram
{
	internal readonly ProgramInstruction ParseTree;

	internal readonly ExecutionResult CompilerResult;

	// The script the program was compiled from; a saved job keeps it (ExecutionContextStore).
	internal readonly byte[] Source;

	internal bool VisibleToUser;

	public override string ToString()
	{
		if (CompilerResult.Errors.ErrorCount == 0 && CompilerResult.Errors.WarningCount == 0)
		{
			return "Cached";
		}
		return $"Errors: {CompilerResult.Errors.ErrorCount}, Warnings: {CompilerResult.Errors.WarningCount}";
	}

	internal CompiledProgram(ExecutionResult compileResult, ProgramInstruction tree, byte[] source)
	{
		ParseTree = tree;
		CompilerResult = compileResult;
		Source = source;
		VisibleToUser = true;
	}
}
