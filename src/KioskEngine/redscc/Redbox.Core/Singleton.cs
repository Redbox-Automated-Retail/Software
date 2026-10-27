using System;
using System.Reflection;

namespace Redbox.Core;

public static class Singleton<T> where T : class
{
	public static T Instance { get; private set; }

	static Singleton()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length > 0)
		{
			throw new SingletonException("A Singleton<T> class must not implement public instance constructors.");
		}
		ConstructorInfo constructor = typeFromHandle.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
		if (constructor == null)
		{
			throw new SingletonException("A Singleton<T> class must implement a non-public instance constructor.");
		}
		Instance = constructor.Invoke(null) as T;
	}
}
