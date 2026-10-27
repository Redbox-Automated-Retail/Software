using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.ControlPanel.UI.Views;

public partial class ConfigureKioskViewUserControl : UserControl, IComponentConnector
{
	private List<IState> _stateSource;

	private List<IBanner> _bannerSource;

	private List<IStore> _storeSource;

	private int _selectedStateInd;

	private int _selectedStateId;

	private int _selectedBannerId;

	private int _selectedStoreId;

	private string _selectedBannerName;

	private string _marketName;

	private string _kaseyaMarketName;

	private string _dueTime;

	public ConfigureKioskViewUserControl()
	{
		InitializeComponent();
		ProcessFunctions.GetPendingStates();
		_stateSource = ProcessFunctions.tableStates;
		if (_stateSource.Count() == 0)
		{
			lblNotfound.Visibility = Visibility.Visible;
			listvStates.Visibility = Visibility.Hidden;
		}
		listvStates.ItemsSource = _stateSource;
		previous_button.Visibility = Visibility.Hidden;
	}

	public void ResetConfigureKiosk()
	{
		_stateSource = ProcessFunctions.tableStates;
		if (_stateSource.Count() == 0)
		{
			title_area.Content = "Error downloading data. Please Check Network Connection.";
			scrol1.Visibility = Visibility.Hidden;
			scrol2.Visibility = Visibility.Hidden;
			next_button.IsEnabled = false;
			next_button.Foreground = Brushes.Gray;
			cancel_button.Visibility = Visibility.Hidden;
			backGroundTop.Visibility = Visibility.Visible;
			Col0Caption.Visibility = Visibility.Hidden;
			Col1Caption.Visibility = Visibility.Hidden;
			Col2Caption.Visibility = Visibility.Hidden;
			Col3Caption.Visibility = Visibility.Hidden;
			Col4Caption.Visibility = Visibility.Hidden;
			listvStates.Visibility = Visibility.Hidden;
			Spinner.Visibility = Visibility.Hidden;
			lblNotfound.Visibility = Visibility.Hidden;
			return;
		}
		listvStates.ItemsSource = _stateSource;
		listvStates.SelectedIndex = -1;
		title_area.Content = "Select State";
		Col0Caption.Content = "State Id";
		Col2Caption.Content = "Name";
		Col0Caption.Visibility = Visibility.Visible;
		Col1Caption.Visibility = Visibility.Hidden;
		Col2Caption.Visibility = Visibility.Visible;
		Col3Caption.Visibility = Visibility.Hidden;
		Col4Caption.Visibility = Visibility.Hidden;
		listvStates.Visibility = Visibility.Visible;
		lblNotfound.Visibility = Visibility.Hidden;
		scrol1.Visibility = Visibility.Visible;
		scrol2.Visibility = Visibility.Hidden;
		previous_button.Visibility = Visibility.Hidden;
		previous_button.Click -= previousButtonClick2;
		previous_button.Click -= previousButtonClick3;
		previous_button.Click -= previousButtonClick1;
		next_button.Click -= nextButtonClick2;
		next_button.Click -= nextButtonClick1;
		next_button.Click -= nextButtonClick3;
		next_button.Click -= FinishButtonClick;
		next_button.Click += nextButtonClick1;
		lblConfirmState.Visibility = Visibility.Hidden;
		lblConfirmBanner.Visibility = Visibility.Hidden;
		lblConfirmStore.Visibility = Visibility.Hidden;
		backGroundTop.Visibility = Visibility.Visible;
		cancel_button.Visibility = Visibility.Hidden;
		_selectedStateId = -1;
		_selectedBannerId = -1;
		_selectedBannerName = string.Empty;
		_kaseyaMarketName = string.Empty;
		_marketName = string.Empty;
		_dueTime = string.Empty;
		next_button.Content = "Continue";
		_storeSource = null;
		_bannerSource = null;
		Spinner.Visibility = Visibility.Hidden;
		_selectedStoreId = -1;
		_selectedStateInd = -1;
		next_button.IsEnabled = false;
		next_button.Foreground = Brushes.Gray;
	}

