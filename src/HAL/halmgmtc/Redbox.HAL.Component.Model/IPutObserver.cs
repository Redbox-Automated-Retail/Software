namespace Redbox.HAL.Component.Model;

public interface IPutObserver
{
	void OnSuccessfulPut(IPutResult result, IFormattedLog log);

	void OnFailedPut(IPutResult result, IFormattedLog log);
}
