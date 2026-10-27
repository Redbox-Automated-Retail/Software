using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal sealed class Program
{
	private class FraudLocInfo
	{
		internal readonly List<DecodeInfo> Dumps = new List<DecodeInfo>();

		internal readonly Location Location;

		internal string Matrix
		{
			get
			{
				List<DecodeInfo> list = Dumps.FindAll((DecodeInfo each) => !each.IsSecure);
				if (list.Count == 0)
				{
					return "UNKNOWN";
				}
				if (list.Count > 1)
				{
					throw new InvalidOperationException();
				}
				return list[0].Matrix;
			}
		}

		public override string ToString()
		{
			return $"Deck = {Location.Deck} Slot = {Location.Slot}";
		}

		internal bool HasSecureRead()
		{
			return Dumps.FindAll((DecodeInfo each) => each.IsSecure).Count > 0;
		}

		internal FraudLocInfo(int deck, int slot)
		{
			Location = new Location
			{
				Deck = deck,
				Slot = slot
			};
		}
	}

	private class DecodeInfo
	{
		internal readonly bool IsSecure;

		internal string Matrix;

		internal int Count;

		internal DecodeInfo(string matrix, int count)
		{
			Matrix = matrix;
			Count = count;
			IsSecure = matrix.Equals("YES", StringComparison.CurrentCultureIgnoreCase);
		}
	}

	[Flags]
	private enum CorrectionOptions
	{
		None = 0,
		RestartDevice = 1
	}

	private delegate IQueryUsbDeviceResult GetResult(IUsbDeviceService usb);

	private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(initialState: false);

	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

	private static void PrintUsageAndExit()
	{
		PrintUsageAndExit(new string[0]);
	}

	private static void PrintUsageAndExit(string[] secureOptions)
	{
		Console.WriteLine("HALUtilities.exe version {0}", typeof(Program).Assembly.GetName().Version);
		Console.WriteLine("## Options ## ");
		Console.WriteLine("  Where: ");
		Console.WriteLine("  \t\t--cameraGeneration: returns which camera generation is in use.");
		Console.WriteLine("  \t\t--decoderType: returns which barcode decoder is in use.");
		Console.WriteLine("  \t\t--findTripplite:<true|false>  Probes for any tripplite devices attached to the computer.");
		Console.WriteLine("  \t\t-findABEDevice  Probes for an attached ABE device via HAL service.");
		Console.WriteLine("  \t\t-findABEDeviceThroughScan  Probes for an attached ABE device via USB query.");
		Console.WriteLine("  \t\t-findKFCDevices  Probes for active touchscreen & camera attached to computer. Logs to KFC.log.");
		Console.WriteLine("  \t\t-findActiveTouchScreen  Probes for active touchscreen attached to computer. Logs to TSLocator.log.");
		Console.WriteLine("  \t\t-findActiveCamera  Probes for active camera attached to computer. Logs to CameraLocator.log.");
		Console.WriteLine("  \t\t-resetControlSystem  Resets the kiosk control system.");
		Console.WriteLine("  \t\t-resetMotionControl  Resets the motion control system.");
		Console.WriteLine("  \t\t-resetPDCMCounter  Resets the PDCM failure count.");
		Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
		Console.WriteLine("  \t\t-dumpTouchScreens  Dumps monitor class devices to Monitor.log");
		Console.WriteLine("  \t\t-dumpDevices  Dumps all device classes to AllDevices.log");
		Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
		Console.WriteLine("  \t\t-e syncs empties ");
		Console.WriteLine("  \t\t-unknown syncs unknowns ");
		Console.WriteLine("  \t\t-fullSync schedules a low-priority sync");
		Console.WriteLine("  \t\t-dumpInventory writes the inventory to the file \"inventory.xml\"");
		Console.WriteLine("  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
		Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
		Console.WriteLine("  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
		Console.WriteLine("  \t\t-restartQR Tries to restart the QR device.");
		Console.WriteLine("  \t\t-stopQR Tries to stop the QR device (does not remove it from configuration).");
		Console.WriteLine("  \t\t-checkQRStatus If the QR device is not configured and active, will generate a 'qr-status.log' file with any errors.");
		Console.WriteLine("  \t\t-version Report the tool's verison.");
		Console.WriteLine("  \t\t-checkDupCounter Generate a log file if the machine has counters.");
		Console.WriteLine("  \t\t-resetDupCounter Reset the duplicate barcode counter.");
		Console.WriteLine("  \t\t-updateReboot={time} Updates the reboot time and cycles the service.");
		Console.WriteLine("  \t\t-checkInventory={path} Checks HAL inventory against file specified.");
		Console.WriteLine("  \t\t-assemblyVersions Dumps the HAL assembly versions to the console.");
		Console.WriteLine("  \t\t-bmpconverter Specifies to run as bitmap converter; passes remaining arguments to converter.");
		Array.ForEach(secureOptions, delegate(string secMsg)
		{
			Console.WriteLine(secMsg);
		});
		Console.WriteLine("  \t\t-help print this message and exit.");
		Environment.Exit(1);
	}

	private static void PrintSecureHelp()
	{
		PrintUsageAndExit(new string[10] { "  \t\t-randomSync  Runs a random sync of the kiosk.", "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.", "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.", "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.", "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.", "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.", "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.", "  \t\t-backupConfig Backs up the hal.xml & gamp files.", "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.", "  \t\t-fraudStressTest [--scanPause:pause --iterations:iter --readPause:rp] Runs a stress test on the fraud sensor." });
	}

	private static void GenericimerFired(object source, ElapsedEventArgs e)
	{
		GenericWaiter.Set();
	}

	private static string[] GetProgramArgs(string[] allArgs, int start)
	{
		string[] array = new string[allArgs.Length - (start + 1)];
		Array.Copy(allArgs, start + 1, array, 0, array.Length);
		return array;
	}

	private static void OnSearchAndFix(GetResult getResult, IUsbDeviceService usb, CorrectionOptions options)
	{
		new ConsoleLogger(enableDebug: true);
		IQueryUsbDeviceResult queryUsbDeviceResult = getResult(usb);
		if (!queryUsbDeviceResult.Found)
		{
			LogHelper.Instance.Log("Unable to find device.");
			Environment.Exit(256);
		}
		int exitCode = 0;
		LogHelper.Instance.Log("Found device {0}", queryUsbDeviceResult);
		if (queryUsbDeviceResult.Running)
		{
			LogHelper.Instance.Log("The device appears to be working.");
		}
		else if ((CorrectionOptions.RestartDevice & options) != CorrectionOptions.None)
		{
			if (queryUsbDeviceResult.IsNotStarted)
			{
				LogHelper.Instance.Log(" Device not started; RESET returned {0}", queryUsbDeviceResult.Descriptor.ResetDriver());
			}
			else if (queryUsbDeviceResult.IsDisabled)
			{
				LogHelper.Instance.Log(" Device not enabled; change to enable returned {0}", usb.ChangeByHWID(queryUsbDeviceResult.Descriptor, DeviceState.Enable));
			}
			DeviceStatus deviceStatus = usb.FindDeviceStatus(queryUsbDeviceResult.Descriptor);
			LogHelper.Instance.Log(" Device status AFTER fix = {0}", deviceStatus);
			if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None && (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
			{
				LogHelper.Instance.Log("  ** Successfully reset the device **");
			}
			else
			{
				LogHelper.Instance.Log("  !!Failed to reset the device!!");
				exitCode = 258;
			}
		}
		Environment.Exit(exitCode);
	}

	private static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			PrintUsageAndExit();
		}
		IRuntimeService runtimeService = new RuntimeService();
		ServiceLocator.Instance.AddService<IRuntimeService>(runtimeService);
		ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>(new DeviceSetupClassFactory());
		UsbDeviceService usbDeviceService = new UsbDeviceService(debug: true);
		ServiceLocator.Instance.AddService<IUsbDeviceService>(usbDeviceService);
		HardwareService hardwareService = new HardwareService(IPCProtocol.Parse(Constants.HALIPCStrings.TcpServer));
		ClientHelper clientHelper = new ClientHelper(hardwareService);
		bool flag = true;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("--protocol"))
			{
				string optionValue = CommandLineOption.GetOptionValue(args[i], "=");
				try
				{
					hardwareService = new HardwareService(IPCProtocol.Parse(optionValue));
					clientHelper.Dispose();
					clientHelper = new ClientHelper(hardwareService);
				}
				catch
				{
				}
				continue;
			}
			if (args[i] == "--cameraGeneration")
			{
				Console.WriteLine(hardwareService.CurrentCameraGeneration.ToString());
				continue;
			}
			if (args[i] == "--decoderType")
			{
				Console.WriteLine(hardwareService.BarcodeDecoder.ToString());
				continue;
			}
			if (args[i] == "-scheduleTestSync")
			{
				List<Location> locations = new List<Location>
				{
					new Location
					{
						Deck = 1,
						Slot = 1
					},
					new Location
					{
						Deck = 1,
						Slot = 2
					}
				};
				hardwareService.HardSync(locations, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Low
				}, out var _);
				break;
			}
			if (args[i].StartsWith("--statusCCR"))
			{
				using (TestAndResetCCRExecutor testAndResetCCRExecutor = new TestAndResetCCRExecutor(hardwareService))
				{
					testAndResetCCRExecutor.Run();
					if (HardwareJobStatus.Errored == testAndResetCCRExecutor.EndStatus)
					{
						Console.WriteLine("!! Failed to reset CCR !!");
					}
					else if (HardwareJobStatus.Completed == testAndResetCCRExecutor.EndStatus)
					{
						Console.WriteLine("CCR should be working.");
					}
				}
				continue;
			}
			if (args[i].StartsWith("--statusCamera"))
			{
				CorrectionOptions correctionOptions = CorrectionOptions.None;
				if (CommandLineOption.GetOptionVal(args[i], defVal: false))
				{
					correctionOptions |= CorrectionOptions.RestartDevice;
				}
				OnSearchAndFix((IUsbDeviceService _usb) => _usb.FindCamera(), usbDeviceService, correctionOptions);
				continue;
			}
			if (args[i].StartsWith("--removeStats"))
			{
				HardwareCorrectionStatistic optionVal = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal != HardwareCorrectionStatistic.None)
				{
					if (!hardwareService.RemoveHardwareCorrectionStats(optionVal, new HardwareJobSchedule
					{
						Priority = HardwareJobPriority.High
					}, out var job2).Success)
					{
						Console.WriteLine("Failed to schedule remove stats job.");
						continue;
					}
					clientHelper.WaitForJob(job2, out var endStatus);
					Console.WriteLine("Remove all stats job {0} ended with status {1}", job2.ID, endStatus);
				}
				continue;
			}
			if (args[i].StartsWith("-removeAllStats"))
			{
				if (!hardwareService.RemoveHardwareCorrectionStats(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job3).Success)
				{
					Console.WriteLine("Failed to schedule remove all stats job.");
					continue;
				}
				clientHelper.WaitForJob(job3, out var endStatus2);
				Console.WriteLine("Remove all stats job {0} ended with status {1}", job3.ID, endStatus2);
				continue;
			}
			if (args[i].StartsWith("--consoleDebug"))
			{
				flag = CommandLineOption.GetOptionVal(args[i], flag);
				continue;
			}
			if (args[i] == "-customerReturn")
			{
				Thread thread = new Thread(CustomerReturn.RunExecutor);
				thread.Start(hardwareService);
				thread.Join();
				break;
			}
			if (args[i] == "-returnUnknown")
			{
				using (ReturnUnknownExecutor returnUnknownExecutor = new ReturnUnknownExecutor(hardwareService))
				{
					returnUnknownExecutor.Run();
					Console.WriteLine("{0} ( ID = {1} ) ended with status {2}", returnUnknownExecutor.Job.ProgramName, returnUnknownExecutor.ID, returnUnknownExecutor.EndStatus);
					Console.WriteLine(" -- program results --");
					returnUnknownExecutor.Results.ForEach(delegate(ProgramResult each)
					{
						Console.WriteLine("  Code: {0}  Message {1}", each.Code, each.Message);
					});
					break;
				}
			}
			if (args[i].StartsWith("-randomSync"))
			{
				int num = 1;
				int num2 = 1;
				int num3 = 1;
				bool flag2 = false;
				for (int num4 = i + 1; num4 < args.Length; num4++)
				{
					if (args[num4].StartsWith("--iterations"))
					{
						num = CommandLineOption.GetOptionVal(args[num4], num);
					}
					else if (args[num4].StartsWith("--vendTime"))
					{
						num2 = CommandLineOption.GetOptionVal(args[num4], num2);
					}
					else if (args[num4].StartsWith("--vendFrequency"))
					{
						num3 = CommandLineOption.GetOptionVal(args[num4], num3);
					}
					else if (args[num4].StartsWith("--oneDisk"))
					{
						flag2 = CommandLineOption.GetOptionVal(args[num4], flag2);
					}
				}
				int exitCode = 0;
				for (int num5 = 1; num5 <= num; num5++)
				{
					Console.WriteLine("Iteration {0} of {1} total.", num5, num);
					JobExecutor jobExecutor;
					if (flag2)
					{
						jobExecutor = new OneDiskRandomSyncExecutor(hardwareService, num2, num3);
						jobExecutor.AddSink(delegate(HardwareJob j, DateTime eventTime, string eventMessage)
						{
							Console.WriteLine("{0}: {1}", eventTime, eventMessage);
						});
					}
					else
					{
						jobExecutor = new RandomSyncExecutor(hardwareService, num2, num3);
					}
					using (jobExecutor)
					{
						jobExecutor.Run();
						Console.WriteLine("Job {0} ends with status {1}", jobExecutor.ID, jobExecutor.EndStatus.ToString());
						if (HardwareJobStatus.Completed != jobExecutor.EndStatus)
						{
							Console.WriteLine("** Tool is exiting after job failure **");
							exitCode = 1;
							break;
						}
					}
				}
				Environment.Exit(exitCode);
				continue;
			}
			if (args[i].StartsWith("--loopy"))
			{
				LoopyExecutor.RunExecutor(CommandLineOption.GetOptionVal(args[i], 10), hardwareService);
				break;
			}
			if (args[i].StartsWith("--msgServer"))
			{
				AbstractPipeSever.RunServer(args[i], PipeServers.Message, flag);
				continue;
			}
			if (args[i].StartsWith("--pipeServer"))
			{
				AbstractPipeSever.RunServer(args[i], PipeServers.Basic, flag);
				continue;
			}
			if (args[i] == "-rdbxPipeServer")
			{
				ServiceHostRunner.Run(Constants.HALIPCStrings.PipeServer, flag);
				continue;
			}
			if (args[i] == "-rdbxTcpServer")
			{
				ServiceHostRunner.Run(Constants.HALIPCStrings.TcpServer, flag);
				continue;
			}
			if ("-kioskTest" == args[i])
			{
				Environment.Exit(KioskTestRunner.RunKioskTest(GetProgramArgs(args, i), hardwareService));
				continue;
			}
			if (args[i] == "-findABEDevice")
			{
				new ConsoleLogger(flag);
				Console.WriteLine("ABE device {0}", hardwareService.HasABEDevice ? "found" : "not found");
				continue;
			}
			if (args[i] == "-findABEDeviceThroughScan")
			{
				AbeDeviceDescriptor descriptor = new AbeDeviceDescriptor(usbDeviceService);
				IUsbDeviceSearchResult usbDeviceSearchResult = usbDeviceService.FindDevice(descriptor);
				Console.WriteLine("ABE device {0}", usbDeviceSearchResult.Found ? "found" : "not found");
				continue;
			}
			if ("-findKFCDevices" == args[i])
			{
				string path = "KFC.log";
				using (StreamWriter streamWriter = new StreamWriter(runtimeService.CreateBackup(path)))
				{
					new ConsoleLogger(flag);
					ITouchscreenDescriptor touchscreenDescriptor = usbDeviceService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor.ToString(), touchscreenDescriptor.Friendlyname);
					}
					IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: true);
					if (deviceDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a camera or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found camera {0} {1}", deviceDescriptor.ToString(), deviceDescriptor.Friendlyname);
					}
				}
				continue;
			}
			if ("-findActiveTouchScreen" == args[i])
			{
				string path2 = "TSLocator.log";
				runtimeService.CreateBackup(path2);
				using (StreamWriter streamWriter2 = new StreamWriter(path2))
				{
					new ConsoleLogger(flag);
					ITouchscreenDescriptor touchscreenDescriptor2 = usbDeviceService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor2 == null)
					{
						streamWriter2.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter2.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor2.ToString(), touchscreenDescriptor2.Friendlyname);
					}
				}
				continue;
			}
			if (args[i].StartsWith("--findActiveCamera"))
			{
				bool optionVal2 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				string path3 = "CameraLocator.log";
				runtimeService.CreateBackup(path3);
				using (StreamWriter streamWriter3 = new StreamWriter(path3))
				{
					new ConsoleLogger(flag);
					IDeviceDescriptor deviceDescriptor2 = usbDeviceService.FindActiveCamera(matchDriver: true);
					if (deviceDescriptor2 == null)
					{
						streamWriter3.WriteLine("Could not find a camera or match its driver.");
						continue;
					}
					streamWriter3.WriteLine("## Camera Info ## ");
					streamWriter3.WriteLine("  HWID         : {0}", deviceDescriptor2.ToString());
					streamWriter3.WriteLine("  Friendly     : {0}", deviceDescriptor2.Friendlyname);
					streamWriter3.WriteLine("  Status       : {0}", deviceDescriptor2.GetStatus().ToString());
					if (optionVal2)
					{
						streamWriter3.WriteLine("  RESET status : {0}", deviceDescriptor2.ResetDriver() ? "Ok" : "Failed");
					}
				}
				continue;
			}
			if ("-resetControlSystem" == args[i])
			{
				new ConsoleLogger(flag);
				using (ResetControlSystemJob resetControlSystemJob = new ResetControlSystemJob(hardwareService))
				{
					resetControlSystemJob.Run();
					LogHelper.Instance.Log("ResetControlSystem job {0} ended with status {1}", resetControlSystemJob.ID, resetControlSystemJob.EndStatus.ToString());
				}
				continue;
			}
			if ("-resetPDCMCounter" == args[i])
			{
				Console.WriteLine("Resetting PDCM state ...");
				RegistryStore registryStore = new RegistryStore("SOFTWARE\\Redbox\\HAL");
				registryStore.SetSecretValue("PDCMUpdateCount", 0);
				int secretValue = registryStore.GetSecretValue("PDCMUpdateCount", -1);
				Console.WriteLine("PDCM state now at {0}", secretValue);
				continue;
			}
			if (args[i] == "-interactive")
			{
				string[] programArgs = GetProgramArgs(args, i);
				using (ScriptMode scriptMode = new ScriptMode(hardwareService))
				{
					scriptMode.Run(programArgs);
				}
				continue;
			}
			if (args[i].StartsWith("-resetDeck"))
			{
				string optionVal3 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				string path4 = runtimeService.RuntimePath("resetdecks.txt");
				StreamWriter vl = new StreamWriter(File.Open(path4, FileMode.Append, FileAccess.Write, FileShare.Read));
				try
				{
					if (string.IsNullOrEmpty(optionVal3))
					{
						vl.WriteLine("Invalid deck list specified.");
						break;
					}
					StringBuilder buffer = new StringBuilder();
					char[] separator = new char[1] { ',' };
					Array.ForEach(optionVal3.Split(separator, StringSplitOptions.RemoveEmptyEntries), delegate(string d)
					{
						vl.WriteLine(" Resetting deck {0}", d);
						buffer.AppendLine($" DECK RESET NUMBER=\"{d}\"");
					});
					if (hardwareService == null)
					{
						vl.WriteLine("Cannot connect to service. Please try again.");
						break;
					}
					hardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(buffer.ToString()), out var _);
				}
				finally
				{
					if (vl != null)
					{
						((IDisposable)vl).Dispose();
					}
				}
				continue;
			}
			if (args[i].StartsWith("--analyzeSync"))
			{
				new SyncCheck(CommandLineOption.GetOptionVal(args[i], "EMPTY"), runtimeService).Summarize();
				continue;
			}
			if (args[i].StartsWith("--analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole = new DefaultConsole())
				{
					string optionVal4 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal4 == "EMPTY")
					{
						defaultConsole.WriteLine("Invalid job id specified.");
						break;
					}
					HardwareJob job5 = null;
					HardwareCommandResult job6 = hardwareService.GetJob(optionVal4, out job5);
					if (!job6.Success || job5 == null)
					{
						job6.Errors.Dump(Console.Out);
						break;
					}
					clientHelper.WaitForJob(job5, out var endStatus3);
					if (endStatus3 != HardwareJobStatus.Completed)
					{
						defaultConsole.WriteLine("job {0} ended with status {1}", optionVal4, endStatus3.ToString());
						break;
					}
					using InventoryHelper inventoryHelper = new InventoryHelper(hardwareService);
					inventoryHelper.CompareSyncResults(job5, outputSuccessfulCompare: false);
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal5 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal5);
				continue;
			}
			if (args[i].StartsWith("-assemblyVersions"))
			{
				string path5 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list = new List<string>();
				string[] array = new string[2] { "*.dll", "*.exe" };
				foreach (string searchPattern in array)
				{
					list.AddRange(Directory.GetFiles(path5, searchPattern));
				}
				Console.WriteLine("{0} ( {1} files total )", DateTime.Now, list.Count);
				list.ForEach(delegate(string assembly)
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly);
						Console.WriteLine("Assembly {0} has version {1}", assembly, assemblyName.Version.ToString());
					}
					catch (Exception ex3)
					{
						Console.WriteLine("Unable to get version for {0}; message = {1}", assembly, ex3.Message);
					}
				});
				continue;
			}
			if (args[i].StartsWith("-dumpDevices"))
			{
				string path6 = "AllDevices.txt";
				runtimeService.CreateBackup(path6);
				using (StreamWriter streamWriter4 = new StreamWriter(path6))
				{
					Dictionary<string, List<string>> deviceMap = new Dictionary<string, List<string>>();
					Action<string, string> onDeviceFound = delegate(string clazz, string hwid)
					{
						List<string> list5 = null;
						list5 = (deviceMap.ContainsKey(clazz) ? deviceMap[clazz] : (deviceMap[clazz] = new List<string>()));
						list5.Add(hwid);
					};
					usbDeviceService.EnumDevices(onDeviceFound);
					foreach (string key in deviceMap.Keys)
					{
						streamWriter4.WriteLine("Devices for class {0}", key);
						foreach (string item in deviceMap[key])
						{
							streamWriter4.WriteLine("  HWID: {0}", item);
						}
					}
					foreach (string key2 in deviceMap.Keys)
					{
						deviceMap[key2].Clear();
					}
				}
				continue;
			}
			if (args[i].StartsWith("-backupConfig"))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper = new ServiceConfigurationHelper(hardwareService))
				{
					serviceConfigurationHelper.BackupConfiguration(runtimeService.KioskId);
				}
				continue;
			}
			if (args[i].StartsWith("-configureVMZ"))
			{
				using (StreamWriter streamWriter5 = new StreamWriter(File.Open(runtimeService.RuntimePath("configure-VMZ.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					streamWriter5.WriteLine("The upgrade process is no longer supported.");
				}
				continue;
			}
			if (args[i].StartsWith("-configureDoorSensors"))
			{
				string optionValue2 = CommandLineOption.GetOptionValue(args[i]);
				if (string.IsNullOrEmpty(optionValue2))
				{
					Console.WriteLine(" The -configureDoorSensors switch requires an argument");
					PrintSecureHelp();
				}
				HardwareCommandResult hardwareCommandResult = hardwareService.ScheduleJob("door-sensor-software-override", string.Empty, enableDebugging: false, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Highest
				}, out var job7);
				if (!hardwareCommandResult.Success)
				{
					Console.WriteLine("Failed to schedule configuration job.");
					hardwareCommandResult.Errors.Dump(Console.Out);
					continue;
				}
				job7.Push(optionValue2);
				job7.Pend();
				clientHelper.WaitForJob(job7, out var endStatus4);
				Console.WriteLine("Change door sensor configuration ended with status {0}", endStatus4);
				continue;
			}
			if (args[i].Contains("-checkInventory"))
			{
				string optionValue3 = CommandLineOption.GetOptionValue(args[i], "=");
				string path7 = "InventoryCompare.txt";
				runtimeService.CreateBackup(path7);
				using (StreamWriter log = new StreamWriter(path7))
				{
					InventoryChecker.Instance.CheckInventory(hardwareService, optionValue3, log);
				}
				continue;
			}
			if ("-resetDupCounter" == args[i])
			{
				if (hardwareService != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService);
				}
				continue;
			}
			if ("-checkDupCounter" == args[i])
			{
				runtimeService.CreateBackup("DuplicateCounter.log");
				HardwareService hardwareService2 = hardwareService;
				if (hardwareService2 == null)
				{
					break;
				}
				new CounterHelper().GetDuplicateCounterValue(hardwareService2, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter6 = new StreamWriter("DuplicateCounter.log");
					streamWriter6.WriteLine("The kiosk has duplicates.");
					streamWriter6.Flush();
				}
				continue;
			}
			if (args[i].StartsWith("--preposVend"))
			{
				int optionVal6 = CommandLineOption.GetOptionVal(args[i], 3);
				using (DefaultConsole defaultConsole2 = new DefaultConsole())
				{
					using PrepositionTest prepositionTest = new PrepositionTest(optionVal6, hardwareService, defaultConsole2);
					defaultConsole2.Write("Type C for continue, otherwise bail:   ");
					string text = Console.ReadLine();
					prepositionTest.PreposJob.Signal((text == "C") ? "continue" : "bail");
					clientHelper.WaitForJob(prepositionTest.PreposJob, out var endStatus5);
					defaultConsole2.WriteLine("Preposition job {0} ended with status {1}", prepositionTest.PreposJob.ID, endStatus5.ToString());
				}
				continue;
			}
			if (args[i].StartsWith("-vendTest"))
			{
				int optionVal7 = CommandLineOption.GetOptionVal(args[i], 3);
				int? startAt = null;
				if (args.Length > 1 && args[i + 1].StartsWith("-next"))
				{
					startAt = CommandLineOption.GetOptionVal(args[i + 1], -1);
				}
				using (VendTest vendTest = new VendTest(optionVal7, hardwareService))
				{
					using DefaultConsole console = new DefaultConsole();
					vendTest.Execute(console, startAt);
				}
				continue;
			}
			if (args[i].StartsWith("--scheduleClean"))
			{
				int? optionVal8 = CommandLineOption.GetOptionVal<int?>(args[i], null);
				using (DefaultConsole defaultConsole3 = new DefaultConsole())
				{
					if (optionVal8.HasValue && optionVal8 != -1)
					{
						RedboxTimer redboxTimer = new RedboxTimer("Clean", GenericimerFired);
						redboxTimer.ScheduleAtNext(optionVal8.Value, 0);
						defaultConsole3.WriteLine("Clean timer should fire {0} {1}", redboxTimer.NextFireTime.Value.ToShortDateString(), redboxTimer.NextFireTime.Value.ToShortTimeString());
						GenericWaiter.WaitOne();
					}
					if (!hardwareService.CleanZone(out var job8).Success)
					{
						defaultConsole3.WriteLine("Failed to schedule clean job.");
						break;
					}
					job8.Pend();
					clientHelper.WaitForJob(job8, out var endStatus6);
					defaultConsole3.WriteLine("Clean {0} ended with status {1}", job8.ID, endStatus6.ToString());
				}
				continue;
			}
			if ("-e".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper2 = new InventoryHelper(hardwareService))
				{
					inventoryHelper2.FixType(SyncType.Empty);
				}
				continue;
			}
			if ("-unknown".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper3 = new InventoryHelper(hardwareService))
				{
					inventoryHelper3.FixType(SyncType.Unknown);
				}
				continue;
			}
			if (args[i].StartsWith("--fullSync"))
			{
				string optionVal9 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				SyncOptions options = SyncOptions.None;
				if (string.Empty != optionVal9)
				{
					string[] array2 = optionVal9.Split(';');
					if (array2.Length == 1)
					{
						options = Enum<SyncOptions>.ParseIgnoringCase(array2[0], SyncOptions.None);
					}
					else
					{
						Array.ForEach(array2, delegate(string option)
						{
							SyncOptions syncOptions = Enum<SyncOptions>.ParseIgnoringCase(option, SyncOptions.None);
							options |= syncOptions;
						});
					}
				}
				new ConsoleLogger((options & SyncOptions.Debug) != 0);
				using (InventoryHelper inventoryHelper4 = new InventoryHelper(hardwareService))
				{
					inventoryHelper4.ScheduleFullSync(options);
				}
				continue;
			}
			if ("-fullSync".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper5 = new InventoryHelper(hardwareService))
				{
					inventoryHelper5.ScheduleFullSync();
				}
				continue;
			}
			if ("-dumpInventory".Equals(args[i]))
			{
				string path8 = runtimeService.RuntimePath("inventory.xml");
				runtimeService.CreateBackup(path8);
				using (InventoryHelper inventoryHelper6 = new InventoryHelper(hardwareService))
				{
					inventoryHelper6.DumpInventoryToFile(path8);
				}
				continue;
			}
			if ("-secureHelp".Equals(args[i]))
			{
				PrintSecureHelp();
				continue;
			}
			if ("-help".Equals(args[i]))
			{
				PrintUsageAndExit();
				continue;
			}
			if ("-importInventory".Equals(args[i]))
			{
				if (i >= args.Length - 1)
				{
					PrintUsageAndExit();
				}
				using (InventoryHelper inventoryHelper7 = new InventoryHelper(hardwareService))
				{
					if (inventoryHelper7.ImportInventoryFromFile(args[++i]))
					{
						Console.WriteLine("Inventory import OK.");
					}
					else
					{
						Console.WriteLine("Failed to import Inventory!");
					}
				}
				continue;
			}
			if ("-inventorysnapshot".Equals(args[i]))
			{
				if (!Directory.Exists("C:\\Program Files\\Redbox\\inventory-snapshots"))
				{
					try
					{
						Directory.CreateDirectory("C:\\Program Files\\Redbox\\inventory-snapshots");
					}
					catch (Exception ex)
					{
						Console.WriteLine("Unable to create directory: {0}", ex.Message);
					}
				}
				int num7 = 1;
				DateTime now = DateTime.Now;
				string empty = string.Empty;
				do
				{
					string path9 = $"{runtimeService.KioskId}-{now.Month}-{now.Day}-{now.Year}-{num7++}.csv";
					empty = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path9);
				}
				while (File.Exists(empty));
				using (KioskInventory kioskInventory = new KioskInventory(hardwareService))
				{
					List<IInventoryLocation> deckInventory = kioskInventory.DeckInventory;
					StreamWriter file = new StreamWriter(empty);
					try
					{
						file.WriteLine("Deck,Slot,Barcode");
						deckInventory.ForEach(delegate(IInventoryLocation loc)
						{
							file.WriteLine($"{loc.Location.Deck},{loc.Location.Slot},'{loc.Matrix}'");
						});
						int itemCount = 1;
						kioskInventory.DumpbinItems.ForEach(delegate(IDumpbinItem item)
						{
							file.WriteLine($"9,{itemCount++},'{item.Matrix}'");
						});
					}
					finally
					{
						if (file != null)
						{
							((IDisposable)file).Dispose();
						}
					}
				}
				continue;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper2 = new ServiceConfigurationHelper(hardwareService))
				{
					serviceConfigurationHelper2.UpdateOption(args[++i]);
					break;
				}
			}
			if ("-version".Equals(args[i]))
			{
				string path10 = runtimeService.RuntimePath("version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter7 = new StreamWriter(path10))
				{
					streamWriter7.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
			}
			else if ("-dumpExcluded".Equals(args[i]))
			{
				string path11 = runtimeService.RuntimePath("excluded.log");
				using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(hardwareService);
				excludedLocsJob.Run();
				using StreamWriter streamWriter8 = new StreamWriter(path11);
				IList<Location> excludedLocations = excludedLocsJob.ExcludedLocations;
				if (excludedLocations.Count == 0)
				{
					streamWriter8.WriteLine("There are no excluded locations.");
					continue;
				}
				streamWriter8.WriteLine("There are {0} locations total.", excludedLocsJob.Results.Count);
				foreach (Location item2 in excludedLocations)
				{
					streamWriter8.WriteLine("Excluded location deck = {0} slot = {1}", item2.Deck, item2.Slot);
				}
			}
			else if (args[i].StartsWith("--hwSurvey"))
			{
				new HardwareSurvey(CommandLineOption.GetOptionVal(args[i], SurveyModes.Console)).Run(hardwareService);
			}
			else if (args[i].StartsWith("--3m"))
			{
				new ConsoleLogger(enableDebug: true);
				new _3MTouchscreen(CommandLineOption.GetOptionVal(args[i], Operation_3m.None)).Execute(Console.Out);
			}
			else if (args[i].StartsWith("--dumpStats"))
			{
				HardwareCorrectionStatistic optionVal10 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal10 != HardwareCorrectionStatistic.None)
				{
					using DumpHardwareStatisticsExecutor dumpHardwareStatisticsExecutor = new DumpHardwareStatisticsExecutor(hardwareService, optionVal10);
					dumpHardwareStatisticsExecutor.Run();
				}
			}
			else if (args[i].StartsWith("--resetStats"))
			{
				HardwareCorrectionStatistic optionVal11 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal11 != HardwareCorrectionStatistic.None)
				{
					using ResetHardwareStatisticsExecutor resetHardwareStatisticsExecutor = new ResetHardwareStatisticsExecutor(hardwareService, optionVal11);
					resetHardwareStatisticsExecutor.Run();
				}
			}
			else if (args[i] == "-resetTouchscreen")
			{
				new ConsoleLogger(enableDebug: true);
				if (hardwareService.ResetTouchscreenController(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job9).Success)
				{
					clientHelper.WaitForJob(job9, out var endStatus7);
					Console.WriteLine("Reset touchscreen controller ended with status {0}", endStatus7.ToString());
				}
				else
				{
					Console.WriteLine("Unable to schedule reset touchscreen job.");
				}
			}
			else if (args[i].StartsWith("--dumpKFCSessions"))
			{
				OutputModes optionVal12 = CommandLineOption.GetOptionVal(args[i], OutputModes.Console);
				using KioskFunctionCheckHelper kioskFunctionCheckHelper = new KioskFunctionCheckHelper(hardwareService);
				kioskFunctionCheckHelper.DumpSessions(optionVal12);
			}
			else if (args[i] == "-ipcClient")
			{
				IpcClientTester.RunTest(GetProgramArgs(args, i));
				Environment.Exit(0);
			}
			else if (args[i] == "-powerhouseBackup")
			{
				PowerhouseUtilities.Run(PowerhouseOperations.Backup, runtimeService);
			}
			else if (args[i] == "-powerhouseRestore")
			{
				PowerhouseUtilities.Run(PowerhouseOperations.Restore, runtimeService);
			}
			else if (args[i].StartsWith("--hashFile"))
			{
				string optionValue4 = CommandLineOption.GetOptionValue(args[i], "=");
				if (string.IsNullOrEmpty(optionValue4))
				{
					Console.WriteLine("Invalid file specified - correct the command line.");
					continue;
				}
				TripleDesEncryptionService tripleDesEncryptionService = new TripleDesEncryptionService();
				try
				{
					Console.WriteLine("{0} --> {1}", optionValue4, tripleDesEncryptionService.HashFile(optionValue4));
				}
				catch (Exception ex2)
				{
					Console.WriteLine("Hash failed with an exception:");
					Console.WriteLine(ex2.Message);
					Console.WriteLine(ex2.StackTrace);
				}
			}
			else if (args[i].StartsWith("-initSys"))
			{
				new ConsoleLogger(enableDebug: true);
				int exitCode2 = 0;
				using (InitJob initJob = new InitJob(hardwareService))
				{
					initJob.Job.EventRaised += delegate(HardwareJob hardwareJob, DateTime eventTime, string eventMessage)
					{
						LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
					};
					initJob.Run();
					initJob.Results.ForEach(delegate(ProgramResult pr)
					{
						LogHelper.Instance.Log("  Code:{0} Message:{1} ", pr.Code, pr.Message);
					});
					LogHelper.Instance.Log("Init job {0} ends with status {1}", initJob.ID, initJob.EndStatus);
					exitCode2 = ((HardwareJobStatus.Completed != initJob.EndStatus) ? (-1) : 0);
				}
				Environment.Exit(exitCode2);
			}
			else if (args[i].StartsWith("-powerCycleRouter"))
			{
				using PowerCycleRouterExecutor powerCycleRouterExecutor = new PowerCycleRouterExecutor(hardwareService);
				powerCycleRouterExecutor.Run();
				Console.WriteLine("Power cycle router job {0} ended with status {1}", powerCycleRouterExecutor.ID, powerCycleRouterExecutor.EndStatus);
			}
			else if (args[i] == "-qlmUnload")
			{
				HardwareCommandResult hardwareCommandResult2 = hardwareService.Unload(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job10);
				if (hardwareCommandResult2.Success)
				{
					job10.Pend();
					Console.WriteLine("{0} ( ID = {1} ) started", job10.ProgramName, job10.ID);
				}
				else
				{
					Console.WriteLine("Failed to schedule job");
					hardwareCommandResult2.Errors.Dump(Console.Out);
				}
			}
			else if (args[i] == "-qlmThin")
			{
				List<string> list2 = new List<string>();
				using (new DisposeableList<string>(list2))
				{
					for (int num8 = i + 1; num8 < args.Length; num8++)
					{
						list2.Add(args[num8]);
					}
					HardwareCommandResult hardwareCommandResult3 = hardwareService.Thin(list2.ToArray(), new HardwareJobSchedule
					{
						Priority = HardwareJobPriority.High
					}, out var job11);
					if (hardwareCommandResult3.Success)
					{
						job11.Pend();
						Console.WriteLine("{0} ( ID = {1} ) started", job11.ProgramName, job11.ID);
					}
					else
					{
						Console.WriteLine("Failed to schedule job");
						hardwareCommandResult3.Errors.Dump(Console.Out);
					}
				}
			}
			else if (args[i].StartsWith("--rebuildInventory"))
			{
				bool optionVal13 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				using InventoryHelper inventoryHelper8 = new InventoryHelper(hardwareService);
				inventoryHelper8.RebuildInventory(optionVal13);
			}
			else if (args[i].StartsWith("-restoreInventory"))
			{
				using InventoryHelper inventoryHelper9 = new InventoryHelper(hardwareService);
				inventoryHelper9.RestoreInventory();
			}
			else if (args[i].StartsWith("--decodeResults"))
			{
				string optionVal14 = CommandLineOption.GetOptionVal<string>(args[i], null);
				if (string.IsNullOrEmpty(optionVal14) || !File.Exists(optionVal14))
				{
					Console.WriteLine("please specify a valid file.");
					break;
				}
				string[] array3 = File.ReadAllLines(optionVal14);
				List<FraudLocInfo> list3 = new List<FraudLocInfo>();
				FraudLocInfo fraudLocInfo = null;
				char[] separator2 = new char[1] { ' ' };
				string[] array = array3;
				foreach (string text2 in array)
				{
					if (text2.Contains("GET Deck"))
					{
						string[] array4 = text2.Split(separator2);
						fraudLocInfo = new FraudLocInfo(Convert.ToInt32(array4[6]), Convert.ToInt32(array4[9]));
						list3.Add(fraudLocInfo);
					}
					else if (text2.Contains("PUT Deck"))
					{
						fraudLocInfo = null;
					}
					else if (text2.Contains("Matrix"))
					{
						string[] array5 = text2.Split(separator2);
						fraudLocInfo.Dumps.Add(new DecodeInfo(array5[6], Convert.ToInt32(array5[8])));
					}
				}
				StreamWriter output = new StreamWriter(Path.GetFileNameWithoutExtension(optionVal14) + ".csv");
				try
				{
					output.WriteLine("deck,slot,matrix,secure_found");
					list3.ForEach(delegate(FraudLocInfo each)
					{
						output.WriteLine("{0},{1},{2},{3}", each.Location.Deck, each.Location.Slot, each.Matrix, each.HasSecureRead());
					});
					output.WriteLine(" -- decode stats -- ");
					List<FraudLocInfo> list4 = list3.FindAll((FraudLocInfo each) => each.Matrix != "UNKNOWN");
					output.WriteLine("  Items with found barcode: {0}", list4.Count);
					output.WriteLine("  items with a secure read: {0}", list3.FindAll((FraudLocInfo each) => each.HasSecureRead()).Count);
				}
				finally
				{
					if (output != null)
					{
						((IDisposable)output).Dispose();
					}
				}
			}
			else
			{
				Console.WriteLine("Unrecognized option '{0}'", args[i]);
				PrintUsageAndExit();
			}
		}
	}
}
