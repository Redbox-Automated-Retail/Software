using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

public class QueueService : IQueueService, IDisposable
{
	private class QueueServicePriority : IQueueServicePriority
	{
		public QueueServicePriorityType PriorityType { get; set; }

		public string Description { get; set; }

		public int MinimumPriorityValue { get; set; }

		public int MaximumPriorityValue { get; set; }

		public TimeSpan? StartTime { get; set; }

		public TimeSpan? EndTime { get; set; }

		public List<IQueueServicePriority> ExcludeTimeRanges => new List<IQueueServicePriority>();

		public bool IsInTimeRange(TimeSpan timeSpan)
		{
			if (StartTime.HasValue || EndTime.HasValue)
			{
				TimeSpan? startTime = StartTime;
				TimeSpan timeSpan2 = timeSpan;
				if (!startTime.HasValue || !(startTime.GetValueOrDefault() <= timeSpan2) || !(EndTime > timeSpan))
				{
					return false;
				}
			}
			return !ExcludeTimeRanges.Any((IQueueServicePriority x) => x.IsInTimeRange(timeSpan));
		}

		public override string ToString()
		{
			return string.Format("{0}: Min priority value: {1}, Max priority value: {2}, Start time: {3}, End time: {4}, Exclude priority time ranges: {5}", PriorityType, MinimumPriorityValue, MaximumPriorityValue, StartTime, EndTime, ExcludeTimeRanges.Select((IQueueServicePriority y) => y.PriorityType.ToString()).Join(", "));
		}
	}

	internal const int DefaultQueueReadPeriod = 60000;

	private TimeSpan _overnightPriorityStartTime = TimeSpan.FromHours(1.0);

	private TimeSpan _overnightPriorityEndTime = TimeSpan.FromHours(5.0);

	private TimeSpan _peakPriorityStartTime = QueueServiceConstants.PeakPriorityStartTimeDefault;

	private TimeSpan _peakPriorityEndTime = QueueServiceConstants.PeakPriorityEndTimeDefault;

	private IQueueServiceData _queueServiceData;

	private readonly List<QueueServicePriority> _queueServicePriorities = new List<QueueServicePriority>();

	private readonly List<IQueueMessageProcessorService> _queueMessageProcessorServices = new List<IQueueMessageProcessorService>();

	private Timer m_queueTimer;

	private volatile bool m_isProcessingMessage;

	private const string EncryptedPrefix = "RedboxEncrypted";

	public static QueueService Instance => Singleton<QueueService>.Instance;

	public int QueueReadPeriod => 60000;

	public void StopQueueWorker()
	{
		if (m_queueTimer != null)
		{
			m_queueTimer.Dispose();
			m_queueTimer = null;
			LogHelper.Instance.Log("Queue worker stopped.");
		}
	}

	public bool IsWorkerStarted()
	{
		return m_queueTimer != null;
	}

	public T GetObjectFromMessageData<T>(IMessage message)
	{
		T result = default(T);
		if (message?.Data != null)
		{
			string json = Encoding.ASCII.GetString(message.Data);
			try
			{
				result = json.ToObject<T>();
				return result;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to convert message.data to {typeof(T)}", e);
			}
		}
		return result;
	}

