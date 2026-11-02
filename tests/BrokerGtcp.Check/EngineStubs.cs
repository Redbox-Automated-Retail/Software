using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using Redbox.KioskEngine.ComponentModel;

namespace BrokerGtcp.Check;

// Stand-ins for the engine services BrokerServicesProxy asks the ServiceLocator for (the same approach as the backend's
// tests\clients\BrokerHarness.cs).

/// <summary>"Remote Services\Reservation Services\BrokerServicesURL" (and defaults for everything else).</summary>
internal sealed class FakeSettings : IMachineSettingsStore
{
	public string BrokerUrl { get; set; } = null!; // set by LiveBrokerChecks.SetUpEngineServices

	public T? GetValue<T>(string path, string key) => GetValue(path, key, default(T));

	public T GetValue<T>(string path, string key, T defaultValue) => key == "BrokerServicesURL" ? (T)(object)BrokerUrl : defaultValue;

	public void SetValue<T>(string path, string key, T value)
	{
	}

	public void RemoveKey(string path)
	{
	}

	public void RemoveValue(string path, string key)
	{
	}
}

/// <summary>Runs every callback entry at once on a worker thread (the engine runs them on its UI thread).</summary>
internal sealed class FakeCallbacks : ICallbackService
{
	public int QueueCount => 0;

	public void Reset()
	{
	}

	public void Flush()
	{
	}

	public void Resume()
	{
	}

	public void Suspend()
	{
	}

	public bool InvokeNextCallback() => false;

	public void EnqueueCallback(ICallbackEntry callbackEntry)
	{
		ThreadPool.QueueUserWorkItem(_ =>
		{
			try
			{
				callbackEntry.Invoke();
			}
			catch (Exception e)
			{
				Console.WriteLine("     callback failed: " + e);
			}
		});
	}

	public DateTime? GetEnqueuedDateTimeFromNextCallbackEntry() => null;

	public ICallbackServiceStatistics? GetStatics() => null;

	public void ResetStatistics()
	{
	}

	public void LogStatistics()
	{
	}

	public void LogCallbackTracking()
	{
	}
}

/// <summary>
/// Implements any interface with members returning default values, except bool getters listed as true
/// (IConfiguration.UseLegacyBrokerService).
/// </summary>
public class InterfaceStub : DispatchProxy
{
	private static readonly ConcurrentDictionary<object, string[]> TrueGetters = new ConcurrentDictionary<object, string[]>();

	public static T Create<T>(params string[] trueGetters)
	{
		T stub = Create<T, InterfaceStub>();
		TrueGetters[stub!] = trueGetters;
		return stub;
	}

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		if (targetMethod!.ReturnType == typeof(bool) && TrueGetters.TryGetValue(this, out string[]? names) && Array.IndexOf(names, targetMethod.Name) >= 0)
		{
			return true;
		}
		return targetMethod.ReturnType.IsValueType && targetMethod.ReturnType != typeof(void) ? Activator.CreateInstance(targetMethod.ReturnType) : null;
	}
}
