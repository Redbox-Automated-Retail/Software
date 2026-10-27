using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class KioskFunctionCheckExecutor : JobExecutor
{
	private class KFCData : IKioskFunctionCheckData
	{
		public string VerticalSlotTestResult { get; internal set; }

		public string InitTestResult { get; internal set; }

		public string VendDoorTestResult { get; internal set; }

		public string TrackTestResult { get; internal set; }

		public string SnapDecodeTestResult { get; internal set; }

		public string TouchscreenDriverTestResult { get; internal set; }

		public string CameraDriverTestResult { get; internal set; }

		public DateTime Timestamp { get; internal set; }

		public string UserIdentifier { get; internal set; }
	}

	public IList<IKioskFunctionCheckData> Sessions { get; private set; }

	protected override string JobName => "load-kiosk-function-check-data";

	public KioskFunctionCheckExecutor(HardwareService service)
		: base(service)
	{
		Sessions = new List<IKioskFunctionCheckData>();
	}

	protected override void DisposeInner()
	{
		Sessions.Clear();
	}

	protected override void OnJobCompleted()
	{
		if (base.Job.GetStack(out var stack).Success)
		{
			int num = int.Parse(stack.Pop());
			for (int i = 0; i < num; i++)
			{
				KFCData kFCData = new KFCData();
				kFCData.VerticalSlotTestResult = stack.Pop();
				kFCData.InitTestResult = stack.Pop();
				kFCData.VendDoorTestResult = stack.Pop();
				kFCData.TrackTestResult = stack.Pop();
				kFCData.SnapDecodeTestResult = stack.Pop();
				kFCData.CameraDriverTestResult = stack.Pop();
				kFCData.TouchscreenDriverTestResult = stack.Pop();
				kFCData.Timestamp = DateTime.Parse(stack.Pop());
				kFCData.UserIdentifier = stack.Pop();
				Sessions.Add(kFCData);
			}
		}
	}
}
