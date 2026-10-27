using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.MSHALTester;

public class ConfiguredDevicesForm : Form
{
	private sealed class ConfiguredDeviceOuptutBox : OutputBox
	{
		protected override string PrewriteFormat(string msg)
		{
			return $"{DateTime.Now.ToLongTimeString()} {msg}";
		}

		internal ConfiguredDeviceOuptutBox(ListBox box)
			: base(box)
		{
		}
	}

	private class FraudTestExecutor : JobExecutor
	{
		protected override string JobName => "fraud-test-disk-in-picker";

		internal FraudTestExecutor(HardwareService service)
			: base(service)
		{
		}
	}

	private class FraudTakeReadReturnExecutor : JobExecutor
	{
		protected override string JobName => "fraud-test-take-read-return";

		internal FraudTakeReadReturnExecutor(HardwareService service)
			: base(service)
		{
		}
	}

	private class FraudStressTestExecutor : JobExecutor
	{
		private readonly int Iterations;

		private readonly int ScanPause;

		private readonly int IterationPause;

		protected override string JobName => "fraud-sensor-stress-test";

		protected override void SetupJob()
		{
			base.Job.Push(IterationPause);
			base.Job.Push(ScanPause);
			base.Job.Push(Iterations);
		}

		internal FraudStressTestExecutor(HardwareService service, int iterations, int scanPause, int iterationPause)
			: base(service)
		{
			Iterations = iterations;
			ScanPause = scanPause;
			IterationPause = iterationPause;
		}
	}

	private class FraudDetectionTestExecutor : JobExecutor
	{
		private readonly int StartupPause;

		protected override string JobName => "fraud-sensor-detect-test";

		protected override void SetupJob()
		{
			base.Job.Push(StartupPause);
		}

		internal FraudDetectionTestExecutor(HardwareService service, int startupPause)
			: base(service)
		{
			StartupPause = startupPause;
		}
	}

	private bool m_allowSessionDisplay;

	private KioskFunctionTest FunctionTest;

	private readonly HardwareService Service;

	private readonly OutputBox KioskTestOutput;

	private readonly OutputBox AirExchangerOutput;

	private readonly OutputBox FraudSensorOutput;

	private readonly OutputBox OutputBox;

	private readonly OutputBox EngineeringOutput;

	private readonly OutputBox HardwareStatsOutput;

	private readonly ButtonAspectsManager Manager;

	private readonly OutputBox IrOutput;

	private bool m_irConfigured;

	private readonly byte[] CenterItemProgram;

	private readonly AirExchangerState ExchangerState;

	private ScannerServices ScannerService = ScannerServices.Emulated;

	private bool HasFraudSensor;

	private bool HasRouterPowerRelay;

	private bool ArcusResetConfigured;

	private bool m_displayEngineeringTab;

	private IContainer components;

	private TabControl m_devicesTab;

	private TabPage m_airExchangerTab;

	private Label m_iceQubeExchangerLabel;

	private Button m_configureQubeButton;

	private Button m_resetQubeBoard;

	private Button m_qubeStatusButton;

	private Button m_resetPersistentCounter;

	private Button m_exitButton;

	private TabPage m_cortexTab;

	private Button m_installCortexButton;

	private Button m_uninstallCortexButton;

	private Label label3;

	private ListBox m_cortexStatusBox;

	private Button m_startCortexButton;

	private TabPage m_fraudSensorTab;

	private ListBox m_fraudSensorOutput;

	private Button m_fraudPOSTButton;

	private Button m_testFraudButton;

	private Button m_configureFraudSensor;

	private TextBox m_fraudDeckTB;

	private GroupBox groupBox1;

	private Button button1;

	private TextBox m_fraudSlotTB;

	private Button button4;

	private ListBox m_iceQubeOutput;

	private Button m_resetFraudButton;

	private Button button6;

	private Button button5;

	private Button button7;

	private Button m_stopFraudScan;

	private Button m_startFraudScan;

	private Button m_testDiskForMarkers;

	private Button button3;

	private Button button10;

	private Button m_powerOnFraudSensor;

	private Button m_powerOffFraudSensor;

	private Button button13;

	private TextBox m_scanPauseTB;

	private Label label4;

	private Label label2;

	private TextBox m_stressIterationsTB;

	private Label label5;

	private TextBox m_readPauseTB;

	private Button button14;

	private Button button15;

	private TabPage m_kioskFunctionCheck;

	private Button button2;

	private ListBox m_kioskTestOutput;

	private TextBox m_userName;

	private Label label1;

	private GroupBox groupBox2;

	private Button btn_KFCCheckDrivers;

	private Button btn_KFCInit;

	private Button btn_KFCTestVendDoor;

	private Button btn_KFCVerticalSlotTest;

	private Button btn_KFCUnknownCount;

	private Button button19;

	private TextBox m_verticalTestTB;

	private TextBox m_initTB;

	private TextBox m_testVendDoorTB;

	private TextBox m_checkDriversStatusTB;

	private TextBox m_unknownCountTB;

	private TextBox m_cameraSnapTB;

	private Button btn_KFCDecodeTest;

	private ErrorProvider errorProvider1;

	private TextBox m_sourceDeckTB;

	private Button button21;

	private TextBox m_sourceSlotTB;

	private Button button22;

	private Button m_displaySessionsButton;

	private TabPage tabPage1;

	private Button button16;

	private Button button11;

	private ListView listView1;

	private TabPage m_hwCorrections;

	private Button button23;

	private ListBox m_hwStatsOutput;

	private Button button24;

	private GroupBox m_hwCorrectiontypeGB;

	private RadioButton radioButton3;

	private RadioButton radioButton1;

	private TabPage m_engineeringTab;

	private Button button25;

	private ListBox m_engineeringOutput;

	private Button button26;

	private Button button27;

	private Button button28;

	private Button button29;

	private TextBox m_engSlotTB;

	private TextBox m_engDeckTB;

	private Button button30;

	private GroupBox groupBox3;

	private Button button31;

	private TextBox m_engEndSlotTB;

	private TextBox m_engEndDeckTB;

	private Button button32;

	private Button button34;

	private Button button33;

	private Button button36;

	private Button button35;

	private GroupBox groupBox5;

	private Button m_restartTouchscreen;

	private GroupBox groupBox6;

	private Button m_restartArcus;

	private Button m_configureArcus;

	private RadioButton radioButton2;

	private RadioButton radioButton5;

	private RadioButton radioButton4;

	private GroupBox groupBox4;

	private Button m_configureRouterRelay;

	private Button m_testRouterRelay;

	private BackgroundWorker m_verticalSyncWorker;

	private BackgroundWorker m_runInitWorker;

	private BackgroundWorker m_runDecodeWorker;

	private TabPage m_irConfigurationTab;

	private ListBox m_irCameraOutput;

	private Button button37;

	private GroupBox groupBox8;

	private TextBox textBox2;

	private TextBox textBox1;

	private Button button47;

	private Button button46;

	private Button button45;

	private Button button44;

	private Button button43;

	private GroupBox groupBox7;

	private Button button42;

	private Button button41;

	private Button button40;

	private Button button39;

	private Button button38;

	public bool DisplayEngineeringTab
	{
		get
		{
			return m_displayEngineeringTab;
		}
		set
		{
			m_displayEngineeringTab = value;
			if (!m_displayEngineeringTab)
			{
				m_devicesTab.TabPages.Remove(m_engineeringTab);
			}
		}
	}

	public int RouterPowerCyclePause { get; set; }

	public bool AllowSessionDisplay
	{
		get
		{
			return m_allowSessionDisplay;
		}
		set
		{
			m_allowSessionDisplay = value;
			Button displaySessionsButton = m_displaySessionsButton;
			bool enabled = (m_displaySessionsButton.Visible = m_allowSessionDisplay);
			displaySessionsButton.Enabled = enabled;
		}
	}

	private int SourceDeck => m_engDeckTB.GetInteger("Source Deck", EngineeringOutput);

	private int SourceSlot => m_engSlotTB.GetInteger("Source Slot", EngineeringOutput);

	private int DestinationDeck => m_engEndDeckTB.GetInteger("Source Deck", EngineeringOutput);

	private int DestinationSlot => m_engEndSlotTB.GetInteger("Source Slot", EngineeringOutput);

	private int? GetDeck
	{
		get
		{
			int integer = textBox1.GetInteger("Deck", IrOutput);
			if (-1 == integer)
			{
				errorProvider1.SetError(textBox1, "Please specify a valid deck");
				return null;
			}
			return integer;
		}
	}

	private int? GetSlot
	{
		get
		{
			int integer = textBox2.GetInteger("Slot", IrOutput);
			if (-1 == integer)
			{
				errorProvider1.SetError(textBox2, "Please specify a valid deck");
				return null;
			}
			return integer;
		}
	}

	public ConfiguredDevicesForm(HardwareService service)
	{
		InitializeComponent();
		OutputBox = new ConfiguredDeviceOuptutBox(m_cortexStatusBox);
		Service = service;
		m_devicesTab.Selected += OnTabSelectedChange;
		Manager = new ButtonAspectsManager();
		AirExchangerOutput = new ConfiguredDeviceOuptutBox(m_iceQubeOutput);
		FraudSensorOutput = new ConfiguredDeviceOuptutBox(m_fraudSensorOutput);
		KioskTestOutput = new ConfiguredDeviceOuptutBox(m_kioskTestOutput);
		HardwareStatsOutput = new ConfiguredDeviceOuptutBox(m_hwStatsOutput);
		EngineeringOutput = new ConfiguredDeviceOuptutBox(m_engineeringOutput);
		ExchangerState = new AirExchangerState(service);
		IrOutput = new ConfiguredDeviceOuptutBox(m_irCameraOutput);
		using (MachineConfiguration machineConfiguration = new MachineConfiguration(Service))
		{
			machineConfiguration.Run();
			ExchangerState.Configure(machineConfiguration.AirExchangerStatus, machineConfiguration.AirExchangerFanStatus);
			HasFraudSensor = machineConfiguration.HasFraudDevice;
			ScannerService = machineConfiguration.ConfiguredCamera;
			HasRouterPowerRelay = machineConfiguration.HasRouterPowerRelay;
			ArcusResetConfigured = machineConfiguration.ArcusResetConfigured;
			m_irConfigured = machineConfiguration.HasIRHardware;
			btn_KFCCheckDrivers.Enabled = !machineConfiguration.KFCDisableCheckDrivers;
			btn_KFCDecodeTest.Enabled = !machineConfiguration.KFCDisableDecodeTest;
			btn_KFCTestVendDoor.Enabled = !machineConfiguration.KFCDisableVendDoorTest;
			btn_KFCInit.Enabled = !machineConfiguration.KFCDisableInit;
			btn_KFCVerticalSlotTest.Enabled = !machineConfiguration.KFCDisableVerticalSlotTest;
			btn_KFCUnknownCount.Enabled = !machineConfiguration.KFCDisableUnknownCount;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("ROLLER POS=6 TIMEOUT=4000 WAIT=TRUE ");
		stringBuilder.AppendLine(" ROLLER POS=3 TIMEOUT=4000 WAIT=TRUE ");
		stringBuilder.AppendLine(" CLEAR");
		CenterItemProgram = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		listView1.Columns.Add("Component", 200);
		listView1.Columns.Add("Value", 200);
		RouterPowerCyclePause = 1500;
	}

	private void ConfigureCortexPanel()
	{
		bool flag = ScannerServices.Cortex == ScannerService;
		m_installCortexButton.Enabled = !flag;
		m_uninstallCortexButton.Enabled = flag;
		WriteToBox(string.Format("The camera {0} enabled.", flag ? "is" : "is not"));
	}

	private void OnConfigureIceQubeBoard()
	{
		bool configured = ExchangerState.Configured;
		m_iceQubeExchangerLabel.Text = string.Format("The board {0} configured.", configured ? "is" : "is not");
		m_configureQubeButton.Text = (configured ? "Remove" : "Configure");
		m_resetQubeBoard.Enabled = configured;
		m_qubeStatusButton.Enabled = configured;
		m_resetPersistentCounter.Enabled = configured;
	}

	private void OnTabSelectedChange(object sender, EventArgs e)
	{
		switch (m_devicesTab.SelectedIndex)
		{
		case 0:
			OnConfigureIceQubeBoard();
			break;
		case 1:
			ConfigureCortexPanel();
			break;
		case 2:
			OnConfigureFraudSensor();
			break;
		case 3:
			OnConfigureKioskTest();
			break;
		case 5:
			ConfigureRouterGroup();
			ConfigureArcusGroup();
			break;
		case 6:
			ConfigureIrCameraGroup();
			break;
		case 4:
			break;
		}
	}

	private void ConfigureIrCameraGroup()
	{
		bool flag = ScannerServices.Cortex == ScannerService;
		if (!flag)
		{
			button37.Text = (m_irConfigured ? "Un-configure" : "Configure");
		}
		else
		{
			button37.Text = "Unavailable";
		}
		Button button = button38;
		Button button2 = button39;
		Button button3 = button40;
		bool flag2 = (button41.Enabled = !flag);
		bool flag4 = (button3.Enabled = flag2);
		bool enabled = (button2.Enabled = flag4);
		button.Enabled = enabled;
		Button button4 = button42;
		Button button5 = button43;
		Button button6 = button46;
		flag2 = (button47.Enabled = !flag);
		flag4 = (button6.Enabled = flag2);
		enabled = (button5.Enabled = flag4);
		button4.Enabled = enabled;
		Button button7 = button44;
		Button button8 = button45;
		flag4 = (button37.Enabled = !flag);
		enabled = (button8.Enabled = flag4);
		button7.Enabled = enabled;
	}

	private void OnConfigureKioskTest()
	{
		ISessionUser currentSession = ServiceLocator.Instance.GetService<ISessionUserService>().GetCurrentSession();
		FunctionTest = new KioskFunctionTest(Service, KioskTestOutput, ScannerServices.Cortex == ScannerService, currentSession.User);
		TextBox testVendDoorTB = m_testVendDoorTB;
		TextBox checkDriversStatusTB = m_checkDriversStatusTB;
		TextBox cameraSnapTB = m_cameraSnapTB;
		TextBox initTB = m_initTB;
		TextBox verticalTestTB = m_verticalTestTB;
		string text = (m_unknownCountTB.Text = "Not Started");
		string text3 = (verticalTestTB.Text = text);
		string text5 = (initTB.Text = text3);
		string text7 = (cameraSnapTB.Text = text5);
		string text9 = (checkDriversStatusTB.Text = text7);
		testVendDoorTB.Text = text9;
		m_userName.Text = currentSession.User;
	}

	private void OnConfigureFraudSensor()
	{
		FraudSensorOutput.Write("Fraud sensor is {0}.", HasFraudSensor ? "enabled" : "disabled");
		m_configureFraudSensor.Text = (HasFraudSensor ? "Disable" : "Enable");
		Button fraudPOSTButton = m_fraudPOSTButton;
		bool enabled = (m_resetFraudButton.Enabled = HasFraudSensor);
		fraudPOSTButton.Enabled = enabled;
		Button powerOnFraudSensor = m_powerOnFraudSensor;
		enabled = (m_powerOffFraudSensor.Enabled = HasFraudSensor);
		powerOnFraudSensor.Enabled = enabled;
		Button startFraudScan = m_startFraudScan;
		enabled = (m_stopFraudScan.Enabled = HasFraudSensor);
		startFraudScan.Enabled = enabled;
		Button testDiskForMarkers = m_testDiskForMarkers;
		enabled = (m_testFraudButton.Enabled = HasFraudSensor);
		testDiskForMarkers.Enabled = enabled;
	}

	private void WriteToOutput(string msg)
	{
		ServiceLocator.Instance.GetService<ILogger>().Log(msg, LogEntryType.Info);
	}

	private void m_configureQubeButton_Click(object sender, EventArgs e)
	{
		if (!ExchangerState.ToggleConfiguration())
		{
			AirExchangerOutput.Write("Failed to update configuration.");
			return;
		}
		AirExchangerOutput.Write("The air exchanger is {0}.", ExchangerState.Configured ? "configured" : "not configured");
		OnConfigureIceQubeBoard();
	}

	private void m_configureAuxBoardButton_Click(object sender, EventArgs e)
	{
		m_iceQubeExchangerLabel.Text = "Power Relay board no longer supported.";
	}

	private void OnExecuteFraudSensorInstruction(object sender, EventArgs e)
	{
		ExecuteInstruction(sender, FraudSensorOutput, "FRAUDSENSOR");
	}

	private void OnExecuteIceQubeBoardCommand(object sender, EventArgs e)
	{
		ExecuteInstruction(sender, AirExchangerOutput, "AIRXCHGR");
	}

	private void ExecuteInstruction(object sender, OutputBox box, string mnemonic)
	{
		using ButtonAspects buttonAspects = Manager.MakeAspect(sender);
		string tagInstruction = buttonAspects.GetTagInstruction();
		if (string.IsNullOrEmpty(tagInstruction))
		{
			box.Write("There is no operand associated with that button.");
			return;
		}
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		string instruction = $"{mnemonic} {tagInstruction}";
		string text = instructionHelper.ExecuteGeneric(instruction);
		box.Write(string.IsNullOrEmpty(text) ? "Instruction execution failed." : text);
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void m_installCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender);
	}

