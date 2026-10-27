using System;
using System.ComponentModel.Design;

namespace Redbox.Core;

public class ServiceLocator : IServiceProvider
{
	private ServiceContainer m_services;

	[ThreadStatic]
	private static ServiceContainer m_threadLocalServices;

	public static ServiceLocator Instance => Singleton<ServiceLocator>.Instance;

	internal IServiceContainer Services
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

	internal IServiceContainer ThreadLocalServices
	{
		get
		{
			if (m_threadLocalServices == null)
			{
				m_threadLocalServices = new ServiceContainer();
			}
			return m_threadLocalServices;
		}
	}

	public void AddService(Type serviceType, object instance)
	{
		Services.AddService(serviceType, instance);
	}

	public void AddService(Type serviceType, ServiceCreatorCallback callback)
	{
		Services.AddService(serviceType, callback);
	}

	public void RemoveService(Type serviceType)
	{
		Services.RemoveService(serviceType);
	}

	public void AddThreadLocalService(Type serviceType, object instance)
	{
		ThreadLocalServices.AddService(serviceType, instance);
	}

	public void AddThreadLocalService(Type serviceType, ServiceCreatorCallback callback)
	{
		ThreadLocalServices.AddService(serviceType, callback);
	}

	public void RemoveThreadLocalService(Type serviceType)
	{
		ThreadLocalServices.RemoveService(serviceType);
	}

	public T GetService<T>()
	{
		return (T)GetService(typeof(T));
	}

	public object GetService(Type serviceType)
	{
		return ThreadLocalServices.GetService(serviceType) ?? Services.GetService(serviceType);
	}

	private ServiceLocator()
	{
	}
}
