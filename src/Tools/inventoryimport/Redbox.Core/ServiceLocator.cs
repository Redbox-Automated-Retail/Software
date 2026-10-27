using System;
using System.ComponentModel.Design;

namespace Redbox.Core;

public class ServiceLocator : IServiceProvider
{
	private ServiceContainer m_services;

	public IServiceContainer Services
	{
		get
		{
			if (m_services == null)
			{
				m_services = new ServiceContainer();
			}
			return m_services;
		}
	}

	public static ServiceLocator Instance => Singleton<ServiceLocator>.Instance;

	public T GetService<T>()
	{
		return (T)Services.GetService(typeof(T));
	}

	public object GetService(Type serviceType)
	{
		return Services.GetService(serviceType);
	}

	private ServiceLocator()
	{
	}
}
