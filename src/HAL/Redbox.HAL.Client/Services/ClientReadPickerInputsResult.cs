using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientReadPickerInputsResult : AbstractClientReadInputsResult<PickerInputs>
{
	protected override string LogHeader => "Picker Inputs";

	protected override InputState OnGetInputState(PickerInputs input)
	{
		if (!(Inputs[(int)input] == "1"))
		{
			return InputState.Inactive;
		}
		return InputState.Active;
	}

	protected override void OnForeachInput(Action<PickerInputs> a)
	{
		foreach (PickerInputs value in Enum<PickerInputs>.GetValues())
		{
			a(value);
		}
	}

	internal ClientReadPickerInputsResult(HardwareService service)
		: base(service, "SENSOR READ-PICKER-INPUTS")
	{
	}
}
