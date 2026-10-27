using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions;

public class QRInstallHelper : UserControl
{
	private enum InstallState
	{
		Installed,
		NotInstalled
	}

	private const string StateFile = "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state";

	private const string DefaultStateFile = "C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default";

	private InstallState m_state = InstallState.NotInstalled;

	private readonly HardwareService m_hardwareService;

	private IContainer components;

	private TextBox m_comPortTextBox;

	private Label label1;

	private Button m_installIntoHALButton;

	private TextBox m_outputTextBox;

	private Button m_uninstallButton;

	private GroupBox groupBox1;

	private GroupBox groupBox2;

	private Button m_redArrowBlink;

	private Button m_redArrowOn;

	private Button m_redArrowOff;

	private Button m_blinkGreenArrow;

	private Button m_greenArrowOff;

	private Button m_greenArrowOn;

	private Button m_suspendDaemonButton;

	private GroupBox groupBox3;

	private Button m_checkStatus;

	private Button m_blinkButton;

	private Button m_turnOnButton;

	private Button m_turnOffButton;

	private Button m_clearStatus;

	private TextBox m_suspendJobTB;

	private Button m_turnOffBackLight;

	private Button m_turnOnBackLight;

	private Button m_blinkRedButton;

	private Button m_turnOnRedButton;

	private Button m_turnOffRedButton;

	private GroupBox groupBox5;

	private GroupBox groupBox4;

	private GroupBox groupBox6;

	private Button m_resumeDaemonButton;

	private Label label2;

	private ListBox listBox1;

	private Label label3;

	private Button m_scheduleJobButton;

	private Button button3;

	private Button m_restartPollingButton;

