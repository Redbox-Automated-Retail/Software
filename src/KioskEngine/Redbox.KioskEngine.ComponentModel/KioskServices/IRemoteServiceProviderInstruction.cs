using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IRemoteServiceProviderInstruction
{
	InstructionType Type { get; }

	IDictionary<string, object> Parameters { get; }
}
