namespace Redbox.HAL.Component.Model;

public interface IGetObserver
{
	void OnStuck(IGetResult result);

	bool OnEmpty(IGetResult result);
}
