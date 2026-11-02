using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Redbox.Remoting.Nrbf;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// The reservation broker's IKioskRegister over GTCP, without .NET Remoting: what
/// Activator.GetObject(typeof(IKioskRegister), "gtcp://host:7890/RegistrationBroker/Default.rem") returned on .NET
/// Framework. Register passes the kiosk's IKioskOperations as a callback object; the broker's calls to it (Ping,
/// Reserve3, CancelReservation, ...) arrive on the same connection and are run against that object.
/// </summary>
internal sealed class GtcpBrokerClient : IKioskRegister, IDisposable
{
	private static readonly string RegisterTypeName = FrameworkTypeNames.AssemblyQualifiedName(typeof(IKioskRegister));

	private readonly string _host;
	private readonly int _port;
	private readonly string _objectUri;
	private readonly GtcpConnectionSettings _settings;
	private readonly Action<string, Exception?> _log;
	private readonly object _openLock = new object();

	// The URI the broker puts in __RequestUri when it calls our callback object (any string works; 5.1).
	private readonly string _callbackUri = "/" + Guid.NewGuid().ToString("N") + "/kiosk_operations.rem";

	private GtcpConnection? _connection;
	private volatile IKioskOperations? _callbackTarget;
	private bool _disposed;

	public GtcpBrokerClient(string url, GtcpConnectionSettings settings, Action<string, Exception?> log)
	{
		Uri uri = new Uri(url);
		if (!string.Equals(uri.Scheme, "gtcp", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException($"Not a gtcp:// URL: {url}", nameof(url));
		}
		_host = uri.Host;
		_port = uri.Port > 0 ? uri.Port : throw new ArgumentException($"The broker URL has no port: {url}", nameof(url));
		_objectUri = uri.AbsolutePath.TrimStart('/'); // "RegistrationBroker/Default.rem", as GenuineChannels sends it
		_settings = settings;
		_log = log ?? ((_, _) => { });
	}

	/// <summary>The connection was lost for good; the kiosk has to register again (on a thread-pool thread).</summary>
	public event Action<GtcpBrokerClient, string>? ConnectionClosed;

	/// <summary>Our GTCP host URI, as the broker sees it (null before the first call).</summary>
	public string? HostUri => _connection?.HostUri;

	public bool IsConnected => _connection?.IsConnected ?? false;

	public void Register(int kioskID, IKioskOperations kiosk, string kioskVersion)
	{
		_callbackTarget = kiosk ?? throw new ArgumentNullException(nameof(kiosk));
		GtcpConnection connection = EnsureConnection();
		NrbfSyntheticObject objRef = RemotingMessages.ObjRef(_callbackUri, connection.HostUri, typeof(IKioskOperations));
		byte[] call = NrbfWriter.MethodCall(nameof(Register), RegisterTypeName, new object[] { kioskID, objRef, kioskVersion });
		RemotingMessages.ReadReturnValue(connection.Invoke(_objectUri, call), nameof(Register));
	}

	public bool IsRegistered(int kioskID)
	{
		GtcpConnection connection = EnsureConnection();
		byte[] call = NrbfWriter.MethodCall(nameof(IsRegistered), RegisterTypeName, new object[] { kioskID });
		object? result = RemotingMessages.ReadReturnValue(connection.Invoke(_objectUri, call), nameof(IsRegistered));
		return result is bool registered && registered;
	}

	public void Dispose()
	{
		GtcpConnection? connection;
		lock (_openLock)
		{
			_disposed = true;
			connection = _connection;
		}
		connection?.Dispose();
	}

	private GtcpConnection EnsureConnection()
	{
		lock (_openLock)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(GtcpBrokerClient));
			}
			if (_connection == null)
			{
				GtcpConnection connection = new GtcpConnection(_host, _port, _settings, HandleCall, _log);
				connection.Closed += reason => ConnectionClosed?.Invoke(this, reason);
				connection.Open();
				_connection = connection;
			}
			return _connection;
		}
	}

	/// <summary>
	/// Runs one call from the broker against the callback object and returns the BinaryMethodReturn to send back (5.2).
	/// Never throws: failures go back to the broker as an exception return.
	/// </summary>
	internal byte[] HandleCall(GtcpMessage message)
	{
		string methodName = "(unreadable call)";
		try
		{
			NrbfMethodCall call = NrbfReader.ReadMethodCall(message.Content);
			methodName = call.MethodName;
			IKioskOperations? target = _callbackTarget;
			if (target == null || message.RequestUri != _callbackUri)
			{
				throw new InvalidOperationException($"No object is published at {message.RequestUri}.");
			}
			return Dispatch(target, call);
		}
		catch (Exception e)
		{
			Exception error = e is TargetInvocationException { InnerException: not null } ? e.InnerException : e;
			_log($"GTCP: the broker's call {methodName} failed.", error);
			return NrbfWriter.ExceptionReturn(error);
		}
	}

	internal static byte[] Dispatch(IKioskOperations target, NrbfMethodCall call)
	{
		MethodInfo method = typeof(IKioskOperations).GetMethods()
			.FirstOrDefault(m => m.Name == call.MethodName && m.GetParameters().Length == call.Args.Length)
			?? throw new InvalidDataException($"IKioskOperations has no method {call.MethodName} with {call.Args.Length} argument(s).");
		ParameterInfo[] parameters = method.GetParameters();
		object?[] args = new object?[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			args[i] = NrbfObjectMapper.Map(call.Args[i], parameters[i].ParameterType);
		}
		object? result = method.Invoke(target, args);
		return NrbfWriter.MethodReturn(result, method.ReturnType == typeof(void));
	}
}
