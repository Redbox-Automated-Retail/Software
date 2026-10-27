using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientReadAuxInputsResult : AbstractClientReadInputsResult<AuxInputs>
{
	protected override string LogHeader => "Aux Inputs";

	protected override InputState OnGetInputState(AuxInputs input)
	{
		if (!(Inputs[(int)input] == "1"))
		{
			return InputState.Inactive;
		}
		return InputState.Active;
	}

	protected override void OnForeachInput(Action<AuxInputs> a)
	{
		foreach (AuxInputs value in Enum<AuxInputs>.GetValues())
		{
			a(value);
		}
	}

	internal ClientReadAuxInputsResult(HardwareService service)
		: base(service, "SENSOR READ-AUX-INPUTS")
	{
	}
}
