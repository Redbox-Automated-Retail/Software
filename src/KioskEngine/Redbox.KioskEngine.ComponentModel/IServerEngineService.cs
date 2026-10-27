namespace Redbox.KioskEngine.ComponentModel;

public interface IServerEngineService
{
	string Url { get; set; }

	int Timeout { get; set; }
}
