using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.ControlPanel.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "CloseKioskViewUserControl")]
public partial class CloseKioskViewUserControl : UserControl, IComponentConnector
{
	public CloseKioskViewUserControl()
	{
		InitializeComponent();
		ResetCloseKioskView();
	}

	private void ButtonNoClick(object sender, RoutedEventArgs e)
	{
		ResetCloseKioskView();
		base.Visibility = Visibility.Hidden;
	}

	private void ResetCloseKioskView()
	{
		header_textbox.Text = "Warning!";
		message_text.Text = "Clicking Yes will mark ALL known kiosk inventory as removed.\n\nAre you sure you want to continue?";
		confirm_textbox.Visibility = Visibility.Hidden;
		Spinner.Visibility = Visibility.Hidden;
		confirm_textbox.Visibility = Visibility.Hidden;
		confirm_textbox.Text = "";
		button_yes.IsEnabled = true;
		button_yes.Click -= YesButtonClickHandle2;
		button_yes.Click -= YesButtonClickHandle3;
		button_yes.Click -= YesButtonClickHandle1;
		button_yes.Click += YesButtonClickHandle1;
		button_yes.Foreground = Brushes.White;
		button_yes.Content = "Yes";
		button_no.Visibility = Visibility.Visible;
	}

	public void YesButtonClickHandle1(object sender, RoutedEventArgs e)
	{
		header_textbox.Text = "Are You Sure?";
		message_text.Text = "Clicking Yes again will mark ALL known kiosk inventory as removed. This action is not reversible.\n\nTo continue, enter 'close' into the textbox below.";
		confirm_textbox.Visibility = Visibility.Visible;
		confirm_textbox.IsEnabled = true;
		confirm_textbox.TextAlignment = TextAlignment.Left;
		confirm_textbox.Focus();
		button_yes.IsEnabled = false;
		button_yes.Foreground = Brushes.Gray;
		button_yes.Click -= YesButtonClickHandle1;
		button_yes.Click += YesButtonClickHandle2;
	}

