namespace Redbox.HAL.Client;

public sealed class KioskTestJob : JobExecutor
{
	private Location m_start;

	private Location m_end;

	private bool m_vend;

	protected override string JobName => "KioskTest";

	public KioskTestJob(HardwareService service, Location start, Location end, bool vend)
		: base(service)
	{
		m_start = start;
		m_end = end;
		m_vend = vend;
	}

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(m_end.Slot);
		job.Push(m_end.Deck);
		job.Push(m_start.Slot);
		job.Push(m_start.Deck);
		job.Push(m_vend.ToString());
	}
}
