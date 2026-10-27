namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class AddKioskSessionRequest
{
	public int KioskId { get; set; }

	public string SessionId { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }

	public bool IsUnCompressed { get; set; }

	public string SessionData { get; set; }
}