	public bool RegisterMessageProcessorService(IQueueMessageProcessorService queueMessageProcessorService)
	{
		bool flag = false;
		if (queueMessageProcessorService != null)
		{
			lock (_queueMessageProcessorServices)
			{
				if (_queueMessageProcessorServices.Any((IQueueMessageProcessorService x) => x.Name == queueMessageProcessorService?.Name))
				{
					LogHelper.Instance.Log("Error in RegisterMessageProcessorService.  Duplicate name " + queueMessageProcessorService?.Name);
				}
				else if (queueMessageProcessorService.IsDefault && _queueMessageProcessorServices.Count((IQueueMessageProcessorService x) => x.IsDefault) >= 1)
				{
					LogHelper.Instance.Log("Warning: More than one default Queue Message Processor Service.");
				}
				else
				{
					_queueMessageProcessorServices.Add(queueMessageProcessorService);
					LogHelper.Instance.Log("QueueService registerd Queue Message Processor Service " + queueMessageProcessorService?.Name);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Error in RegisterMessageProcessorService. " + queueMessageProcessorService?.Name + " not registered.");
		}
		return flag;
	}

	public bool UnRegisterMessageProcessorService(IQueueMessageProcessorService queueMessageProcessorService)
	{
		bool result = false;
		lock (_queueMessageProcessorServices)
		{
			if (_queueMessageProcessorServices.Contains(queueMessageProcessorService))
			{
				_queueMessageProcessorServices.Remove(queueMessageProcessorService);
				LogHelper.Instance.Log("QueueService Unregistered Queue Message Processor Service " + queueMessageProcessorService?.Name);
				result = true;
			}
		}
		return result;
	}

	public void StartQueueWorker()
	{
		if (m_queueTimer != null)
		{
			return;
		}
		m_queueTimer = new Timer(delegate
		{
			if (m_isProcessingMessage)
			{
				return;
			}
			try
			{
				m_isProcessingMessage = true;
				ErrorList errorList = Dequeue(delegate(IMessage message)
				{
					bool errored = false;
					bool result = true;
					try
					{
						List<IQueueMessageProcessorService> queueProcessorsForMessageType = GetQueueProcessorsForMessageType(message?.Type);
						if (queueProcessorsForMessageType.Any())
						{
							queueProcessorsForMessageType.ForEach(delegate(IQueueMessageProcessorService q)
							{
								object clientData = null;
								try
								{
									q.BeforeMessageProcessing?.Invoke(message, out clientData);
								}
								catch (Exception e2)
								{
									LogHelper.Instance.Log("An unhandled exception was raised while running BeforeMessageProcessing in QueueService.StartQueueWorker.", e2);
								}
								if (q.ProcessMessage != null)
								{
									IDictionary<string, object> response = null;
									try
									{
										result &= q.ProcessMessage(message, out response);
									}
									catch (Exception e3)
									{
										LogHelper.Instance.Log("An unhandled exception was raised while running ProcessMessage in QueueService.StartQueueWorker.", e3);
										errored = true;
									}
									if (result)
									{
										try
										{
											q.MessageProcessedSuccessfully?.Invoke(message, response, clientData);
										}
										catch (Exception e4)
										{
											LogHelper.Instance.Log("An unhandled exception was raised while running MessageProcessed in QueueService.StartQueueWorker.", e4);
											errored = true;
										}
									}
									try
									{
										q.AfterMessageProcessed?.Invoke(message, clientData);
									}
									catch (Exception e5)
									{
										LogHelper.Instance.Log("An unhandled exception was raised while running AfterMessageProcessed in QueueService.StartQueueWorker.", e5);
									}
								}
							});
						}
						else
						{
							lock (_queueMessageProcessorServices)
							{
								string[] array = _queueMessageProcessorServices.Select((IQueueMessageProcessorService x) => x.Name).ToArray();
								LogHelper.Instance.Log("Error in QueueService. Unable to find QueueMessageProcessorService for message type " + message?.Type + ".  QueueMessageProcessorServices registered are: " + array?.Join(", "));
							}
							errored = true;
						}
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log("Exception in Dequeue predicate processing message type " + message?.Type + "!", e);
						errored = true;
					}
					return !errored && result;
				});
				if (errorList.ContainsError())
				{
					LogHelper.Instance.Log("Unable to dequeue message in Queue Service.StartQueueWorker; errors follow:");
					{
						foreach (Redbox.KioskEngine.ComponentModel.Error item in errorList)
						{
							LogHelper.Instance.Log("...{0}", item);
						}
						return;
					}
				}
			}
			finally
			{
				m_isProcessingMessage = false;
			}
		}, null, QueueReadPeriod, QueueReadPeriod);
		LogHelper.Instance.Log("Queue worker started at read period of: {0}", QueueReadPeriod);
	}

	public void Dispose()
	{
		_queueServiceData?.Dispose();
		if (m_queueTimer != null)
		{
			m_queueTimer.Dispose();
			m_queueTimer = null;
		}
	}

	public ErrorList Clear()
	{
		return _queueServiceData?.Clear() ?? new ErrorList();
	}

	public ErrorList GetDepth(out int count)
	{
		count = 0;
		return _queueServiceData?.GetDepth(out count) ?? new ErrorList();
	}

	public ErrorList Enqueue(byte priority, string type, object message)
	{
		ErrorList obj = _queueServiceData?.Enqueue(priority, type, ToEncryptedMessage(message)) ?? new ErrorList();
		if (obj.ContainsCode("Q999"))
		{
			HandleCorruptDatabase();
		}
		return obj;
	}

	public ErrorList EnqueueKioskEvent(object message)
	{
		return _queueServiceData?.Enqueue(200, "KioskEvents", ToEncryptedMessage(message)) ?? new ErrorList();
	}

	public ErrorList EnqueueOnlyOne(byte priority, string type, object message, out int newMessageId)
	{
		int newMessageId2 = 0;
		ErrorList errorList = _queueServiceData?.DeleteMessageByType(type) ?? new ErrorList();
		if (errorList == null || !errorList.ContainsError())
		{
			errorList = _queueServiceData?.Enqueue(priority, type, ToEncryptedMessage(message), out newMessageId2) ?? new ErrorList();
		}
		newMessageId = newMessageId2;
		if (errorList.ContainsCode("Q999"))
		{
			HandleCorruptDatabase();
		}
		return errorList;
	}

	public ErrorList Update(int id, string message)
	{
		ErrorList obj = _queueServiceData?.Update(id, ToEncryptedMessage(message)) ?? new ErrorList();
		if (obj.ContainsCode("Q999"))
		{
			HandleCorruptDatabase();
		}
		return obj;
	}

	public void ForEachPriority(Action<IQueueServicePriority> action)
	{
		foreach (QueueServicePriority queueServicePriority in _queueServicePriorities)
		{
			action(queueServicePriority);
		}
	}

	public ErrorList ForEach(Action<IMessage> action)
	{
		return _queueServiceData?.ForEach(delegate(IMessage encryptedMessage)
		{
			IMessage obj = new QueueMessage
			{
				ID = encryptedMessage.ID,
				Type = encryptedMessage.Type,
				CreatedOn = encryptedMessage.CreatedOn,
				Priority = encryptedMessage.Priority,
				Data = FromEncryptedMessage(encryptedMessage.Data)
			};
			action(obj);
		}) ?? new ErrorList();
	}

	public ErrorList ForEach(Action<IMessage> action, string type)
	{
		return _queueServiceData?.ForEach(delegate(IMessage encryptedMessage)
		{
			IMessage obj = new QueueMessage
			{
				ID = encryptedMessage.ID,
				Type = encryptedMessage.Type,
				CreatedOn = encryptedMessage.CreatedOn,
				Priority = encryptedMessage.Priority,
				Data = FromEncryptedMessage(encryptedMessage.Data)
			};
			action(obj);
		}, type) ?? new ErrorList();
	}

	public ErrorList Initialize(string path, IQueueServiceData queueServiceData)
	{
		_queueServiceData = queueServiceData;
		GetQueueServiceConfigurationValues();
		QueueServicePriority item = new QueueServicePriority
		{
			PriorityType = QueueServicePriorityType.Peak,
			MinimumPriorityValue = 100,
			MaximumPriorityValue = 255,
			StartTime = _peakPriorityStartTime,
			EndTime = _peakPriorityEndTime,
			Description = "Used to set the time range during which only the highest priority messages (peak) can be sent to Kiosk Services"
		};
		QueueServicePriority queueServicePriority = new QueueServicePriority
		{
			PriorityType = QueueServicePriorityType.offPeak,
			MinimumPriorityValue = 50,
			MaximumPriorityValue = 99,
			Description = "Used to set the time range during which peak priority and offpeak priority messages can be sent to Kiosk Services"
		};
		queueServicePriority.ExcludeTimeRanges.Add(item);
		_queueServicePriorities.Add(item);
		_queueServicePriorities.Add(queueServicePriority);
		_queueServicePriorities.Add(new QueueServicePriority
		{
			PriorityType = QueueServicePriorityType.overNight,
			MinimumPriorityValue = 0,
			MaximumPriorityValue = 49,
			StartTime = _overnightPriorityStartTime,
			EndTime = _overnightPriorityEndTime,
			Description = "Used to set the time range during which the lowest priority messages (over night) can be sent to Kiosk Services"
		});
		_queueServicePriorities.ForEach(delegate(QueueServicePriority x)
		{
			LogHelper.Instance.Log($"Setting QueueService Priority: {x}");
		});
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Initialize queue service, storage path: {0}", path);
		if (ServiceLocator.Instance.GetService<IQueueService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IQueueService), Instance);
		}
		try
		{
			errorList = _queueServiceData.Initialize(path);
			if (errorList != null && errorList.ContainsError())
			{
				HandleCorruptDatabase();
			}
			IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
			if (service != null)
			{
				service.AddPreferencePage("QueueServices_General", "Remote Services\\Queue Services", "Engine Core/Queue Services/Message Queue", PreferencePageTarget.LocalSystem, new QueuePreferencePage());
				service.AddPreferencePage("QueueServices_Priorities", "Remote Services\\Queue Services", "Engine Core/Queue Services/Message Priorities", PreferencePageTarget.LocalSystem, new QueuePrioritiesPreferencePage());
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Initialize.", e);
			HandleCorruptDatabase();
		}
		return errorList;
	}

	public ErrorList Dequeue(Predicate<IMessage> predicate)
	{
		IQueueServicePriority currentQueueServicePriority = GetCurrentQueueServicePriority();
		ErrorList errorList = _queueServiceData.SelectNextMessage(currentQueueServicePriority, out var message);
		if ((errorList == null || !errorList.ContainsError()) && message != null)
		{
			IMessage message2 = new QueueMessage
			{
				ID = message.ID,
				Priority = message.Priority,
				CreatedOn = message.CreatedOn,
				Type = message.Type,
				Data = FromEncryptedMessage(message.Data)
			};
			predicate(message2);
			_queueServiceData.DeleteMessage(message2.ID);
		}
		if (errorList.ContainsCode("Q999"))
		{
			HandleCorruptDatabase();
		}
		return errorList;
	}

	public ErrorList DeleteMessage(int id)
	{
		return _queueServiceData?.DeleteMessage(id);
	}

	public ErrorList DeleteMessageByType(string type)
	{
		return _queueServiceData.DeleteMessageByType(type);
	}

	public ErrorList ExportToXml(string fileName)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(fileName, Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("queue-export");
			_queueServiceData.ForEach(delegate(IMessage m)
			{
				xmlWriter.WriteStartElement("message");
				xmlWriter.WriteAttributeString("id", m.ID.ToString());
				xmlWriter.WriteAttributeString("type", m.Type);
				xmlWriter.WriteAttributeString("priority", m.Priority.ToString());
				xmlWriter.WriteAttributeString("createdon", m.CreatedOn.ToString());
				xmlWriter.WriteCData(Encoding.ASCII.GetString(m.Data));
				xmlWriter.WriteEndElement();
			});
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.ExportToXml.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.ExportToXml.", e));
		}
		return errorList;
	}

