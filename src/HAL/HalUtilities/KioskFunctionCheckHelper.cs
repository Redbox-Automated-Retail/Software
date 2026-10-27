using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class KioskFunctionCheckHelper : IDisposable
{
	private const string OutputFile = "KFCSessions.txt";

	private readonly HardwareService Service;

	internal IList<IKioskFunctionCheckData> Sessions { get; private set; }

	public void Dispose()
	{
		Sessions.Clear();
	}

	internal void DumpSessions(OutputModes mode)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		TextWriter textWriter = Console.Out;
		if (mode == OutputModes.File)
		{
			service.SafeDelete("KFCSessions.txt");
			textWriter = new StreamWriter("KFCSessions.txt");
		}
		DumpSessions(textWriter);
		if (mode == OutputModes.File)
		{
			textWriter.Dispose();
		}
	}

	internal void DumpSessions(TextWriter writer)
	{
		if (Sessions.Count == 0)
		{
			writer.WriteLine("There is no session data available.");
			return;
		}
		foreach (IKioskFunctionCheckData session in Sessions)
		{
			DumpSession(session, writer);
		}
	}

	internal KioskFunctionCheckHelper(HardwareService s)
	{
		new ConsoleLogger(enableDebug: true);
		Service = s;
		Sessions = new List<IKioskFunctionCheckData>();
		HardwareCommandResult kioskFunctionCheckData = Service.GetKioskFunctionCheckData(out var sessions);
		if (kioskFunctionCheckData.Success)
		{
			foreach (IKioskFunctionCheckData item in sessions)
			{
				Sessions.Add(item);
			}
			sessions.Clear();
		}
		else
		{
			kioskFunctionCheckData.Dump();
		}
	}

	private void DumpSession(IKioskFunctionCheckData session, TextWriter stream)
	{
		stream.WriteLine("## Session User {0} on {1} ##", session.UserIdentifier, session.Timestamp);
		stream.WriteLine("   Vertical result = {0}", session.VerticalSlotTestResult);
		stream.WriteLine("   Init result = {0}", session.InitTestResult);
		stream.WriteLine("   Venddoor result = {0}", session.VendDoorTestResult);
		stream.WriteLine("   Track result = {0}", session.TrackTestResult);
		stream.WriteLine("   Snap and decode result = {0}", session.SnapDecodeTestResult);
		stream.WriteLine("   Touchscreen driver result = {0}", session.TouchscreenDriverTestResult);
		stream.WriteLine("   Camera driver result = {0}", session.CameraDriverTestResult);
	}
}
