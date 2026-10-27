using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services;

public class InventoryService : IInventoryService
{
	private IInventoryData _inventoryData;

	private int _random;

	private int m_working;

	public DateTime? InventoryActionModificationTime => _inventoryData.GetInventoryActionModificationTime();

	public DateTime? RapidStatusModificationTime => _inventoryData.GetRapidStatusModificationTime();

	public InventoryService(IInventoryData inventoryData)
	{
		_inventoryData = inventoryData;
		Random random = new Random();
		_random = random.Next(0, 30);
	}

	public Inventory GetInventory(string barcode)
	{
		try
		{
			return _inventoryData.GetInventory(barcode);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetInventory - An unhandled exception occurred.", e);
			return null;
		}
	}

	public Dictionary<string, Inventory> GetInventories(List<string> barcodes)
	{
		try
		{
			return _inventoryData.GetInventories(barcodes);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetInventories - An unhandled exception occurred.", e);
			return null;
		}
	}

	public string GetInventoryDataCreatedOn()
	{
		try
		{
			return _inventoryData.GetInventoryDataCreatedOn();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetInventoryDataCreatedOn - An unhandled exception occurred.", e);
			return null;
		}
	}

	public string GetInventoryDataMachineOrigin()
	{
		try
		{
			return _inventoryData.GetInventoryDataMachineOrigin();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetInventoryDataMachineOrigin - An unhandled exception occurred.", e);
			return null;
		}
	}

	public long GetInventoryDataCount()
	{
		try
		{
			return _inventoryData.GetInventoryDataCount();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetInventoryDataCount - An unhandled exception occurred.", e);
			return -1L;
		}
	}

	public Inventory GetRapidStatus(string barcode)
	{
		try
		{
			return _inventoryData.GetRapidStatus(barcode);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRapidStatus - An unhandled exception occurred.", e);
			return null;
		}
	}

	public ProductLookupCatalogModel GetRapidStatus()
	{
		try
		{
			return _inventoryData.GetRapidStatus();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRapidStatus - An unhandled exception occurred.", e);
			return null;
		}
	}

	public List<RebalancesInModel> GetRebalancesIn()
	{
		List<RebalancesInModel> result = new List<RebalancesInModel>();
		try
		{
			InventoryActionFile inventoryActionFile = _inventoryData.GetInventoryActionFile();
			if (inventoryActionFile == null || inventoryActionFile.RebalancesIn == null)
			{
				return result;
			}
			inventoryActionFile.RebalancesIn.ForEach(delegate(RebalancesIn item)
			{
				result.Add(new RebalancesInModel
				{
					TitleId = item.Id,
					Quantity = item.Qty
				});
			});
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRebalancesIn - An unhandled exception occurred.", e);
			return result;
		}
	}

	public int GetFillQuantity()
	{
		int num = 0;
		try
		{
			return _inventoryData.GetInventoryActionFile()?.FillQty ?? num;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetFillQuantity - An unhandled exception occurred.", e);
			return num;
		}
	}

