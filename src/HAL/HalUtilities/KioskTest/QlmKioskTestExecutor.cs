using Redbox.HAL.Client;

namespace HALUtilities.KioskTest;

internal sealed class QlmKioskTestExecutor : JobExecutor
{
	private readonly TestLocation Source;

	private readonly TestLocation Destination;

	private readonly bool m_vend;

	private readonly bool m_runQlm;

	protected override string JobName => "qlm-kiosk-test";

	internal QlmKioskTestExecutor(HardwareService service, TestLocation source, TestLocation dest, bool vend, bool runQlm)
		: base(service)
	{
		Source = source;
		Destination = dest;
		m_vend = vend;
		m_runQlm = runQlm;
	}

	protected override void SetupJob()
	{
		base.Job.Push(Destination.Slot);
		base.Job.Push(Destination.Deck);
		base.Job.Push(Source.Slot);
		base.Job.Push(Source.Deck);
		base.Job.Push(m_vend.ToString());
		base.Job.Push(m_runQlm.ToString());
	}
}