	private void GetQueueServiceConfigurationValues()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service.TryGetTimeSpanRangeValue("system", "QueueService", "PeakPriorityOnlyTimeRange", out var startTime, out var endTime))
		{
			_peakPriorityStartTime = startTime.Value;
			_peakPriorityEndTime = endTime.Value;
		}
		else
		{
			_peakPriorityStartTime = QueueServiceConstants.PeakPriorityStartTimeDefault;
			_peakPriorityEndTime = QueueServiceConstants.PeakPriorityEndTimeDefault;
		}
		if (service.TryGetTimeSpanRangeValue("system", "QueueService", "OvernightPriorityTimeRange", out startTime, out endTime))
		{
			_overnightPriorityStartTime = startTime.Value;
			_overnightPriorityEndTime = endTime.Value;
		}
		else
		{
			_overnightPriorityStartTime = QueueServiceConstants.OvernightPriorityStartTimeDefault;
			_overnightPriorityEndTime = QueueServiceConstants.OvernightPriorityEndTimeDefault;
		}
	}

	private IQueueServicePriority GetCurrentQueueServicePriority()
	{
		TimeSpan currentTime = DateTime.Now.TimeOfDay;
		return (from x in _queueServicePriorities
			where x.IsInTimeRange(currentTime)
			orderby x.MinimumPriorityValue
			select x).FirstOrDefault();
	}

	private QueueService()
	{
	}

	private void HandleCorruptDatabase()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		bool num = ServiceLocator.Instance.GetService<IApplicationState>()?.HasMaintenanceModeReason(MaintenanceModeSource.Database, "SOFT-FILE01") ?? false;
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
		if (num)
		{
			return;
		}
		try
		{
			var data = new
			{
				MessageType = "KioskAlert",
				MessageId = Guid.NewGuid(),
				KioskId = int.Parse(service2.Filter["store_number"]),
				EngineVersion = service["EngineVersion"],
				BundleVersion = service["ProductVersion"],
				SubType = "mm:DataStoreError, SOFT-FILE01",
				Detail = "<h2>Kiosk Engine Data File Error</h2><p>There was an error accessing or updating the database: [ message.data ] <br><br>Placing Kiosk into Maintenance Mode.<br>SOFT-FILE01</p>",
				Type = "ApplicationCrash",
				Time = DateTime.Now
			};
			IEnumerable<IQueueMessageProcessorService> items;
			lock (_queueMessageProcessorServices)
			{
				items = _queueMessageProcessorServices.Where((IQueueMessageProcessorService x) => x.Name.Equals("KSKioskAlertMessageProcessorService") || x.Name.Equals("AWSKioskAlertMessageProcessorService"));
			}
			items.ForEach(delegate(IQueueMessageProcessorService q)
			{
				if (q != null && q.ProcessMessage != null)
				{
					q?.ProcessMessage(new QueueMessage
					{
						ID = 0,
						Type = "KioskAlert",
						CreatedOn = DateTime.Now,
						Data = Encoding.ASCII.GetBytes(data.ToJson()),
						Priority = byte.MaxValue
					}, out var _);
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("HandleCorruptDatabase: Error creating and sending KioskAlert message.", e);
		}
	}

	private static int GetRandomNumber(int min, int max)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[4];
		rNGCryptoServiceProvider.GetBytes(array);
		rNGCryptoServiceProvider.Dispose();
		int num = BitConverter.ToInt32(array, 0);
		if (min != max)
		{
			return min + Math.Abs(num % (max - min));
		}
		return min;
	}

	private byte[] ToEncryptedMessage(object message)
	{
		return ToEncryptedMessage(message.ToJson());
	}

	private byte[] ToEncryptedMessage(string message)
	{
		byte[] array = Encoding.ASCII.GetBytes(message).Encrypt();
		byte[] array2 = new byte["RedboxEncrypted".Length + array.Length];
		byte[] bytes = Encoding.ASCII.GetBytes("RedboxEncrypted");
		Buffer.BlockCopy(bytes, 0, array2, 0, bytes.Length);
		Buffer.BlockCopy(array, 0, array2, bytes.Length, array.Length);
		return array2;
	}

	private byte[] FromEncryptedMessage(byte[] data)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("RedboxEncrypted");
		if (data.Length > bytes.Length)
		{
			byte[] array = new byte[bytes.Length];
			Buffer.BlockCopy(data, 0, array, 0, array.Length);
			string value = Encoding.ASCII.GetString(array);
			if ("RedboxEncrypted".Equals(value))
			{
				byte[] array2 = new byte[data.Length - bytes.Length];
				Buffer.BlockCopy(data, bytes.Length, array2, 0, array2.Length);
				return array2.Decrypt();
			}
		}
		return data;
	}

	private List<IQueueMessageProcessorService> GetQueueProcessorsForMessageType(string type)
	{
		List<IQueueMessageProcessorService> list = new List<IQueueMessageProcessorService>();
		lock (_queueMessageProcessorServices)
		{
			list.AddRange(_queueMessageProcessorServices.Where((IQueueMessageProcessorService x) => x.SupportedMessageTypes.Contains(type)));
			if (!list.Any())
			{
				IQueueMessageProcessorService queueMessageProcessorService = _queueMessageProcessorServices.FirstOrDefault((IQueueMessageProcessorService x) => x.IsDefault);
				if (queueMessageProcessorService != null)
				{
					list.Add(queueMessageProcessorService);
				}
			}
		}
		return list;
	}
}