	public void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus)
	{
		try
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - starting");
			_inventoryData.CheckForNewFiles(out var rapidStatusChanged);
			if (rapidStatusChanged)
			{
				newRapidStatus?.Invoke();
			}
			InventoryActionFile inventoryActionFile = _inventoryData.GetInventoryActionFile();
			if (inventoryActionFile == null)
			{
				LogHelper.Instance.Log("InventoryService.CheckForNewFiles - no inventory action completed file exists.");
				return;
			}
			DateTime? lastCompletedDate = _inventoryData.GetLastCompletedDate();
			DateTime dateTime = DateTime.MinValue;
			if (inventoryActionFile.CompletedDate != null)
			{
				dateTime = DateTime.ParseExact(inventoryActionFile.CompletedDate, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			}
			if (lastCompletedDate.HasValue)
			{
				DateTime value = dateTime;
				DateTime? dateTime2 = lastCompletedDate;
				if (!(value != dateTime2))
				{
					return;
				}
			}
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - New completed date found, running thinjob now.");
			_inventoryData.SetLastCompletedDate(dateTime);
			_inventoryData.SetLastRunThinJobDate(DateTime.Now);
			if (inventoryCompletion != null)
			{
				_inventoryData.ReportMerchandizedBarcodes();
				inventoryCompletion();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - An unhandled exception occurred.", e);
		}
		finally
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - finished");
			RunByDeadline(inventoryCompletion);
		}
	}

	private void RunByDeadline(Action inventoryCompletion)
	{
		try
		{
			DateTime dateTime = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.UseMerchOrdersForThinning) ? DateTime.Today.AddHours(3.0).AddMinutes(30 + _random) : (DateTime.Today + Redbox.Rental.Services.Configuration.Configuration.Instance.ThinJobRangeEnd).AddMinutes(-_random));
			DateTime? lastRunThinJobDate = _inventoryData.GetLastRunThinJobDate();
			if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - auto thin job does not run on Saturday or Sunday");
				return;
			}
			LogHelper.Instance.Log("InventoryService.RunByDeadline - Runby: {0} LastRunThinJob {1}", dateTime, lastRunThinJobDate);
			if ((!lastRunThinJobDate.HasValue || !lastRunThinJobDate.Value.Date.Equals(DateTime.Today)) && DateTime.Now >= dateTime)
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - Thinjob has not run by deadine, running now.");
				_inventoryData.SetLastRunThinJobDate(DateTime.Now);
				if (inventoryCompletion != null)
				{
					_inventoryData.ReportMerchandizedBarcodes();
					inventoryCompletion();
				}
			}
			else
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - Thinjob has run by deadine or it has not passed the deadline yet.");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.RunByDeadline - An unhandled exception occurred.", e);
		}
		finally
		{
			LogHelper.Instance.Log("InventoryService.RunByDeadline - finished");
		}
	}

	public void ReportUnmerchandizedBarcodes(List<string> barcodesToReport)
	{
		_inventoryData.ReportUnmerchandizedBarcodes(barcodesToReport);
	}

	public async void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			IReturnResult returnResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ReturnAsync(storeNumber, returnType, barcode, returnTime, deck, slot, failedSecurityRead, fileName);
			if (returnResult == null || !returnResult.Success)
			{
				errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "Non 200 level result returned from the ReturnAsync call..", "") };
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in InventoryService.Return.", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				if (callback != null)
				{
					ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
					if (service != null)
					{
						RemoteServiceResult remoteServiceResult = new RemoteServiceResult
						{
							Success = true
						};
						if (errors != null)
						{
							remoteServiceResult.Errors.AddRange(errors);
							remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
						}
						service.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from InventoryService.Return."
						});
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	public async void Snapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryService.Snapshot - Enqueue soft sync call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			List<RemovedQueueItem> removedItems = ServiceLocator.Instance.GetService<IDataService>().GetRemovedQueueItems();
			LogHelper.Instance.Log("InventoryService.Snapshot - appending the following removed barcodes: " + string.Join(",", removedItems.Select((RemovedQueueItem ri) => ri.Barcode)));
			List<IInventoryItem> list = new List<IInventoryItem>(items);
			list.AddRange(removedItems.ToInventoryItems());
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.SnapshotAsync(storeNumber, list);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryService.Snapshot - KCS call to snapshot failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log("InventoryService.Snapshot - error returned: " + kioskClientServiceResult.Error.Code + ", " + kioskClientServiceResult.Error.Description);
				}
				return;
			}
			LogHelper.Instance.Log("InventoryService.Snapshot - KCS snapshot call successful");
			if (removedItems.Any())
			{
				List<Task> processingTasks = new List<Task>();
				removedItems.ForEach(delegate(RemovedQueueItem i)
				{
					processingTasks.Add(Delete(i));
				});
				await Task.WhenAll(processingTasks);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryService.Snapshot - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
				{
					RemoteServiceResult remoteServiceResult = new RemoteServiceResult
					{
						Success = true
					};
					if (errors != null)
					{
						remoteServiceResult.Errors.AddRange(errors);
						remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
					}
					service.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking callback from InventoryService.Snapshot."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
		static async Task Delete(RemovedQueueItem item)
		{
			await Task.Run(delegate
			{
				LogHelper.Instance.Log("SoftSyncMessageProcessorService.Delete - deleting removedqueue record for barcode: " + item.Barcode + " with hash: " + item.HashId);
				try
				{
					ServiceLocator.Instance.GetService<IDataService>().DeleteRemovedQueueItem(item);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("SoftSyncMessageProcessorService.Delete - exception trying to delete removedqueue record for barcode: " + item.Barcode + "!", e);
					string storeNumber2 = (ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString();
					ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(storeNumber2, "TitleEmpty", "RemovedQueueDeleteFail", "Unable to delete RemovedQueue record for hash: " + item.HashId + ", barcode: " + item.Barcode, DateTime.Now, null);
				}
			});
		}
	}

	public async void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback, long? transactionId = null, long? discId = null, DateTime? returnDate = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string> { { "FraudType", "Paper" } };
		if (discId.HasValue)
		{
			dictionary.Add("DiscId", discId.Value.ToString());
		}
		if (returnDate.HasValue)
		{
			dictionary.Add("ReturnDate", returnDate.Value.ToString("o"));
		}
		if (!DateTime.TryParseExact(readTime, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
		{
			result = DateTime.Now;
		}
		await KioskClientServiceUpdateFraudStatus(storeNumber, barcode, result, completeCallback, dictionary);
	}

	private async Task KioskClientServiceUpdateFraudStatus(string storeNumber, string barcode, DateTime readDate, RemoteServiceCallback callback, Dictionary<string, string> properties = null)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscFraudStatus - Enqueue call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.DiscFraudAsync(storeNumber, barcode, readDate, properties);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscFraudStatus - KCS call to disc action failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscFraudStatus - error returned: " + kioskClientServiceResult.Error.Code + ", " + kioskClientServiceResult.Error.Description);
				}
			}
			else
			{
				LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscFraudStatus - KCS disc action call successful");
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscFraudService - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
				{
					RemoteServiceResult remoteServiceResult = new RemoteServiceResult
					{
						Success = true
					};
					if (errors != null)
					{
						remoteServiceResult.Errors.AddRange(errors);
						remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
					}
					service.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking callback from InventoryService.KioskClientServiceDiscFraudService."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	public void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback completeCallback)
	{
		KioskClientServiceDiscAction(storeNumber, username, jobName, jobTimeStamp, actionList, completeCallback);
	}

	private async void KioskClientServiceDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscAction - Enqueue call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			DateTime jobTimestamp = UnLUATimmestamp(jobTimeStamp);
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.DiscActionAsync(storeNumber, username, jobName, jobTimestamp, actionList);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscAction - KCS call to disc action failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscAction - error returned: " + kioskClientServiceResult.Error.Code + ", " + kioskClientServiceResult.Error.Description);
				}
			}
			else
			{
				LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscAction - KCS disc action call successful");
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryService.KioskClientServiceDiscAction - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
				{
					RemoteServiceResult remoteServiceResult = new RemoteServiceResult
					{
						Success = true
					};
					if (errors != null)
					{
						remoteServiceResult.Errors.AddRange(errors);
						remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
					}
					service.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking callback from InventoryService.KioskClientServiceDiscAction."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	private DateTime UnLUATimmestamp(string source)
	{
		string text = source.Substring(0, 4) + "-" + source.Substring(4, 2) + "-" + source.Substring(6, 2);
		text = text + " " + source.Substring(8, 2) + ":" + source.Substring(10, 2) + ":00";
		if (DateTime.TryParse(text, null, DateTimeStyles.AssumeLocal, out var result))
		{
			return result;
		}
		return DateTime.Now;
	}

	public IInventoryItem CreateInventoryItem(string barcode, int? deck, int? slot, InventoryItemStatus status)
	{
		return new InventoryItem
		{
			Barcode = barcode,
			Deck = deck,
			Slot = slot,
			Status = status
		};
	}
}
