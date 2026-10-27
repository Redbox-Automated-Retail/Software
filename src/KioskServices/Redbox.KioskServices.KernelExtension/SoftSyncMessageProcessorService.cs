using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Local;

namespace Redbox.KioskServices.KernelExtension;

internal class SoftSyncMessageProcessorService : BaseQueueMessageProcessorService
{
	private class ClientDataMessage
	{
		public List<RemovedQueueItem> RemovedQueueItems { get; set; }
	}

	public override string Name => "SoftSync";

	public override bool IsDefault => false;

	public override List<string> SupportedMessageTypes => new List<string> { Name };

	public override BeforeMessageProcessingDelegate BeforeMessageProcessing => OnBeforeMessageProcessing;

	public override MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully => OnMessageProcessedSuccessfully;

	public override ProcessMessageDelegate ProcessMessage => OnProcessMessage;

	private static void OnBeforeMessageProcessing(IMessage message, out object clientData)
	{
		ClientDataMessage clientDataMessage = ServiceLocator.Instance.GetService<IQueueService>()?.GetObjectFromMessageData<ClientDataMessage>(message);
		if (clientDataMessage != null && clientDataMessage.RemovedQueueItems != null && clientDataMessage.RemovedQueueItems.Any())
		{
			clientData = clientDataMessage.RemovedQueueItems;
		}
		else
		{
			clientData = null;
		}
	}

	private static bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		response = new Dictionary<string, object>();
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null || service.IsServerBusy || ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
		{
			return false;
		}
		try
		{
			string jsonRequest = Encoding.ASCII.GetString(message.Data);
			response = KioskServicesProxy.Instance.ExecuteMessage(jsonRequest);
			return response.ContainsKey("Success") && (bool)response["Success"];
		}
		catch
		{
			return false;
		}
	}

	private static async Task Delete(RemovedQueueItem item)
	{
		await Task.Run(delegate
		{
			LogHelper.Instance.Log($"SoftSyncMessageProcessorService.Delete - deleting removedqueue record for barcode: {item.Barcode} with hash: {item.HashId}");
			try
			{
				ServiceLocator.Instance.GetService<IDataService>().DeleteRemovedQueueItem(item);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"SoftSyncMessageProcessorService.Delete - exception trying to delete removedqueue record for barcode: {item.Barcode}!", e);
				string storeNumber = (ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString();
				ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(storeNumber, "TitleEmpty", "RemovedQueueDeleteFail", $"Unable to delete RemovedQueue record for hash: {item.HashId}, barcode: {item.Barcode}", DateTime.Now, null);
			}
		});
	}

	private static async void OnMessageProcessedSuccessfully(IMessage message, IDictionary<string, object> result, object clientData)
	{
		if (clientData != null && clientData is List<RemovedQueueItem> list && list.Any())
		{
			List<Task> processingTasks = new List<Task>();
			list.ForEach(delegate(RemovedQueueItem i)
			{
				processingTasks.Add(Delete(i));
			});
			await Task.WhenAll(processingTasks);
		}
	}
}
