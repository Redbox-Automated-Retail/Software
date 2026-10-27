using System;
using System.ComponentModel.Design;

namespace Redbox.HAL.Component.Model;

public sealed class ServiceLocator : IServiceProvider
{
	private readonly ServiceContainer Services = new ServiceContainer();

	private static readonly ServiceLocator m_instance;

	public static ServiceLocator Instance => m_instance;

	public void AddService<T>(object instance)
	{
		AddService(typeof(T), instance);
	}

	public void AddService(Type serviceType, object instance)
	{
		Services.AddService(serviceType, instance);
	}

	public T GetService<T>()
	{
		return (T)GetService(typeof(T));
	}

	public object GetService(Type serviceType)
	{
		return Services.GetService(serviceType);
	}

	public void RemoveService<T>()
	{
		Services.RemoveService(typeof(T));
	}

	public void RemoveService(Type serviceType)
	{
		Services.RemoveService(serviceType);
	}

	private ServiceLocator()
	{
	}

	static ServiceLocator()
	{
		m_instance = new ServiceLocator();
	}
}
