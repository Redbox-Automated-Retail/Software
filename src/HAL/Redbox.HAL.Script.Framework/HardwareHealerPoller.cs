using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "HardwareHealer", ConfigNotifications = true)]
internal class HardwareHealerPoller : ScriptPoller
{
	private bool m_configured;

	private int m_pauseTime = 900000;

	protected override string ThreadName => "Hardware Healer";

	protected override string JobName => "hardware-healer";

	protected override string JobLabel => "Hardware healer process";

	protected override ProgramPriority Priority => ProgramPriority.Highest;

	protected override int PollSleep => m_pauseTime;

	protected override bool IsConfigured => m_configured;

	protected override void OnConfigurationLoad()
	{
		m_configured = ControllerConfiguration.Instance.EnableHardwareHealing;
		m_pauseTime = ControllerConfiguration.Instance.HardwareHealerPauseTime;
	}

	protected override void OnConfigurationChangeEnd()
	{
		m_configured = ControllerConfiguration.Instance.EnableHardwareHealing;
		m_pauseTime = ControllerConfiguration.Instance.HardwareHealerPauseTime;
	}

	protected override bool CoreExecute()
	{
		ScheduleAndBlockOnJob();
		return true;
	}

	private HardwareHealerPoller()
	{
	}
}
