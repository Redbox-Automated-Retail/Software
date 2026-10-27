using System.Collections.Generic;

namespace Redbox.HAL.Script.Framework;

internal static class ExecutionErrorsTable
{
	internal static readonly Dictionary<ExecutionErrors, string> ErrorTable;

	internal static readonly Dictionary<ExecutionErrors, string> ErrorCodeTable;

	static ExecutionErrorsTable()
	{
		ErrorTable = new Dictionary<ExecutionErrors, string>();
		ErrorCodeTable = new Dictionary<ExecutionErrors, string>();
		ErrorTable[ExecutionErrors.None] = "Success.";
		ErrorCodeTable[ExecutionErrors.None] = "E001";
		ErrorTable[ExecutionErrors.MissingOperand] = "Missing operand.";
		ErrorCodeTable[ExecutionErrors.MissingOperand] = "E002";
		ErrorTable[ExecutionErrors.InvalidOperand] = "Unexpected operand.";
		ErrorCodeTable[ExecutionErrors.InvalidOperand] = "E003";
		ErrorTable[ExecutionErrors.MissingProgram] = "Missing program.";
		ErrorCodeTable[ExecutionErrors.MissingProgram] = "E004";
		ErrorTable[ExecutionErrors.DeprecatedOperand] = "Deprecated operand.";
		ErrorCodeTable[ExecutionErrors.DeprecatedOperand] = "E005";
		ErrorTable[ExecutionErrors.InvalidAssignment] = "Invalid Assignment.";
		ErrorCodeTable[ExecutionErrors.InvalidAssignment] = "E006";
		ErrorTable[ExecutionErrors.InvalidSymbolValue] = "Unexpected symbol value.";
		ErrorCodeTable[ExecutionErrors.InvalidSymbolValue] = "E007";
	}
}
