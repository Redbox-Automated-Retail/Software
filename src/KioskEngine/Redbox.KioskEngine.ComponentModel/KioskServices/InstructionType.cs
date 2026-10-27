namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public enum InstructionType : byte
{
	Start = 1,
	Shutdown,
	WaitAbsolute,
	SetRequestsPerSecond,
	WaitForSuccessfulPing
}
