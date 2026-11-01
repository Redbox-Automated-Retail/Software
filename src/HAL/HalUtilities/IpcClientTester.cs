using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Pipes;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal sealed class IpcClientTester : IDisposable
{
	private enum ClientType
	{
		None,
		Redbox,
		Basic,
		Message
	}

	private readonly HardwareService Service;

	private readonly IPCProtocol Protocol;

	public void Dispose()
	{
	}

	internal static void RunTest(string[] args)
	{
		bool debug = true;
		string pipeName = "HAL7001";
		int iterations = 5;
		int dataSize = 65536;
		ClientType clientType = ClientType.None;
		string protocol = "rcp://127.0.0.1:7001";
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "-pipeName")
			{
				pipeName = args[++i];
			}
			else if (args[i].StartsWith("--type"))
			{
				clientType = CommandLineOption.GetOptionVal(args[i], clientType);
			}
			else if (args[i].StartsWith("--iterations"))
			{
				iterations = CommandLineOption.GetOptionVal(args[i], iterations);
			}
			else if (args[i].StartsWith("--dataSize"))
			{
				dataSize = CommandLineOption.GetOptionVal(args[i], dataSize);
			}
			else if (args[i].StartsWith("-protocol"))
			{
				protocol = args[++i];
			}
			else if (args[i].StartsWith("--debug"))
			{
				debug = CommandLineOption.GetOptionVal(args[i], debug);
			}
		}
		using IpcClientTester ipcClientTester = new IpcClientTester(protocol);
		new ConsoleLogger(debug);
		if (ClientType.Message == clientType)
		{
			ipcClientTester.RunMessageClient(pipeName);
		}
		else if (ClientType.Basic == clientType)
		{
			ipcClientTester.RunBasic(pipeName);
		}
		else if (ClientType.Redbox == clientType)
		{
			ipcClientTester.RedboxClient(iterations, dataSize);
		}
	}

	private void TestGuid()
	{
		byte[] guidBytes = Guid.NewGuid().ToByteArray();
		byte[] buffer = new byte[64];
		Array.Copy(guidBytes, buffer, guidBytes.Length);
		new Guid(buffer);
	}

	private void RunMessageClient(string pipeName)
	{
		IPCMessageFactory iPCMessageFactory = new IPCMessageFactory();
		ClientPipeChannel clientPipeChannel = new ClientPipeChannel(pipeName, "Client-tester");
		if (clientPipeChannel.Connect())
		{
			IIPCMessage iIPCMessage = iPCMessageFactory.Create(MessageType.Information, MessageSeverity.Low, "Hello world!");
			bool sent = iPCMessageFactory.Write(iIPCMessage, clientPipeChannel);
			Console.WriteLine("[MsgPipeClient] {0} Message sent {1}", DateTime.Now.ToString(), sent ? "successfully" : "failed");
			Console.WriteLine(iIPCMessage.ToString());
			IIPCMessage reply = iPCMessageFactory.Read(clientPipeChannel);
			Console.WriteLine("[MsgPipeClient] {0} Received message {1} from server", DateTime.Now, reply.ToString());
		}
	}

	private void RunBasic(string pipeName)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		using ClientPipeChannel clientPipeChannel = new ClientPipeChannel(pipeName, "basic client");
		if (clientPipeChannel.Connect())
		{
			string arg = Encoding.ASCII.GetString(clientPipeChannel.Read());
			Console.WriteLine("Received message {0} from server.", arg);
			byte[] buffer = new byte[65536];
			rNGCryptoServiceProvider.GetBytes(buffer);
			bool sent = clientPipeChannel.Write(buffer);
			Console.WriteLine("[PipeClient] {0} Message sent {1}", DateTime.Now.ToString(), sent ? "successfully" : "failed");
			arg = Encoding.ASCII.GetString(clientPipeChannel.Read());
			Console.WriteLine("[PipeClient] {0} Received message '{0}' from server", DateTime.Now, arg);
		}
	}

	private void RunDataTest(object o)
	{
		int size = (int)o;
		RunDataTest(size);
	}

	private string RunDataTest(int size)
	{
		StringBuilder builder = new StringBuilder();
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			HardwareCommandResult hardwareCommandResult = Service.TestSomeData(size);
			if (hardwareCommandResult.Success)
			{
				builder.AppendFormat("[Thread-{0}] Test succeeded!", Thread.CurrentThread.Name);
				builder.AppendLine();
				builder.AppendLine("  Messages: ");
				hardwareCommandResult.CommandMessages.ForEach(delegate(string msg)
				{
					builder.AppendFormat("  {0}", msg);
					builder.AppendLine();
				});
			}
			else
			{
				builder.AppendFormat("[Thread-{0}] ** Test failed **", Thread.CurrentThread.Name);
				builder.AppendLine();
				builder.AppendLine("  Messages:");
				hardwareCommandResult.CommandMessages.ForEach(delegate(string msg)
				{
					builder.AppendFormat("  {0}", msg);
					builder.AppendLine();
				});
				builder.AppendLine("  Errors:");
				hardwareCommandResult.Errors.ForEach(delegate(Error error)
				{
					builder.AppendFormat("  {0} {1}: {2}", error.Code, error.Description, error.Details);
					builder.AppendLine();
				});
			}
			executionTimer.Stop();
			builder.AppendFormat("Execution time = {0}ms", executionTimer.ElapsedMilliseconds);
			builder.AppendLine();
		}
		return builder.ToString();
	}

	private void RedboxClient(int iterations, int size)
	{
		Thread[] threads = new Thread[iterations];
		for (int i = 0; i < iterations; i++)
		{
			Thread thread = (threads[i] = new Thread(RunDataTest));
			thread.Name = $"Clientthread-{i}";
			thread.Start(size);
		}
		Array.ForEach(threads, delegate(Thread t)
		{
			t.Join();
		});
	}

	private IpcClientTester(string _protocol)
	{
		Protocol = IPCProtocol.Parse(_protocol);
		Service = new HardwareService(Protocol);
	}
}
