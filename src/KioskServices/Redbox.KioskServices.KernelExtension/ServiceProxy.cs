using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class ServiceProxy : IServiceProxy
{
	public T GetProperty<T>(string name)
	{
		if (name == "IsStandAlone")
		{
			return (T)(object)ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
		}
		if (name == "Url")
		{
			return (T)(object)KioskServicesProxy.Instance.Url;
		}
		return default(T);
	}

	public void SetProperty<T>(string name, T value)
	{
	}

	public object ExecuteMethod(string name, params object[] parms)
	{
		if (!(name == "ExecuteMessage"))
		{
			return null;
		}
		return KioskServicesProxy.Instance.ExecuteMessage(parms[0]);
	}
}