	public void YesButtonClickHandle2(object sender, RoutedEventArgs e)
	{
		if (confirm_textbox.Text.Trim().ToUpper() == "CLOSE")
		{
			DispatcherTimer timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1.0)
			};
			timer.Start();
			int tickcounter = 0;
			timer.Tick += delegate
			{
				if (tickcounter == 0)
				{
					confirm_textbox.Visibility = Visibility.Hidden;
					Spinner.Visibility = Visibility.Visible;
					header_textbox.Text = "Updating Inventory...";
					message_text.Visibility = Visibility.Hidden;
					button_yes.Visibility = Visibility.Hidden;
					button_no.Visibility = Visibility.Hidden;
					Spinner.IsAnimated = true;
					tickcounter++;
				}
				else
				{
					timer.Stop();
					UpdateInventory();
				}
			};
		}
		else
		{
			MessageBox.Show("word 'close' is the expected value!", "Redbox Kiosk Engine", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
	}

	public void YesButtonClickHandle3(object sender, RoutedEventArgs e)
	{
		ResetCloseKioskView();
		base.Visibility = Visibility.Hidden;
	}

	private void UpdateInventory()
	{
		try
		{
			HardwareStatusChangeHandler statusChangeHandler = ProcessUpdateAfterSoftSync;
			ErrorList errorList = null;
			errorList = HardwareSoftSync(null, statusChangeHandler);
			if (errorList != null && errorList.Count() > 0)
			{
				ProcessFunctions.LogErrorsFromList(errorList);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected exception in UpdateInventory function, Close Kiosk Processing. ", ex.Message);
			MessageBox.Show("Unexpected error updating inventory: " + ex.Message, "Error");
		}
	}

	private void ProcessUpdateAfterSoftSync(string jobid, string status)
	{
		RemoteServiceCallback callBackDelegate = ResultSetter;
		if (!(status.ToUpper() == "COMPLETED"))
		{
			return;
		}
		List<IHardwareResult> list = ProcessFunctions.HardwareGetProgramResults(jobid);
		if (list != null)
		{
			List<ProcessFunctions.Item> list2 = (from item in list
				where !string.IsNullOrEmpty(item.ItemID) && item.ItemID.ToLower() != "unknown" && item.ItemID.ToLower() != "redbox" && item.ItemID.ToLower() != "empty" && item.ItemID.ToLower() != "undefined" && item.Deck != 0 && item.Slot != 0
				select new ProcessFunctions.Item
				{
					barcode = item.ItemID,
					deck = item.Deck,
					slot = item.Slot,
					status = "Removed"
				}).ToList();
			ProcessFunctions.kioskItemsCount = list2.Count();
			SnapshotImmediate(ProcessFunctions.kioskId.ToString(), list2, callBackDelegate);
		}
	}

	private void SnapshotImmediate(string storeNumber, List<ProcessFunctions.Item> items, RemoteServiceCallback callBackDelegate)
	{
		try
		{
			List<IInventoryItem> list = new List<IInventoryItem>();
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			if (service == null)
			{
				LogHelper.Instance.Log("Unexpected exception in SnapshotImmidiate, Unable to connect to Inventory Service IInventoryService");
				return;
			}
			ProcessFunctions.InnerSnapshot(storeNumber, items, list);
			service.Snapshot(storeNumber, list.AsReadOnly(), callBackDelegate);
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected exception in SnapshotImmidiate, Close Kiosk Processing. ", ex.Message);
		}
	}

	private ErrorList HardwareSoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		string jobId;
		return ServiceLocator.Instance.GetService<IHardwareService>()?.SoftSync(eventHandler, statusChangeHandler, out jobId);
	}

	private void ResultSetter(IRemoteServiceResult result)
	{
		HardwareImmediateExecutionHandler eventHandler = ProcessUpdateAfterHardwareExec;
		if (result.Success)
		{
			HardwareExecuteImmediate("INVENTORY CLEARALL", eventHandler, clearStack: false);
			ProcessFunctions.DeleteFileFromRelativePath("data\\cache.data");
			ProcessFunctions.DeleteFileFromRelativePath("data\\local.data");
			header_textbox.Text = "Inventory Removed";
			message_text.Visibility = Visibility.Visible;
			message_text.Text = "Inventory successfully marked as removed for this kiosk.\n\n" + ProcessFunctions.kioskItemsCount + " Inventory Record(s) Updated.\n\nYou may continue the process of closing the kiosk.";
			string userName = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser.CheckNotAvailable();
			ProcessFunctions.InnerSendAuditEvent(ProcessFunctions.kioskId.ToString(), "CP_CloseKiosk", userName, "CP Bundle", "Marked " + ProcessFunctions.kioskItemsCount + " barcode(s) as removed.");
			button_yes.Click -= YesButtonClickHandle2;
			button_yes.Click += YesButtonClickHandle3;
			Spinner.IsAnimated = false;
			Spinner.Visibility = Visibility.Hidden;
			button_yes.Content = "Continue";
			button_yes.Visibility = Visibility.Visible;
			return;
		}
		confirm_textbox.Text = "";
		header_textbox.Text = "Error";
		message_text.Visibility = Visibility.Visible;
		message_text.Text = "An error occurred while trying to update the inventory.\n\nWould you like to try again ?";
		button_no.Visibility = Visibility.Visible;
		button_yes.Visibility = Visibility.Visible;
		button_yes.IsEnabled = true;
		button_yes.Content = "Yes";
		button_yes.Foreground = Brushes.White;
		Spinner.IsAnimated = false;
		Spinner.Visibility = Visibility.Hidden;
		button_yes.Click -= YesButtonClickHandle2;
		button_yes.Click += YesButtonClickHandle1;
		if (result.Errors != null && result.Errors.Count > 0)
		{
			ProcessFunctions.LogErrorsFromList(result.Errors);
		}
	}

	public ErrorList HardwareExecuteImmediate(string command, HardwareImmediateExecutionHandler eventHandler, bool clearStack)
	{
		return ServiceLocator.Instance.GetService<IHardwareService>()?.ExecuteImmediate(command, eventHandler, clearStack, null);
	}

	public void ProcessUpdateAfterHardwareExec(string command, bool success, object stack, object errors)
	{
		if (errors != null)
		{
			ProcessFunctions.LogErrorsFromList((ErrorList)errors);
		}
	}

	private void ConfirmTextBoxTextChanged(object sender, TextChangedEventArgs e)
	{
		if (confirm_textbox.Text.Length >= 5 && confirm_textbox.Text.Trim().ToUpper() == "CLOSE")
		{
			button_yes.IsEnabled = true;
			button_yes.Foreground = Brushes.White;
		}
		else
		{
			button_yes.IsEnabled = false;
			button_yes.Foreground = Brushes.Gray;
		}
	}
}