	private string InstalledComPortNumber
	{
		get
		{
			return m_comPortTextBox.Text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_comPortTextBox.Text = FindPortNumber(value);
			}
		}
	}

	private InstallState State
	{
		get
		{
			return m_state;
		}
		set
		{
			m_state = value;
			m_uninstallButton.Enabled = m_state == InstallState.Installed;
			m_installIntoHALButton.Enabled = InstallState.NotInstalled == m_state;
		}
	}

	public QRInstallHelper(HardwareService service, DeviceStatus status)
	{
		InitializeComponent();
		if (service == null)
		{
			throw new ArgumentNullException("Hardware service");
		}
		m_hardwareService = service;
		bool flag = (status & DeviceStatus.Configured) != 0;
		State = ((!flag) ? InstallState.NotInstalled : InstallState.Installed);
		InstalledComPortNumber = LocatePortViaRegistry();
	}

	private void SetToolButtonState(bool enabled)
	{
		m_suspendDaemonButton.Enabled = enabled;
		m_resumeDaemonButton.Enabled = enabled;
		m_scheduleJobButton.Enabled = enabled;
		m_turnOnBackLight.Enabled = enabled;
		m_turnOffBackLight.Enabled = enabled;
		m_greenArrowOff.Enabled = enabled;
		m_greenArrowOn.Enabled = enabled;
		m_blinkGreenArrow.Enabled = enabled;
		m_redArrowOff.Enabled = enabled;
		m_redArrowOn.Enabled = enabled;
		m_redArrowBlink.Enabled = enabled;
		m_checkStatus.Enabled = enabled;
		m_clearStatus.Enabled = enabled;
		m_turnOffButton.Enabled = enabled;
		m_turnOnButton.Enabled = enabled;
		m_blinkButton.Enabled = enabled;
		m_blinkRedButton.Enabled = enabled;
		m_turnOnRedButton.Enabled = enabled;
		m_turnOffRedButton.Enabled = enabled;
		m_restartPollingButton.Enabled = enabled;
	}

	private void WriteToInstallOutput(string fmt, params object[] stuff)
	{
		WriteToInstallOutput(string.Format(fmt, stuff));
	}

	private void WriteToInstallOutput(string msg)
	{
		msg += Environment.NewLine;
		m_outputTextBox.Text += msg;
		Application.DoEvents();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		m_outputTextBox.Text = string.Empty;
		if (!int.TryParse(InstalledComPortNumber, out var result))
		{
			WriteToInstallOutput("Port not recognized - INSTALL FAILED.");
			return;
		}
		if (!UpdateConfiguration(install: true, result))
		{
			WriteToInstallOutput("Writing installed devices failed - INSTALL FAILED.");
			return;
		}
		WriteToInstallOutput("Successfully installed Quick Return device in HAL.");
		StartDevice();
	}

	private void m_uninstallButton_Click(object sender, EventArgs e)
	{
		StopDevice();
		if (!UpdateConfiguration(install: false, null))
		{
			WriteToInstallOutput("FAILED to uninstall device in HAL.xml - uninstall FAILED.");
		}
		else
		{
			WriteToInstallOutput("Successfully uninstalled Quick Return from HAL.");
		}
	}

	private bool UpdateConfiguration(bool install, int? newComPort)
	{
		string arg = (newComPort.HasValue ? $"COM{newComPort.Value}" : "NONE");
		if (!m_hardwareService.ExecuteImmediate($"SETCFG \"QuickReturnComPort\" \"{arg}\" TYPE=CONTROLLER", out var _).Success)
		{
			return false;
		}
		if (install)
		{
			if (!File.Exists("C:\\Program Files\\Redbox\\HALService\\bin\\qr.state") && File.Exists("C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default"))
			{
				try
				{
					File.Copy("C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default", "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state", overwrite: false);
				}
				catch (Exception ex)
				{
					WriteToInstallOutput("Unable to copy {0} to {1}: error = {2}", "C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default", "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state", ex.Message);
				}
			}
		}
		else
		{
			ServiceLocator.Instance.GetService<IRuntimeService>().SafeDelete("C:\\Program Files\\Redbox\\HALService\\bin\\qr.state");
		}
		return true;
	}

	private void button3_Click(object sender, EventArgs e)
	{
		StopDevice();
	}

	private void m_greenArrowOff_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"Off\"");
	}

	private void m_greenArrowOn_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"On\"");
	}

	private void m_blinkGreenArrow_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"Blink\"");
	}

	private void m_redArrowOff_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"Off\"");
	}

	private void m_redArrowOn_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"On\"");
	}

	private void m_redArrowBlink_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"Blink\"");
	}

	private void m_checkStatus_Click(object sender, EventArgs e)
	{
		if (m_hardwareService.ExecuteImmediate("QUICKRETURN CHECKATTBUTTON", out var job).Success)
		{
			if (job.GetStack(out var stack).Success)
			{
				if (stack.Count > 1)
				{
					stack.Pop();
					listBox1.Items.Add(ConvertMessage(stack.Peek()));
				}
				else
				{
					listBox1.Items.Add("COMMUNICATION ERROR");
				}
			}
			else
			{
				listBox1.Items.Add("FAILURE");
			}
		}
		else
		{
			listBox1.Items.Add("FAILURE");
		}
		Application.DoEvents();
	}

	private void m_clearStatus_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN CLEARATTBUTTON");
	}

	private void m_turnOffButton_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"Off\"");
	}

	private void m_turnOnButton_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"On\"");
	}

	private void m_blinkButton_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"Blink\"");
	}

	private void m_turnOnBackLight_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"Background\" MODE=\"On\"");
	}

	private void m_turnOffBackLight_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"Background\" MODE=\"Off\"");
	}

	private void button3_Click_1(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"Off\"");
	}

	private void button4_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"On\"");
	}

	private void button5_Click(object sender, EventArgs e)
	{
		ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"Blink\"");
	}

	private void ExecuteInstruction(string instruction)
	{
		HardwareCommandResult hardwareCommandResult = m_hardwareService.ExecuteImmediate(instruction, out var job);
		if (hardwareCommandResult.Success)
		{
			WriteToCommandOutput(hardwareCommandResult, job);
		}
		else
		{
			listBox1.Items.Add("FAILURE");
		}
	}

	private string ConvertMessage(string msg)
	{
		if ("true".Equals(msg, StringComparison.CurrentCultureIgnoreCase))
		{
			return "SUCCESS";
		}
		if ("false".Equals(msg, StringComparison.CurrentCultureIgnoreCase))
		{
			return "FAILURE";
		}
		return msg;
	}

	private void WriteToCommandOutput(HardwareCommandResult result, HardwareJob job)
	{
		if (job.GetStack(out var stack).Success)
		{
			if (stack.Count > 0)
			{
				listBox1.Items.Add(ConvertMessage(stack.Peek()));
			}
			else
			{
				listBox1.Items.Add("FAILURE");
			}
		}
		else
		{
			listBox1.Items.Add("FAILURE");
		}
		Application.DoEvents();
	}

	private void button3_Click_2(object sender, EventArgs e)
	{
		Stack<string> stack;
		if (!m_hardwareService.ExecuteImmediate("QUICKRETURN POLLSTATUS", out var job).Success)
		{
			m_suspendJobTB.Text = "COMM Error";
		}
		else if (job.GetStack(out stack).Success)
		{
			string text = stack.Pop();
			m_suspendJobTB.Text = text;
		}
		else
		{
			m_suspendJobTB.Text = "COMM Error";
		}
	}

	private void button4_Click_1(object sender, EventArgs e)
	{
		StartTimeForm startTimeForm = new StartTimeForm();
		if (startTimeForm.ShowDialog() != DialogResult.Cancel)
		{
			HardwareJobSchedule schedule = new HardwareJobSchedule
			{
				StartTime = startTimeForm.StartTime.Value,
				Priority = HardwareJobPriority.High
			};
			HardwareCommandResult hardwareCommandResult = m_hardwareService.ScheduleJob("quick-return-restart", string.Empty, enableDebugging: false, schedule, out var job);
			if (hardwareCommandResult.Success)
			{
				job.Pend();
			}
			m_suspendJobTB.Text = (hardwareCommandResult.Success ? "SUCCESS" : "COMMAND ERROR");
			Application.DoEvents();
		}
	}

	private void button3_Click_3(object sender, EventArgs e)
	{
		SetToolButtonState(enabled: true);
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
		StartDevice();
	}

	private string LocatePortViaRegistry()
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Enum\\FTDIBUS");
			if (registryKey == null)
			{
				return null;
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				if (!text.StartsWith("VID_0403+PID_6001"))
				{
					continue;
				}
				RegistryKey registryKey2 = registryKey.OpenSubKey($"{text}\\0000\\Control");
				if (registryKey2 != null)
				{
					registryKey2.Close();
					RegistryKey registryKey3 = registryKey.OpenSubKey($"{text}\\0000\\Device Parameters");
					if (registryKey3 != null)
					{
						string result = (string)registryKey3.GetValue("PortName");
						registryKey3.Close();
						return result;
					}
				}
			}
			return null;
		}
		catch (Exception)
		{
			return null;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private string FindPortNumber(string port)
	{
		if (!port.StartsWith("COM", StringComparison.CurrentCultureIgnoreCase))
		{
			return port;
		}
		return port.Substring(3, port.Length - 3);
	}

	private void StartDevice()
	{
		if (!m_hardwareService.ExecuteImmediate("QUICKRETURN RESTART", out var job).Success)
		{
			m_suspendJobTB.Text = "COMMAND FAILED.";
			Application.DoEvents();
		}
		else
		{
			job.GetStack(out var stack);
			m_suspendJobTB.Text = ((stack.Count > 0) ? stack.Pop() : "COMMAND ERROR");
		}
	}

	private void StopDevice()
	{
		HardwareCommandResult hardwareCommandResult = m_hardwareService.ExecuteImmediate("QUICKRETURN HALTPOLL", out var _);
		m_suspendJobTB.Text = (hardwareCommandResult.Success ? "STOPPED" : "RUNNING");
		m_comPortTextBox.Text = string.Empty;
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
		this.m_comPortTextBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_installIntoHALButton = new System.Windows.Forms.Button();
		this.m_outputTextBox = new System.Windows.Forms.TextBox();
		this.m_uninstallButton = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.button3 = new System.Windows.Forms.Button();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label2 = new System.Windows.Forms.Label();
		this.listBox1 = new System.Windows.Forms.ListBox();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.m_redArrowOff = new System.Windows.Forms.Button();
		this.m_greenArrowOff = new System.Windows.Forms.Button();
		this.m_redArrowOn = new System.Windows.Forms.Button();
		this.m_greenArrowOn = new System.Windows.Forms.Button();
		this.m_redArrowBlink = new System.Windows.Forms.Button();
		this.m_blinkGreenArrow = new System.Windows.Forms.Button();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.m_turnOnBackLight = new System.Windows.Forms.Button();
		this.m_turnOffBackLight = new System.Windows.Forms.Button();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.m_blinkRedButton = new System.Windows.Forms.Button();
		this.m_turnOnRedButton = new System.Windows.Forms.Button();
		this.m_turnOffRedButton = new System.Windows.Forms.Button();
		this.m_blinkButton = new System.Windows.Forms.Button();
		this.m_turnOnButton = new System.Windows.Forms.Button();
		this.m_turnOffButton = new System.Windows.Forms.Button();
		this.m_clearStatus = new System.Windows.Forms.Button();
		this.m_checkStatus = new System.Windows.Forms.Button();
		this.m_suspendDaemonButton = new System.Windows.Forms.Button();
		this.m_suspendJobTB = new System.Windows.Forms.TextBox();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.m_restartPollingButton = new System.Windows.Forms.Button();
		this.label3 = new System.Windows.Forms.Label();
		this.m_scheduleJobButton = new System.Windows.Forms.Button();
		this.m_resumeDaemonButton = new System.Windows.Forms.Button();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBox6.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox4.SuspendLayout();
		base.SuspendLayout();
		this.m_comPortTextBox.Location = new System.Drawing.Point(165, 20);
		this.m_comPortTextBox.Name = "m_comPortTextBox";
		this.m_comPortTextBox.Size = new System.Drawing.Size(52, 20);
		this.m_comPortTextBox.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 23);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(139, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Install on COM port (e.g., 6):";
		this.m_installIntoHALButton.BackColor = System.Drawing.Color.LightGray;
		this.m_installIntoHALButton.Location = new System.Drawing.Point(9, 45);
		this.m_installIntoHALButton.Name = "m_installIntoHALButton";
		this.m_installIntoHALButton.Size = new System.Drawing.Size(75, 61);
		this.m_installIntoHALButton.TabIndex = 2;
		this.m_installIntoHALButton.Text = "Install into HAL";
		this.m_installIntoHALButton.UseVisualStyleBackColor = false;
		this.m_installIntoHALButton.Click += new System.EventHandler(button1_Click);
		this.m_outputTextBox.Location = new System.Drawing.Point(9, 121);
		this.m_outputTextBox.Multiline = true;
		this.m_outputTextBox.Name = "m_outputTextBox";
		this.m_outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.m_outputTextBox.Size = new System.Drawing.Size(311, 158);
		this.m_outputTextBox.TabIndex = 3;
		this.m_uninstallButton.BackColor = System.Drawing.Color.LightGray;
		this.m_uninstallButton.Location = new System.Drawing.Point(99, 45);
		this.m_uninstallButton.Name = "m_uninstallButton";
		this.m_uninstallButton.Size = new System.Drawing.Size(75, 61);
		this.m_uninstallButton.TabIndex = 4;
		this.m_uninstallButton.Text = "Uninstall from HAL ";
		this.m_uninstallButton.UseVisualStyleBackColor = false;
		this.m_uninstallButton.Click += new System.EventHandler(m_uninstallButton_Click);
		this.groupBox1.Controls.Add(this.button3);
		this.groupBox1.Controls.Add(this.m_outputTextBox);
		this.groupBox1.Controls.Add(this.m_uninstallButton);
		this.groupBox1.Controls.Add(this.m_comPortTextBox);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.m_installIntoHALButton);
		this.groupBox1.Location = new System.Drawing.Point(12, 12);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(350, 285);
		this.groupBox1.TabIndex = 6;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Installation";
		this.button3.BackColor = System.Drawing.Color.LightGray;
		this.button3.Location = new System.Drawing.Point(180, 46);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 59);
		this.button3.TabIndex = 6;
		this.button3.Text = "Connect to HAL";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(button3_Click_3);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.listBox1);
		this.groupBox2.Controls.Add(this.groupBox6);
		this.groupBox2.Controls.Add(this.groupBox5);
		this.groupBox2.Controls.Add(this.groupBox3);
		this.groupBox2.Location = new System.Drawing.Point(399, 18);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(517, 469);
		this.groupBox2.TabIndex = 7;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Device Control";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(324, 21);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(89, 13);
		this.label2.TabIndex = 20;
		this.label2.Text = "Command Output";
		this.listBox1.FormattingEnabled = true;
		this.listBox1.HorizontalScrollbar = true;
		this.listBox1.Location = new System.Drawing.Point(327, 38);
		this.listBox1.Name = "listBox1";
		this.listBox1.Size = new System.Drawing.Size(174, 420);
		this.listBox1.TabIndex = 19;
		this.groupBox6.Controls.Add(this.m_redArrowOff);
		this.groupBox6.Controls.Add(this.m_greenArrowOff);
		this.groupBox6.Controls.Add(this.m_redArrowOn);
		this.groupBox6.Controls.Add(this.m_greenArrowOn);
		this.groupBox6.Controls.Add(this.m_redArrowBlink);
		this.groupBox6.Controls.Add(this.m_blinkGreenArrow);
		this.groupBox6.Location = new System.Drawing.Point(16, 115);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(289, 144);
		this.groupBox6.TabIndex = 18;
		this.groupBox6.TabStop = false;
		this.groupBox6.Text = "Arrow Commands";
		this.m_redArrowOff.BackColor = System.Drawing.Color.LightGray;
		this.m_redArrowOff.Enabled = false;
		this.m_redArrowOff.Location = new System.Drawing.Point(6, 78);
		this.m_redArrowOff.Name = "m_redArrowOff";
		this.m_redArrowOff.Size = new System.Drawing.Size(75, 57);
		this.m_redArrowOff.TabIndex = 4;
		this.m_redArrowOff.Text = "Turn Off Red Arrow";
		this.m_redArrowOff.UseVisualStyleBackColor = false;
		this.m_redArrowOff.Click += new System.EventHandler(m_redArrowOff_Click);
		this.m_greenArrowOff.BackColor = System.Drawing.Color.LightGray;
		this.m_greenArrowOff.Enabled = false;
		this.m_greenArrowOff.Location = new System.Drawing.Point(6, 15);
		this.m_greenArrowOff.Name = "m_greenArrowOff";
		this.m_greenArrowOff.Size = new System.Drawing.Size(75, 57);
		this.m_greenArrowOff.TabIndex = 2;
		this.m_greenArrowOff.Text = "Turn Off Green Arrow";
		this.m_greenArrowOff.UseVisualStyleBackColor = false;
		this.m_greenArrowOff.Click += new System.EventHandler(m_greenArrowOff_Click);
		this.m_redArrowOn.BackColor = System.Drawing.Color.LightGray;
		this.m_redArrowOn.Enabled = false;
		this.m_redArrowOn.Location = new System.Drawing.Point(91, 78);
		this.m_redArrowOn.Name = "m_redArrowOn";
		this.m_redArrowOn.Size = new System.Drawing.Size(75, 57);
		this.m_redArrowOn.TabIndex = 5;
		this.m_redArrowOn.Text = "Turn On Red Arrow";
		this.m_redArrowOn.UseVisualStyleBackColor = false;
		this.m_redArrowOn.Click += new System.EventHandler(m_redArrowOn_Click);
		this.m_greenArrowOn.BackColor = System.Drawing.Color.LightGray;
		this.m_greenArrowOn.Enabled = false;
		this.m_greenArrowOn.Location = new System.Drawing.Point(91, 15);
		this.m_greenArrowOn.Name = "m_greenArrowOn";
		this.m_greenArrowOn.Size = new System.Drawing.Size(75, 57);
		this.m_greenArrowOn.TabIndex = 1;
		this.m_greenArrowOn.Text = "Turn On Green Arrow";
		this.m_greenArrowOn.UseVisualStyleBackColor = false;
		this.m_greenArrowOn.Click += new System.EventHandler(m_greenArrowOn_Click);
		this.m_redArrowBlink.BackColor = System.Drawing.Color.LightGray;
		this.m_redArrowBlink.Enabled = false;
		this.m_redArrowBlink.Location = new System.Drawing.Point(186, 78);
		this.m_redArrowBlink.Name = "m_redArrowBlink";
		this.m_redArrowBlink.Size = new System.Drawing.Size(75, 57);
		this.m_redArrowBlink.TabIndex = 6;
		this.m_redArrowBlink.Text = "Blink Red Arrow";
		this.m_redArrowBlink.UseVisualStyleBackColor = false;
		this.m_redArrowBlink.Click += new System.EventHandler(m_redArrowBlink_Click);
		this.m_blinkGreenArrow.BackColor = System.Drawing.Color.LightGray;
		this.m_blinkGreenArrow.Enabled = false;
		this.m_blinkGreenArrow.Location = new System.Drawing.Point(186, 15);
		this.m_blinkGreenArrow.Name = "m_blinkGreenArrow";
		this.m_blinkGreenArrow.Size = new System.Drawing.Size(75, 57);
		this.m_blinkGreenArrow.TabIndex = 3;
		this.m_blinkGreenArrow.Text = "Blink Green Arrow";
		this.m_blinkGreenArrow.UseVisualStyleBackColor = false;
		this.m_blinkGreenArrow.Click += new System.EventHandler(m_blinkGreenArrow_Click);
		this.groupBox5.Controls.Add(this.m_turnOnBackLight);
		this.groupBox5.Controls.Add(this.m_turnOffBackLight);
		this.groupBox5.Location = new System.Drawing.Point(16, 19);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(289, 81);
		this.groupBox5.TabIndex = 17;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Backlight Commands";
		this.m_turnOnBackLight.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOnBackLight.Enabled = false;
		this.m_turnOnBackLight.Location = new System.Drawing.Point(6, 19);
		this.m_turnOnBackLight.Name = "m_turnOnBackLight";
		this.m_turnOnBackLight.Size = new System.Drawing.Size(106, 52);
		this.m_turnOnBackLight.TabIndex = 14;
		this.m_turnOnBackLight.Text = "Turn On BackLight";
		this.m_turnOnBackLight.UseVisualStyleBackColor = false;
		this.m_turnOnBackLight.Click += new System.EventHandler(m_turnOnBackLight_Click);
		this.m_turnOffBackLight.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOffBackLight.Enabled = false;
		this.m_turnOffBackLight.Location = new System.Drawing.Point(149, 19);
		this.m_turnOffBackLight.Name = "m_turnOffBackLight";
		this.m_turnOffBackLight.Size = new System.Drawing.Size(94, 52);
		this.m_turnOffBackLight.TabIndex = 15;
		this.m_turnOffBackLight.Text = "Turn Off BackLight";
		this.m_turnOffBackLight.UseVisualStyleBackColor = false;
		this.m_turnOffBackLight.Click += new System.EventHandler(m_turnOffBackLight_Click);
		this.groupBox3.Controls.Add(this.m_blinkRedButton);
		this.groupBox3.Controls.Add(this.m_turnOnRedButton);
		this.groupBox3.Controls.Add(this.m_turnOffRedButton);
		this.groupBox3.Controls.Add(this.m_blinkButton);
		this.groupBox3.Controls.Add(this.m_turnOnButton);
		this.groupBox3.Controls.Add(this.m_turnOffButton);
		this.groupBox3.Controls.Add(this.m_clearStatus);
		this.groupBox3.Controls.Add(this.m_checkStatus);
		this.groupBox3.Location = new System.Drawing.Point(16, 265);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(289, 190);
		this.groupBox3.TabIndex = 11;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Button Commands";
		this.m_blinkRedButton.BackColor = System.Drawing.Color.LightGray;
		this.m_blinkRedButton.Enabled = false;
		this.m_blinkRedButton.Location = new System.Drawing.Point(172, 129);
		this.m_blinkRedButton.Name = "m_blinkRedButton";
		this.m_blinkRedButton.Size = new System.Drawing.Size(75, 43);
		this.m_blinkRedButton.TabIndex = 12;
		this.m_blinkRedButton.Text = "Blink Red";
		this.m_blinkRedButton.UseVisualStyleBackColor = false;
		this.m_blinkRedButton.Click += new System.EventHandler(button5_Click);
		this.m_turnOnRedButton.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOnRedButton.Enabled = false;
		this.m_turnOnRedButton.Location = new System.Drawing.Point(91, 129);
		this.m_turnOnRedButton.Name = "m_turnOnRedButton";
		this.m_turnOnRedButton.Size = new System.Drawing.Size(75, 43);
		this.m_turnOnRedButton.TabIndex = 11;
		this.m_turnOnRedButton.Text = "Turn On Red";
		this.m_turnOnRedButton.UseVisualStyleBackColor = false;
		this.m_turnOnRedButton.Click += new System.EventHandler(button4_Click);
		this.m_turnOffRedButton.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOffRedButton.Enabled = false;
		this.m_turnOffRedButton.Location = new System.Drawing.Point(10, 129);
		this.m_turnOffRedButton.Name = "m_turnOffRedButton";
		this.m_turnOffRedButton.Size = new System.Drawing.Size(75, 43);
		this.m_turnOffRedButton.TabIndex = 10;
		this.m_turnOffRedButton.Text = "Turn Off Red";
		this.m_turnOffRedButton.UseVisualStyleBackColor = false;
		this.m_turnOffRedButton.Click += new System.EventHandler(button3_Click_1);
		this.m_blinkButton.BackColor = System.Drawing.Color.LightGray;
		this.m_blinkButton.Enabled = false;
		this.m_blinkButton.Location = new System.Drawing.Point(172, 80);
		this.m_blinkButton.Name = "m_blinkButton";
		this.m_blinkButton.Size = new System.Drawing.Size(75, 43);
		this.m_blinkButton.TabIndex = 5;
		this.m_blinkButton.Text = "Blink Green";
		this.m_blinkButton.UseVisualStyleBackColor = false;
		this.m_blinkButton.Click += new System.EventHandler(m_blinkButton_Click);
		this.m_turnOnButton.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOnButton.Enabled = false;
		this.m_turnOnButton.Location = new System.Drawing.Point(91, 80);
		this.m_turnOnButton.Name = "m_turnOnButton";
		this.m_turnOnButton.Size = new System.Drawing.Size(75, 43);
		this.m_turnOnButton.TabIndex = 4;
		this.m_turnOnButton.Text = "Turn On Green";
		this.m_turnOnButton.UseVisualStyleBackColor = false;
		this.m_turnOnButton.Click += new System.EventHandler(m_turnOnButton_Click);
		this.m_turnOffButton.BackColor = System.Drawing.Color.LightGray;
		this.m_turnOffButton.Enabled = false;
		this.m_turnOffButton.Location = new System.Drawing.Point(10, 80);
		this.m_turnOffButton.Name = "m_turnOffButton";
		this.m_turnOffButton.Size = new System.Drawing.Size(75, 43);
		this.m_turnOffButton.TabIndex = 3;
		this.m_turnOffButton.Text = "Turn Off Green";
		this.m_turnOffButton.UseVisualStyleBackColor = false;
		this.m_turnOffButton.Click += new System.EventHandler(m_turnOffButton_Click);
		this.m_clearStatus.BackColor = System.Drawing.Color.LightGray;
		this.m_clearStatus.Enabled = false;
		this.m_clearStatus.Location = new System.Drawing.Point(131, 19);
		this.m_clearStatus.Name = "m_clearStatus";
		this.m_clearStatus.Size = new System.Drawing.Size(95, 44);
		this.m_clearStatus.TabIndex = 2;
		this.m_clearStatus.Text = "Clear Status";
		this.m_clearStatus.UseVisualStyleBackColor = false;
		this.m_clearStatus.Click += new System.EventHandler(m_clearStatus_Click);
		this.m_checkStatus.BackColor = System.Drawing.Color.LightGray;
		this.m_checkStatus.Enabled = false;
		this.m_checkStatus.Location = new System.Drawing.Point(10, 19);
		this.m_checkStatus.Name = "m_checkStatus";
		this.m_checkStatus.Size = new System.Drawing.Size(100, 44);
		this.m_checkStatus.TabIndex = 0;
		this.m_checkStatus.Text = "Check Status";
		this.m_checkStatus.UseVisualStyleBackColor = false;
		this.m_checkStatus.Click += new System.EventHandler(m_checkStatus_Click);
		this.m_suspendDaemonButton.BackColor = System.Drawing.Color.LightGray;
		this.m_suspendDaemonButton.Enabled = false;
		this.m_suspendDaemonButton.Location = new System.Drawing.Point(6, 19);
		this.m_suspendDaemonButton.Name = "m_suspendDaemonButton";
		this.m_suspendDaemonButton.Size = new System.Drawing.Size(75, 61);
		this.m_suspendDaemonButton.TabIndex = 0;
		this.m_suspendDaemonButton.Text = "Stop QR Polling";
		this.m_suspendDaemonButton.UseVisualStyleBackColor = false;
		this.m_suspendDaemonButton.Click += new System.EventHandler(button3_Click);
		this.m_suspendJobTB.Location = new System.Drawing.Point(111, 100);
		this.m_suspendJobTB.Name = "m_suspendJobTB";
		this.m_suspendJobTB.ReadOnly = true;
		this.m_suspendJobTB.Size = new System.Drawing.Size(100, 20);
		this.m_suspendJobTB.TabIndex = 12;
		this.groupBox4.Controls.Add(this.m_restartPollingButton);
		this.groupBox4.Controls.Add(this.label3);
		this.groupBox4.Controls.Add(this.m_scheduleJobButton);
		this.groupBox4.Controls.Add(this.m_resumeDaemonButton);
		this.groupBox4.Controls.Add(this.m_suspendDaemonButton);
		this.groupBox4.Controls.Add(this.m_suspendJobTB);
		this.groupBox4.Location = new System.Drawing.Point(12, 312);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(350, 143);
		this.groupBox4.TabIndex = 8;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Poll Job Control";
		this.m_restartPollingButton.BackColor = System.Drawing.Color.LightGray;
		this.m_restartPollingButton.Enabled = false;
		this.m_restartPollingButton.Location = new System.Drawing.Point(87, 19);
		this.m_restartPollingButton.Name = "m_restartPollingButton";
		this.m_restartPollingButton.Size = new System.Drawing.Size(75, 61);
		this.m_restartPollingButton.TabIndex = 16;
		this.m_restartPollingButton.Text = "Restart QR Polling";
		this.m_restartPollingButton.UseVisualStyleBackColor = false;
		this.m_restartPollingButton.Click += new System.EventHandler(button1_Click_1);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(108, 81);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(37, 13);
		this.label3.TabIndex = 15;
		this.label3.Text = "Status";
		this.m_scheduleJobButton.BackColor = System.Drawing.Color.LightGray;
		this.m_scheduleJobButton.Enabled = false;
		this.m_scheduleJobButton.Location = new System.Drawing.Point(249, 19);
		this.m_scheduleJobButton.Name = "m_scheduleJobButton";
		this.m_scheduleJobButton.Size = new System.Drawing.Size(75, 61);
		this.m_scheduleJobButton.TabIndex = 14;
		this.m_scheduleJobButton.Text = "Schedule QR Job in Future";
		this.m_scheduleJobButton.UseVisualStyleBackColor = false;
		this.m_scheduleJobButton.Click += new System.EventHandler(button4_Click_1);
		this.m_resumeDaemonButton.BackColor = System.Drawing.Color.LightGray;
		this.m_resumeDaemonButton.Enabled = false;
		this.m_resumeDaemonButton.Location = new System.Drawing.Point(168, 19);
		this.m_resumeDaemonButton.Name = "m_resumeDaemonButton";
		this.m_resumeDaemonButton.Size = new System.Drawing.Size(75, 61);
		this.m_resumeDaemonButton.TabIndex = 13;
		this.m_resumeDaemonButton.Text = "QR Poll Status";
		this.m_resumeDaemonButton.UseVisualStyleBackColor = false;
		this.m_resumeDaemonButton.Click += new System.EventHandler(button3_Click_2);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBox4);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Name = "QRInstallHelper";
		base.Size = new System.Drawing.Size(937, 504);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.groupBox6.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox4.PerformLayout();
		base.ResumeLayout(false);
	}
}