	public void GetPendingBannersResult(IRemoteServiceResult result)
	{
		_bannerSource = (List<IBanner>)result.Properties["banners"];
		_bannerSource = _bannerSource.OrderBy((IBanner x) => x.Name).ToList();
		listvStates.ItemsSource = _bannerSource;
		previous_button.Visibility = Visibility.Visible;
		Spinner.Visibility = Visibility.Hidden;
	}

	public void GetOpsMarketResult(IRemoteServiceResult result)
	{
		try
		{
			string text = "C:\\Program Files\\Redbox\\Reds\\Installers\\" + result.Properties["kaseya_server"]?.ToString() + "Install.exe";
			LogHelper.Instance.Log("Configuring Kiosk, Kaseya Path: " + text);
			string arguments = "/s /w /r /v /g=redbox." + result.Properties["ops_market"]?.ToString() + " /m=redbox-" + _selectedStoreId;
			ProcessFunctions.StartProcessWaitForExit(text, arguments, 20000);
			Spinner.Visibility = Visibility.Hidden;
			if (MessageBox.Show("Do you want to reboot the kiosk now?", "Redbox Control Panel", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				next_button.Click -= FinishButtonClick;
				ProcessFunctions.LaunchProcess("shutdown.exe", "-r -t 0", hiddenProcess: false);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected exception in Configure Kiosk, Fisnish Button Processing. ", ex.Message);
		}
	}

	public void GetStoresResult(IRemoteServiceResult result)
	{
		_storeSource = (List<IStore>)result.Properties["stores"];
		_storeSource = _storeSource.OrderBy((IStore x) => x.Id).ToList();
		scrol2.Visibility = Visibility.Visible;
		scrol1.Visibility = Visibility.Hidden;
		listvStores.ItemsSource = _storeSource;
		Spinner.Visibility = Visibility.Hidden;
		Col0Caption.Content = "Store Id";
		Col1Caption.Visibility = Visibility.Visible;
		Col2Caption.Visibility = Visibility.Visible;
		Col2Caption.Content = "City";
		Col3Caption.Visibility = Visibility.Visible;
		Col4Caption.Visibility = Visibility.Visible;
		title_area.Content = "Select State > Select Banner > Select Store";
	}

	private void closeButtonClick(object sender, RoutedEventArgs e)
	{
		base.Visibility = Visibility.Hidden;
	}

	private void StateBannersSelectedRowHandle(object sender, SelectionChangedEventArgs e)
	{
		int selectedIndex = listvStates.SelectedIndex;
		next_button.IsEnabled = false;
		next_button.Foreground = Brushes.Gray;
		if (selectedIndex >= 0)
		{
			next_button.IsEnabled = true;
			next_button.Foreground = Brushes.White;
			if (listvStates.ItemsSource == _stateSource)
			{
				_selectedStateInd = selectedIndex;
				_selectedStateId = _stateSource[selectedIndex].Id;
			}
			else if (listvStates.ItemsSource == _bannerSource)
			{
				_selectedBannerId = _bannerSource[selectedIndex].Id;
				_selectedBannerName = _bannerSource[selectedIndex].Name;
			}
		}
	}

	private void StoresSelectedRowHandle(object sender, SelectionChangedEventArgs e)
	{
		int selectedIndex = listvStores.SelectedIndex;
		next_button.IsEnabled = false;
		next_button.Foreground = Brushes.Gray;
		if (selectedIndex >= 0)
		{
			next_button.IsEnabled = true;
			next_button.Foreground = Brushes.White;
			lblConfirmState.Content = "State: " + _storeSource[selectedIndex].State;
			lblConfirmBanner.Content = "Banner: " + _selectedBannerName;
			lblConfirmStore.Content = "Store: " + _storeSource[selectedIndex].Id;
			_selectedStoreId = _storeSource[selectedIndex].Id;
			_marketName = _storeSource[selectedIndex].MarketName;
			_kaseyaMarketName = _storeSource[selectedIndex].KaseyaMarketName;
			_dueTime = _storeSource[selectedIndex].DueTime;
		}
	}

	private void nextButtonClick1(object sender, RoutedEventArgs e)
	{
		if (_selectedStateId < 0)
		{
			MessageBox.Show("Please Select the State!", "Configure Kiosk", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		Spinner.Visibility = Visibility.Visible;
		Spinner.IsAnimated = true;
		RemoteServiceCallback resultCallback = GetPendingBannersResult;
		ProcessFunctions.GetPendingBanners(_selectedStateId, resultCallback);
		Col0Caption.Content = "Banner Id";
		title_area.Content = "Select State > Select Banner";
		previous_button.Visibility = Visibility.Visible;
		previous_button.Click += previousButtonClick1;
		next_button.Click -= nextButtonClick1;
		next_button.Click += nextButtonClick2;
	}

	private void nextButtonClick2(object sender, RoutedEventArgs e)
	{
		if (_selectedBannerId < 0)
		{
			MessageBox.Show("Please Select the Banner!", "Configure Kiosk", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		Spinner.Visibility = Visibility.Visible;
		Spinner.IsAnimated = true;
		previous_button.Click -= previousButtonClick1;
		previous_button.Click += previousButtonClick2;
		next_button.Click -= nextButtonClick1;
		next_button.Click -= nextButtonClick2;
		next_button.Click += nextButtonClick3;
		RemoteServiceCallback resultCallback = GetStoresResult;
		ProcessFunctions.GetPendingStores(_selectedBannerId, _selectedStateId, resultCallback);
	}

	private void nextButtonClick3(object sender, RoutedEventArgs e)
	{
		next_button.Click -= nextButtonClick2;
		if (listvStores.SelectedIndex < 0)
		{
			MessageBox.Show("Please Select the Store!", "Configure Kiosk", MessageBoxButton.OK);
			return;
		}
		title_area.Content = "Select State > Select Banner > Select Store > Confirm";
		scrol2.Visibility = Visibility.Hidden;
		scrol1.Visibility = Visibility.Hidden;
		cancel_button.Visibility = Visibility.Visible;
		lblConfirmState.Visibility = Visibility.Visible;
		lblConfirmBanner.Visibility = Visibility.Visible;
		lblConfirmStore.Visibility = Visibility.Visible;
		cancel_button.Visibility = Visibility.Visible;
		next_button.Content = "Finish";
		previous_button.Click -= previousButtonClick2;
		previous_button.Click += previousButtonClick3;
		backGroundTop.Visibility = Visibility.Hidden;
		Col1Caption.Visibility = Visibility.Hidden;
		Col2Caption.Visibility = Visibility.Hidden;
		Col3Caption.Visibility = Visibility.Hidden;
		Col4Caption.Visibility = Visibility.Hidden;
		Col0Caption.Visibility = Visibility.Hidden;
		next_button.Click -= nextButtonClick3;
		next_button.Click += FinishButtonClick;
	}

	private void previousButtonClick1(object sender, RoutedEventArgs e)
	{
		title_area.Content = "Select State";
		previous_button.Visibility = Visibility.Hidden;
		listvStates.ItemsSource = _stateSource;
		listvStates.SelectedIndex = _selectedStateInd;
		next_button.Click += nextButtonClick1;
		next_button.Click -= nextButtonClick2;
		Col0Caption.Content = "State Id";
		Col2Caption.Content = "Name";
		Col3Caption.Visibility = Visibility.Hidden;
		Col4Caption.Visibility = Visibility.Hidden;
		Col1Caption.Visibility = Visibility.Hidden;
		previous_button.Click -= previousButtonClick1;
	}

	private void previousButtonClick2(object sender, RoutedEventArgs e)
	{
		title_area.Content = "Select State > Select Banner";
		Col0Caption.Content = "Banner Id";
		Col2Caption.Content = "Name";
		Col3Caption.Visibility = Visibility.Hidden;
		Col4Caption.Visibility = Visibility.Hidden;
		Col1Caption.Visibility = Visibility.Hidden;
		lblConfirmState.Visibility = Visibility.Hidden;
		lblConfirmBanner.Visibility = Visibility.Hidden;
		lblConfirmStore.Visibility = Visibility.Hidden;
		scrol2.Visibility = Visibility.Hidden;
		scrol1.Visibility = Visibility.Visible;
		listvStates.ItemsSource = _bannerSource;
		previous_button.Click += previousButtonClick1;
		previous_button.Click -= previousButtonClick2;
		next_button.Click += nextButtonClick2;
		next_button.Click -= nextButtonClick3;
	}

	private void previousButtonClick3(object sender, RoutedEventArgs e)
	{
		title_area.Content = "Select State > Select Banner > Select Store";
		Col0Caption.Content = "Store Id";
		previous_button.Visibility = Visibility.Visible;
		scrol2.Visibility = Visibility.Visible;
		scrol1.Visibility = Visibility.Hidden;
		previous_button.Click += previousButtonClick2;
		previous_button.Click -= previousButtonClick3;
		previous_button.Click -= previousButtonClick1;
		next_button.Click += nextButtonClick3;
		next_button.Click -= FinishButtonClick;
		next_button.Content = "Continue";
		cancel_button.Visibility = Visibility.Hidden;
		lblConfirmState.Visibility = Visibility.Hidden;
		lblConfirmBanner.Visibility = Visibility.Hidden;
		lblConfirmStore.Visibility = Visibility.Hidden;
		Col1Caption.Visibility = Visibility.Visible;
		Col2Caption.Visibility = Visibility.Visible;
		Col3Caption.Visibility = Visibility.Visible;
		Col4Caption.Visibility = Visibility.Visible;
		Col0Caption.Visibility = Visibility.Visible;
		backGroundTop.Visibility = Visibility.Visible;
	}

	private void FinishButtonClick(object sender, RoutedEventArgs e)
	{
		next_button.Click -= nextButtonClick3;
		string eventType = string.Empty;
		Spinner.Visibility = Visibility.Visible;
		Spinner.IsAnimated = true;
		try
		{
			string kaseyaIniDPath = KaseyaMethods.GetKaseyaIniDPath();
			string newvalue = "Redbox-" + _selectedStoreId + _kaseyaMarketName;
			ProcessFunctions.ReplaceInFile(kaseyaIniDPath, "User_Name\\s+(?<KaseyaId>[\\w|\\-|\\.]+)", "KaseyaId", newvalue);
			ProcessFunctions.SetMachineSettingValue("Store", "ID", _selectedStoreId.ToString());
			LogHelper.Instance.Log(string.Format("Configuring Kiosk, {0}", "prior_kiosk_id: " + ProcessFunctions.kioskId));
			LogHelper.Instance.Log(string.Format("Configuring Kiosk, {0}", "new_kiosk_id: " + _selectedStoreId));
			if (ProcessFunctions.kioskId == 1 || ProcessFunctions.kioskId != _selectedStoreId)
			{
				eventType = "open_kiosk";
			}
			else if (ProcessFunctions.kioskId == _selectedStoreId)
			{
				eventType = "configure_kiosk";
			}
			string userName = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser.CheckNotAvailable();
			ProcessFunctions.SendAuditEvent(_selectedStoreId.ToString(), eventType, userName, "CP Bundle", "Activate Control Panel function.");
			ProcessFunctions.RenameComputer("redbox-" + _selectedStoreId);
			ProcessFunctions.SetMachineSettingValue("Store", "Banner", _selectedBannerName);
			ProcessFunctions.SetMachineSettingValue("Store", "Market", _marketName);
			ProcessFunctions.SetMachineSettingValue("Store", "Duetime", _dueTime);
			RemoteServiceCallback completeCallback = GetOpsMarketResult;
			ProcessFunctions.GetOpsMarket(_selectedStoreId.ToString(), completeCallback);
		}
		catch (Exception ex)
		{
			Spinner.Visibility = Visibility.Hidden;
			LogHelper.Instance.Log("Unexpected exception in Configure Kiosk, Fisnish Button Processing. ", ex.Message);
			MessageBox.Show("Unexpected exception in Configure Kiosk, Finish Button Click. Please review the log file", "Configure Kiosk", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}
}
