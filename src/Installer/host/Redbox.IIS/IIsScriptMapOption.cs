using System;

namespace Redbox.IIS;

[Flags]
public enum IIsScriptMapOption
{
	None = 0,
	ScriptEngine = 1,
	VerifyFileExists = 4
}
