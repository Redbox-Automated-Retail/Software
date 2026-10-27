namespace Redbox.HAL.Component.Model;

public interface IConfigurationObserver
{
	void NotifyConfigurationLoaded();

	void NotifyConfigurationChangeStart();

	void NotifyConfigurationChangeEnd();
}
