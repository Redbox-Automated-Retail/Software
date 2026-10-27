using System;
using System.Collections.Generic;
using System.Diagnostics;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "init", Operand = "INIT")]
internal sealed class InitJob : NativeJobAdapter
{
	private class EventEntryComparer : IComparer<EventLogEntry>
	{
		public int Compare(EventLogEntry x, EventLogEntry y)
		{
			return y.TimeGenerated.CompareTo(x.TimeGenerated);
		}
	}

	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "init", flushOnWrite: true, null);
		applicationLog.Write("Init start.");
		applicationLog.Write("RESET controller boards.");
		object symbolValue = base.Context.GetSymbolValue("__EE_BOOTSTRAP_INIT", base.Result.Errors);
		bool flag = false;
		if (base.Result.Errors.Count == 0)
		{
			flag = (bool)symbolValue;
		}
		else
		{
			base.Result.Errors.Clear();
		}
		if (flag)
		{
			if (ControllerConfiguration.Instance.DisableAutoExposureAtInit)
			{
				ExposureFixer.ResetCameraProperties();
			}
			service.Wait(ControllerConfiguration.Instance.MinInitWakeupTime);
			LogHelper.Instance.Log("Wait for COM ports complete.");
		}
		try
		{
			if (ServiceLocator.Instance.GetService<IControlSystemService>().IsInitialized)
			{
				base.ControlSystem.Shutdown();
				service.Wait(2000);
			}
			if (!base.ControlSystem.Initialize().Success)
			{
				applicationLog.Write("Reset of the boards timed out.");
				LogHelper.Instance.Log("RESET instruction returned timeout; maybe communication error?", LogEntryType.Error);
				base.Context.Send("RESET instruction returned timeout.");
				base.Context.CreateInfoResult("RESET", "RESET instruction timed out.");
				AddError("RESET instruction returned timeout.");
				return;
			}
			ServiceLocator.Instance.GetService<IMotionControlService>().Initialize();
			applicationLog.Write("RESET completed successfully; get controller board VERSION stamps.");
			base.Context.Send("GET VERSION INFO");
			IControlSystemRevision revision = base.ControlSystem.GetRevision();
			if (!revision.Success)
			{
				applicationLog.Write("The Version instruction timed out.");
				LogHelper.Instance.Log("VERSION instruction returned timeout.", LogEntryType.Error);
				base.Context.Send("VERSION instruction returned timeout.");
				base.Context.CreateInfoResult("VERSION", "VERSION instruction timed out.");
				AddError("VERSION instruction returned timeout.");
				return;
			}
			Array.ForEach(revision.Responses, delegate(IBoardVersionResponse each)
			{
				base.Context.CreateInfoResult("VERSION INFO", each.Version);
			});
			base.Context.CreateInfoResult("VERSION INFO", revision.Revision);
			base.Context.Send("TEST TRACK");
			base.ControlSystem.TrackOpen();
			service.Wait(200);
			base.ControlSystem.TrackClose();
			service.Wait(200);
			if (!base.ControlSystem.TrackOpen().Success)
			{
				base.Context.CreateInfoResult("TRACK OPEN FAILED", "The TRACK OPEN instruction timed out.");
				AddError("TRACK OPEN instruction failed.");
				return;
			}
			service.Wait(200);
			if (!base.ControlSystem.TrackClose().Success)
			{
				base.Context.CreateInfoResult("TRACK CLOSE FAILED", "The TRACK CLOSE instruction timed out.");
				AddError("TRACK CLOSE instruction failed.");
				return;
			}
			service.Wait(200);
			applicationLog.Write("Ensure the gripper is not obstructed.");
			base.Context.Send("CHECK OBSTRUCTIONS");
			ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IControllerService>().ClearGripper();
			if (errorCodes != ErrorCodes.Success)
			{
				if (base.MotionService.CurrentLocation != null)
				{
					base.Context.CreateResult(StackEnd.Top, "PICKER OBSTRUCTED", "The picker is obstructed and cannot be moved.", base.MotionService.CurrentLocation.Deck, base.MotionService.CurrentLocation.Slot, null, null, null);
				}
				string errMsg = $"The picker is obstructed and cannot be moved: clear gripper returned {errorCodes.ToString()}.";
				AddError(errMsg);
				return;
			}
			ErrorCodes errorCodes2 = base.MotionService.InitAxes();
			if (errorCodes2 != ErrorCodes.Success)
			{
				string text = $"Initializing the axes failed with code {errorCodes2.ToString()}";
				base.Result.Errors.Add(Error.NewError("E999", "Init axes error.", text));
				applicationLog.WriteFormatted(text, LogEntryType.Error);
				base.Context.CreateInfoResult("InitError", text);
				return;
			}
			StartCamera();
			using (CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context))
			{
				if (checkPickerOperation.CheckPicker(base.Result, null) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disk away.");
					return;
				}
			}
			applicationLog.Write("Move to the vend position.");
			base.Context.Send("MOVE TO VEND");
			ErrorCodes errorCodes3 = base.MotionService.MoveVend(MoveMode.Get, base.AppLog);
			if (errorCodes3 != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult(StackEnd.Top, errorCodes3.ToString().ToUpper(), "There was an error executing the MOVEVEND instruction.");
				AddError("The MOVEVEND instruction failed.");
			}
		}
		finally
		{
			StartCamera();
			if (flag)
			{
				CheckShutdown(base.Context, service);
			}
			applicationLog.Write("Initialization complete.");
			base.Context.Send("INIT STOP");
		}
	}

	internal InitJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void CheckShutdown(ExecutionContext ctx, IRuntimeService runtimeService)
	{
		if (!ControllerConfiguration.Instance.TrackDirtyShutdown)
		{
			return;
		}
		EventLog eventLog = Array.Find(EventLog.GetEventLogs(), (EventLog each) => each.Log == "System");
		if (eventLog == null)
		{
			return;
		}
		List<EventLogEntry> list = new List<EventLogEntry>();
		using (new DisposeableList<EventLogEntry>(list))
		{
			foreach (EventLogEntry entry in eventLog.Entries)
			{
				if (entry.InstanceId == 41 || entry.InstanceId == 12)
				{
					list.Add(entry);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			list.Sort(new EventEntryComparer());
			EventLogEntry e = list[0];
			if (e.InstanceId == 41)
			{
				IHardwareCorrectionStatisticService service = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
				List<IHardwareCorrectionStatistic> stats = service.GetStats(HardwareCorrectionStatistic.UnexpectedPowerLoss);
				if (stats.Count > 0)
				{
					IHardwareCorrectionStatistic hardwareCorrectionStatistic = stats.Find((IHardwareCorrectionStatistic each) => each.CorrectionTime == e.TimeGenerated && each.ProgramName == "init");
					if (hardwareCorrectionStatistic != null)
					{
						LogHelper.Instance.Log("[init, {0}] Kiosk shutdown due to power event ( id = {1} timestamp = {2} ) - event exists.", ctx.ID, e.InstanceId, e.TimeGenerated);
						return;
					}
				}
				LogHelper.Instance.Log("[init, {0}] Kiosk shutdown due to power event ( id = {1} timestamp = {2} )", ctx.ID, e.InstanceId, e.TimeGenerated);
				service.Insert(HardwareCorrectionStatistic.UnexpectedPowerLoss, ctx, correctionOk: false, e.TimeGenerated);
			}
			else
			{
				LogHelper.Instance.Log("[init, {0}] Kiosk shutdown was ok", ctx.ID);
			}
		}
	}

	private void StartCamera()
	{
		IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		if (!configuredDevice.IsConnected)
		{
			bool flag = configuredDevice.Start(testFunction: true);
			if (!flag)
			{
				flag = configuredDevice.Restart();
			}
			LogHelper.Instance.WithContext("CAMERA START returned {0}", flag);
		}
	}
}
