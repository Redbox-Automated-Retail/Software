using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

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

		protected override void SetupJob(HardwareJob job)
		{
			job.Push(IterationPause);
			job.Push(ScanPause);
			job.Push(Iterations);
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

		protected override void SetupJob(HardwareJob job)
		{
			job.Push(StartupPause);
		}

		internal FraudDetectionTestExecutor(HardwareService service, int startupPause)
			: base(service)
		{
			StartupPause = startupPause;
		}
	}

	private IDictionary<string, string> FormProperties;

	private KioskFunctionTest FunctionTest;

	private readonly HardwareService Service;

	private readonly OutputBox KioskTestOutput;

	private readonly OutputBox AirExchangerOutput;

	private readonly OutputBox FraudSensorOutput;

	private readonly OutputBox OutputBox;

	private readonly ButtonAspectsManager Manager;

	private readonly byte[] CenterItemProgram;

	private bool HasAirExchanger;

	private bool HasFraudSensor;

	private bool HasCortexCamera;

	private readonly QRInstallHelper QuickReturnControl;

	private IContainer components;

	private TabControl m_devicesTab;

	private TabPage m_quickReturnTab;

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

	private Button button8;

	private Button button9;

	private Button button12;

	private Button button17;

	private Button button18;

	private Button button19;

	private TextBox m_verticalTestTB;

	private TextBox m_initTB;

	private TextBox m_testVendDoorTB;

	private TextBox m_checkDriversStatusTB;

	private TextBox m_unknownCountTB;

	private TextBox m_cameraSnapTB;

	private Button button20;

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

	public ConfiguredDevicesForm(HardwareService service, IDictionary<string, string> properties)
	{
		InitializeComponent();
		OutputBox = new ConfiguredDeviceOuptutBox(m_cortexStatusBox);
		Service = service;
		m_devicesTab.Selected += OnTabSelectedChange;
		Manager = new ButtonAspectsManager();
		AirExchangerOutput = new ConfiguredDeviceOuptutBox(m_iceQubeOutput);
		FraudSensorOutput = new ConfiguredDeviceOuptutBox(m_fraudSensorOutput);
		KioskTestOutput = new ConfiguredDeviceOuptutBox(m_kioskTestOutput);
		using (MachineConfiguration machineConfiguration = new MachineConfiguration(Service))
		{
			machineConfiguration.Run();
			HasAirExchanger = machineConfiguration.AirExchangerConfigured;
			HasFraudSensor = machineConfiguration.HasFraudDevice;
			HasCortexCamera = !machineConfiguration.LegacyCamera;
			QuickReturnControl = new QRInstallHelper(Service, machineConfiguration.QuickReturnStatus);
		}
		QuickReturnControl.Dock = DockStyle.Fill;
		m_quickReturnTab.Controls.Add(QuickReturnControl);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("ROLLER POS=6 TIMEOUT=4000 WAIT=TRUE ");
		stringBuilder.AppendLine(" ROLLER POS=3 TIMEOUT=4000 WAIT=TRUE ");
		stringBuilder.AppendLine(" CLEAR");
		CenterItemProgram = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		FormProperties = properties;
		bool flag = false;
		if (FormProperties.ContainsKey("AllowSessionDisplay"))
		{
			try
			{
				flag = bool.Parse(FormProperties["AllowSessionDisplay"]);
			}
			catch
			{
				flag = false;
			}
		}
		m_displaySessionsButton.Enabled = (m_displaySessionsButton.Visible = flag);
		listView1.Columns.Add("Component", 200);
		listView1.Columns.Add("Value", 200);
	}

	private void ConfigureCortexPanel()
	{
		m_installCortexButton.Enabled = !HasCortexCamera;
		m_uninstallCortexButton.Enabled = HasCortexCamera;
		WriteToBox(string.Format("The camera {0} enabled.", HasCortexCamera ? "is" : "is not"));
	}

	private void OnConfigureIceQubeBoard()
	{
		m_iceQubeExchangerLabel.Text = string.Format("The board {0} configured.", HasAirExchanger ? "is" : "is not");
		m_configureQubeButton.Text = (HasAirExchanger ? "Remove" : "Configure");
		m_resetQubeBoard.Enabled = HasAirExchanger;
		m_qubeStatusButton.Enabled = HasAirExchanger;
		m_resetPersistentCounter.Enabled = HasAirExchanger;
	}

	private void OnTabSelectedChange(object sender, EventArgs e)
	{
		switch (m_devicesTab.SelectedIndex)
		{
		case 1:
			OnConfigureIceQubeBoard();
			break;
		case 2:
			ConfigureCortexPanel();
			break;
		case 3:
			OnConfigureFraudSensor();
			break;
		case 4:
			OnConfigureKioskTest();
			break;
		}
	}

	private void OnConfigureKioskTest()
	{
		ISessionUser currentSession = ServiceLocator.Instance.GetService<ISessionUserService>().GetCurrentSession();
		FunctionTest = new KioskFunctionTest(Service, KioskTestOutput, HasCortexCamera, currentSession.User);
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
		bool hasFraudSensor = !HasFraudSensor;
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate($"SETCFG \"EnableIceQubePolling\" \"{hasFraudSensor.ToString()}\" TYPE=CONTROLLER", out var _);
		if (!hardwareCommandResult.Success)
		{
			AirExchangerOutput.Write("Failed to update configuration.");
			{
				foreach (Error error in hardwareCommandResult.Errors)
				{
					AirExchangerOutput.Write(error.ToString());
				}
				return;
			}
		}
		HasFraudSensor = hasFraudSensor;
		AirExchangerOutput.Write("The air exchanger is {0}.", HasFraudSensor ? "configured" : "not configured");
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
		using (Manager.MakeAspect(sender))
		{
			string tagFromButton = GetTagFromButton(sender);
			if (string.IsNullOrEmpty(tagFromButton))
			{
				box.Write("There is no operand associated with that button.");
				return;
			}
			string instruction = $"{mnemonic} {tagFromButton}";
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, instruction);
			if (hardwareJob == null)
			{
				box.Write("Instruction execution failed.");
			}
			else
			{
				box.Write(hardwareJob.GetTopOfStack());
			}
		}
	}

	private string GetTagFromButton(object sender)
	{
		if (!(sender is Button button))
		{
			return string.Empty;
		}
		if (button.Tag is string result)
		{
			return result;
		}
		return string.Empty;
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void m_installCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender, install: true);
	}

	private void m_uninstallCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender, install: false);
	}

	private void ToggleCortex(object sender, bool install)
	{
		using (Manager.MakeAspect(sender))
		{
			string statement = string.Format("SETCFG \"ScannerService\" \"{0}\" TYPE=CAMERA", install ? "Cortex" : "Legacy");
			if (Service.ExecuteImmediate(statement, out var job).Success)
			{
				WriteToBox("Successfully changed camera configuration.");
				PerfFunctions.Wait(2000);
				if (install)
				{
					WriteToBox("Probing ports for device...");
					job = CommonFunctions.ExecuteInstruction(Service, "CAMERA SCANFORPORT");
					if (job != null)
					{
						string topOfStack = job.GetTopOfStack();
						if (topOfStack == "NONE")
						{
							WriteToBox("Device probe failed; install unsuccessful.");
						}
						else
						{
							statement = $"SETCFG \"SnapDecodePort\" \"{topOfStack}\" TYPE=CAMERA";
							if (Service.ExecuteImmediate(statement, out job).Success)
							{
								WriteToBox($"The camera was successfully installed on port {topOfStack}");
							}
							else
							{
								WriteToBox("There was an error during installation.");
							}
						}
					}
				}
				else
				{
					Service.ExecuteImmediate("SETCFG \"SnapDecodePort\" \"NONE\" TYPE=CAMERA", out job);
				}
				m_installCortexButton.Enabled = !install;
				Button uninstallCortexButton = m_uninstallCortexButton;
				bool enabled = (m_startCortexButton.Enabled = install);
				uninstallCortexButton.Enabled = enabled;
			}
			else
			{
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
			WriteToBox("Waiting for camera to start - could take up to 15s.");
			bool flag = bool.Parse(CommonFunctions.ExecuteInstruction(Service, "CAMERA START").GetTopOfStack());
			WriteToBox(flag ? "Camera started." : "Camera DIDN'T START.");
		}
	}

	private void RunPostTest()
	{
		using (FraudSensorPost fraudSensorPost = new FraudSensorPost(Service))
		{
			fraudSensorPost.AddSink(ProcessPostEvent);
			fraudSensorPost.Run();
			foreach (ProgramResult result in fraudSensorPost.Results)
			{
				FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
			}
		}
		BeginInvoke((MethodInvoker)delegate
		{
			m_fraudPOSTButton.Enabled = true;
		});
	}

	private void ProcessPostEvent(HardwareJob job, DateTime time, string msg)
	{
		FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg);
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
				{
					foreach (Error error in hardwareCommandResult.Errors)
					{
						FraudSensorOutput.Write(error.ToString());
					}
					return;
				}
			}
			FraudSensorOutput.Write("Fraud sensor is {0}.", flag ? "enabled" : "disabled");
			HasFraudSensor = flag;
			OnConfigureFraudSensor();
		}
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
			CompositeFunctions.GetItem(sender, Manager, integer, integer2, FraudSensorOutput, Service);
			Service.ExecuteImmediateProgram(CenterItemProgram, out var _);
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		CompositeFunctions.VendDisk(sender, Service, Manager, FraudSensorOutput);
	}

	private void button4_Click_1(object sender, EventArgs e)
	{
		int integer = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
		int integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
		if (-1 == integer || -1 == integer2)
		{
			FraudSensorOutput.Write("Check deck and slot!");
		}
		else
		{
			CompositeFunctions.PutItem(sender, Manager, Service, integer, integer2, OutputBox);
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
		using (Manager.MakeAspect(sender))
		{
			GetInstructionFromButtonAndExecute(sender, FraudSensorOutput);
		}
	}

	private void GetInstructionFromButtonAndExecute(object sender, OutputBox box)
	{
		if (!(sender is Button button))
		{
			return;
		}
		string text = button.Tag as string;
		if (string.IsNullOrEmpty(text))
		{
			OutputBox.Write("Unable to parse instruction.");
			return;
		}
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, text, 120000);
		if (hardwareJob != null)
		{
			string topOfStack = hardwareJob.GetTopOfStack();
			box.Write(text + " - " + topOfStack);
		}
		else
		{
			box.Write("Command failed.");
		}
	}

	private void RunUserReturn()
	{
		using (ReturnExecutor returnExecutor = new ReturnExecutor(Service))
		{
			returnExecutor.AddSink(ProcessPostEvent);
			returnExecutor.Run();
			foreach (ProgramResult result in returnExecutor.Results)
			{
				FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
			}
		}
		BeginInvoke((MethodInvoker)delegate
		{
			button7.Enabled = true;
		});
	}

	private void button3_Click_1(object sender, EventArgs e)
	{
		FraudSensorOutput.Clear();
	}

	private void button10_Click(object sender, EventArgs e)
	{
		AirExchangerOutput.Clear();
	}

	private void button13_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			GetInstructionFromButtonAndExecute(sender, FraudSensorOutput);
		}
	}

	private void RunStressTest()
	{
		int integer = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
		int integer2 = m_stressIterationsTB.GetInteger("iterations", FraudSensorOutput);
		int integer3 = m_readPauseTB.GetInteger("Iteration Pause", FraudSensorOutput);
		using (FraudStressTestExecutor fraudStressTestExecutor = new FraudStressTestExecutor(Service, integer2, integer, integer3))
		{
			fraudStressTestExecutor.AddSink(ProcessPostEvent);
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
			CommonFunctions.ExecuteInstruction(Service, "CONTROLSYSTEM CENTER");
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
		using (Manager.MakeAspect(sender))
		{
			m_initTB.Text = FunctionTest.RunInit().ToString().ToUpper();
		}
	}

	private void button17_Click(object sender, EventArgs e)
	{
		int integer = m_sourceSlotTB.GetInteger("Test Slot", KioskTestOutput);
		if (-1 != integer)
		{
			using (Manager.MakeAspect(sender))
			{
				m_verticalTestTB.Text = FunctionTest.RunVertialSlotTest(integer).ToString().ToUpper();
			}
		}
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
		PopulateLocationBox(m_sourceSlotTB);
	}

	private void button20_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int integer = m_sourceDeckTB.GetInteger("Sync deck", KioskTestOutput);
			int integer2 = m_sourceSlotTB.GetInteger("Sync slot", KioskTestOutput);
			if (-1 == integer)
			{
				errorProvider1.SetError(m_sourceDeckTB, "Please enter valid deck.");
			}
			else if (-1 == integer2)
			{
				errorProvider1.SetError(m_sourceSlotTB, "Please enter valid slot.");
			}
			else
			{
				m_cameraSnapTB.Text = FunctionTest.TestCameraSnap(integer, integer2).ToString().ToUpper();
			}
		}
	}

	private void button21_Click(object sender, EventArgs e)
	{
		PopulateLocationBox(m_sourceDeckTB);
	}

	private void button22_Click(object sender, EventArgs e)
	{
		PopulateLocationBox(m_sourceSlotTB);
	}

	private void PopulateLocationBox(TextBox box)
	{
		LocationNumberPad locationNumberPad = new LocationNumberPad();
		if (locationNumberPad.ShowDialog() == DialogResult.OK)
		{
			box.Text = locationNumberPad.Number.ToString();
		}
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
		this.m_quickReturnTab = new System.Windows.Forms.TabPage();
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
		this.button20 = new System.Windows.Forms.Button();
		this.m_verticalTestTB = new System.Windows.Forms.TextBox();
		this.m_initTB = new System.Windows.Forms.TextBox();
		this.m_testVendDoorTB = new System.Windows.Forms.TextBox();
		this.m_checkDriversStatusTB = new System.Windows.Forms.TextBox();
		this.button18 = new System.Windows.Forms.Button();
		this.button17 = new System.Windows.Forms.Button();
		this.button12 = new System.Windows.Forms.Button();
		this.button9 = new System.Windows.Forms.Button();
		this.button8 = new System.Windows.Forms.Button();
		this.m_userName = new System.Windows.Forms.TextBox();
		this.button21 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.button2 = new System.Windows.Forms.Button();
		this.m_kioskTestOutput = new System.Windows.Forms.ListBox();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.button16 = new System.Windows.Forms.Button();
		this.button11 = new System.Windows.Forms.Button();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.listView1 = new System.Windows.Forms.ListView();
		this.m_devicesTab.SuspendLayout();
		this.m_airExchangerTab.SuspendLayout();
		this.m_cortexTab.SuspendLayout();
		this.m_fraudSensorTab.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.m_kioskFunctionCheck.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.tabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		base.SuspendLayout();
		this.m_devicesTab.Controls.Add(this.m_quickReturnTab);
		this.m_devicesTab.Controls.Add(this.m_airExchangerTab);
		this.m_devicesTab.Controls.Add(this.m_cortexTab);
		this.m_devicesTab.Controls.Add(this.m_fraudSensorTab);
		this.m_devicesTab.Controls.Add(this.m_kioskFunctionCheck);
		this.m_devicesTab.Controls.Add(this.tabPage1);
		this.m_devicesTab.Location = new System.Drawing.Point(2, 12);
		this.m_devicesTab.Name = "m_devicesTab";
		this.m_devicesTab.Padding = new System.Drawing.Point(10, 20);
		this.m_devicesTab.SelectedIndex = 0;
		this.m_devicesTab.Size = new System.Drawing.Size(951, 626);
		this.m_devicesTab.TabIndex = 0;
		this.m_quickReturnTab.BackColor = System.Drawing.Color.LightGray;
		this.m_quickReturnTab.Location = new System.Drawing.Point(4, 56);
		this.m_quickReturnTab.Name = "m_quickReturnTab";
		this.m_quickReturnTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_quickReturnTab.Size = new System.Drawing.Size(943, 566);
		this.m_quickReturnTab.TabIndex = 0;
		this.m_quickReturnTab.Text = "Quick Return Device";
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
		this.m_airExchangerTab.Size = new System.Drawing.Size(943, 566);
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
		this.m_cortexTab.Size = new System.Drawing.Size(943, 566);
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
		this.m_fraudSensorTab.Size = new System.Drawing.Size(943, 566);
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
		this.button13.Click += new System.EventHandler(button13_Click);
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
		this.m_kioskFunctionCheck.Size = new System.Drawing.Size(943, 566);
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
		this.groupBox2.Controls.Add(this.button20);
		this.groupBox2.Controls.Add(this.m_verticalTestTB);
		this.groupBox2.Controls.Add(this.m_initTB);
		this.groupBox2.Controls.Add(this.m_testVendDoorTB);
		this.groupBox2.Controls.Add(this.m_checkDriversStatusTB);
		this.groupBox2.Controls.Add(this.button18);
		this.groupBox2.Controls.Add(this.button17);
		this.groupBox2.Controls.Add(this.button12);
		this.groupBox2.Controls.Add(this.button9);
		this.groupBox2.Controls.Add(this.button8);
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
		this.button20.Location = new System.Drawing.Point(12, 89);
		this.button20.Name = "button20";
		this.button20.Size = new System.Drawing.Size(100, 60);
		this.button20.TabIndex = 2;
		this.button20.Text = "Decode Test";
		this.button20.UseVisualStyleBackColor = true;
		this.button20.Click += new System.EventHandler(button20_Click);
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
		this.button18.Location = new System.Drawing.Point(12, 371);
		this.button18.Name = "button18";
		this.button18.Size = new System.Drawing.Size(100, 60);
		this.button18.TabIndex = 7;
		this.button18.Text = "Unknown Count";
		this.button18.UseVisualStyleBackColor = true;
		this.button18.Click += new System.EventHandler(button18_Click);
		this.button17.Location = new System.Drawing.Point(12, 298);
		this.button17.Name = "button17";
		this.button17.Size = new System.Drawing.Size(100, 60);
		this.button17.TabIndex = 6;
		this.button17.Text = "Vertical Slot Test";
		this.button17.UseVisualStyleBackColor = true;
		this.button17.Click += new System.EventHandler(button17_Click);
		this.button12.Location = new System.Drawing.Point(12, 155);
		this.button12.Name = "button12";
		this.button12.Size = new System.Drawing.Size(100, 60);
		this.button12.TabIndex = 4;
		this.button12.Text = "Test Vend Door";
		this.button12.UseVisualStyleBackColor = true;
		this.button12.Click += new System.EventHandler(button12_Click);
		this.button9.Location = new System.Drawing.Point(12, 232);
		this.button9.Name = "button9";
		this.button9.Size = new System.Drawing.Size(100, 60);
		this.button9.TabIndex = 5;
		this.button9.Text = "Init";
		this.button9.UseVisualStyleBackColor = true;
		this.button9.Click += new System.EventHandler(button9_Click);
		this.button8.Location = new System.Drawing.Point(12, 19);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(100, 60);
		this.button8.TabIndex = 0;
		this.button8.Text = "Check Drivers";
		this.button8.UseVisualStyleBackColor = true;
		this.button8.Click += new System.EventHandler(button8_Click);
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
		this.tabPage1.Size = new System.Drawing.Size(943, 566);
		this.tabPage1.TabIndex = 7;
		this.tabPage1.Text = "Hardware Survey";
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
		this.m_exitButton.BackColor = System.Drawing.Color.GreenYellow;
		this.m_exitButton.Location = new System.Drawing.Point(12, 644);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(135, 59);
		this.m_exitButton.TabIndex = 14;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		this.errorProvider1.ContainerControl = this;
		this.listView1.Location = new System.Drawing.Point(20, 35);
		this.listView1.Name = "listView1";
		this.listView1.Size = new System.Drawing.Size(502, 378);
		this.listView1.TabIndex = 3;
		this.listView1.UseCompatibleStateImageBehavior = false;
		this.listView1.View = System.Windows.Forms.View.Details;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(975, 715);
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
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		base.ResumeLayout(false);
	}
}
