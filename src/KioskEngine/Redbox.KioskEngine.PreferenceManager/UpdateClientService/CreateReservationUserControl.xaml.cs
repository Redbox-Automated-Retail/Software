using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Newtonsoft.Json;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.KioskEngine.PreferenceManager.Controls;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

[PreferencePage(Name = "Create Reservation", Path = "Update Client Service/Reservations/Create Reservation", TreeOrder = 240)]
public partial class CreateReservationUserControl : UserControl, Redbox.KioskEngine.PreferenceManager.Preferences.IPreferencePageHost, IComponentConnector
{
	private StoreInfo _storeInfo;

	private ReservationRequest _reservationRequest;

	private bool _updatingTotals;

	public CreateReservationUserControl()
	{
		InitializeComponent();
	}

	public async Task<bool> SaveValues(Redbox.KioskEngine.PreferenceManager.Preferences.IPreferencePage preferencePage)
	{
		return await Task.Run(() => true);
	}

	public void LoadValues(Redbox.KioskEngine.PreferenceManager.Preferences.IPreferencePage preferencePage)
	{
		PreferencePageHeaderUserControl.SetStatusText("Loading...");
		InitializeReservationRequest();
		PreferencePageHeaderUserControl.SetStatusText(null);
	}

	private async void InitializeReservationRequest()
	{
		HistoryTitlesTextBox.Text = null;
		First6TextBox.Text = null;
		_storeInfo = await GetStoreInfo();
		ReservationRequest reservationRequest = new ReservationRequest
		{
			ReserveDate = DateTime.Now,
			Titles = new List<ReservationRequestTitle>()
		};
		ReservationRequest reservationRequest2 = reservationRequest;
		reservationRequest2.ReferenceNumber = await GetMaxReferenceNumber();
		reservationRequest.TaxRate = _storeInfo?.RentalTaxRate ?? 0m;
		reservationRequest.DefaultServiceFeeAmount = _storeInfo?.ServiceFee ?? 0m;
		reservationRequest.CardID = "tE+xZHoxAjrOrueI34+SXg4Q+xwchyIQu8vdRMSTz3k=";
		reservationRequest.ReservationCardHash = new ReservationRequestCardHash
		{
			Last4 = "1111"
		};
		_reservationRequest = reservationRequest;
		First6TextBox.Text = "411111";
		TaxRateTextBox.Text = _reservationRequest.TaxRate.ToString();
		ActualServiceFeeAmountTextBox.Text = _reservationRequest.ActualServiceFeeAmount.ToString();
		LabelServiceFeeTaxRate.Content = _storeInfo?.ServiceFeeTaxRate ?? 0m;
		base.DataContext = null;
		base.DataContext = _reservationRequest;
	}

	private async Task<StoreInfo> GetStoreInfo()
	{
		StoreInfo result = null;
		APIResponse<GetStoreInfoResponse> aPIResponse = await KioskEngineApiEngineCoreClient.GetStoreInfo();
		if (aPIResponse != null && aPIResponse.IsSuccessStatusCode)
		{
			result = aPIResponse.Response.Content;
		}
		return result;
	}

	private async Task<long> GetMaxReferenceNumber()
	{
		long result = 123456L;
		APIResponse<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.GetReservationsResponse> aPIResponse = await KioskEngineApiReservationClient.GetReservations();
		if (aPIResponse != null && aPIResponse.IsSuccessStatusCode)
		{
			foreach (Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.Reservation item in aPIResponse.Response.Content)
			{
				if (long.TryParse(item.ReferenceNumber, out var result2) && result2 >= result)
				{
					result = result2 + 1;
				}
			}
		}
		return result;
	}

