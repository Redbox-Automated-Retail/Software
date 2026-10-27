using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public class TouchScreenAccess : UserControl
{
	private IContainer components;

	private TabControl m_touchScreenAccessTabControl;

	private TabPage m_movementTabPage;

	private TabPage m_inventoryTabPage;

	private TabPage m_rollerTabPage;

	private TabPage m_gripperTabPage;

	private TabPage m_cameraTabPage;

	private TabPage m_vendDoorTabPage;

	private TabPage m_trackTabPage;

	private TabPage m_sensorTabPage;

	private TabPage m_qlmTabPage;

	private TabPage m_audioTabPage;

	private TabPage m_systemTabPage;

	private Button m_movementMoveVendButton;

	private Button m_movementMoveButton;

	private Button m_movementMoveNoCheckButton;

	private Button m_movementCurrentLocationButton;

	private Button m_movementTransferButton;

	private Button m_movementHomeYButton;

	private Button m_movementHomeXButton;

	private FlowLayoutPanel m_movementFlowLayoutPanel;

	private FlowLayoutPanel m_inventoryFlowLayoutPanel;

	private Button m_invGetButton;

	private Button m_invPutButton;

	private Button m_invReadButton;

	private Button m_invInspectButton;

	private Button m_invFindEmptyButton;

	private Button m_invSyncSlotButton;

	private Button m_invPutinSlotButton;

	private Button m_invGetVendButton;

	private Button m_invGetCenterButton;

	private FlowLayoutPanel m_rollerFlowLayoutPanel;

	private Button m_rollerRollerInButton;

	private Button m_rollerRollerOut;

	private Button m_rollerToPositionButton;

	private Button m_rollerStopButton;

	private FlowLayoutPanel m_gripperFlowLayoutPanel;

	private Button m_gripperExtendButton;

	private Button m_gripperRetractButton;

	private Button m_gripperOpenButton;

	private Button m_gripperRentButton;

	private Button m_gripperCloseButton;

	private Button m_gripperPeekButton;

	private Button m_gripperPushButton;

	private Button m_gripperStatusButton;

	private FlowLayoutPanel m_cameraFlowLayoutPanel;

	private Button m_cameraRingOnButton;

	private Button m_cameraRingOffButton;

	private Button m_cameraStartButton;

	private Button m_cameraSnap;

	private Button m_cameraStopButton;

	private Button m_cameraStatusButton;

	private FlowLayoutPanel m_trackFlowLayoutPanel;

	private Button m_trackOpenButton;

	private Button m_trackCloseButton;

	private Button m_trackStatusButton;

	private FlowLayoutPanel m_sensorFlowLayoutPanel;

	private Button m_sensorPickerOnButton;

	private Button m_sensorPickerOffButton;

	private Button m_sensorPickerReadButton;

	private Button m_sensorPickerWaitClearButton;

	private Button m_sensorPickerWaitBlockButton;

	private Button m_sensorReadPickerInputsButton;

	private Button m_sensorReadAuxInputsButton;

	private FlowLayoutPanel m_qlmFlowLayoutPanel;

	private Button m_qlmEngageButton;

	private Button m_qlmDisengageButton;

	private Button m_qlmStatusButton;

	private Button m_qlmLiftButton;

	private Button m_qlmDropButton;

	private FlowLayoutPanel m_systemFlowLayoutPanel;

	private Button m_systemResetButton;

	private Button m_systemVersionButton;

	private Button m_gripperClearButton;

	private Button m_gripperPushDvdInButton;

	private FlowLayoutPanel m_vendDoorFlowLayoutPanel;

	private Button m_vendDoorRentButton;

	private Button m_vendDoorStatusButton;

	private Button m_vendDoorCloseButton;

	private Button m_qlmHaltButton;

	private Button m_gripperPutInEmptyButton;

	private Button button1;

	public static TouchScreenAccess Instance => Singleton<TouchScreenAccess>.Instance;

	public ButtonAspectsManager Manager { get; private set; }

	private TouchScreenAccess()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		Color running = Color.Red;
		if (!string.IsNullOrEmpty(Settings.Default.ButtonRunningColor))
		{
			running = Color.FromName(Settings.Default.ButtonRunningColor);
		}
		Manager = new ButtonAspectsManager(running);
		ToggleStyle();
		m_touchScreenAccessTabControl.Enabled = false;
		BackColor = Color.White;
		base.Enabled = false;
		base.EnabledChanged += OnEnabledChanged;
		EnvironmentHelper.ExecutingImmediateStausChanged += OnImmediateExecutingStatusChanged;
	}

	private void TouchAccessTabControl_DrawItem(object sender, DrawItemEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TabPage tabPage = m_touchScreenAccessTabControl.TabPages[e.Index];
		Rectangle bounds = e.Bounds;
		bounds = new Rectangle(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height - 3);
		Brush brush;
		Font font;
		if (e.State == DrawItemState.Selected)
		{
			brush = new SolidBrush(Color.Black);
			font = new Font(e.Font, FontStyle.Bold);
			if (ProfileManager.Instance.IsConnected)
			{
				graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
			}
			else
			{
				graphics.FillRectangle(Brushes.LightGray, e.Bounds);
			}
		}
		else
		{
			brush = new SolidBrush(e.ForeColor);
			graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);
			e.DrawBackground();
			font = e.Font;
		}
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		graphics.DrawString(tabPage.Text, font, brush, bounds, new StringFormat(stringFormat));
	}

	private void OnEnabledChanged(object sender, EventArgs e)
	{
		ToggleStyle();
		m_touchScreenAccessTabControl.Enabled = base.Enabled;
	}

	private void OnImmediateExecutingStatusChanged(object sender, BoolEventArgs e)
	{
		base.Enabled = !e.State;
	}

	private void OnSimpleCommandButton(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string text = (sender as Button).Tag as string;
			if (!string.IsNullOrEmpty(text))
			{
				ExecuteInstruction(text);
			}
		}
	}

	internal static void WriteInstructionToOutput(string instruction)
	{
		OutputWindow.Instance.Append(instruction);
	}

	private void ToggleStyle()
	{
		bool enabled = base.Enabled;
		foreach (TabPage tabPage in m_touchScreenAccessTabControl.TabPages)
		{
			foreach (Button control in tabPage.Controls[0].Controls)
			{
				if (control != null)
				{
					if (enabled)
					{
						control.FlatStyle = FlatStyle.Standard;
					}
					else
					{
						control.FlatStyle = FlatStyle.Flat;
					}
				}
			}
		}
	}

	private void OnMove(object sender, EventArgs args)
	{
		using (Manager.MakeAspect(sender))
		{
			Location location = CommonFunctions.CurrentLocation();
			if (location == null)
			{
				location = new Location
				{
					Deck = 1,
					Slot = 1
				};
			}
			MoveToForm moveToForm = new MoveToForm
			{
				Deck = location.Deck,
				Slot = location.Slot
			};
			if (moveToForm.ShowDialog() == DialogResult.OK && moveToForm.Deck.HasValue && moveToForm.Slot.HasValue)
			{
				string text = $"MOVE DECK={moveToForm.Deck} SLOT={moveToForm.Slot}";
				if (!string.IsNullOrEmpty(moveToForm.Mode))
				{
					text = text + " MODE=" + moveToForm.Mode;
				}
				ExecuteInstruction(text);
			}
		}
	}

	private void OnTransfer(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string text = "XFER";
			MoveToForm moveToForm = new MoveToForm();
			moveToForm.DisableModes();
			moveToForm.Text = "Enter Source Deck/Slot";
			if (moveToForm.ShowDialog() == DialogResult.OK && moveToForm.Deck.HasValue && moveToForm.Slot.HasValue)
			{
				text += $" SRC-DECK={moveToForm.Deck} SRC-SLOT={moveToForm.Slot}";
				moveToForm.ClearFields();
				moveToForm.Text = "Enter Destination Deck/Slot";
				if (moveToForm.ShowDialog() == DialogResult.OK && moveToForm.Deck.HasValue && moveToForm.Slot.HasValue)
				{
					text += $" DEST-DECK={moveToForm.Deck} DEST-SLOT={moveToForm.Slot}";
					ExecuteInstruction(text);
				}
			}
		}
	}

	private void OnInspect(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			MoveToForm moveToForm = new MoveToForm
			{
				Text = "Location to Inspect"
			};
			moveToForm.DisableModes();
			if (moveToForm.ShowDialog() == DialogResult.OK && moveToForm.Deck.HasValue && moveToForm.Slot.HasValue)
			{
				string instruction = $"INSPECT DECK={moveToForm.Deck} SLOT={moveToForm.Slot}";
				ExecuteInstruction(instruction);
			}
		}
	}

	private void OnReadPickerSensors(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int num = 1;
			int num2 = 6;
			string instruction = $"SENSOR READ PICKER-SENSOR={num}..{num2}";
			HardwareJob hardwareJob = ExecuteInstruction(instruction);
			if (hardwareJob == null || !hardwareJob.GetStack(out var stack).Success)
			{
				return;
			}
			SensorView.Instance.ResetSensors();
			if (stack.Count != 0)
			{
				stack.Pop();
				for (int num3 = num2; num3 >= num; num3--)
				{
					string text = stack.Pop();
					SensorView.Instance.Sensors[num3 - 1] = text.Contains("BLOCKED");
				}
				string instruction2 = "SENSOR PICKER-OFF";
				ExecuteInstruction(instruction2);
			}
		}
	}

	private void OnMoveNoCheck(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			new IncrementalMoveForm(ProfileManager.Instance.Service).ShowDialog();
		}
	}

	private void OnWaitPickerSensorsClear(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			SensorRangeForm sensorRangeForm = new SensorRangeForm
			{
				MinimumValue = 1,
				MaximumValue = 6
			};
			if (sensorRangeForm.ShowDialog() == DialogResult.OK && sensorRangeForm.Start.HasValue)
			{
				string instruction = string.Format("SENSOR WAIT-CLEAR PICKER-SENSOR={0}{1} TIMEOUT=30000", sensorRangeForm.Start, sensorRangeForm.End.HasValue ? (".." + sensorRangeForm.End) : string.Empty);
				ExecuteInstruction(instruction);
			}
		}
	}

	private void OnWaitPickerSensorsBlock(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			SensorRangeForm sensorRangeForm = new SensorRangeForm
			{
				MinimumValue = 1,
				MaximumValue = 6
			};
			if (sensorRangeForm.ShowDialog() == DialogResult.OK && sensorRangeForm.Start.HasValue)
			{
				string instruction = string.Format("SENSOR WAIT-BLOCK PICKER-SENSOR={0}{1} TIMEOUT=30000", sensorRangeForm.Start, sensorRangeForm.End.HasValue ? (".." + sensorRangeForm.End) : string.Empty);
				ExecuteInstruction(instruction);
			}
		}
	}

	private void OnTurnOffSensorsButton(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			OnSimpleCommandButton(sender, e);
			SensorView.Instance.ResetSensors();
		}
	}

	private void OnRollerToPosition(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			SensorRangeForm sensorRangeForm = new SensorRangeForm
			{
				MinimumValue = 1,
				MaximumValue = 6
			};
			sensorRangeForm.DisableEnd();
			if (sensorRangeForm.ShowDialog() == DialogResult.OK && sensorRangeForm.Start.HasValue)
			{
				string instruction = $"ROLLER POS={sensorRangeForm.Start} TIMEOUT=3000";
				ExecuteInstruction(instruction);
			}
		}
	}

	private void OnSyncSlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			bool flag = CommonFunctions.SyncSlot();
			string instruction = string.Format("PUSH \"{0}\"", flag ? "SYNC OK" : "SYNC ERRORED");
			ExecuteInstruction(instruction);
		}
	}

	private void OnPushDVDIn(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			PushDVDInPickerIntoSlotViaJob();
		}
	}

	private void PushDVDInPickerIntoSlotViaJob()
	{
		try
		{
			Application.DoEvents();
			HardwareJobSchedule schedule = new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			};
			if (!ProfileManager.Instance.Service.ScheduleJob("push-in-dvd", string.Empty, enableDebugging: false, schedule, out var job).Success)
			{
				LogHelper.Instance.Log("Unable to schedule job", LogEntryType.Error);
				ExecuteInstruction("PUSH \"JOBERROR\"");
				return;
			}
			job.Resume();
			job.WaitForCompletion();
			string iD = job.ID;
			if (!ProfileManager.Instance.Service.GetJob(iD, out job).Success)
			{
				LogHelper.Instance.Log("Unable to get job status", LogEntryType.Error);
				ExecuteInstruction("PUSH \"JOBERROR\"");
				return;
			}
			_ = job.Status;
			if (job.GetStack(out var stack).Errors.Count > 0)
			{
				LogHelper.Instance.Log($"Error getting stack for push-dvd job {job.ID}", LogEntryType.Error);
				ExecuteInstruction("PUSH \"JOBERROR\"");
				return;
			}
			for (int i = 0; i < stack.Count - 1; i++)
			{
				stack.Pop();
			}
			string arg = stack.Pop();
			ProfileManager.Instance.Service.TrashJob(job.ID);
			ExecuteInstruction($"PUSH \"{arg}\"");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ERROR: push-dvd-in failed.", e);
		}
	}

	private void OnPutInEmptySlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			new PutInEmptySlotResult(ProfileManager.Instance.Service).Run();
		}
	}

	private void OnPutDvdInPickerIntoSlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			Location location = CommonFunctions.CurrentLocation();
			if (location == null)
			{
				return;
			}
			using PutInLocationResult putInLocationResult = new PutInLocationResult(ProfileManager.Instance.Service, location.Deck, location.Slot);
			putInLocationResult.Run();
		}
	}

	private void OnGetAndCenter(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			MoveToForm moveToForm = new MoveToForm();
			moveToForm.DisableModes();
			moveToForm.Text = "Get From Location";
			if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue || !moveToForm.Slot.HasValue)
			{
				return;
			}
			using GetAndCenterResult getAndCenterResult = new GetAndCenterResult(ProfileManager.Instance.Service, moveToForm.Deck.Value, moveToForm.Slot.Value, center: true);
			getAndCenterResult.Run();
		}
	}

	private void OnGetAndVend(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			MoveToForm moveToForm = new MoveToForm();
			moveToForm.DisableModes();
			moveToForm.Text = "Vend from Location";
			if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue || !moveToForm.Slot.HasValue)
			{
				return;
			}
			List<Redbox.HAL.Client.Location> list = new List<Redbox.HAL.Client.Location>();
			list.Add(new Redbox.HAL.Client.Location
			{
				Deck = moveToForm.Deck.Value,
				Slot = moveToForm.Slot.Value
			});
			HardwareCommandResult hardwareCommandResult = ProfileManager.Instance.Service.Vend(list, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Normal
			}, out var job);
			if (hardwareCommandResult.Success)
			{
				job.Resume();
				Application.DoEvents();
				hardwareCommandResult = job.WaitForCompletion();
				if (hardwareCommandResult.Success)
				{
					CommonFunctions.ProcessCommandResult(hardwareCommandResult);
					ProfileManager.Instance.Service.TrashJob(job.ID);
				}
			}
		}
	}

	private HardwareJob ExecuteInstruction(string instruction)
	{
		Application.DoEvents();
		WriteInstructionToOutput(instruction);
		HardwareJob result = CommonFunctions.ExecuteInstruction(instruction);
		Application.DoEvents();
		return result;
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
		this.m_touchScreenAccessTabControl = new System.Windows.Forms.TabControl();
		this.m_movementTabPage = new System.Windows.Forms.TabPage();
		this.m_movementFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_movementMoveButton = new System.Windows.Forms.Button();
		this.m_movementMoveVendButton = new System.Windows.Forms.Button();
		this.m_movementHomeXButton = new System.Windows.Forms.Button();
		this.m_movementHomeYButton = new System.Windows.Forms.Button();
		this.m_movementTransferButton = new System.Windows.Forms.Button();
		this.m_movementCurrentLocationButton = new System.Windows.Forms.Button();
		this.m_movementMoveNoCheckButton = new System.Windows.Forms.Button();
		this.m_inventoryTabPage = new System.Windows.Forms.TabPage();
		this.m_inventoryFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_invGetButton = new System.Windows.Forms.Button();
		this.m_invPutButton = new System.Windows.Forms.Button();
		this.m_invReadButton = new System.Windows.Forms.Button();
		this.m_invInspectButton = new System.Windows.Forms.Button();
		this.m_invFindEmptyButton = new System.Windows.Forms.Button();
		this.m_invSyncSlotButton = new System.Windows.Forms.Button();
		this.m_invPutinSlotButton = new System.Windows.Forms.Button();
		this.m_invGetVendButton = new System.Windows.Forms.Button();
		this.m_invGetCenterButton = new System.Windows.Forms.Button();
		this.m_gripperPutInEmptyButton = new System.Windows.Forms.Button();
		this.m_rollerTabPage = new System.Windows.Forms.TabPage();
		this.m_rollerFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_rollerRollerInButton = new System.Windows.Forms.Button();
		this.m_rollerRollerOut = new System.Windows.Forms.Button();
		this.m_rollerToPositionButton = new System.Windows.Forms.Button();
		this.m_rollerStopButton = new System.Windows.Forms.Button();
		this.m_gripperTabPage = new System.Windows.Forms.TabPage();
		this.m_gripperFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_gripperExtendButton = new System.Windows.Forms.Button();
		this.m_gripperRetractButton = new System.Windows.Forms.Button();
		this.m_gripperOpenButton = new System.Windows.Forms.Button();
		this.m_gripperRentButton = new System.Windows.Forms.Button();
		this.m_gripperCloseButton = new System.Windows.Forms.Button();
		this.m_gripperPeekButton = new System.Windows.Forms.Button();
		this.m_gripperPushButton = new System.Windows.Forms.Button();
		this.m_gripperStatusButton = new System.Windows.Forms.Button();
		this.m_gripperClearButton = new System.Windows.Forms.Button();
		this.m_gripperPushDvdInButton = new System.Windows.Forms.Button();
		this.m_cameraTabPage = new System.Windows.Forms.TabPage();
		this.m_cameraFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_cameraRingOnButton = new System.Windows.Forms.Button();
		this.m_cameraRingOffButton = new System.Windows.Forms.Button();
		this.m_cameraStartButton = new System.Windows.Forms.Button();
		this.m_cameraSnap = new System.Windows.Forms.Button();
		this.m_cameraStopButton = new System.Windows.Forms.Button();
		this.m_cameraStatusButton = new System.Windows.Forms.Button();
		this.m_vendDoorTabPage = new System.Windows.Forms.TabPage();
		this.m_vendDoorFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_vendDoorRentButton = new System.Windows.Forms.Button();
		this.m_vendDoorStatusButton = new System.Windows.Forms.Button();
		this.m_vendDoorCloseButton = new System.Windows.Forms.Button();
		this.m_trackTabPage = new System.Windows.Forms.TabPage();
		this.m_trackFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_trackOpenButton = new System.Windows.Forms.Button();
		this.m_trackCloseButton = new System.Windows.Forms.Button();
		this.m_trackStatusButton = new System.Windows.Forms.Button();
		this.m_sensorTabPage = new System.Windows.Forms.TabPage();
		this.m_sensorFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_sensorPickerOnButton = new System.Windows.Forms.Button();
		this.m_sensorPickerOffButton = new System.Windows.Forms.Button();
		this.m_sensorPickerReadButton = new System.Windows.Forms.Button();
		this.m_sensorPickerWaitClearButton = new System.Windows.Forms.Button();
		this.m_sensorPickerWaitBlockButton = new System.Windows.Forms.Button();
		this.m_sensorReadPickerInputsButton = new System.Windows.Forms.Button();
		this.m_sensorReadAuxInputsButton = new System.Windows.Forms.Button();
		this.m_qlmTabPage = new System.Windows.Forms.TabPage();
		this.m_qlmFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_qlmEngageButton = new System.Windows.Forms.Button();
		this.m_qlmDisengageButton = new System.Windows.Forms.Button();
		this.m_qlmStatusButton = new System.Windows.Forms.Button();
		this.m_qlmLiftButton = new System.Windows.Forms.Button();
		this.m_qlmDropButton = new System.Windows.Forms.Button();
		this.m_qlmHaltButton = new System.Windows.Forms.Button();
		this.m_systemTabPage = new System.Windows.Forms.TabPage();
		this.m_systemFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		this.m_systemResetButton = new System.Windows.Forms.Button();
		this.m_systemVersionButton = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.m_audioTabPage = new System.Windows.Forms.TabPage();
		this.m_touchScreenAccessTabControl.SuspendLayout();
		this.m_movementTabPage.SuspendLayout();
		this.m_movementFlowLayoutPanel.SuspendLayout();
		this.m_inventoryTabPage.SuspendLayout();
		this.m_inventoryFlowLayoutPanel.SuspendLayout();
		this.m_rollerTabPage.SuspendLayout();
		this.m_rollerFlowLayoutPanel.SuspendLayout();
		this.m_gripperTabPage.SuspendLayout();
		this.m_gripperFlowLayoutPanel.SuspendLayout();
		this.m_cameraTabPage.SuspendLayout();
		this.m_cameraFlowLayoutPanel.SuspendLayout();
		this.m_vendDoorTabPage.SuspendLayout();
		this.m_vendDoorFlowLayoutPanel.SuspendLayout();
		this.m_trackTabPage.SuspendLayout();
		this.m_trackFlowLayoutPanel.SuspendLayout();
		this.m_sensorTabPage.SuspendLayout();
		this.m_sensorFlowLayoutPanel.SuspendLayout();
		this.m_qlmTabPage.SuspendLayout();
		this.m_qlmFlowLayoutPanel.SuspendLayout();
		this.m_systemTabPage.SuspendLayout();
		this.m_systemFlowLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_movementTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_inventoryTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_rollerTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_gripperTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_cameraTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_vendDoorTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_trackTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_sensorTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_qlmTabPage);
		this.m_touchScreenAccessTabControl.Controls.Add(this.m_systemTabPage);
		this.m_touchScreenAccessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_touchScreenAccessTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
		this.m_touchScreenAccessTabControl.ItemSize = new System.Drawing.Size(75, 75);
		this.m_touchScreenAccessTabControl.Location = new System.Drawing.Point(0, 0);
		this.m_touchScreenAccessTabControl.Name = "m_touchScreenAccessTabControl";
		this.m_touchScreenAccessTabControl.Padding = new System.Drawing.Point(3, 0);
		this.m_touchScreenAccessTabControl.RightToLeftLayout = true;
		this.m_touchScreenAccessTabControl.SelectedIndex = 0;
		this.m_touchScreenAccessTabControl.Size = new System.Drawing.Size(620, 620);
		this.m_touchScreenAccessTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		this.m_touchScreenAccessTabControl.TabIndex = 0;
		this.m_touchScreenAccessTabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(TouchAccessTabControl_DrawItem);
		this.m_movementTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_movementTabPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_movementTabPage.Controls.Add(this.m_movementFlowLayoutPanel);
		this.m_movementTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_movementTabPage.Name = "m_movementTabPage";
		this.m_movementTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_movementTabPage.TabIndex = 0;
		this.m_movementTabPage.Tag = "Movement";
		this.m_movementTabPage.Text = "Movement";
		this.m_movementFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementMoveButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementMoveVendButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementHomeXButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementHomeYButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementTransferButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementCurrentLocationButton);
		this.m_movementFlowLayoutPanel.Controls.Add(this.m_movementMoveNoCheckButton);
		this.m_movementFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_movementFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_movementFlowLayoutPanel.Name = "m_movementFlowLayoutPanel";
		this.m_movementFlowLayoutPanel.Size = new System.Drawing.Size(610, 535);
		this.m_movementFlowLayoutPanel.TabIndex = 7;
		this.m_movementMoveButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementMoveButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.m_movementMoveButton.Location = new System.Drawing.Point(3, 3);
		this.m_movementMoveButton.Name = "m_movementMoveButton";
		this.m_movementMoveButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementMoveButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementMoveButton.TabIndex = 0;
		this.m_movementMoveButton.Tag = "Move";
		this.m_movementMoveButton.Text = "Move";
		this.m_movementMoveButton.UseVisualStyleBackColor = false;
		this.m_movementMoveButton.Click += new System.EventHandler(OnMove);
		this.m_movementMoveVendButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementMoveVendButton.Location = new System.Drawing.Point(159, 3);
		this.m_movementMoveVendButton.Name = "m_movementMoveVendButton";
		this.m_movementMoveVendButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementMoveVendButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementMoveVendButton.TabIndex = 1;
		this.m_movementMoveVendButton.Tag = "MOVEVEND";
		this.m_movementMoveVendButton.Text = "Move Vend";
		this.m_movementMoveVendButton.UseVisualStyleBackColor = false;
		this.m_movementMoveVendButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_movementHomeXButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementHomeXButton.Location = new System.Drawing.Point(315, 3);
		this.m_movementHomeXButton.Name = "m_movementHomeXButton";
		this.m_movementHomeXButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementHomeXButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementHomeXButton.TabIndex = 2;
		this.m_movementHomeXButton.Tag = "HOMEX";
		this.m_movementHomeXButton.Text = "Home X";
		this.m_movementHomeXButton.UseVisualStyleBackColor = false;
		this.m_movementHomeXButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_movementHomeYButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementHomeYButton.Location = new System.Drawing.Point(3, 84);
		this.m_movementHomeYButton.Name = "m_movementHomeYButton";
		this.m_movementHomeYButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementHomeYButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementHomeYButton.TabIndex = 3;
		this.m_movementHomeYButton.Tag = "HOMEY";
		this.m_movementHomeYButton.Text = "Home Y";
		this.m_movementHomeYButton.UseVisualStyleBackColor = false;
		this.m_movementHomeYButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_movementTransferButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementTransferButton.Location = new System.Drawing.Point(159, 84);
		this.m_movementTransferButton.Name = "m_movementTransferButton";
		this.m_movementTransferButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementTransferButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementTransferButton.TabIndex = 4;
		this.m_movementTransferButton.Tag = "Transfer";
		this.m_movementTransferButton.Text = "Transfer";
		this.m_movementTransferButton.UseVisualStyleBackColor = false;
		this.m_movementTransferButton.Click += new System.EventHandler(OnTransfer);
		this.m_movementCurrentLocationButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementCurrentLocationButton.Location = new System.Drawing.Point(315, 84);
		this.m_movementCurrentLocationButton.Name = "m_movementCurrentLocationButton";
		this.m_movementCurrentLocationButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementCurrentLocationButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementCurrentLocationButton.TabIndex = 5;
		this.m_movementCurrentLocationButton.Tag = "LOC";
		this.m_movementCurrentLocationButton.Text = "Current Location";
		this.m_movementCurrentLocationButton.UseVisualStyleBackColor = false;
		this.m_movementCurrentLocationButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_movementMoveNoCheckButton.BackColor = System.Drawing.Color.LightGray;
		this.m_movementMoveNoCheckButton.Location = new System.Drawing.Point(3, 165);
		this.m_movementMoveNoCheckButton.Name = "m_movementMoveNoCheckButton";
		this.m_movementMoveNoCheckButton.Padding = new System.Windows.Forms.Padding(10);
		this.m_movementMoveNoCheckButton.Size = new System.Drawing.Size(150, 75);
		this.m_movementMoveNoCheckButton.TabIndex = 6;
		this.m_movementMoveNoCheckButton.Tag = "Move Without Sensor Check";
		this.m_movementMoveNoCheckButton.Text = "Move Without Sensor Check";
		this.m_movementMoveNoCheckButton.UseVisualStyleBackColor = false;
		this.m_movementMoveNoCheckButton.Click += new System.EventHandler(OnMoveNoCheck);
		this.m_inventoryTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_inventoryTabPage.Controls.Add(this.m_inventoryFlowLayoutPanel);
		this.m_inventoryTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_inventoryTabPage.Name = "m_inventoryTabPage";
		this.m_inventoryTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_inventoryTabPage.TabIndex = 1;
		this.m_inventoryTabPage.Tag = "Inventory";
		this.m_inventoryTabPage.Text = "Inventory";
		this.m_inventoryFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invGetButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invPutButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invReadButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invInspectButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invFindEmptyButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invSyncSlotButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invPutinSlotButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invGetVendButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_invGetCenterButton);
		this.m_inventoryFlowLayoutPanel.Controls.Add(this.m_gripperPutInEmptyButton);
		this.m_inventoryFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_inventoryFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_inventoryFlowLayoutPanel.Name = "m_inventoryFlowLayoutPanel";
		this.m_inventoryFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_inventoryFlowLayoutPanel.TabIndex = 0;
		this.m_invGetButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invGetButton.Location = new System.Drawing.Point(3, 3);
		this.m_invGetButton.Name = "m_invGetButton";
		this.m_invGetButton.Size = new System.Drawing.Size(150, 75);
		this.m_invGetButton.TabIndex = 1;
		this.m_invGetButton.Tag = "GET";
		this.m_invGetButton.Text = "Get";
		this.m_invGetButton.UseVisualStyleBackColor = false;
		this.m_invGetButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_invPutButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invPutButton.Location = new System.Drawing.Point(159, 3);
		this.m_invPutButton.Name = "m_invPutButton";
		this.m_invPutButton.Size = new System.Drawing.Size(150, 75);
		this.m_invPutButton.TabIndex = 2;
		this.m_invPutButton.Tag = "PUT";
		this.m_invPutButton.Text = "Put";
		this.m_invPutButton.UseVisualStyleBackColor = false;
		this.m_invPutButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_invReadButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invReadButton.Location = new System.Drawing.Point(315, 3);
		this.m_invReadButton.Name = "m_invReadButton";
		this.m_invReadButton.Size = new System.Drawing.Size(150, 75);
		this.m_invReadButton.TabIndex = 3;
		this.m_invReadButton.Tag = "READ";
		this.m_invReadButton.Text = "Read";
		this.m_invReadButton.UseVisualStyleBackColor = false;
		this.m_invReadButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_invInspectButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invInspectButton.Location = new System.Drawing.Point(3, 84);
		this.m_invInspectButton.Name = "m_invInspectButton";
		this.m_invInspectButton.Size = new System.Drawing.Size(150, 75);
		this.m_invInspectButton.TabIndex = 4;
		this.m_invInspectButton.Tag = "Inspect";
		this.m_invInspectButton.Text = "Inspect";
		this.m_invInspectButton.UseVisualStyleBackColor = false;
		this.m_invInspectButton.Click += new System.EventHandler(OnInspect);
		this.m_invFindEmptyButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invFindEmptyButton.Location = new System.Drawing.Point(159, 84);
		this.m_invFindEmptyButton.Name = "m_invFindEmptyButton";
		this.m_invFindEmptyButton.Size = new System.Drawing.Size(150, 75);
		this.m_invFindEmptyButton.TabIndex = 5;
		this.m_invFindEmptyButton.Tag = "FINDEMPTY";
		this.m_invFindEmptyButton.Text = "Find Empty Location";
		this.m_invFindEmptyButton.UseVisualStyleBackColor = false;
		this.m_invFindEmptyButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_invSyncSlotButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invSyncSlotButton.Location = new System.Drawing.Point(315, 84);
		this.m_invSyncSlotButton.Name = "m_invSyncSlotButton";
		this.m_invSyncSlotButton.Size = new System.Drawing.Size(150, 75);
		this.m_invSyncSlotButton.TabIndex = 7;
		this.m_invSyncSlotButton.Tag = "Sync Slot";
		this.m_invSyncSlotButton.Text = "Sync Slot";
		this.m_invSyncSlotButton.UseVisualStyleBackColor = false;
		this.m_invSyncSlotButton.Click += new System.EventHandler(OnSyncSlot);
		this.m_invPutinSlotButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invPutinSlotButton.Location = new System.Drawing.Point(3, 165);
		this.m_invPutinSlotButton.Name = "m_invPutinSlotButton";
		this.m_invPutinSlotButton.Size = new System.Drawing.Size(150, 75);
		this.m_invPutinSlotButton.TabIndex = 8;
		this.m_invPutinSlotButton.Tag = "Put DVD In Picker Into Slot";
		this.m_invPutinSlotButton.Text = "Put DVD In Picker Into Slot";
		this.m_invPutinSlotButton.UseVisualStyleBackColor = false;
		this.m_invPutinSlotButton.Click += new System.EventHandler(OnPutDvdInPickerIntoSlot);
		this.m_invGetVendButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invGetVendButton.Location = new System.Drawing.Point(159, 165);
		this.m_invGetVendButton.Name = "m_invGetVendButton";
		this.m_invGetVendButton.Size = new System.Drawing.Size(150, 75);
		this.m_invGetVendButton.TabIndex = 9;
		this.m_invGetVendButton.Tag = "Get and Vend DVD";
		this.m_invGetVendButton.Text = "Get and Vend DVD";
		this.m_invGetVendButton.UseVisualStyleBackColor = false;
		this.m_invGetVendButton.Click += new System.EventHandler(OnGetAndVend);
		this.m_invGetCenterButton.BackColor = System.Drawing.Color.LightGray;
		this.m_invGetCenterButton.Location = new System.Drawing.Point(315, 165);
		this.m_invGetCenterButton.Name = "m_invGetCenterButton";
		this.m_invGetCenterButton.Size = new System.Drawing.Size(150, 75);
		this.m_invGetCenterButton.TabIndex = 10;
		this.m_invGetCenterButton.Tag = "Get And Center DVD";
		this.m_invGetCenterButton.Text = "Get And Center DVD";
		this.m_invGetCenterButton.UseVisualStyleBackColor = false;
		this.m_invGetCenterButton.Click += new System.EventHandler(OnGetAndCenter);
		this.m_gripperPutInEmptyButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperPutInEmptyButton.Location = new System.Drawing.Point(3, 246);
		this.m_gripperPutInEmptyButton.Name = "m_gripperPutInEmptyButton";
		this.m_gripperPutInEmptyButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperPutInEmptyButton.TabIndex = 11;
		this.m_gripperPutInEmptyButton.Tag = "GRIPPER PUT-DISC-AWAY";
		this.m_gripperPutInEmptyButton.Text = "Put In Empty Slot";
		this.m_gripperPutInEmptyButton.UseVisualStyleBackColor = false;
		this.m_gripperPutInEmptyButton.Click += new System.EventHandler(OnPutInEmptySlot);
		this.m_rollerTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_rollerTabPage.Controls.Add(this.m_rollerFlowLayoutPanel);
		this.m_rollerTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_rollerTabPage.Name = "m_rollerTabPage";
		this.m_rollerTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_rollerTabPage.TabIndex = 1;
		this.m_rollerTabPage.Tag = "Roller";
		this.m_rollerTabPage.Text = "Roller";
		this.m_rollerFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_rollerFlowLayoutPanel.Controls.Add(this.m_rollerRollerInButton);
		this.m_rollerFlowLayoutPanel.Controls.Add(this.m_rollerRollerOut);
		this.m_rollerFlowLayoutPanel.Controls.Add(this.m_rollerToPositionButton);
		this.m_rollerFlowLayoutPanel.Controls.Add(this.m_rollerStopButton);
		this.m_rollerFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_rollerFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_rollerFlowLayoutPanel.Name = "m_rollerFlowLayoutPanel";
		this.m_rollerFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_rollerFlowLayoutPanel.TabIndex = 0;
		this.m_rollerRollerInButton.BackColor = System.Drawing.Color.LightGray;
		this.m_rollerRollerInButton.Location = new System.Drawing.Point(3, 3);
		this.m_rollerRollerInButton.Name = "m_rollerRollerInButton";
		this.m_rollerRollerInButton.Size = new System.Drawing.Size(150, 75);
		this.m_rollerRollerInButton.TabIndex = 0;
		this.m_rollerRollerInButton.Tag = "ROLLER IN";
		this.m_rollerRollerInButton.Text = "Roller In";
		this.m_rollerRollerInButton.UseVisualStyleBackColor = false;
		this.m_rollerRollerInButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_rollerRollerOut.BackColor = System.Drawing.Color.LightGray;
		this.m_rollerRollerOut.Location = new System.Drawing.Point(159, 3);
		this.m_rollerRollerOut.Name = "m_rollerRollerOut";
		this.m_rollerRollerOut.Size = new System.Drawing.Size(150, 75);
		this.m_rollerRollerOut.TabIndex = 1;
		this.m_rollerRollerOut.Tag = "ROLLER OUT";
		this.m_rollerRollerOut.Text = "Roller Out";
		this.m_rollerRollerOut.UseVisualStyleBackColor = false;
		this.m_rollerRollerOut.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_rollerToPositionButton.BackColor = System.Drawing.Color.LightGray;
		this.m_rollerToPositionButton.Location = new System.Drawing.Point(315, 3);
		this.m_rollerToPositionButton.Name = "m_rollerToPositionButton";
		this.m_rollerToPositionButton.Size = new System.Drawing.Size(150, 75);
		this.m_rollerToPositionButton.TabIndex = 2;
		this.m_rollerToPositionButton.Tag = "Roller To Position";
		this.m_rollerToPositionButton.Text = "Roller To Position";
		this.m_rollerToPositionButton.UseVisualStyleBackColor = false;
		this.m_rollerToPositionButton.Click += new System.EventHandler(OnRollerToPosition);
		this.m_rollerStopButton.BackColor = System.Drawing.Color.LightGray;
		this.m_rollerStopButton.Location = new System.Drawing.Point(3, 84);
		this.m_rollerStopButton.Name = "m_rollerStopButton";
		this.m_rollerStopButton.Size = new System.Drawing.Size(150, 75);
		this.m_rollerStopButton.TabIndex = 3;
		this.m_rollerStopButton.Tag = "ROLLER STOP";
		this.m_rollerStopButton.Text = "Roller Stop";
		this.m_rollerStopButton.UseVisualStyleBackColor = false;
		this.m_rollerStopButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_gripperTabPage.Controls.Add(this.m_gripperFlowLayoutPanel);
		this.m_gripperTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_gripperTabPage.Name = "m_gripperTabPage";
		this.m_gripperTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_gripperTabPage.TabIndex = 1;
		this.m_gripperTabPage.Tag = "Gripper";
		this.m_gripperTabPage.Text = "Gripper";
		this.m_gripperFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperExtendButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperRetractButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperOpenButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperRentButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperCloseButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperPeekButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperPushButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperStatusButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperClearButton);
		this.m_gripperFlowLayoutPanel.Controls.Add(this.m_gripperPushDvdInButton);
		this.m_gripperFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_gripperFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_gripperFlowLayoutPanel.Name = "m_gripperFlowLayoutPanel";
		this.m_gripperFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_gripperFlowLayoutPanel.TabIndex = 0;
		this.m_gripperExtendButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperExtendButton.Location = new System.Drawing.Point(3, 3);
		this.m_gripperExtendButton.Name = "m_gripperExtendButton";
		this.m_gripperExtendButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperExtendButton.TabIndex = 0;
		this.m_gripperExtendButton.Tag = "GRIPPER EXTEND";
		this.m_gripperExtendButton.Text = "Gripper Extend";
		this.m_gripperExtendButton.UseVisualStyleBackColor = false;
		this.m_gripperExtendButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperRetractButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperRetractButton.Location = new System.Drawing.Point(159, 3);
		this.m_gripperRetractButton.Name = "m_gripperRetractButton";
		this.m_gripperRetractButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperRetractButton.TabIndex = 1;
		this.m_gripperRetractButton.Tag = "GRIPPER RETRACT";
		this.m_gripperRetractButton.Text = "Gripper Retract";
		this.m_gripperRetractButton.UseVisualStyleBackColor = false;
		this.m_gripperRetractButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperOpenButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperOpenButton.Location = new System.Drawing.Point(315, 3);
		this.m_gripperOpenButton.Name = "m_gripperOpenButton";
		this.m_gripperOpenButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperOpenButton.TabIndex = 2;
		this.m_gripperOpenButton.Tag = "GRIPPER OPEN";
		this.m_gripperOpenButton.Text = "Gripper Open";
		this.m_gripperOpenButton.UseVisualStyleBackColor = false;
		this.m_gripperOpenButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperRentButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperRentButton.Location = new System.Drawing.Point(3, 84);
		this.m_gripperRentButton.Name = "m_gripperRentButton";
		this.m_gripperRentButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperRentButton.TabIndex = 3;
		this.m_gripperRentButton.Tag = "GRIPPER RENT";
		this.m_gripperRentButton.Text = "Gripper Rent";
		this.m_gripperRentButton.UseVisualStyleBackColor = false;
		this.m_gripperRentButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperCloseButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperCloseButton.Location = new System.Drawing.Point(159, 84);
		this.m_gripperCloseButton.Name = "m_gripperCloseButton";
		this.m_gripperCloseButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperCloseButton.TabIndex = 4;
		this.m_gripperCloseButton.Tag = "GRIPPER CLOSE";
		this.m_gripperCloseButton.Text = "Gripper Close";
		this.m_gripperCloseButton.UseVisualStyleBackColor = false;
		this.m_gripperCloseButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperPeekButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperPeekButton.Location = new System.Drawing.Point(315, 84);
		this.m_gripperPeekButton.Name = "m_gripperPeekButton";
		this.m_gripperPeekButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperPeekButton.TabIndex = 5;
		this.m_gripperPeekButton.Tag = "GRIPPER PEEK";
		this.m_gripperPeekButton.Text = "Gripper Peek";
		this.m_gripperPeekButton.UseVisualStyleBackColor = false;
		this.m_gripperPeekButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperPushButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperPushButton.Location = new System.Drawing.Point(3, 165);
		this.m_gripperPushButton.Name = "m_gripperPushButton";
		this.m_gripperPushButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperPushButton.TabIndex = 6;
		this.m_gripperPushButton.Tag = "GRIPPER PUSH";
		this.m_gripperPushButton.Text = "Gripper Push";
		this.m_gripperPushButton.UseVisualStyleBackColor = false;
		this.m_gripperPushButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperStatusButton.Location = new System.Drawing.Point(159, 165);
		this.m_gripperStatusButton.Name = "m_gripperStatusButton";
		this.m_gripperStatusButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperStatusButton.TabIndex = 7;
		this.m_gripperStatusButton.Tag = "GRIPPER STATUS";
		this.m_gripperStatusButton.Text = "Gripper Status";
		this.m_gripperStatusButton.UseVisualStyleBackColor = false;
		this.m_gripperStatusButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperClearButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperClearButton.Location = new System.Drawing.Point(315, 165);
		this.m_gripperClearButton.Name = "m_gripperClearButton";
		this.m_gripperClearButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperClearButton.TabIndex = 8;
		this.m_gripperClearButton.Tag = "GRIPPER CLEAR";
		this.m_gripperClearButton.Text = "Gripper Clear";
		this.m_gripperClearButton.UseVisualStyleBackColor = false;
		this.m_gripperClearButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_gripperPushDvdInButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperPushDvdInButton.Location = new System.Drawing.Point(3, 246);
		this.m_gripperPushDvdInButton.Name = "m_gripperPushDvdInButton";
		this.m_gripperPushDvdInButton.Size = new System.Drawing.Size(150, 75);
		this.m_gripperPushDvdInButton.TabIndex = 9;
		this.m_gripperPushDvdInButton.Tag = "Push DVD In";
		this.m_gripperPushDvdInButton.Text = "Push DVD In";
		this.m_gripperPushDvdInButton.UseVisualStyleBackColor = false;
		this.m_gripperPushDvdInButton.Click += new System.EventHandler(OnPushDVDIn);
		this.m_cameraTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_cameraTabPage.Controls.Add(this.m_cameraFlowLayoutPanel);
		this.m_cameraTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_cameraTabPage.Name = "m_cameraTabPage";
		this.m_cameraTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_cameraTabPage.TabIndex = 2;
		this.m_cameraTabPage.Tag = "Camera";
		this.m_cameraTabPage.Text = "Camera";
		this.m_cameraFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraRingOnButton);
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraRingOffButton);
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraStartButton);
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraSnap);
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraStopButton);
		this.m_cameraFlowLayoutPanel.Controls.Add(this.m_cameraStatusButton);
		this.m_cameraFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_cameraFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_cameraFlowLayoutPanel.Name = "m_cameraFlowLayoutPanel";
		this.m_cameraFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_cameraFlowLayoutPanel.TabIndex = 0;
		this.m_cameraRingOnButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraRingOnButton.Location = new System.Drawing.Point(3, 3);
		this.m_cameraRingOnButton.Name = "m_cameraRingOnButton";
		this.m_cameraRingOnButton.Size = new System.Drawing.Size(150, 75);
		this.m_cameraRingOnButton.TabIndex = 0;
		this.m_cameraRingOnButton.Tag = "RINGLIGHT ON";
		this.m_cameraRingOnButton.Text = "Ring Light On";
		this.m_cameraRingOnButton.UseVisualStyleBackColor = false;
		this.m_cameraRingOnButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_cameraRingOffButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraRingOffButton.Location = new System.Drawing.Point(159, 3);
		this.m_cameraRingOffButton.Name = "m_cameraRingOffButton";
		this.m_cameraRingOffButton.Size = new System.Drawing.Size(150, 75);
		this.m_cameraRingOffButton.TabIndex = 1;
		this.m_cameraRingOffButton.Tag = "RINGLIGHT OFF";
		this.m_cameraRingOffButton.Text = "Ring Light Off";
		this.m_cameraRingOffButton.UseVisualStyleBackColor = false;
		this.m_cameraRingOffButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_cameraStartButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraStartButton.Location = new System.Drawing.Point(315, 3);
		this.m_cameraStartButton.Name = "m_cameraStartButton";
		this.m_cameraStartButton.Size = new System.Drawing.Size(150, 75);
		this.m_cameraStartButton.TabIndex = 2;
		this.m_cameraStartButton.Tag = "CAMERA START CENTER=TRUE";
		this.m_cameraStartButton.Text = "Camera Start";
		this.m_cameraStartButton.UseVisualStyleBackColor = false;
		this.m_cameraStartButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_cameraSnap.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraSnap.Location = new System.Drawing.Point(3, 84);
		this.m_cameraSnap.Name = "m_cameraSnap";
		this.m_cameraSnap.Size = new System.Drawing.Size(150, 75);
		this.m_cameraSnap.TabIndex = 3;
		this.m_cameraSnap.Tag = "CAMERA SNAP";
		this.m_cameraSnap.Text = "Camera Snap";
		this.m_cameraSnap.UseVisualStyleBackColor = false;
		this.m_cameraSnap.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_cameraStopButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraStopButton.Location = new System.Drawing.Point(159, 84);
		this.m_cameraStopButton.Name = "m_cameraStopButton";
		this.m_cameraStopButton.Size = new System.Drawing.Size(150, 75);
		this.m_cameraStopButton.TabIndex = 4;
		this.m_cameraStopButton.Tag = "CAMERA STOP";
		this.m_cameraStopButton.Text = "Camera Stop";
		this.m_cameraStopButton.UseVisualStyleBackColor = false;
		this.m_cameraStopButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_cameraStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraStatusButton.Location = new System.Drawing.Point(315, 84);
		this.m_cameraStatusButton.Name = "m_cameraStatusButton";
		this.m_cameraStatusButton.Size = new System.Drawing.Size(150, 75);
		this.m_cameraStatusButton.TabIndex = 5;
		this.m_cameraStatusButton.Tag = "CAMERA STATUS";
		this.m_cameraStatusButton.Text = "Camera Status";
		this.m_cameraStatusButton.UseVisualStyleBackColor = false;
		this.m_cameraStatusButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_vendDoorTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_vendDoorTabPage.Controls.Add(this.m_vendDoorFlowLayoutPanel);
		this.m_vendDoorTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_vendDoorTabPage.Name = "m_vendDoorTabPage";
		this.m_vendDoorTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_vendDoorTabPage.TabIndex = 3;
		this.m_vendDoorTabPage.Tag = "VendDoor";
		this.m_vendDoorTabPage.Text = "VendDoor";
		this.m_vendDoorFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_vendDoorFlowLayoutPanel.Controls.Add(this.m_vendDoorRentButton);
		this.m_vendDoorFlowLayoutPanel.Controls.Add(this.m_vendDoorStatusButton);
		this.m_vendDoorFlowLayoutPanel.Controls.Add(this.m_vendDoorCloseButton);
		this.m_vendDoorFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_vendDoorFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_vendDoorFlowLayoutPanel.Name = "m_vendDoorFlowLayoutPanel";
		this.m_vendDoorFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_vendDoorFlowLayoutPanel.TabIndex = 0;
		this.m_vendDoorRentButton.BackColor = System.Drawing.Color.LightGray;
		this.m_vendDoorRentButton.Location = new System.Drawing.Point(3, 3);
		this.m_vendDoorRentButton.Name = "m_vendDoorRentButton";
		this.m_vendDoorRentButton.Size = new System.Drawing.Size(150, 75);
		this.m_vendDoorRentButton.TabIndex = 1;
		this.m_vendDoorRentButton.Tag = "VENDDOOR RENT";
		this.m_vendDoorRentButton.Text = "Vend Door Rent";
		this.m_vendDoorRentButton.UseVisualStyleBackColor = false;
		this.m_vendDoorRentButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_vendDoorStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_vendDoorStatusButton.Location = new System.Drawing.Point(159, 3);
		this.m_vendDoorStatusButton.Name = "m_vendDoorStatusButton";
		this.m_vendDoorStatusButton.Size = new System.Drawing.Size(150, 75);
		this.m_vendDoorStatusButton.TabIndex = 2;
		this.m_vendDoorStatusButton.Tag = "VENDDOOR STATUS";
		this.m_vendDoorStatusButton.Text = "Vend Door Status";
		this.m_vendDoorStatusButton.UseVisualStyleBackColor = false;
		this.m_vendDoorStatusButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_vendDoorCloseButton.BackColor = System.Drawing.Color.LightGray;
		this.m_vendDoorCloseButton.Location = new System.Drawing.Point(315, 3);
		this.m_vendDoorCloseButton.Name = "m_vendDoorCloseButton";
		this.m_vendDoorCloseButton.Size = new System.Drawing.Size(150, 75);
		this.m_vendDoorCloseButton.TabIndex = 3;
		this.m_vendDoorCloseButton.Tag = "VENDDOOR CLOSE";
		this.m_vendDoorCloseButton.Text = "Vend Door Close";
		this.m_vendDoorCloseButton.UseVisualStyleBackColor = false;
		this.m_vendDoorCloseButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_trackTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_trackTabPage.Controls.Add(this.m_trackFlowLayoutPanel);
		this.m_trackTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_trackTabPage.Name = "m_trackTabPage";
		this.m_trackTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_trackTabPage.TabIndex = 4;
		this.m_trackTabPage.Tag = "Track";
		this.m_trackTabPage.Text = "Track";
		this.m_trackFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_trackFlowLayoutPanel.Controls.Add(this.m_trackOpenButton);
		this.m_trackFlowLayoutPanel.Controls.Add(this.m_trackCloseButton);
		this.m_trackFlowLayoutPanel.Controls.Add(this.m_trackStatusButton);
		this.m_trackFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_trackFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_trackFlowLayoutPanel.Name = "m_trackFlowLayoutPanel";
		this.m_trackFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_trackFlowLayoutPanel.TabIndex = 0;
		this.m_trackOpenButton.BackColor = System.Drawing.Color.LightGray;
		this.m_trackOpenButton.Location = new System.Drawing.Point(3, 3);
		this.m_trackOpenButton.Name = "m_trackOpenButton";
		this.m_trackOpenButton.Size = new System.Drawing.Size(150, 75);
		this.m_trackOpenButton.TabIndex = 0;
		this.m_trackOpenButton.Tag = "TRACK OPEN";
		this.m_trackOpenButton.Text = "Track Open";
		this.m_trackOpenButton.UseVisualStyleBackColor = false;
		this.m_trackOpenButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_trackCloseButton.BackColor = System.Drawing.Color.LightGray;
		this.m_trackCloseButton.Location = new System.Drawing.Point(159, 3);
		this.m_trackCloseButton.Name = "m_trackCloseButton";
		this.m_trackCloseButton.Size = new System.Drawing.Size(150, 75);
		this.m_trackCloseButton.TabIndex = 1;
		this.m_trackCloseButton.Tag = "TRACK CLOSE";
		this.m_trackCloseButton.Text = "Track Close";
		this.m_trackCloseButton.UseVisualStyleBackColor = false;
		this.m_trackCloseButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_trackStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_trackStatusButton.Location = new System.Drawing.Point(315, 3);
		this.m_trackStatusButton.Name = "m_trackStatusButton";
		this.m_trackStatusButton.Size = new System.Drawing.Size(150, 75);
		this.m_trackStatusButton.TabIndex = 2;
		this.m_trackStatusButton.Tag = "TRACK STATUS";
		this.m_trackStatusButton.Text = "Track Status";
		this.m_trackStatusButton.UseVisualStyleBackColor = false;
		this.m_trackStatusButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_sensorTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_sensorTabPage.Controls.Add(this.m_sensorFlowLayoutPanel);
		this.m_sensorTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_sensorTabPage.Name = "m_sensorTabPage";
		this.m_sensorTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_sensorTabPage.TabIndex = 5;
		this.m_sensorTabPage.Tag = "Sensor";
		this.m_sensorTabPage.Text = "Sensor";
		this.m_sensorFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorPickerOnButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorPickerOffButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorPickerReadButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorPickerWaitClearButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorPickerWaitBlockButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorReadPickerInputsButton);
		this.m_sensorFlowLayoutPanel.Controls.Add(this.m_sensorReadAuxInputsButton);
		this.m_sensorFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_sensorFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_sensorFlowLayoutPanel.Name = "m_sensorFlowLayoutPanel";
		this.m_sensorFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_sensorFlowLayoutPanel.TabIndex = 0;
		this.m_sensorPickerOnButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorPickerOnButton.Location = new System.Drawing.Point(3, 3);
		this.m_sensorPickerOnButton.Name = "m_sensorPickerOnButton";
		this.m_sensorPickerOnButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorPickerOnButton.TabIndex = 0;
		this.m_sensorPickerOnButton.Tag = "SENSOR PICKER-ON";
		this.m_sensorPickerOnButton.Text = "Picker Sensors On";
		this.m_sensorPickerOnButton.UseVisualStyleBackColor = false;
		this.m_sensorPickerOnButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_sensorPickerOffButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorPickerOffButton.Location = new System.Drawing.Point(159, 3);
		this.m_sensorPickerOffButton.Name = "m_sensorPickerOffButton";
		this.m_sensorPickerOffButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorPickerOffButton.TabIndex = 1;
		this.m_sensorPickerOffButton.Tag = "SENSOR PICKER-OFF";
		this.m_sensorPickerOffButton.Text = "Picker Sensors Off";
		this.m_sensorPickerOffButton.UseVisualStyleBackColor = false;
		this.m_sensorPickerOffButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_sensorPickerReadButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorPickerReadButton.Location = new System.Drawing.Point(315, 3);
		this.m_sensorPickerReadButton.Name = "m_sensorPickerReadButton";
		this.m_sensorPickerReadButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorPickerReadButton.TabIndex = 2;
		this.m_sensorPickerReadButton.Tag = "Read Picker Sensors";
		this.m_sensorPickerReadButton.Text = "Read Picker Sensors";
		this.m_sensorPickerReadButton.UseVisualStyleBackColor = false;
		this.m_sensorPickerReadButton.Click += new System.EventHandler(OnReadPickerSensors);
		this.m_sensorPickerWaitClearButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorPickerWaitClearButton.Location = new System.Drawing.Point(3, 84);
		this.m_sensorPickerWaitClearButton.Name = "m_sensorPickerWaitClearButton";
		this.m_sensorPickerWaitClearButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorPickerWaitClearButton.TabIndex = 3;
		this.m_sensorPickerWaitClearButton.Tag = "Wait for Picker Sensors to Clear";
		this.m_sensorPickerWaitClearButton.Text = "Wait for Picker Sensors to Clear";
		this.m_sensorPickerWaitClearButton.UseVisualStyleBackColor = false;
		this.m_sensorPickerWaitClearButton.Click += new System.EventHandler(OnWaitPickerSensorsClear);
		this.m_sensorPickerWaitBlockButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorPickerWaitBlockButton.Location = new System.Drawing.Point(159, 84);
		this.m_sensorPickerWaitBlockButton.Name = "m_sensorPickerWaitBlockButton";
		this.m_sensorPickerWaitBlockButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorPickerWaitBlockButton.TabIndex = 4;
		this.m_sensorPickerWaitBlockButton.Tag = "Wait for Picker Sensors To Block";
		this.m_sensorPickerWaitBlockButton.Text = "Wait for Picker Sensors To Block";
		this.m_sensorPickerWaitBlockButton.UseVisualStyleBackColor = false;
		this.m_sensorPickerWaitBlockButton.Click += new System.EventHandler(OnWaitPickerSensorsBlock);
		this.m_sensorReadPickerInputsButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorReadPickerInputsButton.Location = new System.Drawing.Point(315, 84);
		this.m_sensorReadPickerInputsButton.Name = "m_sensorReadPickerInputsButton";
		this.m_sensorReadPickerInputsButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorReadPickerInputsButton.TabIndex = 5;
		this.m_sensorReadPickerInputsButton.Tag = "SENSOR READ-ALL-PICKER";
		this.m_sensorReadPickerInputsButton.Text = "Read Picker Inputs";
		this.m_sensorReadPickerInputsButton.UseVisualStyleBackColor = false;
		this.m_sensorReadPickerInputsButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_sensorReadAuxInputsButton.BackColor = System.Drawing.Color.LightGray;
		this.m_sensorReadAuxInputsButton.Location = new System.Drawing.Point(3, 165);
		this.m_sensorReadAuxInputsButton.Name = "m_sensorReadAuxInputsButton";
		this.m_sensorReadAuxInputsButton.Size = new System.Drawing.Size(150, 75);
		this.m_sensorReadAuxInputsButton.TabIndex = 7;
		this.m_sensorReadAuxInputsButton.Tag = "SENSOR READ-ALL-AUX";
		this.m_sensorReadAuxInputsButton.Text = "Read Aux Inputs";
		this.m_sensorReadAuxInputsButton.UseVisualStyleBackColor = false;
		this.m_sensorReadAuxInputsButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_qlmTabPage.Controls.Add(this.m_qlmFlowLayoutPanel);
		this.m_qlmTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_qlmTabPage.Name = "m_qlmTabPage";
		this.m_qlmTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_qlmTabPage.TabIndex = 6;
		this.m_qlmTabPage.Tag = "Qlm";
		this.m_qlmTabPage.Text = "Qlm";
		this.m_qlmFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmEngageButton);
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmDisengageButton);
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmStatusButton);
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmLiftButton);
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmDropButton);
		this.m_qlmFlowLayoutPanel.Controls.Add(this.m_qlmHaltButton);
		this.m_qlmFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_qlmFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_qlmFlowLayoutPanel.Name = "m_qlmFlowLayoutPanel";
		this.m_qlmFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_qlmFlowLayoutPanel.TabIndex = 0;
		this.m_qlmEngageButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmEngageButton.Location = new System.Drawing.Point(3, 3);
		this.m_qlmEngageButton.Name = "m_qlmEngageButton";
		this.m_qlmEngageButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmEngageButton.TabIndex = 3;
		this.m_qlmEngageButton.Tag = "QLM ENGAGE";
		this.m_qlmEngageButton.Text = "QLM Engage";
		this.m_qlmEngageButton.UseVisualStyleBackColor = false;
		this.m_qlmEngageButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmDisengageButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmDisengageButton.Location = new System.Drawing.Point(159, 3);
		this.m_qlmDisengageButton.Name = "m_qlmDisengageButton";
		this.m_qlmDisengageButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmDisengageButton.TabIndex = 4;
		this.m_qlmDisengageButton.Tag = "QLM DISENGAGE";
		this.m_qlmDisengageButton.Text = "QLM Disengage";
		this.m_qlmDisengageButton.UseVisualStyleBackColor = false;
		this.m_qlmDisengageButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmStatusButton.Location = new System.Drawing.Point(315, 3);
		this.m_qlmStatusButton.Name = "m_qlmStatusButton";
		this.m_qlmStatusButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmStatusButton.TabIndex = 5;
		this.m_qlmStatusButton.Tag = "QLM STATUS";
		this.m_qlmStatusButton.Text = "QLM Status";
		this.m_qlmStatusButton.UseVisualStyleBackColor = false;
		this.m_qlmStatusButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmLiftButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmLiftButton.Location = new System.Drawing.Point(3, 84);
		this.m_qlmLiftButton.Name = "m_qlmLiftButton";
		this.m_qlmLiftButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmLiftButton.TabIndex = 6;
		this.m_qlmLiftButton.Tag = "QLM LIFT";
		this.m_qlmLiftButton.Text = "QLM Lift";
		this.m_qlmLiftButton.UseVisualStyleBackColor = false;
		this.m_qlmLiftButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmDropButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmDropButton.Location = new System.Drawing.Point(159, 84);
		this.m_qlmDropButton.Name = "m_qlmDropButton";
		this.m_qlmDropButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmDropButton.TabIndex = 7;
		this.m_qlmDropButton.Tag = "QLM DROP";
		this.m_qlmDropButton.Text = "QLM Drop";
		this.m_qlmDropButton.UseVisualStyleBackColor = false;
		this.m_qlmDropButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_qlmHaltButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmHaltButton.Location = new System.Drawing.Point(315, 84);
		this.m_qlmHaltButton.Name = "m_qlmHaltButton";
		this.m_qlmHaltButton.Size = new System.Drawing.Size(150, 75);
		this.m_qlmHaltButton.TabIndex = 8;
		this.m_qlmHaltButton.Tag = "QLM Halt";
		this.m_qlmHaltButton.Text = "QLM Halt";
		this.m_qlmHaltButton.UseVisualStyleBackColor = false;
		this.m_qlmHaltButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_systemTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_systemTabPage.Controls.Add(this.m_systemFlowLayoutPanel);
		this.m_systemTabPage.Location = new System.Drawing.Point(4, 79);
		this.m_systemTabPage.Name = "m_systemTabPage";
		this.m_systemTabPage.Size = new System.Drawing.Size(612, 537);
		this.m_systemTabPage.TabIndex = 7;
		this.m_systemTabPage.Tag = "System";
		this.m_systemTabPage.Text = "System";
		this.m_systemFlowLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
		this.m_systemFlowLayoutPanel.Controls.Add(this.m_systemResetButton);
		this.m_systemFlowLayoutPanel.Controls.Add(this.m_systemVersionButton);
		this.m_systemFlowLayoutPanel.Controls.Add(this.button1);
		this.m_systemFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_systemFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.m_systemFlowLayoutPanel.Name = "m_systemFlowLayoutPanel";
		this.m_systemFlowLayoutPanel.Size = new System.Drawing.Size(612, 537);
		this.m_systemFlowLayoutPanel.TabIndex = 0;
		this.m_systemResetButton.BackColor = System.Drawing.Color.LightGray;
		this.m_systemResetButton.Location = new System.Drawing.Point(3, 3);
		this.m_systemResetButton.Name = "m_systemResetButton";
		this.m_systemResetButton.Size = new System.Drawing.Size(150, 75);
		this.m_systemResetButton.TabIndex = 0;
		this.m_systemResetButton.Tag = "SERIALBOARD RESET";
		this.m_systemResetButton.Text = "Reset";
		this.m_systemResetButton.UseVisualStyleBackColor = false;
		this.m_systemResetButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_systemVersionButton.BackColor = System.Drawing.Color.LightGray;
		this.m_systemVersionButton.Location = new System.Drawing.Point(159, 3);
		this.m_systemVersionButton.Name = "m_systemVersionButton";
		this.m_systemVersionButton.Size = new System.Drawing.Size(150, 75);
		this.m_systemVersionButton.TabIndex = 1;
		this.m_systemVersionButton.Tag = "VERSION";
		this.m_systemVersionButton.Text = "Version";
		this.m_systemVersionButton.UseVisualStyleBackColor = false;
		this.m_systemVersionButton.Click += new System.EventHandler(OnSimpleCommandButton);
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Location = new System.Drawing.Point(315, 3);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(150, 75);
		this.button1.TabIndex = 2;
		this.button1.Tag = "SERIALBOARD CLOSEPORT";
		this.button1.Text = "Close Serial Board Port";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(OnSimpleCommandButton);
		this.m_audioTabPage.BackColor = System.Drawing.SystemColors.AppWorkspace;
		this.m_audioTabPage.Location = new System.Drawing.Point(0, 0);
		this.m_audioTabPage.Name = "m_audioTabPage";
		this.m_audioTabPage.Size = new System.Drawing.Size(200, 100);
		this.m_audioTabPage.TabIndex = 0;
		this.m_audioTabPage.Tag = "Audio";
		this.m_audioTabPage.Text = "Audio";
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.m_touchScreenAccessTabControl);
		base.Enabled = false;
		base.Name = "TouchScreenAccess";
		base.Size = new System.Drawing.Size(620, 620);
		this.m_touchScreenAccessTabControl.ResumeLayout(false);
		this.m_movementTabPage.ResumeLayout(false);
		this.m_movementFlowLayoutPanel.ResumeLayout(false);
		this.m_inventoryTabPage.ResumeLayout(false);
		this.m_inventoryFlowLayoutPanel.ResumeLayout(false);
		this.m_rollerTabPage.ResumeLayout(false);
		this.m_rollerFlowLayoutPanel.ResumeLayout(false);
		this.m_gripperTabPage.ResumeLayout(false);
		this.m_gripperFlowLayoutPanel.ResumeLayout(false);
		this.m_cameraTabPage.ResumeLayout(false);
		this.m_cameraFlowLayoutPanel.ResumeLayout(false);
		this.m_vendDoorTabPage.ResumeLayout(false);
		this.m_vendDoorFlowLayoutPanel.ResumeLayout(false);
		this.m_trackTabPage.ResumeLayout(false);
		this.m_trackFlowLayoutPanel.ResumeLayout(false);
		this.m_sensorTabPage.ResumeLayout(false);
		this.m_sensorFlowLayoutPanel.ResumeLayout(false);
		this.m_qlmTabPage.ResumeLayout(false);
		this.m_qlmFlowLayoutPanel.ResumeLayout(false);
		this.m_systemTabPage.ResumeLayout(false);
		this.m_systemFlowLayoutPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
