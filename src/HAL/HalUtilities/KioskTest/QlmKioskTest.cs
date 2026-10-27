using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest;

internal sealed class QlmKioskTest : AbstractKioskTest
{
	private int QlmFrequency = 2;

	protected override void OnProcessArgument(string argument)
	{
		if (argument.StartsWith("--qlmFrequency"))
		{
			QlmFrequency = CommandLineOption.GetOptionVal(argument, QlmFrequency);
		}
	}

	protected override int OnPreTest()
	{
		return 0;
	}

	protected override bool CanVisit(TestLocation loc)
	{
		return loc.Deck != 8;
	}

	protected override JobExecutor GetTransferExecutor(TestLocation start, TestLocation end, bool vend, int iteration)
	{
		return new QlmKioskTestExecutor(Service, start, end, vend, iteration % QlmFrequency == 0);
	}

	protected override int OnCleanup(TestLocation diskSource)
	{
		return 0;
	}

	protected override bool SingleTestNeeded()
	{
		return false;
	}

	internal QlmKioskTest(KioskConfiguration config, HardwareService s)
		: base(s, config)
	{
	}
}