	private void m_uninstallCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender);
	}

	private void ToggleCortex(object sender)
	{
		ScannerServices scannerServices = ((ScannerServices.Cortex != ScannerService) ? ScannerServices.Cortex : ScannerServices.Legacy);
		ServiceLocator.Instance.GetService<IRuntimeService>();
		using (Manager.MakeAspect(sender))
		{
			using ChangeCameraConfigurationExecutor changeCameraConfigurationExecutor = new ChangeCameraConfigurationExecutor(Service, scannerServices);
			changeCameraConfigurationExecutor.Run();
			if (HardwareJobStatus.Completed == changeCameraConfigurationExecutor.EndStatus)
			{
				ScannerService = scannerServices;
				bool flag = ScannerServices.Cortex == ScannerService;
				WriteToBox("The camera was successfully installed");
				m_installCortexButton.Enabled = !flag;
				Button uninstallCortexButton = m_uninstallCortexButton;
				bool enabled = (m_startCortexButton.Enabled = flag);
				uninstallCortexButton.Enabled = enabled;
			}
			else
			{
				changeCameraConfigurationExecutor.Errors.ForEach(delegate(Error each)
				{
					WriteToBox(each.Details);
				});
				WriteToBox("Unable to change camera configuration.");
			}
		}
	}

	private void WriteToBox(string msg)
	{
		OutputBox.Write(msg);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using InstructionHelper instructionHelper = new InstructionHelper(Service);
			string value = instructionHelper.ExecuteGeneric("CAMERA START");
			if (string.IsNullOrEmpty(value))
			{
				OutputBox.Write(ErrorCodes.ServiceChannelError.ToString().ToUpper());
				return;
			}
			bool flag = bool.Parse(value);
			WriteToBox(flag ? "Camera started." : "Camera DIDN'T START.");
		}
	}

	private void RunPostTest()
	{
		using (FraudSensorPost fraudSensorPost = new FraudSensorPost(Service))
		{
			fraudSensorPost.AddSink(delegate(HardwareJob j, DateTime time, string msg)
			{
				FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg);
			});
			fraudSensorPost.Run();
			fraudSensorPost.Results.ForEach(delegate(ProgramResult each)
			{
				FraudSensorOutput.Write("{0} : {1}", each.Code, each.Message);
			});
		}
		BeginInvoke((MethodInvoker)delegate
		{
			m_fraudPOSTButton.Enabled = true;
		});
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = !HasFraudSensor;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"SETCFG \"EnableSecureDiskValidator\" \"{flag.ToString()}\" TYPE=CONTROLLER");
			stringBuilder.AppendLine($" SETCFG \"EnableFraudSensorCheck\" \"{flag.ToString()}\" TYPE=CONTROLLER");
			stringBuilder.AppendLine($" SETCFG \"FraudScanJobBitmask2\" \"{(flag ? 32 : 0)}\" TYPE=CONTROLLER");
			stringBuilder.AppendLine(" CLEAR");
			HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			if (!hardwareCommandResult.Success)
			{
				FraudSensorOutput.Write("Failed to update configuration.");
				DumpResult(hardwareCommandResult, FraudSensorOutput);
			}
			else
			{
				HasFraudSensor = flag;
				OnConfigureFraudSensor();
			}
		}
	}

	private void DumpResult(HardwareCommandResult result, OutputBox box)
	{
		result.Errors.ForEach(delegate(Error err)
		{
			box.Write(err.ToString());
		});
	}

	private void m_testFraudButton_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using FraudTestExecutor fraudTestExecutor = new FraudTestExecutor(Service);
			fraudTestExecutor.Run();
			foreach (ProgramResult result in fraudTestExecutor.Results)
			{
				FraudSensorOutput.Write("Code: {0}, Message = {1}", result.Code, result.Message);
			}
		}
	}

	private void m_testUserDiskForFraud(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using FraudTakeReadReturnExecutor fraudTakeReadReturnExecutor = new FraudTakeReadReturnExecutor(Service);
			fraudTakeReadReturnExecutor.Run();
			foreach (ProgramResult result in fraudTakeReadReturnExecutor.Results)
			{
				FraudSensorOutput.Write("Code: {0}, Message = {1}", result.Code, result.Message);
			}
		}
	}

	private void button1_Click_2(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int integer = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
			int integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
			if (-1 == integer || -1 == integer2)
			{
				FraudSensorOutput.Write("Check deck and slot!");
				return;
			}
			CompositeFunctions.GetItem(integer, integer2, FraudSensorOutput, Service);
			Service.ExecuteImmediateProgram(CenterItemProgram, out var _);
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			CompositeFunctions.VendDisk(Service, FraudSensorOutput);
		}
	}

	private void button4_Click_1(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int integer = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
			int integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
			if (-1 == integer || -1 == integer2)
			{
				FraudSensorOutput.Write("Check deck and slot!");
			}
			else
			{
				CompositeFunctions.PutItem(Service, integer, integer2, OutputBox);
			}
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using TakeDiskAtDoorJob takeDiskAtDoorJob = new TakeDiskAtDoorJob(Service);
			takeDiskAtDoorJob.Run();
			foreach (ProgramResult result in takeDiskAtDoorJob.Results)
			{
				OutputBox.Write("Code: {0} Message: {1}", result.Code, result.Message);
			}
		}
	}

	private void button5_Click_1(object sender, EventArgs e)
	{
		LocationNumberPad locationNumberPad = new LocationNumberPad();
		if (locationNumberPad.ShowDialog() == DialogResult.OK)
		{
			m_fraudDeckTB.Text = locationNumberPad.Number.ToString();
		}
	}

	private void button6_Click(object sender, EventArgs e)
	{
		LocationNumberPad locationNumberPad = new LocationNumberPad();
		if (locationNumberPad.ShowDialog() == DialogResult.OK)
		{
			m_fraudSlotTB.Text = locationNumberPad.Number.ToString();
		}
	}

	private void button7_Click(object sender, EventArgs e)
	{
		using ButtonAspects buttonAspects = Manager.MakeAspect(sender);
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		string tagInstruction = buttonAspects.GetTagInstruction();
		if (string.IsNullOrEmpty(tagInstruction))
		{
			FraudSensorOutput.Write("There is no instruction for that button.");
		}
		else
		{
			instructionHelper.ExecuteGeneric(tagInstruction);
		}
	}

	private void button3_Click_1(object sender, EventArgs e)
	{
		FraudSensorOutput.Clear();
	}

	private void button10_Click(object sender, EventArgs e)
	{
		AirExchangerOutput.Clear();
	}

	private void RunStressTest()
	{
		int integer = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
		int integer2 = m_stressIterationsTB.GetInteger("iterations", FraudSensorOutput);
		int integer3 = m_readPauseTB.GetInteger("Iteration Pause", FraudSensorOutput);
		using (FraudStressTestExecutor fraudStressTestExecutor = new FraudStressTestExecutor(Service, integer2, integer, integer3))
		{
			fraudStressTestExecutor.AddSink(delegate(HardwareJob j, DateTime time, string msg)
			{
				FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg);
			});
			fraudStressTestExecutor.Run();
			foreach (ProgramResult result in fraudStressTestExecutor.Results)
			{
				FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
			}
		}
		BeginInvoke((MethodInvoker)delegate
		{
			button13.Enabled = true;
		});
	}

	private void button14_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			ServiceLocator.Instance.GetService<IControlSystem>().Center(CenterDiskMethod.VendDoorAndBack);
		}
	}

	private void button15_Click(object sender, EventArgs e)
	{
		int integer = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
		using (Manager.MakeAspect(sender))
		{
			using FraudDetectionTestExecutor fraudDetectionTestExecutor = new FraudDetectionTestExecutor(Service, integer);
			fraudDetectionTestExecutor.Run();
		}
	}

	private void button8_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			m_checkDriversStatusTB.Text = FunctionTest.TestDrivers().ToString().ToUpper();
		}
	}

	private void button2_Click_1(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			KioskTestOutput.Clear();
		}
	}

	private void button12_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			m_testVendDoorTB.Text = FunctionTest.TestVendDoor().ToString().ToUpper();
		}
	}

	private void button9_Click(object sender, EventArgs e)
	{
		btn_KFCInit.BackColor = Color.Red;
		ToggleKfcButtons(enable: false);
		m_runInitWorker.RunWorkerAsync();
	}

	private void button17_Click(object sender, EventArgs e)
	{
		errorProvider1.Clear();
		int integer = m_sourceSlotTB.GetInteger("Test Slot", KioskTestOutput);
		if (-1 == integer)
		{
			errorProvider1.SetError(m_sourceSlotTB, "Select slot");
			return;
		}
		ToggleKfcButtons(enable: false);
		btn_KFCVerticalSlotTest.BackColor = Color.Red;
		m_verticalSyncWorker.RunWorkerAsync(integer);
	}

	private void button18_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			m_unknownCountTB.Text = (FunctionTest.GetUnknownStats() ? "SUCCESS" : "FAILURE");
		}
	}

	private void button19_Click(object sender, EventArgs e)
	{
		errorProvider1.Clear();
		if (string.IsNullOrEmpty(m_userName.Text))
		{
			KioskTestOutput.Write("Please specify a user name.");
			errorProvider1.SetError(m_userName, "User name required.");
			return;
		}
		using (Manager.MakeAspect(sender))
		{
			FunctionTest.SendData(m_userName.Text);
		}
		FunctionTest = null;
		OnConfigureKioskTest();
	}

	private void button16_Click(object sender, EventArgs e)
	{
		m_sourceSlotTB.InputNumber();
	}

	private void button20_Click(object sender, EventArgs e)
	{
		int integer = m_sourceDeckTB.GetInteger("Sync deck", KioskTestOutput);
		int integer2 = m_sourceSlotTB.GetInteger("Sync slot", KioskTestOutput);
		if (-1 == integer)
		{
			errorProvider1.SetError(m_sourceDeckTB, "Please enter valid deck.");
			return;
		}
		if (-1 == integer2)
		{
			errorProvider1.SetError(m_sourceSlotTB, "Please enter valid slot.");
			return;
		}
		ToggleKfcButtons(enable: false);
		btn_KFCDecodeTest.BackColor = Color.Red;
		m_runDecodeWorker.RunWorkerAsync(new Location
		{
			Deck = integer,
			Slot = integer2
		});
	}

	private void button21_Click(object sender, EventArgs e)
	{
		m_sourceDeckTB.InputNumber();
	}

	private void button22_Click(object sender, EventArgs e)
	{
		m_sourceSlotTB.InputNumber();
	}

	private void button16_Click_1(object sender, EventArgs e)
	{
		if (!Service.GetKioskFunctionCheckData(out var sessions).Success)
		{
			return;
		}
		int num = 1;
		foreach (IKioskFunctionCheckData item in sessions)
		{
			WriteSessionData(item, num++);
		}
	}

	private void WriteSessionData(IKioskFunctionCheckData session, int sessionCount)
	{
		KioskTestOutput.Write("   Vertical result = {0}", session.VerticalSlotTestResult);
		KioskTestOutput.Write("   Init result = {0}", session.InitTestResult);
		KioskTestOutput.Write("   Venddoor result = {0}", session.VendDoorTestResult);
		KioskTestOutput.Write("   Track result = {0}", session.TrackTestResult);
		KioskTestOutput.Write("   Snap and decode result = {0}", session.SnapDecodeTestResult);
		KioskTestOutput.Write("   Touchscreen driver result = {0}", session.TouchscreenDriverTestResult);
		KioskTestOutput.Write("   Camera driver result = {0}", session.CameraDriverTestResult);
		KioskTestOutput.Write("   Timestamp {0}", session.Timestamp.ToString());
		KioskTestOutput.Write("   User identifier {0}", session.UserIdentifier);
		KioskTestOutput.Write("Session {0}", sessionCount);
	}

	private void button11_Click(object sender, EventArgs e1)
	{
		using (Manager.MakeAspect(sender))
		{
			using HardwareSurveyExecutor hardwareSurveyExecutor = new HardwareSurveyExecutor(Service);
			using ExecutionTimer executionTimer = new ExecutionTimer();
			hardwareSurveyExecutor.Run();
			AddItem("Timestamp", hardwareSurveyExecutor.Timestamp.ToString());
			AddItem("Camera", hardwareSurveyExecutor.CameraVersion);
			string col = hardwareSurveyExecutor.TouchscreenFirmware;
			try
			{
				Version version = new Version(hardwareSurveyExecutor.TouchscreenFirmware);
				bool flag = version.Major == 4 && version.Minor == 30;
				col = string.Format("{0} ( {1} CURRENT )", hardwareSurveyExecutor.TouchscreenFirmware, flag ? "MOST" : "NOT MOST");
			}
			catch (Exception)
			{
			}
			AddItem("Touchscreen firmware", col);
			AddItem("Touchscreen model", hardwareSurveyExecutor.Touchscreen);
			AddItem("ABE device", hardwareSurveyExecutor.ABEDevice.ToString());
			AddItem("Fraud sensor", hardwareSurveyExecutor.FraudDevice.ToString());
			AddItem("AuxRelay board", hardwareSurveyExecutor.HasAuxRelayBoard ? "Yes" : "No");
			AddItem("Air exchanger", hardwareSurveyExecutor.AirExchanger.ToString());
			AddItem("Quick return", hardwareSurveyExecutor.QuickReturn.ToString());
			AddItem("Disk free space", $"{hardwareSurveyExecutor.FreeDiskSpace >> 30} GB");
			AddItem("Memory", $"{hardwareSurveyExecutor.Memory >> 20} GB");
			AddItem("PC Model", hardwareSurveyExecutor.PcModel);
			AddItem("PC Manufacturer", hardwareSurveyExecutor.PcManufacturer);
			AddItem("UPS", hardwareSurveyExecutor.UpsModel);
			AddItem("Monitor Model", hardwareSurveyExecutor.Monitor);
			AddItem("Serial controller version", hardwareSurveyExecutor.SerialControllerVersion);
			executionTimer.Stop();
			LogHelper.Instance.Log("[{0}] Hardware survey execution time = {1}", GetType().Name, executionTimer.Elapsed);
		}
	}

	private void AddItem(string col1, string col2)
	{
		ListViewItem value = new ListViewItem(new string[2] { col1, col2 });
		listView1.Items.Add(value);
	}

	private void button16_Click_2(object sender, EventArgs e)
	{
		listView1.Items.Clear();
	}

	private void m_configureRouterRelay_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = !HasRouterPowerRelay;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"SETCFG \"RouterPowerCyclePause\" \"{(flag ? RouterPowerCyclePause : 0)}\" TYPE=CONTROLLER");
			if (flag)
			{
				stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
			}
			stringBuilder.AppendLine(" CLEAR");
			HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			if (!hardwareCommandResult.Success)
			{
				DumpResult(hardwareCommandResult, HardwareStatsOutput);
				HardwareStatsOutput.Write("Failed to update configuration.");
			}
			else
			{
				HasRouterPowerRelay = flag;
				ConfigureRouterGroup();
			}
		}
	}

	private void ConfigureRouterGroup()
	{
		m_configureRouterRelay.Text = (HasRouterPowerRelay ? "Disable" : "Enable");
		HardwareStatsOutput.Write("Router relay is {0}.", HasRouterPowerRelay ? "configured" : "not configured");
		m_testRouterRelay.Enabled = HasRouterPowerRelay;
	}

	private void ConfigureArcusGroup()
	{
		m_restartArcus.Enabled = ArcusResetConfigured;
		m_configureArcus.Text = (ArcusResetConfigured ? "Disable" : "Enable");
		HardwareStatsOutput.Write("Kiosk is {0} to reset the Arcus.", ArcusResetConfigured ? "configured" : "not configured");
	}

	private void m_testRouterRelay_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			HardwareCommandResult hardwareCommandResult = Service.PowerCycleRouter(new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out var job);
			if (hardwareCommandResult.Success)
			{
				using (ClientHelper clientHelper = new ClientHelper(Service))
				{
					clientHelper.WaitForJob(job, out var endStatus);
					HardwareStatsOutput.Write("Power cycle router job {0} ended with status {1}", job.ID, endStatus);
					return;
				}
			}
			DumpResult(hardwareCommandResult, HardwareStatsOutput);
			HardwareStatsOutput.Write("Failed to schedule router power cycle job.");
		}
	}

	private void button24_Click(object sender, EventArgs e)
	{
		HardwareCorrectionStatistic hardwareCorrectionStatistic = HardwareCorrectionStatistic.None;
		foreach (Control control in m_hwCorrectiontypeGB.Controls)
		{
			if (control.GetType() == typeof(RadioButton))
			{
				RadioButton radioButton = (RadioButton)control;
				if (radioButton.Checked)
				{
					hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase((string)radioButton.Tag, HardwareCorrectionStatistic.None);
					break;
				}
			}
		}
		if (hardwareCorrectionStatistic != HardwareCorrectionStatistic.None)
		{
			using (GetHardwareCorrectionStatistics getHardwareCorrectionStatistics = new GetHardwareCorrectionStatistics(Service, hardwareCorrectionStatistic))
			{
				getHardwareCorrectionStatistics.Run();
				if (HardwareJobStatus.Completed != getHardwareCorrectionStatistics.EndStatus)
				{
					HardwareStatsOutput.Write("Failed to get statistics from service.");
				}
				else
				{
					DumpHardwareStats(getHardwareCorrectionStatistics.Stats);
				}
				return;
			}
		}
		using GetAllHardwareCorrectionStatistics getAllHardwareCorrectionStatistics = new GetAllHardwareCorrectionStatistics(Service);
		getAllHardwareCorrectionStatistics.Run();
		if (HardwareJobStatus.Completed != getAllHardwareCorrectionStatistics.EndStatus)
		{
			HardwareStatsOutput.Write("Failed to get statistics from service.");
		}
		else
		{
			DumpHardwareStats(getAllHardwareCorrectionStatistics.Stats);
		}
	}

	private void DumpHardwareStats(List<IHardwareCorrectionStatistic> stats)
	{
		stats.ForEach(delegate(IHardwareCorrectionStatistic s)
		{
			HardwareStatsOutput.Write("{0} {1}: {2} {3}", s.CorrectionTime, s.Statistic, s.ProgramName, s.CorrectionOk ? "SUCCESS" : "FAILURE ");
		});
		HardwareStatsOutput.Write("Total of {0} statistics", stats.Count);
	}

	private void button23_Click(object sender, EventArgs e)
	{
		HardwareStatsOutput.Clear();
	}

	private void button27_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			CompositeFunctions.VendDisk(Service, EngineeringOutput);
		}
	}

	private void button28_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			ScanResult scanResult = ScanResult.ReadBarcodeOfDiskInPicker(Service);
			EngineeringOutput.Write(scanResult.ToString());
		}
	}

	private void button26_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			if (!Service.ScheduleJob("ms-take-disk-at-door", string.Empty, enableDebugging: false, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out var job).Success)
			{
				EngineeringOutput.Write("Failed to schedule job");
				return;
			}
			job.Pend();
			using ClientHelper clientHelper = new ClientHelper(Service);
			clientHelper.WaitForJob(job, out var endStatus);
			if (endStatus != HardwareJobStatus.Completed)
			{
				return;
			}
			if (!job.GetResults(out var results).Success)
			{
				EngineeringOutput.Write("Failed to get program results. I'm sorry!");
				return;
			}
			ProgramResult[] array = results;
			foreach (ProgramResult programResult in array)
			{
				EngineeringOutput.Write(programResult.Message);
			}
		}
	}

	private void button25_Click(object sender, EventArgs e)
	{
		EngineeringOutput.Clear();
	}

	private void button29_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				EngineeringOutput.Write("Please specify source deck/slot properly");
				return;
			}
			int destinationDeck = DestinationDeck;
			int destinationSlot = DestinationSlot;
			if (-1 == destinationDeck || -1 == destinationSlot)
			{
				EngineeringOutput.Write("Please specify destination deck/slot properly");
				return;
			}
			SyncRange range = new SyncRange(sourceDeck, destinationDeck, new SlotRange(sourceSlot, destinationSlot));
			if (!Service.HardSync(range, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out var job).Success)
			{
				EngineeringOutput.Write("Unable to communicate with HAL.");
				return;
			}
			job.Pend();
			job.WaitForCompletion(300000);
			if (job.GetSymbols(out var symbols).Success)
			{
				foreach (string key in symbols.Keys)
				{
					if (key.StartsWith("MSTESTER-SYMBOL"))
					{
						EngineeringOutput.Write(symbols[key]);
					}
				}
			}
			if (!job.GetResults(out var results).Success)
			{
				EngineeringOutput.Write("Failed to get program results. I'm sorry!");
				return;
			}
			ProgramResult[] array = results;
			foreach (ProgramResult programResult in array)
			{
				EngineeringOutput.Write(programResult.Message);
			}
		}
	}

	private void button34_Click(object sender, EventArgs e)
	{
		m_engSlotTB.InputNumber();
	}

	private void button33_Click(object sender, EventArgs e)
	{
		m_engDeckTB.InputNumber();
	}

	private void button31_Click(object sender, EventArgs e)
	{
		m_engEndDeckTB.InputNumber();
	}

	private void button32_Click(object sender, EventArgs e)
	{
		m_engEndSlotTB.InputNumber();
	}

	private void button30_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				EngineeringOutput.Write("Please specify deck/slot properly");
				return;
			}
			List<Location> locations = new List<Location>
			{
				new Location
				{
					Deck = sourceDeck,
					Slot = sourceSlot
				}
			};
			if (!Service.HardSync(locations, "Engineering Location Sync", new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out var job).Success)
			{
				EngineeringOutput.Write("Unable to communicate with HAL.");
				return;
			}
			job.Pend();
			job.WaitForCompletion(300000);
			if (job.GetSymbols(out var symbols).Success)
			{
				foreach (string key in symbols.Keys)
				{
					if (key.StartsWith("MSTESTER-SYMBOL"))
					{
						EngineeringOutput.Write(symbols[key]);
					}
				}
			}
			if (!job.GetResults(out var results).Success)
			{
				EngineeringOutput.Write("Failed to get program results. I'm sorry!");
				return;
			}
			ProgramResult[] array = results;
			foreach (ProgramResult programResult in array)
			{
				EngineeringOutput.Write(programResult.Message);
			}
		}
	}

	private void button35_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				EngineeringOutput.Write("Please specify deck/slot properly");
			}
			else
			{
				CompositeFunctions.GetItem(sourceDeck, sourceSlot, EngineeringOutput, Service);
			}
		}
	}

	private void button36_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				EngineeringOutput.Write("Please specify deck/slot properly");
			}
			else
			{
				CompositeFunctions.PutItem(Service, sourceDeck, sourceSlot, OutputBox);
			}
		}
	}

	private void m_restartTouchscreen_Click(object sender, EventArgs e)
	{
		if (Service.ResetTouchscreenController(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job).Success)
		{
			using (ClientHelper clientHelper = new ClientHelper(Service))
			{
				clientHelper.WaitForJob(job, out var endStatus);
				HardwareStatsOutput.Write("Reset touchscreen controller ended with status {0}", endStatus.ToString());
				return;
			}
		}
		HardwareStatsOutput.Write("Unable to schedule reset touchscreen job.");
	}

	private void m_restartArcus_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using ResetMotionControlExecutor resetMotionControlExecutor = new ResetMotionControlExecutor(Service);
			resetMotionControlExecutor.Run();
			if (resetMotionControlExecutor.EndStatus != HardwareJobStatus.Completed)
			{
				HardwareStatsOutput.Write("ERROR: reset-arcus failed.");
				return;
			}
			foreach (ProgramResult result in resetMotionControlExecutor.Results)
			{
				HardwareStatsOutput.Write(result.Message);
			}
		}
	}

	private void m_configureArcus_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = !ArcusResetConfigured;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"SETCFG \"RestartControllerDuringUserJobs\" \"{flag}\" TYPE=CONTROLLER");
			if (flag)
			{
				stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
			}
			stringBuilder.AppendLine("CLEAR");
			HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			if (!hardwareCommandResult.Success)
			{
				DumpResult(hardwareCommandResult, HardwareStatsOutput);
				HardwareStatsOutput.Write("Failed to update configuration.");
			}
			else
			{
				ArcusResetConfigured = flag;
				ConfigureArcusGroup();
			}
		}
	}

	private void button37_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = !HasRouterPowerRelay;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"SETCFG \"RouterPowerCyclePause\" \"{(flag ? RouterPowerCyclePause : 0)}\" TYPE=CONTROLLER");
			if (flag)
			{
				stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
			}
			stringBuilder.AppendLine(" CLEAR");
			HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			if (!hardwareCommandResult.Success)
			{
				DumpResult(hardwareCommandResult, HardwareStatsOutput);
				HardwareStatsOutput.Write("Failed to update configuration.");
			}
			else
			{
				HasRouterPowerRelay = flag;
				ConfigureRouterGroup();
			}
		}
	}

	private void button38_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			HardwareCommandResult hardwareCommandResult = Service.PowerCycleRouter(new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out var job);
			if (hardwareCommandResult.Success)
			{
				using (ClientHelper clientHelper = new ClientHelper(Service))
				{
					clientHelper.WaitForJob(job, out var endStatus);
					HardwareStatsOutput.Write("Power cycle router job {0} ended with status {1}", job.ID, endStatus);
					return;
				}
			}
			DumpResult(hardwareCommandResult, HardwareStatsOutput);
			HardwareStatsOutput.Write("Failed to schedule router power cycle job.");
		}
	}

	private void ToggleKfcButtons(bool enable)
	{
		Button button = btn_KFCCheckDrivers;
		Button button2 = btn_KFCDecodeTest;
		Button button3 = btn_KFCTestVendDoor;
		Button button4 = btn_KFCInit;
		Button button5 = btn_KFCVerticalSlotTest;
		bool flag = (btn_KFCUnknownCount.Enabled = enable);
		bool flag3 = (button5.Enabled = flag);
		bool flag5 = (button4.Enabled = flag3);
		bool flag7 = (button3.Enabled = flag5);
		bool enabled = (button2.Enabled = flag7);
		button.Enabled = enabled;
	}

	private void m_verticalSyncWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		int slot = (int)e.Argument;
		e.Result = FunctionTest.RunVertialSlotTest(slot).ToString().ToUpper();
	}

	private void m_verticalSyncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		ToggleKfcButtons(enable: true);
		m_verticalTestTB.Text = e.Result as string;
		btn_KFCVerticalSlotTest.BackColor = Color.LightGray;
	}

	private void m_runInitWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		e.Result = FunctionTest.RunInit().ToString().ToUpper();
	}

	private void m_runInitWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		ToggleKfcButtons(enable: true);
		btn_KFCInit.BackColor = Color.LightGray;
		m_initTB.Text = e.Result as string;
	}

	private void m_runDecodeWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		Location loc = e.Argument as Location;
		e.Result = FunctionTest.TestCameraSnap(loc).ToString().ToUpper();
	}

	private void m_runDecodeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		ToggleKfcButtons(enable: true);
		btn_KFCDecodeTest.BackColor = Color.LightGray;
		m_cameraSnapTB.Text = e.Result as string;
	}

	private void button37_Click_1(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = !m_irConfigured;
			DateTime dateTime = DateTime.Now.ToUniversalTime();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format("SETCFG \"IRHardwareInstallDate\" \"{0}\" TYPE=KIOSK", flag ? dateTime.ToString() : "NONE"));
			stringBuilder.AppendLine("CLEAR");
			HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			if (!hardwareCommandResult.Success)
			{
				hardwareCommandResult.Errors.ForEach(delegate(Error err)
				{
					IrOutput.Write(err.ToString());
				});
				IrOutput.Write("Failed to update configuration.");
			}
			else
			{
				m_irConfigured = flag;
				ConfigureIrCameraGroup();
			}
		}
	}

	private void OnExecuteResponseInstruction(object sender, EventArgs e)
	{
		using ButtonAspects buttonAspects = Manager.MakeAspect(sender);
		string tagInstruction = buttonAspects.GetTagInstruction();
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		string text = instructionHelper.ExecuteGeneric(tagInstruction);
		IrOutput.Write("{0} - {1}", tagInstruction, text);
	}

	private void button46_Click(object sender, EventArgs e)
	{
		textBox1.InputNumber();
	}

	private void button47_Click(object sender, EventArgs e)
	{
		textBox2.InputNumber();
	}

	private void button43_Click(object sender, EventArgs e)
	{
		int? getDeck = GetDeck;
		int? getSlot = GetSlot;
		if (!getDeck.HasValue || !getSlot.HasValue)
		{
			return;
		}
		CompositeFunctions.GetItem(getDeck.Value, getSlot.Value, IrOutput, Service);
		using (Manager.MakeAspect(sender))
		{
			ScanResult scanResult = ScanResult.ReadBarcodeOfDiskInPicker(Service);
			IrOutput.Write($"found {scanResult.SecureCount} secure codes.");
			IrOutput.Write(scanResult.ToString());
		}
	}

	private void button44_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int? getDeck = GetDeck;
			int? getSlot = GetSlot;
			if (getDeck.HasValue && getSlot.HasValue)
			{
				CompositeFunctions.PutItem(Service, getDeck.Value, getSlot.Value, IrOutput);
			}
		}
	}

	private void button42_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			TunerLaunchService.LaunchTunerAndWait(Service, Manager);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.m_devicesTab = new System.Windows.Forms.TabControl();
		this.m_airExchangerTab = new System.Windows.Forms.TabPage();
		this.button10 = new System.Windows.Forms.Button();
		this.m_iceQubeOutput = new System.Windows.Forms.ListBox();
		this.m_resetPersistentCounter = new System.Windows.Forms.Button();
		this.m_qubeStatusButton = new System.Windows.Forms.Button();
		this.m_resetQubeBoard = new System.Windows.Forms.Button();
		this.m_configureQubeButton = new System.Windows.Forms.Button();
		this.m_iceQubeExchangerLabel = new System.Windows.Forms.Label();
		this.m_cortexTab = new System.Windows.Forms.TabPage();
		this.m_startCortexButton = new System.Windows.Forms.Button();
		this.m_cortexStatusBox = new System.Windows.Forms.ListBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_uninstallCortexButton = new System.Windows.Forms.Button();
		this.m_installCortexButton = new System.Windows.Forms.Button();
		this.m_fraudSensorTab = new System.Windows.Forms.TabPage();
		this.button15 = new System.Windows.Forms.Button();
		this.button14 = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.m_readPauseTB = new System.Windows.Forms.TextBox();
		this.m_scanPauseTB = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_stressIterationsTB = new System.Windows.Forms.TextBox();
		this.button13 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button7 = new System.Windows.Forms.Button();
		this.button6 = new System.Windows.Forms.Button();
		this.button5 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.m_fraudSlotTB = new System.Windows.Forms.TextBox();
		this.m_fraudDeckTB = new System.Windows.Forms.TextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_powerOffFraudSensor = new System.Windows.Forms.Button();
		this.m_powerOnFraudSensor = new System.Windows.Forms.Button();
		this.m_testDiskForMarkers = new System.Windows.Forms.Button();
		this.m_stopFraudScan = new System.Windows.Forms.Button();
		this.m_startFraudScan = new System.Windows.Forms.Button();
		this.m_resetFraudButton = new System.Windows.Forms.Button();
		this.m_testFraudButton = new System.Windows.Forms.Button();
		this.m_fraudPOSTButton = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.m_configureFraudSensor = new System.Windows.Forms.Button();
		this.m_fraudSensorOutput = new System.Windows.Forms.ListBox();
		this.m_kioskFunctionCheck = new System.Windows.Forms.TabPage();
		this.m_displaySessionsButton = new System.Windows.Forms.Button();
		this.m_sourceSlotTB = new System.Windows.Forms.TextBox();
		this.button19 = new System.Windows.Forms.Button();
		this.m_sourceDeckTB = new System.Windows.Forms.TextBox();
		this.button22 = new System.Windows.Forms.Button();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_unknownCountTB = new System.Windows.Forms.TextBox();
		this.m_cameraSnapTB = new System.Windows.Forms.TextBox();
		this.btn_KFCDecodeTest = new System.Windows.Forms.Button();
		this.m_verticalTestTB = new System.Windows.Forms.TextBox();
		this.m_initTB = new System.Windows.Forms.TextBox();
		this.m_testVendDoorTB = new System.Windows.Forms.TextBox();
		this.m_checkDriversStatusTB = new System.Windows.Forms.TextBox();
		this.btn_KFCUnknownCount = new System.Windows.Forms.Button();
		this.btn_KFCVerticalSlotTest = new System.Windows.Forms.Button();
		this.btn_KFCTestVendDoor = new System.Windows.Forms.Button();
		this.btn_KFCInit = new System.Windows.Forms.Button();
		this.btn_KFCCheckDrivers = new System.Windows.Forms.Button();
		this.m_userName = new System.Windows.Forms.TextBox();
		this.button21 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.button2 = new System.Windows.Forms.Button();
		this.m_kioskTestOutput = new System.Windows.Forms.ListBox();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.listView1 = new System.Windows.Forms.ListView();
		this.button16 = new System.Windows.Forms.Button();
		this.button11 = new System.Windows.Forms.Button();
		this.m_hwCorrections = new System.Windows.Forms.TabPage();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.m_testRouterRelay = new System.Windows.Forms.Button();
		this.m_configureRouterRelay = new System.Windows.Forms.Button();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.m_restartArcus = new System.Windows.Forms.Button();
		this.m_configureArcus = new System.Windows.Forms.Button();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.m_restartTouchscreen = new System.Windows.Forms.Button();
		this.m_hwCorrectiontypeGB = new System.Windows.Forms.GroupBox();
		this.radioButton5 = new System.Windows.Forms.RadioButton();
		this.radioButton4 = new System.Windows.Forms.RadioButton();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.radioButton3 = new System.Windows.Forms.RadioButton();
		this.button24 = new System.Windows.Forms.Button();
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.button23 = new System.Windows.Forms.Button();
		this.m_hwStatsOutput = new System.Windows.Forms.ListBox();
		this.m_irConfigurationTab = new System.Windows.Forms.TabPage();
		this.button37 = new System.Windows.Forms.Button();
		this.groupBox8 = new System.Windows.Forms.GroupBox();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.button47 = new System.Windows.Forms.Button();
		this.button46 = new System.Windows.Forms.Button();
		this.button45 = new System.Windows.Forms.Button();
		this.button44 = new System.Windows.Forms.Button();
		this.button43 = new System.Windows.Forms.Button();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.button42 = new System.Windows.Forms.Button();
		this.button41 = new System.Windows.Forms.Button();
		this.button40 = new System.Windows.Forms.Button();
		this.button39 = new System.Windows.Forms.Button();
		this.button38 = new System.Windows.Forms.Button();
		this.m_irCameraOutput = new System.Windows.Forms.ListBox();
		this.m_engineeringTab = new System.Windows.Forms.TabPage();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.button36 = new System.Windows.Forms.Button();
		this.button35 = new System.Windows.Forms.Button();
		this.button34 = new System.Windows.Forms.Button();
		this.button29 = new System.Windows.Forms.Button();
		this.button30 = new System.Windows.Forms.Button();
		this.button33 = new System.Windows.Forms.Button();
		this.button32 = new System.Windows.Forms.Button();
		this.m_engEndSlotTB = new System.Windows.Forms.TextBox();
		this.button31 = new System.Windows.Forms.Button();
		this.m_engEndDeckTB = new System.Windows.Forms.TextBox();
		this.m_engDeckTB = new System.Windows.Forms.TextBox();
		this.m_engSlotTB = new System.Windows.Forms.TextBox();
		this.button28 = new System.Windows.Forms.Button();
		this.button27 = new System.Windows.Forms.Button();
		this.button26 = new System.Windows.Forms.Button();
		this.button25 = new System.Windows.Forms.Button();
		this.m_engineeringOutput = new System.Windows.Forms.ListBox();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_verticalSyncWorker = new System.ComponentModel.BackgroundWorker();
		this.m_runInitWorker = new System.ComponentModel.BackgroundWorker();
		this.m_runDecodeWorker = new System.ComponentModel.BackgroundWorker();
		this.m_devicesTab.SuspendLayout();
		this.m_airExchangerTab.SuspendLayout();
		this.m_cortexTab.SuspendLayout();
		this.m_fraudSensorTab.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.m_kioskFunctionCheck.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.tabPage1.SuspendLayout();
		this.m_hwCorrections.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox6.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.m_hwCorrectiontypeGB.SuspendLayout();
		this.m_irConfigurationTab.SuspendLayout();
		this.groupBox8.SuspendLayout();
		this.groupBox7.SuspendLayout();
		this.m_engineeringTab.SuspendLayout();
		this.groupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		base.SuspendLayout();
		this.m_devicesTab.Controls.Add(this.m_airExchangerTab);
		this.m_devicesTab.Controls.Add(this.m_cortexTab);
		this.m_devicesTab.Controls.Add(this.m_fraudSensorTab);
		this.m_devicesTab.Controls.Add(this.m_kioskFunctionCheck);
		this.m_devicesTab.Controls.Add(this.tabPage1);
		this.m_devicesTab.Controls.Add(this.m_hwCorrections);
		this.m_devicesTab.Controls.Add(this.m_irConfigurationTab);
		this.m_devicesTab.Controls.Add(this.m_engineeringTab);
		this.m_devicesTab.Location = new System.Drawing.Point(2, 12);
		this.m_devicesTab.Name = "m_devicesTab";
		this.m_devicesTab.Padding = new System.Drawing.Point(10, 20);
		this.m_devicesTab.SelectedIndex = 0;
		this.m_devicesTab.Size = new System.Drawing.Size(1057, 626);
		this.m_devicesTab.TabIndex = 0;
		this.m_airExchangerTab.BackColor = System.Drawing.Color.LightGray;
		this.m_airExchangerTab.Controls.Add(this.button10);
		this.m_airExchangerTab.Controls.Add(this.m_iceQubeOutput);
		this.m_airExchangerTab.Controls.Add(this.m_resetPersistentCounter);
		this.m_airExchangerTab.Controls.Add(this.m_qubeStatusButton);
		this.m_airExchangerTab.Controls.Add(this.m_resetQubeBoard);
		this.m_airExchangerTab.Controls.Add(this.m_configureQubeButton);
		this.m_airExchangerTab.Controls.Add(this.m_iceQubeExchangerLabel);
		this.m_airExchangerTab.Location = new System.Drawing.Point(4, 56);
		this.m_airExchangerTab.Name = "m_airExchangerTab";
		this.m_airExchangerTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_airExchangerTab.Size = new System.Drawing.Size(1049, 566);
		this.m_airExchangerTab.TabIndex = 3;
		this.m_airExchangerTab.Text = "Ice Qube Air Exchanger";
		this.button10.Location = new System.Drawing.Point(26, 473);
		this.button10.Name = "button10";
		this.button10.Size = new System.Drawing.Size(120, 65);
		this.button10.TabIndex = 8;
		this.button10.Text = "Clear output";
		this.button10.UseVisualStyleBackColor = true;
		this.button10.Click += new System.EventHandler(button10_Click);
		this.m_iceQubeOutput.FormattingEnabled = true;
		this.m_iceQubeOutput.Location = new System.Drawing.Point(26, 164);
		this.m_iceQubeOutput.Name = "m_iceQubeOutput";
		this.m_iceQubeOutput.Size = new System.Drawing.Size(436, 290);
		this.m_iceQubeOutput.TabIndex = 7;
		this.m_resetPersistentCounter.Location = new System.Drawing.Point(528, 73);
		this.m_resetPersistentCounter.Name = "m_resetPersistentCounter";
		this.m_resetPersistentCounter.Size = new System.Drawing.Size(120, 65);
		this.m_resetPersistentCounter.TabIndex = 6;
		this.m_resetPersistentCounter.Tag = "RESETFAILURECOUNTER";
		this.m_resetPersistentCounter.Text = "Clear Error Counter";
		this.m_resetPersistentCounter.UseVisualStyleBackColor = true;
		this.m_resetPersistentCounter.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_qubeStatusButton.Location = new System.Drawing.Point(188, 73);
		this.m_qubeStatusButton.Name = "m_qubeStatusButton";
		this.m_qubeStatusButton.Size = new System.Drawing.Size(120, 65);
		this.m_qubeStatusButton.TabIndex = 3;
		this.m_qubeStatusButton.Tag = "BOARDSTATUS";
		this.m_qubeStatusButton.Text = "Exchanger Board Status";
		this.m_qubeStatusButton.UseVisualStyleBackColor = true;
		this.m_qubeStatusButton.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_resetQubeBoard.Location = new System.Drawing.Point(363, 73);
		this.m_resetQubeBoard.Name = "m_resetQubeBoard";
		this.m_resetQubeBoard.Size = new System.Drawing.Size(120, 65);
		this.m_resetQubeBoard.TabIndex = 2;
		this.m_resetQubeBoard.Tag = "RESET";
		this.m_resetQubeBoard.Text = "Reset Board";
		this.m_resetQubeBoard.UseVisualStyleBackColor = true;
		this.m_resetQubeBoard.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_configureQubeButton.Location = new System.Drawing.Point(26, 73);
		this.m_configureQubeButton.Name = "m_configureQubeButton";
		this.m_configureQubeButton.Size = new System.Drawing.Size(120, 65);
		this.m_configureQubeButton.TabIndex = 1;
		this.m_configureQubeButton.Text = "button1";
		this.m_configureQubeButton.UseVisualStyleBackColor = true;
		this.m_configureQubeButton.Click += new System.EventHandler(m_configureQubeButton_Click);
		this.m_iceQubeExchangerLabel.AutoSize = true;
		this.m_iceQubeExchangerLabel.Location = new System.Drawing.Point(23, 27);
		this.m_iceQubeExchangerLabel.Name = "m_iceQubeExchangerLabel";
		this.m_iceQubeExchangerLabel.Size = new System.Drawing.Size(35, 13);
		this.m_iceQubeExchangerLabel.TabIndex = 0;
		this.m_iceQubeExchangerLabel.Text = "label1";
		this.m_cortexTab.BackColor = System.Drawing.Color.LightGray;
		this.m_cortexTab.Controls.Add(this.m_startCortexButton);
		this.m_cortexTab.Controls.Add(this.m_cortexStatusBox);
		this.m_cortexTab.Controls.Add(this.label3);
		this.m_cortexTab.Controls.Add(this.m_uninstallCortexButton);
		this.m_cortexTab.Controls.Add(this.m_installCortexButton);
		this.m_cortexTab.Location = new System.Drawing.Point(4, 56);
		this.m_cortexTab.Name = "m_cortexTab";
		this.m_cortexTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_cortexTab.Size = new System.Drawing.Size(1049, 566);
		this.m_cortexTab.TabIndex = 4;
		this.m_cortexTab.Text = "Cortex Camera";
		this.m_startCortexButton.Enabled = false;
		this.m_startCortexButton.Location = new System.Drawing.Point(375, 51);
		this.m_startCortexButton.Name = "m_startCortexButton";
		this.m_startCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_startCortexButton.TabIndex = 4;
		this.m_startCortexButton.Text = "Start Camera";
		this.m_startCortexButton.UseVisualStyleBackColor = true;
		this.m_startCortexButton.Click += new System.EventHandler(button1_Click);
		this.m_cortexStatusBox.FormattingEnabled = true;
		this.m_cortexStatusBox.Location = new System.Drawing.Point(72, 161);
		this.m_cortexStatusBox.Name = "m_cortexStatusBox";
		this.m_cortexStatusBox.Size = new System.Drawing.Size(453, 290);
		this.m_cortexStatusBox.TabIndex = 3;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(16, 161);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(40, 13);
		this.label3.TabIndex = 2;
		this.label3.Text = "Status:";
		this.m_uninstallCortexButton.Location = new System.Drawing.Point(189, 51);
		this.m_uninstallCortexButton.Name = "m_uninstallCortexButton";
		this.m_uninstallCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_uninstallCortexButton.TabIndex = 1;
		this.m_uninstallCortexButton.Text = "Uninstall";
		this.m_uninstallCortexButton.UseVisualStyleBackColor = true;
		this.m_uninstallCortexButton.Click += new System.EventHandler(m_uninstallCortexButton_Click);
		this.m_installCortexButton.Location = new System.Drawing.Point(19, 51);
		this.m_installCortexButton.Name = "m_installCortexButton";
		this.m_installCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_installCortexButton.TabIndex = 0;
		this.m_installCortexButton.Text = "Install";
		this.m_installCortexButton.UseVisualStyleBackColor = true;
		this.m_installCortexButton.Click += new System.EventHandler(m_installCortexButton_Click);
		this.m_fraudSensorTab.BackColor = System.Drawing.Color.LightGray;
		this.m_fraudSensorTab.Controls.Add(this.button15);
		this.m_fraudSensorTab.Controls.Add(this.button14);
		this.m_fraudSensorTab.Controls.Add(this.label5);
		this.m_fraudSensorTab.Controls.Add(this.m_readPauseTB);
		this.m_fraudSensorTab.Controls.Add(this.m_scanPauseTB);
		this.m_fraudSensorTab.Controls.Add(this.label4);
		this.m_fraudSensorTab.Controls.Add(this.label2);
		this.m_fraudSensorTab.Controls.Add(this.m_stressIterationsTB);
		this.m_fraudSensorTab.Controls.Add(this.button13);
		this.m_fraudSensorTab.Controls.Add(this.button3);
		this.m_fraudSensorTab.Controls.Add(this.button7);
		this.m_fraudSensorTab.Controls.Add(this.button6);
		this.m_fraudSensorTab.Controls.Add(this.button5);
		this.m_fraudSensorTab.Controls.Add(this.button4);
		this.m_fraudSensorTab.Controls.Add(this.m_fraudSlotTB);
		this.m_fraudSensorTab.Controls.Add(this.m_fraudDeckTB);
		this.m_fraudSensorTab.Controls.Add(this.groupBox1);
		this.m_fraudSensorTab.Controls.Add(this.button1);
		this.m_fraudSensorTab.Controls.Add(this.m_configureFraudSensor);
		this.m_fraudSensorTab.Controls.Add(this.m_fraudSensorOutput);
		this.m_fraudSensorTab.Location = new System.Drawing.Point(4, 56);
		this.m_fraudSensorTab.Name = "m_fraudSensorTab";
		this.m_fraudSensorTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_fraudSensorTab.Size = new System.Drawing.Size(1049, 566);
		this.m_fraudSensorTab.TabIndex = 5;
		this.m_fraudSensorTab.Text = "Fraud Sensor";
		this.button15.Enabled = false;
		this.button15.Location = new System.Drawing.Point(670, 435);
		this.button15.Name = "button15";
		this.button15.Size = new System.Drawing.Size(99, 45);
		this.button15.TabIndex = 26;
		this.button15.Text = "Run Detection Test";
		this.button15.UseVisualStyleBackColor = true;
		this.button15.Visible = false;
		this.button15.Click += new System.EventHandler(button15_Click);
		this.button14.Location = new System.Drawing.Point(510, 428);
		this.button14.Name = "button14";
		this.button14.Size = new System.Drawing.Size(120, 52);
		this.button14.TabIndex = 25;
		this.button14.Text = "Center Disk";
		this.button14.UseVisualStyleBackColor = true;
		this.button14.Click += new System.EventHandler(button14_Click);
		this.label5.AutoSize = true;
		this.label5.Enabled = false;
		this.label5.Location = new System.Drawing.Point(801, 516);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(110, 13);
		this.label5.TabIndex = 24;
		this.label5.Text = "Pause between reads";
		this.label5.Visible = false;
		this.m_readPauseTB.Enabled = false;
		this.m_readPauseTB.Location = new System.Drawing.Point(798, 532);
		this.m_readPauseTB.Name = "m_readPauseTB";
		this.m_readPauseTB.Size = new System.Drawing.Size(100, 20);
		this.m_readPauseTB.TabIndex = 23;
		this.m_readPauseTB.Visible = false;
		this.m_scanPauseTB.Enabled = false;
		this.m_scanPauseTB.Location = new System.Drawing.Point(798, 493);
		this.m_scanPauseTB.Name = "m_scanPauseTB";
		this.m_scanPauseTB.Size = new System.Drawing.Size(100, 20);
		this.m_scanPauseTB.TabIndex = 22;
		this.m_scanPauseTB.Visible = false;
		this.label4.AutoSize = true;
		this.label4.Enabled = false;
		this.label4.Location = new System.Drawing.Point(801, 477);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(65, 13);
		this.label4.TabIndex = 21;
		this.label4.Text = "Scan Pause";
		this.label4.Visible = false;
		this.label2.AutoSize = true;
		this.label2.Enabled = false;
		this.label2.Location = new System.Drawing.Point(801, 435);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(76, 13);
		this.label2.TabIndex = 20;
		this.label2.Text = "Test iterations:";
		this.label2.Visible = false;
		this.m_stressIterationsTB.Enabled = false;
		this.m_stressIterationsTB.Location = new System.Drawing.Point(798, 454);
		this.m_stressIterationsTB.Name = "m_stressIterationsTB";
		this.m_stressIterationsTB.Size = new System.Drawing.Size(100, 20);
		this.m_stressIterationsTB.TabIndex = 19;
		this.m_stressIterationsTB.Visible = false;
		this.button13.Location = new System.Drawing.Point(670, 495);
		this.button13.Name = "button13";
		this.button13.Size = new System.Drawing.Size(99, 65);
		this.button13.TabIndex = 18;
		this.button13.Tag = "SERIALBOARD RESET";
		this.button13.Text = "Initialize Control System";
		this.button13.UseVisualStyleBackColor = true;
		this.button13.Click += new System.EventHandler(button7_Click);
		this.button3.Location = new System.Drawing.Point(6, 454);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(120, 65);
		this.button3.TabIndex = 17;
		this.button3.Text = "Clear Output";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(button3_Click_1);
		this.button7.Location = new System.Drawing.Point(510, 493);
		this.button7.Name = "button7";
		this.button7.Size = new System.Drawing.Size(120, 65);
		this.button7.TabIndex = 16;
		this.button7.Tag = "SERIALBOARD CLOSEPORT";
		this.button7.Text = "Shutdown Control System";
		this.button7.UseVisualStyleBackColor = true;
		this.button7.Click += new System.EventHandler(button7_Click);
		this.button6.Location = new System.Drawing.Point(670, 372);
		this.button6.Name = "button6";
		this.button6.Size = new System.Drawing.Size(75, 50);
		this.button6.TabIndex = 15;
		this.button6.Text = "Slot";
		this.button6.UseVisualStyleBackColor = true;
		this.button6.Click += new System.EventHandler(button6_Click);
		this.button5.Location = new System.Drawing.Point(670, 316);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(75, 50);
		this.button5.TabIndex = 14;
		this.button5.Text = "Deck";
		this.button5.UseVisualStyleBackColor = true;
		this.button5.Click += new System.EventHandler(button5_Click_1);
		this.button4.Location = new System.Drawing.Point(510, 372);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(120, 50);
		this.button4.TabIndex = 13;
		this.button4.Text = "Put";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(button4_Click_1);
		this.m_fraudSlotTB.Location = new System.Drawing.Point(751, 402);
		this.m_fraudSlotTB.Name = "m_fraudSlotTB";
		this.m_fraudSlotTB.Size = new System.Drawing.Size(100, 20);
		this.m_fraudSlotTB.TabIndex = 12;
		this.m_fraudDeckTB.Location = new System.Drawing.Point(751, 346);
		this.m_fraudDeckTB.Name = "m_fraudDeckTB";
		this.m_fraudDeckTB.Size = new System.Drawing.Size(100, 20);
		this.m_fraudDeckTB.TabIndex = 8;
		this.groupBox1.Controls.Add(this.m_powerOffFraudSensor);
		this.groupBox1.Controls.Add(this.m_powerOnFraudSensor);
		this.groupBox1.Controls.Add(this.m_testDiskForMarkers);
		this.groupBox1.Controls.Add(this.m_stopFraudScan);
		this.groupBox1.Controls.Add(this.m_startFraudScan);
		this.groupBox1.Controls.Add(this.m_resetFraudButton);
		this.groupBox1.Controls.Add(this.m_testFraudButton);
		this.groupBox1.Controls.Add(this.m_fraudPOSTButton);
		this.groupBox1.Location = new System.Drawing.Point(487, 6);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(390, 297);
		this.groupBox1.TabIndex = 7;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Sensor Tests";
		this.m_powerOffFraudSensor.Location = new System.Drawing.Point(222, 85);
		this.m_powerOffFraudSensor.Name = "m_powerOffFraudSensor";
		this.m_powerOffFraudSensor.Size = new System.Drawing.Size(150, 60);
		this.m_powerOffFraudSensor.TabIndex = 10;
		this.m_powerOffFraudSensor.Tag = "SHUTDOWN";
		this.m_powerOffFraudSensor.Text = "Turn Off Power";
		this.m_powerOffFraudSensor.UseVisualStyleBackColor = true;
		this.m_powerOffFraudSensor.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.m_powerOnFraudSensor.Location = new System.Drawing.Point(23, 85);
		this.m_powerOnFraudSensor.Name = "m_powerOnFraudSensor";
		this.m_powerOnFraudSensor.Size = new System.Drawing.Size(150, 60);
		this.m_powerOnFraudSensor.TabIndex = 9;
		this.m_powerOnFraudSensor.Tag = "INITIALIZE";
		this.m_powerOnFraudSensor.Text = "Power Sensor";
		this.m_powerOnFraudSensor.UseVisualStyleBackColor = true;
		this.m_powerOnFraudSensor.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.m_testDiskForMarkers.Location = new System.Drawing.Point(23, 218);
		this.m_testDiskForMarkers.Name = "m_testDiskForMarkers";
		this.m_testDiskForMarkers.Size = new System.Drawing.Size(150, 60);
		this.m_testDiskForMarkers.TabIndex = 8;
		this.m_testDiskForMarkers.Text = "Test Disk In Picker";
		this.m_testDiskForMarkers.UseVisualStyleBackColor = true;
		this.m_testDiskForMarkers.Click += new System.EventHandler(m_testFraudButton_Click);
		this.m_stopFraudScan.Location = new System.Drawing.Point(222, 152);
		this.m_stopFraudScan.Name = "m_stopFraudScan";
		this.m_stopFraudScan.Size = new System.Drawing.Size(150, 60);
		this.m_stopFraudScan.TabIndex = 7;
		this.m_stopFraudScan.Tag = "STOPSCAN";
		this.m_stopFraudScan.Text = "Stop Scan";
		this.m_stopFraudScan.UseVisualStyleBackColor = true;
		this.m_stopFraudScan.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.m_startFraudScan.Location = new System.Drawing.Point(23, 152);
		this.m_startFraudScan.Name = "m_startFraudScan";
		this.m_startFraudScan.Size = new System.Drawing.Size(150, 60);
		this.m_startFraudScan.TabIndex = 6;
		this.m_startFraudScan.Tag = "STARTSCAN";
		this.m_startFraudScan.Text = "Start Scan";
		this.m_startFraudScan.UseVisualStyleBackColor = true;
		this.m_startFraudScan.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.m_resetFraudButton.Location = new System.Drawing.Point(222, 19);
		this.m_resetFraudButton.Name = "m_resetFraudButton";
		this.m_resetFraudButton.Size = new System.Drawing.Size(150, 60);
		this.m_resetFraudButton.TabIndex = 5;
		this.m_resetFraudButton.Tag = "RESET";
		this.m_resetFraudButton.Text = "Reset";
		this.m_resetFraudButton.UseVisualStyleBackColor = true;
		this.m_resetFraudButton.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.m_testFraudButton.Location = new System.Drawing.Point(222, 218);
		this.m_testFraudButton.Name = "m_testFraudButton";
		this.m_testFraudButton.Size = new System.Drawing.Size(150, 60);
		this.m_testFraudButton.TabIndex = 3;
		this.m_testFraudButton.Text = "Take - Test - Return";
		this.m_testFraudButton.UseVisualStyleBackColor = true;
		this.m_testFraudButton.Click += new System.EventHandler(m_testUserDiskForFraud);
		this.m_fraudPOSTButton.Location = new System.Drawing.Point(23, 19);
		this.m_fraudPOSTButton.Name = "m_fraudPOSTButton";
		this.m_fraudPOSTButton.Size = new System.Drawing.Size(150, 60);
		this.m_fraudPOSTButton.TabIndex = 4;
		this.m_fraudPOSTButton.Tag = "LASTPOSTRESULT";
		this.m_fraudPOSTButton.Text = "Last POST Result";
		this.m_fraudPOSTButton.UseVisualStyleBackColor = true;
		this.m_fraudPOSTButton.Click += new System.EventHandler(OnExecuteFraudSensorInstruction);
		this.button1.Location = new System.Drawing.Point(510, 309);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(120, 57);
		this.button1.TabIndex = 5;
		this.button1.Text = "Get";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click_2);
		this.m_configureFraudSensor.Location = new System.Drawing.Point(6, 62);
		this.m_configureFraudSensor.Name = "m_configureFraudSensor";
		this.m_configureFraudSensor.Size = new System.Drawing.Size(150, 75);
		this.m_configureFraudSensor.TabIndex = 1;
		this.m_configureFraudSensor.Text = "Configure";
		this.m_configureFraudSensor.UseVisualStyleBackColor = true;
		this.m_configureFraudSensor.Click += new System.EventHandler(button1_Click_1);
		this.m_fraudSensorOutput.FormattingEnabled = true;
		this.m_fraudSensorOutput.HorizontalScrollbar = true;
		this.m_fraudSensorOutput.Location = new System.Drawing.Point(6, 158);
		this.m_fraudSensorOutput.Name = "m_fraudSensorOutput";
		this.m_fraudSensorOutput.ScrollAlwaysVisible = true;
		this.m_fraudSensorOutput.Size = new System.Drawing.Size(450, 290);
		this.m_fraudSensorOutput.TabIndex = 0;
		this.m_kioskFunctionCheck.BackColor = System.Drawing.Color.LightGray;
		this.m_kioskFunctionCheck.Controls.Add(this.m_displaySessionsButton);
		this.m_kioskFunctionCheck.Controls.Add(this.m_sourceSlotTB);
		this.m_kioskFunctionCheck.Controls.Add(this.button19);
		this.m_kioskFunctionCheck.Controls.Add(this.m_sourceDeckTB);
		this.m_kioskFunctionCheck.Controls.Add(this.button22);
		this.m_kioskFunctionCheck.Controls.Add(this.groupBox2);
		this.m_kioskFunctionCheck.Controls.Add(this.m_userName);
		this.m_kioskFunctionCheck.Controls.Add(this.button21);
		this.m_kioskFunctionCheck.Controls.Add(this.label1);
		this.m_kioskFunctionCheck.Controls.Add(this.button2);
		this.m_kioskFunctionCheck.Controls.Add(this.m_kioskTestOutput);
		this.m_kioskFunctionCheck.Location = new System.Drawing.Point(4, 56);
		this.m_kioskFunctionCheck.Name = "m_kioskFunctionCheck";
		this.m_kioskFunctionCheck.Padding = new System.Windows.Forms.Padding(3);
		this.m_kioskFunctionCheck.Size = new System.Drawing.Size(1049, 566);
		this.m_kioskFunctionCheck.TabIndex = 6;
		this.m_kioskFunctionCheck.Text = "Kiosk Function Check";
		this.m_displaySessionsButton.Location = new System.Drawing.Point(680, 501);
		this.m_displaySessionsButton.Name = "m_displaySessionsButton";
		this.m_displaySessionsButton.Size = new System.Drawing.Size(104, 54);
		this.m_displaySessionsButton.TabIndex = 22;
		this.m_displaySessionsButton.Text = "Show Sessions";
		this.m_displaySessionsButton.UseVisualStyleBackColor = true;
		this.m_displaySessionsButton.Click += new System.EventHandler(button16_Click_1);
		this.m_sourceSlotTB.Location = new System.Drawing.Point(277, 83);
		this.m_sourceSlotTB.Name = "m_sourceSlotTB";
		this.m_sourceSlotTB.Size = new System.Drawing.Size(75, 20);
		this.m_sourceSlotTB.TabIndex = 11;
		this.button19.Location = new System.Drawing.Point(801, 501);
		this.button19.Name = "button19";
		this.button19.Size = new System.Drawing.Size(100, 55);
		this.button19.TabIndex = 13;
		this.button19.Text = "Send Report";
		this.button19.UseVisualStyleBackColor = true;
		this.button19.Click += new System.EventHandler(button19_Click);
		this.m_sourceDeckTB.Location = new System.Drawing.Point(90, 83);
		this.m_sourceDeckTB.Name = "m_sourceDeckTB";
		this.m_sourceDeckTB.Size = new System.Drawing.Size(68, 20);
		this.m_sourceDeckTB.TabIndex = 9;
		this.button22.Location = new System.Drawing.Point(196, 65);
		this.button22.Name = "button22";
		this.button22.Size = new System.Drawing.Size(75, 38);
		this.button22.TabIndex = 10;
		this.button22.Text = "Slot";
		this.button22.UseVisualStyleBackColor = true;
		this.button22.Click += new System.EventHandler(button22_Click);
		this.groupBox2.Controls.Add(this.m_unknownCountTB);
		this.groupBox2.Controls.Add(this.m_cameraSnapTB);
		this.groupBox2.Controls.Add(this.btn_KFCDecodeTest);
		this.groupBox2.Controls.Add(this.m_verticalTestTB);
		this.groupBox2.Controls.Add(this.m_initTB);
		this.groupBox2.Controls.Add(this.m_testVendDoorTB);
		this.groupBox2.Controls.Add(this.m_checkDriversStatusTB);
		this.groupBox2.Controls.Add(this.btn_KFCUnknownCount);
		this.groupBox2.Controls.Add(this.btn_KFCVerticalSlotTest);
		this.groupBox2.Controls.Add(this.btn_KFCTestVendDoor);
		this.groupBox2.Controls.Add(this.btn_KFCInit);
		this.groupBox2.Controls.Add(this.btn_KFCCheckDrivers);
		this.groupBox2.Location = new System.Drawing.Point(9, 118);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(325, 437);
		this.groupBox2.TabIndex = 4;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Steps:";
		this.m_unknownCountTB.Location = new System.Drawing.Point(133, 411);
		this.m_unknownCountTB.Name = "m_unknownCountTB";
		this.m_unknownCountTB.ReadOnly = true;
		this.m_unknownCountTB.Size = new System.Drawing.Size(129, 20);
		this.m_unknownCountTB.TabIndex = 17;
		this.m_cameraSnapTB.Location = new System.Drawing.Point(133, 129);
		this.m_cameraSnapTB.Name = "m_cameraSnapTB";
		this.m_cameraSnapTB.ReadOnly = true;
		this.m_cameraSnapTB.Size = new System.Drawing.Size(129, 20);
		this.m_cameraSnapTB.TabIndex = 16;
		this.btn_KFCDecodeTest.Location = new System.Drawing.Point(12, 89);
		this.btn_KFCDecodeTest.Name = "btn_KFCDecodeTest";
		this.btn_KFCDecodeTest.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCDecodeTest.TabIndex = 2;
		this.btn_KFCDecodeTest.Text = "Decode Test";
		this.btn_KFCDecodeTest.UseVisualStyleBackColor = true;
		this.btn_KFCDecodeTest.Click += new System.EventHandler(button20_Click);
		this.m_verticalTestTB.Location = new System.Drawing.Point(133, 338);
		this.m_verticalTestTB.Name = "m_verticalTestTB";
		this.m_verticalTestTB.ReadOnly = true;
		this.m_verticalTestTB.Size = new System.Drawing.Size(129, 20);
		this.m_verticalTestTB.TabIndex = 13;
		this.m_initTB.Location = new System.Drawing.Point(133, 272);
		this.m_initTB.Name = "m_initTB";
		this.m_initTB.ReadOnly = true;
		this.m_initTB.Size = new System.Drawing.Size(129, 20);
		this.m_initTB.TabIndex = 12;
		this.m_testVendDoorTB.Location = new System.Drawing.Point(133, 195);
		this.m_testVendDoorTB.Name = "m_testVendDoorTB";
		this.m_testVendDoorTB.ReadOnly = true;
		this.m_testVendDoorTB.Size = new System.Drawing.Size(129, 20);
		this.m_testVendDoorTB.TabIndex = 11;
		this.m_checkDriversStatusTB.Location = new System.Drawing.Point(133, 59);
		this.m_checkDriversStatusTB.Name = "m_checkDriversStatusTB";
		this.m_checkDriversStatusTB.ReadOnly = true;
		this.m_checkDriversStatusTB.Size = new System.Drawing.Size(129, 20);
		this.m_checkDriversStatusTB.TabIndex = 9;
		this.btn_KFCUnknownCount.Location = new System.Drawing.Point(12, 371);
		this.btn_KFCUnknownCount.Name = "btn_KFCUnknownCount";
		this.btn_KFCUnknownCount.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCUnknownCount.TabIndex = 7;
		this.btn_KFCUnknownCount.Text = "Unknown Count";
		this.btn_KFCUnknownCount.UseVisualStyleBackColor = true;
		this.btn_KFCUnknownCount.Click += new System.EventHandler(button18_Click);
		this.btn_KFCVerticalSlotTest.Location = new System.Drawing.Point(12, 298);
		this.btn_KFCVerticalSlotTest.Name = "btn_KFCVerticalSlotTest";
		this.btn_KFCVerticalSlotTest.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCVerticalSlotTest.TabIndex = 6;
		this.btn_KFCVerticalSlotTest.Text = "Vertical Slot Test";
		this.btn_KFCVerticalSlotTest.UseVisualStyleBackColor = true;
		this.btn_KFCVerticalSlotTest.Click += new System.EventHandler(button17_Click);
		this.btn_KFCTestVendDoor.Location = new System.Drawing.Point(12, 155);
		this.btn_KFCTestVendDoor.Name = "btn_KFCTestVendDoor";
		this.btn_KFCTestVendDoor.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCTestVendDoor.TabIndex = 4;
		this.btn_KFCTestVendDoor.Text = "Test Vend Door";
		this.btn_KFCTestVendDoor.UseVisualStyleBackColor = true;
		this.btn_KFCTestVendDoor.Click += new System.EventHandler(button12_Click);
		this.btn_KFCInit.Location = new System.Drawing.Point(12, 232);
		this.btn_KFCInit.Name = "btn_KFCInit";
		this.btn_KFCInit.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCInit.TabIndex = 5;
		this.btn_KFCInit.Text = "Init";
		this.btn_KFCInit.UseVisualStyleBackColor = true;
		this.btn_KFCInit.Click += new System.EventHandler(button9_Click);
		this.btn_KFCCheckDrivers.Location = new System.Drawing.Point(12, 19);
		this.btn_KFCCheckDrivers.Name = "btn_KFCCheckDrivers";
		this.btn_KFCCheckDrivers.Size = new System.Drawing.Size(100, 60);
		this.btn_KFCCheckDrivers.TabIndex = 0;
		this.btn_KFCCheckDrivers.Text = "Check Drivers";
		this.btn_KFCCheckDrivers.UseVisualStyleBackColor = true;
		this.btn_KFCCheckDrivers.Click += new System.EventHandler(button8_Click);
		this.m_userName.Location = new System.Drawing.Point(6, 31);
		this.m_userName.Name = "m_userName";
		this.m_userName.ReadOnly = true;
		this.m_userName.Size = new System.Drawing.Size(228, 20);
		this.m_userName.TabIndex = 3;
		this.button21.Location = new System.Drawing.Point(9, 65);
		this.button21.Name = "button21";
		this.button21.Size = new System.Drawing.Size(75, 38);
		this.button21.TabIndex = 8;
		this.button21.Text = "Deck";
		this.button21.UseVisualStyleBackColor = true;
		this.button21.Click += new System.EventHandler(button21_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(55, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Username";
		this.button2.Location = new System.Drawing.Point(460, 501);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(100, 55);
		this.button2.TabIndex = 12;
		this.button2.Text = "Clear Output";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click_1);
		this.m_kioskTestOutput.FormattingEnabled = true;
		this.m_kioskTestOutput.Location = new System.Drawing.Point(460, 6);
		this.m_kioskTestOutput.Name = "m_kioskTestOutput";
		this.m_kioskTestOutput.Size = new System.Drawing.Size(441, 485);
		this.m_kioskTestOutput.TabIndex = 0;
		this.tabPage1.BackColor = System.Drawing.Color.LightGray;
		this.tabPage1.Controls.Add(this.listView1);
		this.tabPage1.Controls.Add(this.button16);
		this.tabPage1.Controls.Add(this.button11);
		this.tabPage1.Location = new System.Drawing.Point(4, 56);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(1049, 566);
		this.tabPage1.TabIndex = 7;
		this.tabPage1.Text = "Hardware Survey";
		this.listView1.HideSelection = false;
		this.listView1.Location = new System.Drawing.Point(20, 35);
		this.listView1.Name = "listView1";
		this.listView1.Size = new System.Drawing.Size(502, 378);
		this.listView1.TabIndex = 3;
		this.listView1.UseCompatibleStateImageBehavior = false;
		this.listView1.View = System.Windows.Forms.View.Details;
		this.button16.Location = new System.Drawing.Point(656, 194);
		this.button16.Name = "button16";
		this.button16.Size = new System.Drawing.Size(140, 80);
		this.button16.TabIndex = 2;
		this.button16.Text = "Clear";
		this.button16.UseVisualStyleBackColor = true;
		this.button16.Click += new System.EventHandler(button16_Click_2);
		this.button11.Location = new System.Drawing.Point(656, 74);
		this.button11.Name = "button11";
		this.button11.Size = new System.Drawing.Size(140, 80);
		this.button11.TabIndex = 1;
		this.button11.Text = "Run Survey";
		this.button11.UseVisualStyleBackColor = true;
		this.button11.Click += new System.EventHandler(button11_Click);
		this.m_hwCorrections.BackColor = System.Drawing.Color.LightGray;
		this.m_hwCorrections.Controls.Add(this.groupBox4);
		this.m_hwCorrections.Controls.Add(this.groupBox6);
		this.m_hwCorrections.Controls.Add(this.groupBox5);
		this.m_hwCorrections.Controls.Add(this.m_hwCorrectiontypeGB);
		this.m_hwCorrections.Controls.Add(this.button23);
		this.m_hwCorrections.Controls.Add(this.m_hwStatsOutput);
		this.m_hwCorrections.Location = new System.Drawing.Point(4, 56);
		this.m_hwCorrections.Name = "m_hwCorrections";
		this.m_hwCorrections.Padding = new System.Windows.Forms.Padding(3);
		this.m_hwCorrections.Size = new System.Drawing.Size(1049, 566);
		this.m_hwCorrections.TabIndex = 9;
		this.m_hwCorrections.Text = "Hardware Corrections";
		this.groupBox4.Controls.Add(this.m_testRouterRelay);
		this.groupBox4.Controls.Add(this.m_configureRouterRelay);
		this.groupBox4.Location = new System.Drawing.Point(513, 279);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(200, 175);
		this.groupBox4.TabIndex = 8;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Router ";
		this.m_testRouterRelay.Location = new System.Drawing.Point(23, 100);
		this.m_testRouterRelay.Name = "m_testRouterRelay";
		this.m_testRouterRelay.Size = new System.Drawing.Size(120, 50);
		this.m_testRouterRelay.TabIndex = 1;
		this.m_testRouterRelay.Text = "Power Cycle";
		this.m_testRouterRelay.UseVisualStyleBackColor = true;
		this.m_testRouterRelay.Click += new System.EventHandler(button38_Click);
		this.m_configureRouterRelay.Location = new System.Drawing.Point(23, 31);
		this.m_configureRouterRelay.Name = "m_configureRouterRelay";
		this.m_configureRouterRelay.Size = new System.Drawing.Size(120, 50);
		this.m_configureRouterRelay.TabIndex = 0;
		this.m_configureRouterRelay.Text = "Configure";
		this.m_configureRouterRelay.UseVisualStyleBackColor = true;
		this.m_configureRouterRelay.Click += new System.EventHandler(button37_Click);
		this.groupBox6.Controls.Add(this.m_restartArcus);
		this.groupBox6.Controls.Add(this.m_configureArcus);
		this.groupBox6.Location = new System.Drawing.Point(782, 280);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(200, 175);
		this.groupBox6.TabIndex = 7;
		this.groupBox6.TabStop = false;
		this.groupBox6.Text = "Arcus";
		this.m_restartArcus.Location = new System.Drawing.Point(26, 99);
		this.m_restartArcus.Name = "m_restartArcus";
		this.m_restartArcus.Size = new System.Drawing.Size(120, 50);
		this.m_restartArcus.TabIndex = 1;
		this.m_restartArcus.Text = "Restart";
		this.m_restartArcus.UseVisualStyleBackColor = true;
		this.m_restartArcus.Click += new System.EventHandler(m_restartArcus_Click);
		this.m_configureArcus.Location = new System.Drawing.Point(26, 30);
		this.m_configureArcus.Name = "m_configureArcus";
		this.m_configureArcus.Size = new System.Drawing.Size(120, 50);
		this.m_configureArcus.TabIndex = 0;
		this.m_configureArcus.Text = "Configure";
		this.m_configureArcus.UseVisualStyleBackColor = true;
		this.m_configureArcus.Click += new System.EventHandler(m_configureArcus_Click);
		this.groupBox5.Controls.Add(this.m_restartTouchscreen);
		this.groupBox5.Location = new System.Drawing.Point(782, 51);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(200, 117);
		this.groupBox5.TabIndex = 6;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Touchscreen";
		this.m_restartTouchscreen.Location = new System.Drawing.Point(33, 33);
		this.m_restartTouchscreen.Name = "m_restartTouchscreen";
		this.m_restartTouchscreen.Size = new System.Drawing.Size(120, 50);
		this.m_restartTouchscreen.TabIndex = 0;
		this.m_restartTouchscreen.Text = "Reset";
		this.m_restartTouchscreen.UseVisualStyleBackColor = true;
		this.m_restartTouchscreen.Click += new System.EventHandler(m_restartTouchscreen_Click);
		this.m_hwCorrectiontypeGB.Controls.Add(this.radioButton5);
		this.m_hwCorrectiontypeGB.Controls.Add(this.radioButton4);
		this.m_hwCorrectiontypeGB.Controls.Add(this.radioButton2);
		this.m_hwCorrectiontypeGB.Controls.Add(this.radioButton3);
		this.m_hwCorrectiontypeGB.Controls.Add(this.button24);
		this.m_hwCorrectiontypeGB.Controls.Add(this.radioButton1);
		this.m_hwCorrectiontypeGB.Location = new System.Drawing.Point(513, 43);
		this.m_hwCorrectiontypeGB.Name = "m_hwCorrectiontypeGB";
		this.m_hwCorrectiontypeGB.Size = new System.Drawing.Size(200, 217);
		this.m_hwCorrectiontypeGB.TabIndex = 4;
		this.m_hwCorrectiontypeGB.TabStop = false;
		this.m_hwCorrectiontypeGB.Text = "Correction Type";
		this.radioButton5.AutoSize = true;
		this.radioButton5.Location = new System.Drawing.Point(23, 93);
		this.radioButton5.Name = "radioButton5";
		this.radioButton5.Size = new System.Drawing.Size(141, 17);
		this.radioButton5.TabIndex = 6;
		this.radioButton5.TabStop = true;
		this.radioButton5.Tag = "UnexpectedPowerLoss";
		this.radioButton5.Text = "Unexpected Power Loss";
		this.radioButton5.UseVisualStyleBackColor = true;
		this.radioButton4.AutoSize = true;
		this.radioButton4.Location = new System.Drawing.Point(23, 70);
		this.radioButton4.Name = "radioButton4";
		this.radioButton4.Size = new System.Drawing.Size(57, 17);
		this.radioButton4.TabIndex = 5;
		this.radioButton4.TabStop = true;
		this.radioButton4.Tag = "RouterRecycle";
		this.radioButton4.Text = "Router";
		this.radioButton4.UseVisualStyleBackColor = true;
		this.radioButton2.AutoSize = true;
		this.radioButton2.Location = new System.Drawing.Point(23, 116);
		this.radioButton2.Name = "radioButton2";
		this.radioButton2.Size = new System.Drawing.Size(36, 17);
		this.radioButton2.TabIndex = 4;
		this.radioButton2.TabStop = true;
		this.radioButton2.Tag = "None";
		this.radioButton2.Text = "All";
		this.radioButton2.UseVisualStyleBackColor = true;
		this.radioButton3.AutoSize = true;
		this.radioButton3.Location = new System.Drawing.Point(23, 47);
		this.radioButton3.Name = "radioButton3";
		this.radioButton3.Size = new System.Drawing.Size(88, 17);
		this.radioButton3.TabIndex = 2;
		this.radioButton3.TabStop = true;
		this.radioButton3.Tag = "Touchscreen";
		this.radioButton3.Text = "Touchscreen";
		this.radioButton3.UseVisualStyleBackColor = true;
		this.button24.Location = new System.Drawing.Point(23, 150);
		this.button24.Name = "button24";
		this.button24.Size = new System.Drawing.Size(120, 50);
		this.button24.TabIndex = 3;
		this.button24.Text = "Show Stats";
		this.button24.UseVisualStyleBackColor = true;
		this.button24.Click += new System.EventHandler(button24_Click);
		this.radioButton1.AutoSize = true;
		this.radioButton1.Location = new System.Drawing.Point(23, 24);
		this.radioButton1.Name = "radioButton1";
		this.radioButton1.Size = new System.Drawing.Size(52, 17);
		this.radioButton1.TabIndex = 0;
		this.radioButton1.TabStop = true;
		this.radioButton1.Tag = "Arcus";
		this.radioButton1.Text = "Arcus";
		this.radioButton1.UseVisualStyleBackColor = true;
		this.button23.Location = new System.Drawing.Point(21, 473);
		this.button23.Name = "button23";
		this.button23.Size = new System.Drawing.Size(120, 57);
		this.button23.TabIndex = 1;
		this.button23.Text = "Clear Output";
		this.button23.UseVisualStyleBackColor = true;
		this.button23.Click += new System.EventHandler(button23_Click);
		this.m_hwStatsOutput.FormattingEnabled = true;
		this.m_hwStatsOutput.Location = new System.Drawing.Point(21, 35);
		this.m_hwStatsOutput.Name = "m_hwStatsOutput";
		this.m_hwStatsOutput.Size = new System.Drawing.Size(422, 420);
		this.m_hwStatsOutput.TabIndex = 0;
		this.m_irConfigurationTab.BackColor = System.Drawing.Color.LightGray;
		this.m_irConfigurationTab.Controls.Add(this.button37);
		this.m_irConfigurationTab.Controls.Add(this.groupBox8);
		this.m_irConfigurationTab.Controls.Add(this.groupBox7);
		this.m_irConfigurationTab.Controls.Add(this.m_irCameraOutput);
		this.m_irConfigurationTab.Location = new System.Drawing.Point(4, 56);
		this.m_irConfigurationTab.Name = "m_irConfigurationTab";
		this.m_irConfigurationTab.Size = new System.Drawing.Size(1049, 566);
		this.m_irConfigurationTab.TabIndex = 11;
		this.m_irConfigurationTab.Text = "IR Camera";
		this.button37.Location = new System.Drawing.Point(352, 64);
		this.button37.Name = "button37";
		this.button37.Size = new System.Drawing.Size(115, 60);
		this.button37.TabIndex = 3;
		this.button37.Text = "button37";
		this.button37.UseVisualStyleBackColor = true;
		this.button37.Click += new System.EventHandler(button37_Click_1);
		this.groupBox8.Controls.Add(this.textBox2);
		this.groupBox8.Controls.Add(this.textBox1);
		this.groupBox8.Controls.Add(this.button47);
		this.groupBox8.Controls.Add(this.button46);
		this.groupBox8.Controls.Add(this.button45);
		this.groupBox8.Controls.Add(this.button44);
		this.groupBox8.Controls.Add(this.button43);
		this.groupBox8.Location = new System.Drawing.Point(36, 299);
		this.groupBox8.Name = "groupBox8";
		this.groupBox8.Size = new System.Drawing.Size(348, 205);
		this.groupBox8.TabIndex = 2;
		this.groupBox8.TabStop = false;
		this.groupBox8.Text = "Hardware Control";
		this.textBox2.Location = new System.Drawing.Point(225, 81);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(100, 20);
		this.textBox2.TabIndex = 9;
		this.textBox1.Location = new System.Drawing.Point(225, 38);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(100, 20);
		this.textBox1.TabIndex = 8;
		this.button47.Location = new System.Drawing.Point(144, 73);
		this.button47.Name = "button47";
		this.button47.Size = new System.Drawing.Size(75, 40);
		this.button47.TabIndex = 7;
		this.button47.Text = "Slot";
		this.button47.UseVisualStyleBackColor = true;
		this.button47.Click += new System.EventHandler(button47_Click);
		this.button46.Location = new System.Drawing.Point(144, 27);
		this.button46.Name = "button46";
		this.button46.Size = new System.Drawing.Size(75, 40);
		this.button46.TabIndex = 6;
		this.button46.Text = "Deck";
		this.button46.UseVisualStyleBackColor = true;
		this.button46.Click += new System.EventHandler(button46_Click);
		this.button45.Location = new System.Drawing.Point(144, 124);
		this.button45.Name = "button45";
		this.button45.Size = new System.Drawing.Size(115, 75);
		this.button45.TabIndex = 5;
		this.button45.Tag = "CONTROLSYSTEM CENTER";
		this.button45.Text = "Center disk";
		this.button45.UseVisualStyleBackColor = true;
		this.button45.Click += new System.EventHandler(OnExecuteResponseInstruction);
		this.button44.Location = new System.Drawing.Point(12, 124);
		this.button44.Name = "button44";
		this.button44.Size = new System.Drawing.Size(115, 75);
		this.button44.TabIndex = 4;
		this.button44.Text = "Put";
		this.button44.UseVisualStyleBackColor = true;
		this.button44.Click += new System.EventHandler(button44_Click);
		this.button43.Location = new System.Drawing.Point(12, 26);
		this.button43.Name = "button43";
		this.button43.Size = new System.Drawing.Size(115, 75);
		this.button43.TabIndex = 4;
		this.button43.Text = "Get and Read";
		this.button43.UseVisualStyleBackColor = true;
		this.button43.Click += new System.EventHandler(button43_Click);
		this.groupBox7.Controls.Add(this.button42);
		this.groupBox7.Controls.Add(this.button41);
		this.groupBox7.Controls.Add(this.button40);
		this.groupBox7.Controls.Add(this.button39);
		this.groupBox7.Controls.Add(this.button38);
		this.groupBox7.Location = new System.Drawing.Point(36, 34);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(298, 238);
		this.groupBox7.TabIndex = 1;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "Camera Control";
		this.button42.Location = new System.Drawing.Point(18, 162);
		this.button42.Name = "button42";
		this.button42.Size = new System.Drawing.Size(115, 60);
		this.button42.TabIndex = 4;
		this.button42.Text = "Launch Tuner";
		this.button42.UseVisualStyleBackColor = true;
		this.button42.Click += new System.EventHandler(button42_Click);
		this.button41.Location = new System.Drawing.Point(163, 96);
		this.button41.Name = "button41";
		this.button41.Size = new System.Drawing.Size(115, 60);
		this.button41.TabIndex = 4;
		this.button41.Tag = "CONTROLSYSTEM RINGLIGHTOFF";
		this.button41.Text = "Ringlight Off";
		this.button41.UseVisualStyleBackColor = true;
		this.button41.Click += new System.EventHandler(OnExecuteResponseInstruction);
		this.button40.Location = new System.Drawing.Point(18, 96);
		this.button40.Name = "button40";
		this.button40.Size = new System.Drawing.Size(115, 60);
		this.button40.TabIndex = 4;
		this.button40.Tag = "CAMERA STOP";
		this.button40.Text = "Stop Camera";
		this.button40.UseVisualStyleBackColor = true;
		this.button40.Click += new System.EventHandler(OnExecuteResponseInstruction);
		this.button39.Location = new System.Drawing.Point(163, 30);
		this.button39.Name = "button39";
		this.button39.Size = new System.Drawing.Size(115, 60);
		this.button39.TabIndex = 4;
		this.button39.Tag = "CONTROLSYSTEM RINGLIGHTON";
		this.button39.Text = "Ringlight On";
		this.button39.UseVisualStyleBackColor = true;
		this.button39.Click += new System.EventHandler(OnExecuteResponseInstruction);
		this.button38.Location = new System.Drawing.Point(18, 30);
		this.button38.Name = "button38";
		this.button38.Size = new System.Drawing.Size(115, 60);
		this.button38.TabIndex = 4;
		this.button38.Tag = "CAMERA START";
		this.button38.Text = "Start Camera";
		this.button38.UseVisualStyleBackColor = true;
		this.button38.Click += new System.EventHandler(OnExecuteResponseInstruction);
		this.m_irCameraOutput.FormattingEnabled = true;
		this.m_irCameraOutput.Location = new System.Drawing.Point(558, 48);
		this.m_irCameraOutput.Name = "m_irCameraOutput";
		this.m_irCameraOutput.Size = new System.Drawing.Size(440, 420);
		this.m_irCameraOutput.TabIndex = 0;
		this.m_engineeringTab.BackColor = System.Drawing.Color.LightGray;
		this.m_engineeringTab.Controls.Add(this.groupBox3);
		this.m_engineeringTab.Controls.Add(this.button28);
		this.m_engineeringTab.Controls.Add(this.button27);
		this.m_engineeringTab.Controls.Add(this.button26);
		this.m_engineeringTab.Controls.Add(this.button25);
		this.m_engineeringTab.Controls.Add(this.m_engineeringOutput);
		this.m_engineeringTab.Location = new System.Drawing.Point(4, 56);
		this.m_engineeringTab.Name = "m_engineeringTab";
		this.m_engineeringTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_engineeringTab.Size = new System.Drawing.Size(1049, 566);
		this.m_engineeringTab.TabIndex = 10;
		this.m_engineeringTab.Text = "Engineering";
		this.groupBox3.Controls.Add(this.button36);
		this.groupBox3.Controls.Add(this.button35);
		this.groupBox3.Controls.Add(this.button34);
		this.groupBox3.Controls.Add(this.button29);
		this.groupBox3.Controls.Add(this.button30);
		this.groupBox3.Controls.Add(this.button33);
		this.groupBox3.Controls.Add(this.button32);
		this.groupBox3.Controls.Add(this.m_engEndSlotTB);
		this.groupBox3.Controls.Add(this.button31);
		this.groupBox3.Controls.Add(this.m_engEndDeckTB);
		this.groupBox3.Controls.Add(this.m_engDeckTB);
		this.groupBox3.Controls.Add(this.m_engSlotTB);
		this.groupBox3.Location = new System.Drawing.Point(621, 46);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(292, 392);
		this.groupBox3.TabIndex = 14;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Locations";
		this.button36.Location = new System.Drawing.Point(161, 293);
		this.button36.Name = "button36";
		this.button36.Size = new System.Drawing.Size(110, 60);
		this.button36.TabIndex = 18;
		this.button36.Text = "Put to";
		this.button36.UseVisualStyleBackColor = true;
		this.button36.Click += new System.EventHandler(button36_Click);
		this.button35.Location = new System.Drawing.Point(18, 293);
		this.button35.Name = "button35";
		this.button35.Size = new System.Drawing.Size(110, 60);
		this.button35.TabIndex = 17;
		this.button35.Text = "Get from";
		this.button35.UseVisualStyleBackColor = true;
		this.button35.Click += new System.EventHandler(button35_Click);
		this.button34.Location = new System.Drawing.Point(171, 26);
		this.button34.Name = "button34";
		this.button34.Size = new System.Drawing.Size(100, 40);
		this.button34.TabIndex = 16;
		this.button34.Text = "Start slot";
		this.button34.UseVisualStyleBackColor = true;
		this.button34.Click += new System.EventHandler(button34_Click);
		this.button29.Location = new System.Drawing.Point(161, 216);
		this.button29.Name = "button29";
		this.button29.Size = new System.Drawing.Size(110, 60);
		this.button29.TabIndex = 9;
		this.button29.Text = "Sync slots";
		this.button29.UseVisualStyleBackColor = true;
		this.button29.Click += new System.EventHandler(button29_Click);
		this.button30.Location = new System.Drawing.Point(18, 216);
		this.button30.Name = "button30";
		this.button30.Size = new System.Drawing.Size(110, 60);
		this.button30.TabIndex = 10;
		this.button30.Text = "Sync slot";
		this.button30.UseVisualStyleBackColor = true;
		this.button30.Click += new System.EventHandler(button30_Click);
		this.button33.Location = new System.Drawing.Point(18, 26);
		this.button33.Name = "button33";
		this.button33.Size = new System.Drawing.Size(100, 40);
		this.button33.TabIndex = 15;
		this.button33.Text = "Start deck";
		this.button33.UseVisualStyleBackColor = true;
		this.button33.Click += new System.EventHandler(button33_Click);
		this.button32.Location = new System.Drawing.Point(171, 121);
		this.button32.Name = "button32";
		this.button32.Size = new System.Drawing.Size(100, 40);
		this.button32.TabIndex = 14;
		this.button32.Text = "End slot";
		this.button32.UseVisualStyleBackColor = true;
		this.button32.Click += new System.EventHandler(button32_Click);
		this.m_engEndSlotTB.Location = new System.Drawing.Point(171, 167);
		this.m_engEndSlotTB.Name = "m_engEndSlotTB";
		this.m_engEndSlotTB.Size = new System.Drawing.Size(100, 20);
		this.m_engEndSlotTB.TabIndex = 12;
		this.button31.Location = new System.Drawing.Point(18, 121);
		this.button31.Name = "button31";
		this.button31.Size = new System.Drawing.Size(100, 40);
		this.button31.TabIndex = 13;
		this.button31.Text = "End deck";
		this.button31.UseVisualStyleBackColor = true;
		this.button31.Click += new System.EventHandler(button31_Click);
		this.m_engEndDeckTB.Location = new System.Drawing.Point(18, 167);
		this.m_engEndDeckTB.Name = "m_engEndDeckTB";
		this.m_engEndDeckTB.Size = new System.Drawing.Size(100, 20);
		this.m_engEndDeckTB.TabIndex = 11;
		this.m_engDeckTB.Location = new System.Drawing.Point(18, 72);
		this.m_engDeckTB.Name = "m_engDeckTB";
		this.m_engDeckTB.Size = new System.Drawing.Size(100, 20);
		this.m_engDeckTB.TabIndex = 5;
		this.m_engSlotTB.Location = new System.Drawing.Point(171, 72);
		this.m_engSlotTB.Name = "m_engSlotTB";
		this.m_engSlotTB.Size = new System.Drawing.Size(100, 20);
		this.m_engSlotTB.TabIndex = 6;
		this.button28.Location = new System.Drawing.Point(441, 220);
		this.button28.Name = "button28";
		this.button28.Size = new System.Drawing.Size(120, 61);
		this.button28.TabIndex = 4;
		this.button28.Text = "Read barcode";
		this.button28.UseVisualStyleBackColor = true;
		this.button28.Click += new System.EventHandler(button28_Click);
		this.button27.Location = new System.Drawing.Point(441, 133);
		this.button27.Name = "button27";
		this.button27.Size = new System.Drawing.Size(120, 65);
		this.button27.TabIndex = 3;
		this.button27.Text = "Vend disk in picker";
		this.button27.UseVisualStyleBackColor = true;
		this.button27.Click += new System.EventHandler(button27_Click);
		this.button26.Location = new System.Drawing.Point(441, 46);
		this.button26.Name = "button26";
		this.button26.Size = new System.Drawing.Size(120, 68);
		this.button26.TabIndex = 2;
		this.button26.Text = "Take Disk at door";
		this.button26.UseVisualStyleBackColor = true;
		this.button26.Click += new System.EventHandler(button26_Click);
		this.button25.Location = new System.Drawing.Point(21, 455);
		this.button25.Name = "button25";
		this.button25.Size = new System.Drawing.Size(120, 58);
		this.button25.TabIndex = 1;
		this.button25.Text = "Clear Output";
		this.button25.UseVisualStyleBackColor = true;
		this.button25.Click += new System.EventHandler(button25_Click);
		this.m_engineeringOutput.FormattingEnabled = true;
		this.m_engineeringOutput.Location = new System.Drawing.Point(21, 31);
		this.m_engineeringOutput.Name = "m_engineeringOutput";
		this.m_engineeringOutput.Size = new System.Drawing.Size(392, 407);
		this.m_engineeringOutput.TabIndex = 0;
		this.m_exitButton.BackColor = System.Drawing.Color.GreenYellow;
		this.m_exitButton.Location = new System.Drawing.Point(466, 644);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(135, 59);
		this.m_exitButton.TabIndex = 14;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		this.errorProvider1.ContainerControl = this;
		this.m_verticalSyncWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(m_verticalSyncWorker_DoWork);
		this.m_verticalSyncWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(m_verticalSyncWorker_RunWorkerCompleted);
		this.m_runInitWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(m_runInitWorker_DoWork);
		this.m_runInitWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(m_runInitWorker_RunWorkerCompleted);
		this.m_runDecodeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(m_runDecodeWorker_DoWork);
		this.m_runDecodeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(m_runDecodeWorker_RunWorkerCompleted);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1116, 715);
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.m_devicesTab);
		base.MaximizeBox = false;
		base.Name = "ConfiguredDevicesForm";
		this.Text = "ConfiguredDevicesForm";
		this.m_devicesTab.ResumeLayout(false);
		this.m_airExchangerTab.ResumeLayout(false);
		this.m_airExchangerTab.PerformLayout();
		this.m_cortexTab.ResumeLayout(false);
		this.m_cortexTab.PerformLayout();
		this.m_fraudSensorTab.ResumeLayout(false);
		this.m_fraudSensorTab.PerformLayout();
		this.groupBox1.ResumeLayout(false);
		this.m_kioskFunctionCheck.ResumeLayout(false);
		this.m_kioskFunctionCheck.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.tabPage1.ResumeLayout(false);
		this.m_hwCorrections.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox6.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.m_hwCorrectiontypeGB.ResumeLayout(false);
		this.m_hwCorrectiontypeGB.PerformLayout();
		this.m_irConfigurationTab.ResumeLayout(false);
		this.groupBox8.ResumeLayout(false);
		this.groupBox8.PerformLayout();
		this.groupBox7.ResumeLayout(false);
		this.m_engineeringTab.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		base.ResumeLayout(false);
	}
}
