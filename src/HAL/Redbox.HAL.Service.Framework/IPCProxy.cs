using System;
using System.IO;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

public sealed class IPCProxy : IDisposable
{
	private IIpcServiceHost m_listener;

	private bool Disposed;

	private readonly IIpcProtocol Protocol;

	public bool ProtocolValid => Protocol != null;

	public static void InitializeServices(IpcHostVersion version)
	{
		IpcServiceHostFactory instance = new IpcServiceHostFactory(version);
		ServiceLocator.Instance.AddService<IIpcServiceHostFactory>(instance);
	}

	public static IPCProxy Parse(string protocol)
	{
		IIpcProtocol ipcProtocol = null;
		try
		{
			ipcProtocol = IPCProtocol.Parse(protocol);
		}
		catch (UriFormatException e)
		{
			LogHelper.Instance.Log("ProtocolURI is malformed.", e);
			ipcProtocol = null;
		}
		return new IPCProxy(ipcProtocol);
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			StopListener();
		}
	}

	public void StartListener()
	{
		if (Protocol == null)
		{
			throw new Exception("Expected a parsed protocol.");
		}
		Assembly assembly = typeof(ProgramCommand).Assembly;
		IIpcServiceHostFactory service = ServiceLocator.Instance.GetService<IIpcServiceHostFactory>();
		IHostInfo info = service.Create(assembly);
		m_listener = service.Create(Protocol, info);
		m_listener.Start();
	}

	public void StopListener()
	{
		if (m_listener != null)
		{
			m_listener.Stop();
		}
	}

	public bool ExecuteStartupFile(string serviceStartup)
	{
		if (File.Exists(serviceStartup))
		{
			using StreamReader reader = new StreamReader(serviceStartup);
			new BatchCommandRunner(reader).Start();
		}
		return true;
	}

	private IPCProxy(IIpcProtocol protocol)
	{
		Protocol = protocol;
	}
}
