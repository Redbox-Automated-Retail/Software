using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "CCRWatchdog", ConfigNotifications = true)]
internal sealed class CreditCardReaderPoller : ScriptPoller
{
	private int m_pollSleep;

	protected override string ThreadName => "CCR Watchdog";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "test-and-reset-ccr";

	protected override bool IsConfigured => m_pollSleep > 0;

	protected override int PollSleep => m_pollSleep;

	protected override void OnConfigurationLoad()
	{
		m_pollSleep = ControllerConfiguration.Instance.CCRWatchdogSleep;
	}

	protected override void OnConfigurationChangeEnd()
	{
		m_pollSleep = ControllerConfiguration.Instance.CCRWatchdogSleep;
	}

	protected override bool CoreExecute()
	{
		ScheduleAndBlockOnJob();
		return true;
	}

	private CreditCardReaderPoller()
	{
	}
}
