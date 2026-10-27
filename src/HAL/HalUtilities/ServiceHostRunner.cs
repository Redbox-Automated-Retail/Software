using System;
using System.Reflection;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal static class ServiceHostRunner
{
	internal static void Run(string protocol, bool debug)
	{
		new ConsoleLogger(debug);
		IpcServiceHostFactory ipcServiceHostFactory = new IpcServiceHostFactory();
		Assembly assembly = typeof(Program).Assembly;
		IHostInfo info = ((IIpcServiceHostFactory)ipcServiceHostFactory).Create(assembly);
		IPCProtocol protocol2 = IPCProtocol.Parse(protocol);
		IIpcServiceHost ipcServiceHost = ((IIpcServiceHostFactory)ipcServiceHostFactory).Create((IIpcProtocol)protocol2, info);
		Thread thread = new Thread(ipcServiceHost.Start);
		thread.Start();
		Console.WriteLine("Press any key to halt.");
		Console.ReadLine();
		ipcServiceHost.Stop();
		thread.Join(5000);
		Console.WriteLine("Program finished.");
		Environment.Exit(0);
	}
}
