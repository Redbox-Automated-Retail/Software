using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions;

internal sealed class KioskFunctionTest : JobExecutor, IKioskFunctionCheckData
{
	private TestState TSDriverStatus;

	private TestState CameraDriverStatus;

	private TestState SnapDecodeStatus;

	private TestState VendDoorStatus;

	private TestState InitStatus;

	private TestState VerticalSlotTestStatus;

	private readonly bool HasCortex;

	private readonly byte[] CameraSnapImmediate;

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

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(UserIdentifier);
		job.Push(Timestamp.ToString());
		job.Push(TouchscreenDriverTestResult.ToString());
		job.Push(CameraDriverTestResult.ToString());
		job.Push(SnapDecodeTestResult.ToString());
		job.Push(TrackTestResult.ToString());
		job.Push(VendDoorTestResult.ToString());
		job.Push(InitTestResult.ToString());
		job.Push(VerticalSlotTestResult.ToString());
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

	internal TestState TestCameraSnap(int deck, int slot)
	{
		SnapDecodeStatus = TestState.Failure;
		if (!CompositeFunctions.GetItem(deck, slot, Box, Service))
		{
			SnapDecodeStatus = TestState.Failure;
		}
		else
		{
			int num = (HasCortex ? 1 : 3);
			ReadBarcodeResult readBarcodeResult = ReadBarcodeResult.ReadBarcodeOfDiskInPicker(Service);
			Box.Write(string.Format("Found {0} barcodes ({1}), time={2} {3}", readBarcodeResult.Count, readBarcodeResult.Barcode, readBarcodeResult.ScanTime, readBarcodeResult.IsDuplicate ? "DUPLICATE" : string.Empty));
			int num2 = int.Parse(readBarcodeResult.ScanTime.Split('.')[0]);
			int num3 = int.Parse(readBarcodeResult.Count);
			SnapDecodeStatus = ((num3 >= num && num2 <= 0) ? TestState.Success : TestState.Failure);
			CompositeFunctions.PutItem(Service, deck, slot, Box);
		}
		return SnapDecodeStatus;
	}

	internal TestState TestVendDoor()
	{
		if ("SUCCESS" != ExecuteInstructionWithResult("VENDDOOR RENT"))
		{
			VendDoorStatus = TestState.Failure;
		}
		else
		{
			Thread.Sleep(500);
			VendDoorStatus = (("SUCCESS" == ExecuteInstructionWithResult("VENDDOOR CLOSE")) ? TestState.Success : TestState.Failure);
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
		foreach (ProgramResult result in inventoryStatsJob.Results)
		{
			if (result.Code == "TotalEmptyCount")
			{
				Box.Write($"  EMPTY slots: {result.Message}");
			}
			else if (result.Code == "UnknownCount")
			{
				Box.Write($"  UNKNOWN slots: {result.Message}");
			}
		}
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
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLEAR");
		stringBuilder.AppendLine(" RINGLIGHT ON");
		stringBuilder.AppendLine(" CAMERA SNAP");
		stringBuilder.AppendLine(" RINGLIGHT OFF");
		CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		TSDriverStatus = (CameraDriverStatus = (SnapDecodeStatus = TestState.NotStarted));
		VendDoorStatus = (InitStatus = (VerticalSlotTestStatus = TestState.NotStarted));
	}

	private string ExecuteInstructionWithResult(string inst)
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, inst, 120000);
		if (hardwareJob != null)
		{
			string topOfStack = hardwareJob.GetTopOfStack();
			Box.Write(inst + " - " + topOfStack);
			return topOfStack;
		}
		Box.Write("Command failed.");
		return string.Empty;
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
