namespace Redbox.HAL.Component.Model;

public interface IComponentErrorThresholdService
{
	bool Register(string key, IComponentErrorThreshold thresh);

	bool Remove(string key);

	IComponentErrorThreshold Find(string key);
}
