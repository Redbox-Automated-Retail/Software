using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class KioskFunctionCheckExecutor : JobExecutor
{
	private class KFCData : IKioskFunctionCheckData
	{
		// The KFCData properties are all set by OnJobCompleted right after construction.
		public string VerticalSlotTestResult { get; internal set; } = null!;

		public string InitTestResult { get; internal set; } = null!;

		public string VendDoorTestResult { get; internal set; } = null!;

		public string TrackTestResult { get; internal set; } = null!;

		public string SnapDecodeTestResult { get; internal set; } = null!;

		public string TouchscreenDriverTestResult { get; internal set; } = null!;

		public string CameraDriverTestResult { get; internal set; } = null!;

		public DateTime Timestamp { get; internal set; }

		public string UserIdentifier { get; internal set; } = null!;
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
		if (base.Job!.GetStack(out var stack).Success)
		{
			int number = int.Parse(stack.Pop());
			for (int i = 0; i < number; i++)
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
