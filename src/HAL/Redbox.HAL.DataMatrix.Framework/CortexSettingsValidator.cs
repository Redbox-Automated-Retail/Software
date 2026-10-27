using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.DataMatrix.Framework.Cortex;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CortexSettingsValidator
{
	private readonly IRuntimeService RuntimeService;

	private readonly CortexService Service;

	private readonly char[] Separator = new char[1] { ':' };

	private static readonly CortexRegister[] Registers = new CortexRegister[17]
	{
		new CortexRegister("93", "1"),
		new CortexRegister("26", "0"),
		new CortexRegister("d1", "2"),
		new CortexRegister("a8", "100"),
		new CortexRegister("a9", "20"),
		new CortexRegister("66", "100"),
		new CortexRegister("1b7", "0"),
		new CortexRegister("256", "2"),
		new CortexRegister("34", "5"),
		new CortexRegister("b4", "1"),
		new CortexRegister("45", "1000"),
		new CortexRegister("a5", "1000"),
		new CortexRegister("a3", "1000"),
		new CortexRegister("21D", "1"),
		new CortexRegister("42", "0"),
		new CortexRegister("08", "2"),
		new CortexRegister("1b", "6")
	};

	internal CortexSettingsValidator(CortexService service, IRuntimeService rts)
	{
		Service = service;
		RuntimeService = rts;
	}

	internal bool Validate(IMessageSink sink)
	{
		int errors = 0;
		Array.ForEach(Registers, delegate(CortexRegister register)
		{
			GetRegisterValueCommand getRegisterValueCommand = new GetRegisterValueCommand(register);
			getRegisterValueCommand.Send(Service.ConfiguredPort());
			if (!getRegisterValueCommand.IsCorrectlySet)
			{
				int num = errors + 1;
				errors = num;
				sink.Send($"The register {register.Register} received an unexpected response");
			}
			else
			{
				sink.Send($"Register {register.Register} matched expected value");
			}
			RuntimeService.SpinWait(200);
		});
		return errors == 0;
	}
}