	public static string GenerateHash(string sourcString)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		string result = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(sourcString))));
		sHA256Managed.Clear();
		sHA256Managed.Dispose();
		return result;
	}

	private async void CreateReservationButton_Click(object sender, RoutedEventArgs e)
	{
		PreferencePageHeaderUserControl.SetStatusText("Creating Reservation...");
		try
		{
			if (string.IsNullOrWhiteSpace(First6TextBox.Text) || string.IsNullOrWhiteSpace(_reservationRequest.ReservationCardHash.Last4))
			{
				_reservationRequest.ReservationCardHash = null;
			}
			else
			{
				_reservationRequest.ReservationCardHash.CardBinHash = GenerateHash(First6TextBox.Text);
			}
			try
			{
				List<string> list = HistoryTitlesTextBox.Text?.Split(',').ToList();
				if (list != null && list.Count > 0)
				{
					_reservationRequest.HistoryTitleIDs = new List<int>();
					foreach (string item in list)
					{
						if (int.TryParse(item.Trim(), out var result))
						{
							_reservationRequest.HistoryTitleIDs.Add(result);
						}
					}
				}
			}
			catch
			{
			}
			APIResponse<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.CreateReservationResponse> aPIResponse = await KioskEngineApiReservationClient.CreateReservation(_reservationRequest);
			PreferencePageHeaderUserControl.SetStatusText(null);
			string text = $"Reservation Created with reference # {_reservationRequest?.ReferenceNumber}.";
			if (!aPIResponse.IsSuccessStatusCode)
			{
				string text2 = null;
				if (aPIResponse.Errors != null && aPIResponse.Errors.Count > 0)
				{
					text2 = "Errors: " + string.Join(", ", aPIResponse.Errors?.Select((Redbox.Rental.Services.App.Error x) => "Code: " + x.Code + ", Message: " + x.Message).ToArray());
				}
				text = ((aPIResponse != null && aPIResponse.IsSuccessStatusCode) ? string.Empty : $"Creation failed. {System.Environment.NewLine}Status code: {(int)aPIResponse.StatusCode} {aPIResponse.StatusCode} {text2}");
				try
				{
					aPIResponse.Response = JsonConvert.DeserializeObject<Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation.CreateReservationResponse>(aPIResponse.ResponseContent);
				}
				catch
				{
					text = text + System.Environment.NewLine + "Unable to deserialize reponse content CreateReservationResponse.";
				}
				if (aPIResponse?.Response?.Errors != null)
				{
					text2 = null;
					if (aPIResponse.Response.Errors.Count > 0)
					{
						text2 = System.Environment.NewLine + string.Join(", ", aPIResponse.Response.Errors?.Select((ResponseError x) => "Code: " + x.Code + ", Message: " + x.Message).ToArray());
					}
					text += text2;
				}
			}
			SimpleMessageWindow.Show(ControlUtilities.GetParentWindow(this), "Reservation Creation Result", text);
			if (aPIResponse.IsSuccessStatusCode)
			{
				LoadValues(null);
			}
			else if (_reservationRequest.ReservationCardHash == null)
			{
				_reservationRequest.ReservationCardHash = new ReservationRequestCardHash();
			}
		}
		finally
		{
			PreferencePageHeaderUserControl.SetStatusText(null);
		}
	}

	private void ComputeTotals()
	{
		if (_updatingTotals)
		{
			return;
		}
		_updatingTotals = true;
		try
		{
			_reservationRequest.SubTotal = _reservationRequest.Titles?.Sum((ReservationRequestTitle x) => x.Price) ?? 0m;
			decimal num = _reservationRequest.Titles?.Sum((ReservationRequestTitle x) => x.Discount) ?? 0m;
			_reservationRequest.DiscountedSubTotal = _reservationRequest.SubTotal - num;
			decimal tax = _reservationRequest.TaxRate / 100m * _reservationRequest.DiscountedSubTotal;
			_reservationRequest.Tax = tax;
			decimal num2 = (_storeInfo?.ServiceFeeTaxRate ?? 0m) / 100m * _reservationRequest.ActualServiceFeeAmount;
			_reservationRequest.GrandTotal = _reservationRequest.DiscountedSubTotal + _reservationRequest.Tax + _reservationRequest.ActualServiceFeeAmount + num2;
			base.DataContext = null;
			base.DataContext = _reservationRequest;
		}
		finally
		{
			_updatingTotals = false;
		}
	}

	private async void AddTitleButton_Click(object sender, RoutedEventArgs e)
	{
		APIResponse<GetReservableTitlesResponse> aPIResponse = await KioskEngineApiReservationClient.GetReservableTitles();
		List<DisplayReservableTitle> list = new List<DisplayReservableTitle>();
		if (aPIResponse?.Response?.Content != null)
		{
			foreach (ReservableTitle item in aPIResponse.Response.Content)
			{
				list.Add(new DisplayReservableTitle
				{
					ReservableTitle = item
				});
			}
		}
		ReservationRequestTitle reservationRequestTitle = new ReservationRequestTitle
		{
			ItemID = _reservationRequest.Titles.Count + 1,
			ReservationType = ReservationTitleType.Rental
		};
		AddReservationTitleWindow addReservationTitleWindow = new AddReservationTitleWindow
		{
			DisplayReservableTitles = list,
			Owner = GetParentWindow(),
			ReservationRequestTitle = reservationRequestTitle
		};
		if (addReservationTitleWindow.ShowDialog() == true)
		{
			_reservationRequest.Titles.Add(addReservationTitleWindow.ReservationRequestTitle);
			ComputeTotals();
		}
	}

	private Window GetParentWindow()
	{
		Window window = null;
		FrameworkElement frameworkElement = this;
		while (window == null && frameworkElement?.Parent != null)
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
			if (frameworkElement is Window)
			{
				window = frameworkElement as Window;
			}
		}
		return window;
	}

	private void TaxRateTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (_reservationRequest != null && decimal.TryParse(TaxRateTextBox.Text, out var result))
		{
			_reservationRequest.TaxRate = result;
			ComputeTotals();
		}
	}

	private void ActualServiceFeeAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (_reservationRequest != null && decimal.TryParse(ActualServiceFeeAmountTextBox.Text, out var result))
		{
			_reservationRequest.ActualServiceFeeAmount = result;
			ComputeTotals();
		}
	}
}
