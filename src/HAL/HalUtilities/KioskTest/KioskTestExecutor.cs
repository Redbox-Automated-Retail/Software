using Redbox.HAL.Client;

namespace HALUtilities.KioskTest;

internal sealed class KioskTestExecutor : JobExecutor
{
	private readonly TestLocation Source;

	private readonly TestLocation Destination;

	private readonly bool m_vend;

	protected override string JobName => "refurbish-kiosk-test";

	internal KioskTestExecutor(HardwareService service, TestLocation source, TestLocation dest, bool vend)
		: base(service)
	{
		Source = source;
		Destination = dest;
		m_vend = vend;
	}

	protected override void SetupJob()
	{
		base.Job.Push(Destination.Slot);
		base.Job.Push(Destination.Deck);
		base.Job.Push(Source.Slot);
		base.Job.Push(Source.Deck);
		base.Job.Push(m_vend.ToString());
	}
}
