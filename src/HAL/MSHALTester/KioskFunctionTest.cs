using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class KioskFunctionTest : JobExecutor, IKioskFunctionCheckData
{
	private TestState TSDriverStatus;

	private TestState CameraDriverStatus;

	private TestState SnapDecodeStatus;

	private TestState VendDoorStatus;

	private TestState InitStatus;

	private TestState VerticalSlotTestStatus;

	private readonly bool HasCortex;

	private readonly OutputBox Box;

	public string VerticalSlotTestResult => VerticalSlotTestStatus.ToString();

	public string InitTestResult => InitStatus.ToString();

	public string VendDoorTestResult => VendDoorStatus.ToString();

	public string TrackTestResult => InitStatus.ToString();

	public string SnapDecodeTestResult => SnapDecodeStatus.ToString();

	public string TouchscreenDriverTestResult => TSDriverStatus.ToString();

	public string CameraDriverTestResult => CameraDriverStatus.ToString();

	public DateTime Timestamp { get; private set; }

	public string UserIdentifier { get; private set; }

	protected override string JobName => "kiosk-function-test-data";

	protected override void SetupJob()
	{
		base.Job.Push(UserIdentifier);
		base.Job.Push(Timestamp.ToString());
		base.Job.Push(TouchscreenDriverTestResult.ToString());
		base.Job.Push(CameraDriverTestResult.ToString());
		base.Job.Push(SnapDecodeTestResult.ToString());
		base.Job.Push(TrackTestResult.ToString());
		base.Job.Push(VendDoorTestResult.ToString());
		base.Job.Push(InitTestResult.ToString());
		base.Job.Push(VerticalSlotTestResult.ToString());
	}

	internal void SendData(string username)
	{
		UserIdentifier = username;
		Run();
		Box.Write("Report queued.");
	}

	internal TestState TestDrivers()
	{
		TestState testState = MatchTS();
		TestState testState2 = MatchCamera();
		if (TestState.Success != testState || TestState.Success != testState2)
		{
			return TestState.Failure;
		}
		return TestState.Success;
	}

	internal TestState TestCameraSnap(Location loc)
	{
		SnapDecodeStatus = TestState.Failure;
		List<Location> list = new List<Location>();
		list.Add(loc);
		if (!Service.HardSync(list, "KFC Test Sync", new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job).Success)
		{
			Box.Write("Unable to communicate with HAL.");
		}
		else
		{
			using ClientHelper clientHelper = new ClientHelper(Service);
			clientHelper.WaitForJob(job, out var endStatus);
			if (HardwareJobStatus.Completed == endStatus)
			{
				if (job.GetSymbols(out var symbols).Success)
				{
					foreach (string key in symbols.Keys)
					{
						if (key.Equals("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG"))
						{
							string text = symbols[key];
							LogHelper.Instance.Log("[TestCameraSnap] val = {0}", text);
							int num = (HasCortex ? 1 : 3);
							int num2 = text.IndexOf(':');
							text = text.Substring(num2 + 1);
							string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							_ = array[0];
							int num3 = Convert.ToInt32(array[2].Substring(1, 1));
							TimeSpan timeSpan = TimeSpan.Parse(array[5]);
							SnapDecodeStatus = ((num3 >= num && timeSpan.Seconds <= 0) ? TestState.Success : TestState.Failure);
							break;
						}
					}
				}
			}
			else
			{
				Box.Write("Decode test failed.");
			}
		}
		return SnapDecodeStatus;
	}

	internal TestState TestVendDoor()
	{
		VendDoorStatus = TestState.Failure;
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (service.VendDoorRent().Success)
		{
			Thread.Sleep(500);
			VendDoorStatus = (service.VendDoorClose().Success ? TestState.Success : TestState.Failure);
		}
		return VendDoorStatus;
	}

	internal TestState RunVertialSlotTest(int slot)
	{
		using (VerticalSync verticalSync = new VerticalSync(Service, slot))
		{
			verticalSync.Run();
			if (HardwareJobStatus.Completed == verticalSync.EndStatus)
			{
				Box.Write("The job completed successfully.");
				VerticalSlotTestStatus = TestState.Success;
			}
			else
			{
				foreach (ProgramResult result in verticalSync.Results)
				{
					Box.Write($"Failure at Deck {result.Deck} Slot {result.Slot} MSG: {result.Message}");
				}
				VerticalSlotTestStatus = TestState.Failure;
			}
		}
		return VerticalSlotTestStatus;
	}

	internal TestState RunInit()
	{
		using InitJob initJob = new InitJob(Service);
		initJob.Run();
		if (HardwareJobStatus.Completed == initJob.EndStatus)
		{
			Box.Write("Init succeeded.");
			InitStatus = TestState.Success;
		}
		else
		{
			InitStatus = TestState.Failure;
			if (initJob.Errors.Count > 0)
			{
				foreach (Error error in initJob.Errors)
				{
					Box.Write(error.Details);
				}
				Box.Write("Init didn't succeed; errors follow:");
			}
		}
		return InitStatus;
	}

	internal bool GetUnknownStats()
	{
		InventoryStatsJob inventoryStatsJob = new InventoryStatsJob(Service);
		inventoryStatsJob.Run();
		if (inventoryStatsJob.EndStatus != HardwareJobStatus.Completed)
		{
			return false;
		}
		inventoryStatsJob.Results.ForEach(delegate(ProgramResult result)
		{
			if (result.Code == "TotalEmptyCount")
			{
				Box.Write($"  EMPTY slots: {result.Message}");
			}
			else if (result.Code == "UnknownCount")
			{
				Box.Write($"  UNKNOWN slots: {result.Message}");
			}
		});
		Box.Write("Inventory Stats: ");
		return true;
	}

	internal KioskFunctionTest(HardwareService service, OutputBox box, bool hasCortex, string uname)
		: base(service)
	{
		Box = box;
		HasCortex = hasCortex;
		Timestamp = DateTime.Now;
		UserIdentifier = uname;
		TSDriverStatus = (CameraDriverStatus = (SnapDecodeStatus = TestState.NotStarted));
		VendDoorStatus = (InitStatus = (VerticalSlotTestStatus = TestState.NotStarted));
	}

	private TestState MatchCamera()
	{
		IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: true);
		if (deviceDescriptor != null)
		{
			CameraDriverStatus = TestState.Success;
			Box.Write("Matched camera {0}", deviceDescriptor.Friendlyname);
		}
		else
		{
			Box.Write("Failed to locate camera or match camera driver.");
			CameraDriverStatus = TestState.Failure;
		}
		return CameraDriverStatus;
	}

	private TestState MatchTS()
	{
		IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(matchDriver: true);
		if (deviceDescriptor != null)
		{
			TSDriverStatus = TestState.Success;
			Box.Write("Found Touchscreen {0}", deviceDescriptor.Friendlyname);
		}
		else
		{
			TSDriverStatus = TestState.Failure;
			Box.Write("Failed to locate touchscreen or match driver.");
		}
		return TSDriverStatus;
	}
}
