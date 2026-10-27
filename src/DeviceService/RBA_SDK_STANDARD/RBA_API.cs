using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RBA_SDK_ComponentModel;

namespace RBA_SDK;

public class RBA_API : IRBA_API
{
	public class RBASDK : IDisposable
	{
		private int m_instance_id = -50;

		private string m_name;

		private bool m_disposed;

		public PinPadMessageHandler pinpadHandler;

		private unsafe static readonly PinPadMessageCallBackWithInstanceId vsPinpadCallBackWithInstanceId = PinpadCallBackWithInstanceId;

		private ConnectHandler connectHandler;

		private static readonly ConnectCallBackWithInstanceId vsUnmanagedConnectCallBackWithInstanceId = UnmanagedConnectCallBackWithInstanceId;

		public DisconnectHandler disconnectHandler;

		private static readonly DisconnectCallBackWithInstanceId vsUnmanagedDisconnectCallBackWithInstanceId = UnmanagedDisconnectCallBackWithInstance;

		private static List<RBASDK> m_rba_sdk_instances = new List<RBASDK>();

		public unsafe RBASDK(RBA_API rba_api, string name)
		{
			if (rba_api.m_SetUnmanagedLogCalBackOnce)
			{
				SetLogCallBackNative(rba_api.vsUnmanagedTraceLog);
				rba_api.m_SetUnmanagedLogCalBackOnce = false;
			}
			m_instance_id = RBA_SDK_InitializeInstanceNative(name);
			if (m_instance_id > 0)
			{
				m_rba_sdk_instances.Add(this);
				SetMessageCallBackWithInstanceIdNative((uint)m_instance_id, vsPinpadCallBackWithInstanceId, null);
				SetNotifyRbaDisconnectedForNative((uint)m_instance_id, vsUnmanagedDisconnectCallBackWithInstanceId);
				m_name = name;
			}
		}

		~RBASDK()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!m_disposed)
			{
				Shutdown();
				GC.SuppressFinalize(this);
				m_disposed = true;
			}
		}

		public string GetInstanceName()
		{
			return m_name;
		}

		public void SetInstanceName(string name)
		{
			m_name = name;
			RBA_SDK_SetInstanceNameNative((uint)m_instance_id, name);
		}

		public int GetInstanceId()
		{
			return m_instance_id;
		}

		public string GetVersion()
		{
			return RBA_API.GetVersion();
		}

		public string GetAssemblyVersion()
		{
			return RBA_API.GetAssemblyVersion();
		}

		private ERROR_ID Shutdown()
		{
			pinpadHandler = null;
			connectHandler = null;
			disconnectHandler = null;
			ERROR_ID result = ERROR_ID.RESULT_ERROR_NOT_INITIALIZED;
			if (m_instance_id > 0)
			{
				m_rba_sdk_instances.Remove(this);
				result = NumToEnum<ERROR_ID>(RBA_SDK_ShutdownInstanceNative((uint)m_instance_id));
				m_instance_id = -50;
				m_name = "";
			}
			return result;
		}

		private unsafe static void PinpadCallBackWithInstanceId(uint instanceId, int msgId, void* userParam)
		{
			foreach (RBASDK rba_sdk_instance in m_rba_sdk_instances)
			{
				if (rba_sdk_instance.m_instance_id == instanceId)
				{
					if (rba_sdk_instance.pinpadHandler != null)
					{
						rba_sdk_instance.pinpadHandler((MESSAGE_ID)Enum.ToObject(typeof(MESSAGE_ID), msgId));
					}
					else
					{
						LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No pinpadHandler defined");
					}
				}
			}
		}

		private static void UnmanagedConnectCallBackWithInstanceId(uint instanceId)
		{
			foreach (RBASDK rba_sdk_instance in m_rba_sdk_instances)
			{
				if (rba_sdk_instance.m_instance_id == instanceId)
				{
					if (rba_sdk_instance.connectHandler != null)
					{
						rba_sdk_instance.connectHandler();
					}
					else
					{
						LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No connectHandler defined");
					}
				}
			}
		}

		private static void UnmanagedDisconnectCallBackWithInstance(uint instanceId)
		{
			foreach (RBASDK rba_sdk_instance in m_rba_sdk_instances)
			{
				if (rba_sdk_instance.m_instance_id == instanceId)
				{
					if (rba_sdk_instance.disconnectHandler != null)
					{
						rba_sdk_instance.disconnectHandler();
					}
					else
					{
						LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No disconnectHandler defined");
					}
				}
			}
		}

		public ERROR_ID SetNotifyRbaConnected(ConnectHandler handler)
		{
			if (handler != null)
			{
				connectHandler = handler;
				return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedForNative((uint)m_instance_id, vsUnmanagedConnectCallBackWithInstanceId));
			}
			connectHandler = null;
			return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedForNative((uint)m_instance_id, null));
		}

		public ERROR_ID Connect(SETTINGS_COMMUNICATION Settings)
		{
			return NumToEnum<ERROR_ID>(ConnectNativeFor((uint)m_instance_id, Settings));
		}

		public ERROR_ID Reconnect()
		{
			return NumToEnum<ERROR_ID>(ReconnectNativeFor((uint)m_instance_id));
		}

		public ERROR_ID SetParam(PARAMETER_ID id, string data)
		{
			return NumToEnum<ERROR_ID>(SetParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
		}

		public ERROR_ID SetParam(PARAMETER_ID id, byte[] data)
		{
			return NumToEnum<ERROR_ID>(SetParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
		}

		public ERROR_ID AddParam(PARAMETER_ID id, string data)
		{
			return NumToEnum<ERROR_ID>(AddParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
		}

		public ERROR_ID AddParam(PARAMETER_ID id, byte[] data)
		{
			return NumToEnum<ERROR_ID>(AddParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
		}

		public int GetParamLen(PARAMETER_ID id)
		{
			return GetParamLenNativeFor((uint)m_instance_id, (int)id);
		}

		public ERROR_ID GetParam(PARAMETER_ID id, out StringBuilder data)
		{
			ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR_PARAMETER;
			int len = GetParamLen(id);
			if (len < 0)
			{
				data = new StringBuilder(0);
				eRROR_ID = (ERROR_ID)len;
			}
			else
			{
				data = new StringBuilder(len);
				eRROR_ID = (ERROR_ID)GetParamNativeFor((uint)m_instance_id, (int)id, data, ref len);
				data.Length = len;
			}
			return eRROR_ID;
		}

		private ERROR_ID GetParam(PARAMETER_ID id, out byte[] data)
		{
			ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR_PARAMETER;
			int len = GetParamLen(id);
			if (len < 0)
			{
				data = new byte[0];
				return (ERROR_ID)len;
			}
			data = new byte[len];
			return (ERROR_ID)GetParamNativeFor((uint)m_instance_id, (int)id, data, ref len);
		}

		public ERROR_ID GetParamAsByteArray(PARAMETER_ID id, out byte[] data)
		{
			return GetParam(id, out data);
		}

		public byte[] GetParamAsByteArray(PARAMETER_ID id, out ERROR_ID errorId)
		{
			errorId = GetParam(id, out byte[] data);
			return data;
		}

		public byte[] GetParamAsByteArray(PARAMETER_ID id)
		{
			GetParam(id, out byte[] data);
			return data;
		}

		public string GetParam(PARAMETER_ID id, out ERROR_ID errorId)
		{
			int paramLen = GetParamLen(id);
			if (paramLen < 0)
			{
				errorId = (ERROR_ID)paramLen;
				return "";
			}
			errorId = GetParam(id, out byte[] data);
			return Encoding.Default.GetString(data);
		}

		public string GetParam(PARAMETER_ID id)
		{
			if (GetParamLen(id) < 0)
			{
				return "";
			}
			GetParam(id, out byte[] data);
			return Encoding.Default.GetString(data);
		}

		public StringBuilder GetMutableParam(PARAMETER_ID id)
		{
			GetParam(id, out StringBuilder data);
			return data;
		}

		public ERROR_ID ResetParam(PARAMETER_ID id)
		{
			return NumToEnum<ERROR_ID>(ResetParamNativeFor((uint)m_instance_id, (int)id));
		}

		public ERROR_ID LockParam()
		{
			return NumToEnum<ERROR_ID>(LockParamNativeFor((uint)m_instance_id));
		}

		public ERROR_ID UnlockParam()
		{
			return NumToEnum<ERROR_ID>(UnlockParamNativeFor((uint)m_instance_id));
		}

		public ERROR_ID SetAttribute(ATTRIBUTE_ID id, string data)
		{
			return NumToEnum<ERROR_ID>(SetAttributeNativeFor((uint)m_instance_id, (int)id, data, data.Length));
		}

		public int GetAttributeLen(ATTRIBUTE_ID id)
		{
			return GetAttributeLenNativeFor((uint)m_instance_id, (int)id);
		}

		public string GetAttribute(ATTRIBUTE_ID id)
		{
			int len = GetAttributeLen(id);
			string result = "";
			if (len > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(len);
				if (GetAttributeNativeFor((uint)m_instance_id, (int)id, stringBuilder, ref len) == 0 && len <= stringBuilder.Length)
				{
					result = stringBuilder.ToString().Substring(0, len);
				}
			}
			return result;
		}

		public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data)
		{
			return NumToEnum<ERROR_ID>(AddTagParamNativeFor((uint)m_instance_id, (int)msg, tagId, data, data.Length));
		}

		public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, byte[] data)
		{
			return NumToEnum<ERROR_ID>(AddTagParamWithFlagNativeFor((uint)m_instance_id, (int)msg, tagId, emvTag ? 1 : 0, data, data.Length));
		}

		public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data)
		{
			byte[] bytes = new ASCIIEncoding().GetBytes(data);
			return NumToEnum<ERROR_ID>(AddTagParamNativeFor((uint)m_instance_id, (int)msg, tagId, bytes, bytes.Length));
		}

		public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, string data)
		{
			byte[] bytes = new ASCIIEncoding().GetBytes(data);
			return NumToEnum<ERROR_ID>(AddTagParamWithFlagNativeFor((uint)m_instance_id, (int)msg, tagId, emvTag ? 1 : 0, bytes, bytes.Length));
		}

		public int GetTagParamLen(MESSAGE_ID msg)
		{
			return GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
		}

		public int GetTagParam(MESSAGE_ID msg, out byte[] data)
		{
			int result = -1;
			int len = GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
			data = new byte[0];
			if (len > 0)
			{
				data = new byte[len];
				result = GetTagParamNativeFor((uint)m_instance_id, (int)msg, data, ref len);
			}
			return result;
		}

		public Tag GetTag(MESSAGE_ID msg, out byte[] data)
		{
			Tag tag = new Tag();
			int len = GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
			data = new byte[0];
			if (len > 0)
			{
				int emvTag = 0;
				data = new byte[len];
				tag.id = GetTagParamAndFlagNativeFor((uint)m_instance_id, (int)msg, data, ref len, ref emvTag);
				tag.emvTag = emvTag == 1;
			}
			return tag;
		}

		public ERROR_ID ResetTagParam(MESSAGE_ID msg)
		{
			return NumToEnum<ERROR_ID>(ResetTagParamNativeFor((uint)m_instance_id, (int)msg, uint.MaxValue));
		}

		public ERROR_ID ResetTagParam()
		{
			return NumToEnum<ERROR_ID>(ResetTagParamNativeFor((uint)m_instance_id, -1, uint.MaxValue));
		}

		public ERROR_ID ProcessMessage(MESSAGE_ID messageId)
		{
			try
			{
				return NumToEnum<ERROR_ID>(ProcessMessageNativeFor((uint)m_instance_id, (int)messageId));
			}
			catch (Exception)
			{
				return ERROR_ID.RESULT_ERROR;
			}
		}

		public ERROR_ID SetProcessMessageAsyncMode()
		{
			return NumToEnum<ERROR_ID>(SetProcessMessageAsyncModeNativeFor((uint)m_instance_id));
		}

		public ERROR_ID SetProcessMessageSyncMode()
		{
			return NumToEnum<ERROR_ID>(SetProcessMessageSyncModeNativeFor((uint)m_instance_id));
		}

		public ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts)
		{
			return NumToEnum<ERROR_ID>(SetCommTimeoutsNativeFor((uint)m_instance_id, timeouts));
		}

		public ERROR_ID SetResponseTimeout(uint timeoutMsec)
		{
			SETTINGS_COMM_TIMEOUTS commTimeouts = GetCommTimeouts();
			commTimeouts.ReceiveTimeout = timeoutMsec;
			return SetCommTimeouts(commTimeouts);
		}

		public SETTINGS_COMM_TIMEOUTS GetCommTimeouts()
		{
			return GetCommTimeoutsNativeFor((uint)m_instance_id);
		}

		public uint GetResponseTimeout()
		{
			return GetCommTimeouts().ReceiveTimeout;
		}

		public CONNECTION_STATUS GetConnectionStatus()
		{
			return NumToEnum<CONNECTION_STATUS>(GetConnectionStatusNativeFor((uint)m_instance_id));
		}

		public ERROR_ID Disconnect()
		{
			return NumToEnum<ERROR_ID>(DisconnectNativeFor((uint)m_instance_id));
		}

		public ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce)
		{
			StringBuilder stringBuilder = new StringBuilder(RawData);
			int length = stringBuilder.Length;
			return NumToEnum<ERROR_ID>(SendCustomMessageNativeFor((uint)m_instance_id, stringBuilder, length, bWaitResponce));
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void LOUT([In][MarshalAs(UnmanagedType.LPStr)] string line);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void PinPadMessageCallBack(int msgId);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void PinPadMessageCallBackWithInstanceId(uint instaneId, int msgId, void* userParam);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void BatteryLevelCallBack(int level, BATTERY_STATE btSate, BATTERY_LEVEL_STATE btLevelState);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void ConnectCallBack();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void ConnectCallBackWithInstanceId(uint instaneId);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void DisconnectCallBack();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void DisconnectCallBackWithInstanceId(uint instaneId);

	public BatteryLevelHandler batterylevelHandler;

	private ConnectHandler connectHandler;

	private DisconnectHandler disconnectHandler;

	private readonly LOUT vsUnmanagedTraceLog;

	private readonly PinPadMessageCallBack vsUnmanagedPinpadCallBack;

	private readonly BatteryLevelCallBack vsUnmanagedBatteryLevelCallBack;

	private readonly ConnectCallBack vsUnmanagedConnectCallBack;

	private readonly DisconnectCallBack vsUnmanagedDisconnectCallBack;

	private bool m_SetUnmanagedPinpadCalBackOnce = true;

	private bool m_SetUnmanagedLogCalBackOnce = true;

	public LogHandler logHandler { get; set; }

	public PinPadMessageHandler pinpadHandler { get; set; }

	private static string AssemblyVersion
	{
		get
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			return $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.ProductBuildPart}";
		}
	}

	public RBA_API()
	{
		vsUnmanagedPinpadCallBack = unmanagedPinpadCallBack;
		vsUnmanagedBatteryLevelCallBack = unmanagedBatteryLevelCallBack;
		vsUnmanagedConnectCallBack = unmanagedConnectCallBack;
		vsUnmanagedDisconnectCallBack = unmanagedDisconnectCallBack;
		vsUnmanagedTraceLog = unmanagedTraceLog;
	}

	private void unmanagedTraceLog(string line)
	{
		try
		{
			logHandler?.Invoke(line);
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex.ToString());
		}
	}

	private void unmanagedPinpadCallBack(int msgId)
	{
		try
		{
			if (pinpadHandler != null)
			{
				pinpadHandler((MESSAGE_ID)Enum.ToObject(typeof(MESSAGE_ID), msgId));
			}
			else
			{
				LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No pinpadHandler defined");
			}
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex.ToString());
		}
	}

	private void unmanagedBatteryLevelCallBack(int level, BATTERY_STATE btSate, BATTERY_LEVEL_STATE btLevelState)
	{
		try
		{
			batterylevelHandler?.Invoke(level, btSate, btLevelState);
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex.ToString());
		}
	}

	private void unmanagedConnectCallBack()
	{
		try
		{
			if (connectHandler != null)
			{
				connectHandler();
			}
			else
			{
				LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No connectHandler defined");
			}
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex.ToString());
		}
	}

	private void unmanagedDisconnectCallBack()
	{
		try
		{
			if (disconnectHandler != null)
			{
				disconnectHandler();
			}
			else
			{
				LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No disconnectHandler defined");
			}
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex.ToString());
		}
	}

	public string GetVersion_NonStatic()
	{
		return GetVersion();
	}

	public unsafe static string GetVersion()
	{
		return new string((sbyte*)GetVersionNative());
	}

	public ERROR_ID Initialize()
	{
		if (m_SetUnmanagedLogCalBackOnce)
		{
			SetLogCallBackNative(vsUnmanagedTraceLog);
			m_SetUnmanagedLogCalBackOnce = false;
		}
		int num = RBA_SDK_InitializeNative();
		if (num == 0 && m_SetUnmanagedPinpadCalBackOnce)
		{
			m_SetUnmanagedPinpadCalBackOnce = false;
			SetMessageCallBackNative(vsUnmanagedPinpadCallBack);
		}
		return NumToEnum<ERROR_ID>(num);
	}

	public ERROR_ID Shutdown()
	{
		pinpadHandler = null;
		batterylevelHandler = null;
		SetMessageCallBackNative(null);
		ERROR_ID result = NumToEnum<ERROR_ID>(RBA_SDK_ShutdownNative());
		SetMessageCallBackNative(null);
		logHandler = null;
		m_SetUnmanagedLogCalBackOnce = true;
		m_SetUnmanagedPinpadCalBackOnce = true;
		return result;
	}

	private ERROR_ID CheckVersion()
	{
		uint instanceId = uint.MaxValue;
		LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.INFO, "Version: " + AssemblyVersion);
		string text = GetVersion().Split(new char[1] { ' ' })[0];
		if (AssemblyVersion != text)
		{
			LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.ERROR, "Version inconsistency: RBA_SDK_CSharp.dll version is " + AssemblyVersion + " RBA_SDK.DLL version is " + text);
			return ERROR_ID.RESULT_ERROR_NOT_INITIALIZED;
		}
		return ERROR_ID.RESULT_SUCCESS;
	}

	public static string GetAssemblyVersion()
	{
		return AssemblyVersion;
	}

	public ERROR_ID Connect(string comPort)
	{
		return Connect(new SETTINGS_COMMUNICATION
		{
			interface_id = 1u,
			rs232_config = new SETTINGS_RS232
			{
				ComPort = comPort,
				BaudRate = 115200u,
				DataBits = 8u,
				Parity = 0u,
				FlowControl = 0u,
				StopBits = 1u
			}
		});
	}

	public ERROR_ID Connect(SETTINGS_COMMUNICATION Settings)
	{
		return NumToEnum<ERROR_ID>(ConnectNative(Settings));
	}

	public ERROR_ID Reconnect()
	{
		return NumToEnum<ERROR_ID>(ReconnectNative());
	}

	public ERROR_ID SetParam(PARAMETER_ID id, string data)
	{
		return NumToEnum<ERROR_ID>(SetParamNative((int)id, data, data?.Length ?? 0));
	}

	public ERROR_ID SetParam(PARAMETER_ID id, byte[] data)
	{
		return NumToEnum<ERROR_ID>(SetParamNative((int)id, data, data.Length));
	}

	public ERROR_ID AddParam(PARAMETER_ID id, string data)
	{
		return NumToEnum<ERROR_ID>(AddParamNative((int)id, data, data.Length));
	}

	public ERROR_ID AddParam(PARAMETER_ID id, byte[] data)
	{
		return NumToEnum<ERROR_ID>(AddParamNative((int)id, data, data.Length));
	}

	public int GetParamLen(PARAMETER_ID id)
	{
		return GetParamLenNative((int)id);
	}

	public ERROR_ID GetParam(PARAMETER_ID id, out StringBuilder data)
	{
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR_PARAMETER;
		int len = GetParamLen(id);
		if (len < 0)
		{
			data = new StringBuilder(0);
			eRROR_ID = (ERROR_ID)len;
		}
		else
		{
			data = new StringBuilder(len + 1);
			eRROR_ID = (ERROR_ID)GetParamNative((int)id, data, ref len);
			data.Length = len;
		}
		return eRROR_ID;
	}

	private ERROR_ID GetParam(PARAMETER_ID id, out byte[] data)
	{
		ERROR_ID eRROR_ID = ERROR_ID.RESULT_ERROR_PARAMETER;
		int len = GetParamLen(id);
		if (len < 0)
		{
			data = new byte[0];
			return (ERROR_ID)len;
		}
		data = new byte[len];
		return (ERROR_ID)GetParamNative((int)id, data, ref len);
	}

	public string GetParam(PARAMETER_ID id, out ERROR_ID errorId)
	{
		int paramLen = GetParamLen(id);
		if (paramLen < 0)
		{
			errorId = (ERROR_ID)paramLen;
			return "";
		}
		errorId = GetParam(id, out byte[] data);
		return Encoding.Default.GetString(data);
	}

	public byte[] GetParamAsByteArray(PARAMETER_ID id, out ERROR_ID errorId)
	{
		byte[] data = new byte[0];
		int paramLen = GetParamLen(id);
		if (paramLen < 0)
		{
			errorId = (ERROR_ID)paramLen;
		}
		else
		{
			errorId = GetParam(id, out data);
		}
		return data;
	}

	public string GetParam(PARAMETER_ID id)
	{
		if (GetParamLen(id) < 0)
		{
			return "";
		}
		GetParam(id, out byte[] data);
		return Encoding.Default.GetString(data);
	}

	public byte[] GetParamAsByteArray(PARAMETER_ID id)
	{
		byte[] data = null;
		GetParam(id, out data);
		return data;
	}

	public StringBuilder GetMutableParam(PARAMETER_ID id)
	{
		GetParam(id, out StringBuilder data);
		return data;
	}

	public ERROR_ID ResetParam(PARAMETER_ID id)
	{
		return NumToEnum<ERROR_ID>(ResetParamNative((int)id));
	}

	public ERROR_ID LockParam()
	{
		return NumToEnum<ERROR_ID>(LockParamNative());
	}

	public ERROR_ID UnlockParam()
	{
		return NumToEnum<ERROR_ID>(UnlockParamNative());
	}

	public ERROR_ID SetAttribute(ATTRIBUTE_ID id, string data)
	{
		return NumToEnum<ERROR_ID>(SetAttributeNative((int)id, data, data.Length));
	}

	public int GetAttributeLen(ATTRIBUTE_ID id)
	{
		return GetAttributeLenNative((int)id);
	}

	public string GetAttribute(ATTRIBUTE_ID id)
	{
		int len = GetAttributeLen(id);
		string result = "";
		if (len > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(len);
			if (GetAttributeNative((int)id, stringBuilder, ref len) == 0 && len <= stringBuilder.Length)
			{
				result = stringBuilder.ToString().Substring(0, len);
			}
		}
		return result;
	}

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data)
	{
		return NumToEnum<ERROR_ID>(AddTagParamNative((int)msg, tagId, data, data.Length));
	}

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, byte[] data)
	{
		return NumToEnum<ERROR_ID>(AddTagParamWithFlagNative((int)msg, tagId, emvTag ? 1 : 0, data, data.Length));
	}

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data)
	{
		byte[] bytes = new ASCIIEncoding().GetBytes(data);
		return NumToEnum<ERROR_ID>(AddTagParamNative((int)msg, tagId, bytes, bytes.Length));
	}

	public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, string data)
	{
		byte[] bytes = new ASCIIEncoding().GetBytes(data);
		return NumToEnum<ERROR_ID>(AddTagParamWithFlagNative((int)msg, tagId, emvTag ? 1 : 0, bytes, bytes.Length));
	}

	public int GetTagParamLen(MESSAGE_ID msg)
	{
		return GetTagParamLenNative((int)msg);
	}

	public int GetTagParam(MESSAGE_ID msg, out byte[] data)
	{
		int result = -1;
		int len = GetTagParamLen(msg);
		data = new byte[0];
		if (len > 0)
		{
			data = new byte[len];
			result = GetTagParamNative((int)msg, data, ref len);
		}
		return result;
	}

	public Tag GetTag(MESSAGE_ID msg, out byte[] data)
	{
		Tag tag = new Tag();
		int len = GetTagParamLen(msg);
		data = new byte[0];
		if (len > 0)
		{
			int emvTag = 0;
			data = new byte[len];
			tag.id = GetTagParamAndFlagNative((int)msg, data, ref len, ref emvTag);
			tag.emvTag = emvTag == 1;
		}
		return tag;
	}

	public ERROR_ID ResetTagParam(MESSAGE_ID msg)
	{
		return NumToEnum<ERROR_ID>(ResetTagParamNative((int)msg, uint.MaxValue));
	}

	public ERROR_ID ResetTagParam()
	{
		return NumToEnum<ERROR_ID>(ResetTagParamNative(-1, uint.MaxValue));
	}

	public ERROR_ID ProcessMessage(MESSAGE_ID messageId)
	{
		try
		{
			return NumToEnum<ERROR_ID>(ProcessMessageNative((int)messageId));
		}
		catch (Exception)
		{
			return ERROR_ID.RESULT_ERROR;
		}
	}

	public ERROR_ID SetProcessMessageAsyncMode()
	{
		return NumToEnum<ERROR_ID>(SetProcessMessageAsyncModeNative());
	}

	public ERROR_ID SetProcessMessageSyncMode()
	{
		return NumToEnum<ERROR_ID>(SetProcessMessageSyncModeNative());
	}

	public ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts)
	{
		return NumToEnum<ERROR_ID>(SetCommTimeoutsNative(timeouts));
	}

	public ERROR_ID SetResponseTimeout(uint timeoutMsec)
	{
		SETTINGS_COMM_TIMEOUTS commTimeouts = GetCommTimeouts();
		commTimeouts.ReceiveTimeout = timeoutMsec;
		return SetCommTimeouts(commTimeouts);
	}

	public SETTINGS_COMM_TIMEOUTS GetCommTimeouts()
	{
		return GetCommTimeoutsNative();
	}

	public uint GetResponseTimeout()
	{
		return GetCommTimeouts().ReceiveTimeout;
	}

	public CONNECTION_STATUS GetConnectionStatus()
	{
		return NumToEnum<CONNECTION_STATUS>(GetConnectionStatusNative());
	}

	public ERROR_ID Disconnect()
	{
		return NumToEnum<ERROR_ID>(DisconnectNative());
	}

	public ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce)
	{
		StringBuilder stringBuilder = new StringBuilder(RawData);
		int length = stringBuilder.Length;
		return NumToEnum<ERROR_ID>(SendCustomMessageNative(stringBuilder, length, bWaitResponce));
	}

	public void SetDefaultLogLevel(LOG_LEVEL level)
	{
		SetDefaultLogLevelNative(level);
	}

	public void SetModuleLogLevel(string moduleName, LOG_LEVEL level)
	{
		SetModuleLogLevelNative(moduleName, level);
	}

	public void SetTraceOutputFormatOption(LOG_OUTPUT_FORMAT_OPTIONS option)
	{
		SetTraceOutputFormatOptionNative(option);
	}

	public ERROR_ID SetBatteryNotifyThreshold(int threshold)
	{
		return NumToEnum<ERROR_ID>(SetBatteryNotifyThresholdNative(threshold, vsUnmanagedBatteryLevelCallBack));
	}

	public ERROR_ID SetBatteryNotifyThreshold(int threshold, BatteryLevelHandler handler)
	{
		batterylevelHandler = handler;
		return NumToEnum<ERROR_ID>(SetBatteryNotifyThresholdNative(threshold, vsUnmanagedBatteryLevelCallBack));
	}

	public ERROR_ID SetNotifyRbaConnected(ConnectHandler handler)
	{
		if (handler != null)
		{
			connectHandler = handler;
			return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedNative(vsUnmanagedConnectCallBack));
		}
		connectHandler = null;
		return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedNative(null));
	}

	public ERROR_ID SetNotifyRbaDisconnected(DisconnectHandler handler)
	{
		if (handler != null)
		{
			disconnectHandler = handler;
			return NumToEnum<ERROR_ID>(SetNotifyRbaDisconnectedNative(vsUnmanagedDisconnectCallBack));
		}
		disconnectHandler = null;
		return NumToEnum<ERROR_ID>(SetNotifyRbaDisconnectedNative(null));
	}

	private static T NumToEnum<T>(int number)
	{
		return (T)Enum.ToObject(typeof(T), number);
	}

	public void LogOut(string moduleName, LOG_LEVEL logLevel, string logString)
	{
		try
		{
			LogOutNative(moduleName, logLevel, logString);
		}
		catch (Exception ex)
		{
			logHandler?.Invoke(ex?.ToString());
		}
	}

	public static void LogOut(uint instanceId, string moduleName, LOG_LEVEL logLevel, string logString)
	{
		try
		{
			LogOutNativeFor(instanceId, moduleName, logLevel, logString);
		}
		catch (Exception)
		{
		}
	}

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetVersion")]
	private unsafe static extern char* GetVersionNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_Initialize")]
	private static extern int RBA_SDK_InitializeNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_InitializeInstance")]
	private static extern int RBA_SDK_InitializeInstanceNative(string name);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetInstanceName")]
	private static extern void RBA_SDK_SetInstanceNameNative(uint instanceId, string name);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_Shutdown")]
	private static extern int RBA_SDK_ShutdownNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ShutdownInstance")]
	private static extern int RBA_SDK_ShutdownInstanceNative(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetParam")]
	private static extern int SetParamNative(int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetParamFor")]
	private static extern int SetParamNativeFor(uint instanceId, int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetParam")]
	private static extern int SetParamNative(int id, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetParamFor")]
	private static extern int SetParamNativeFor(uint instanceId, int id, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddParam")]
	private static extern int AddParamNative(int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddParamFor")]
	private static extern int AddParamNativeFor(uint instanceId, int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddParam")]
	private static extern int AddParamNative(int id, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddParamFor")]
	private static extern int AddParamNativeFor(uint instanceId, int id, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParam")]
	private static extern int GetParamNative(int id, StringBuilder data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParamFor")]
	private static extern int GetParamNativeFor(uint instanceId, int id, StringBuilder data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParam")]
	private static extern int GetParamNative(int id, byte[] data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParamFor")]
	private static extern int GetParamNativeFor(uint instanceId, int id, byte[] data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParamLen")]
	private static extern int GetParamLenNative(int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetParamLenFor")]
	private static extern int GetParamLenNativeFor(uint instanceId, int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ResetParam")]
	private static extern int ResetParamNative(int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ResetParamFor")]
	private static extern int ResetParamNativeFor(uint instanceId, int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_LockParam")]
	private static extern int LockParamNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_LockParamFor")]
	private static extern int LockParamNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_UnlockParam")]
	private static extern int UnlockParamNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_UnlockParamFor")]
	private static extern int UnlockParamNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetAttribute")]
	private static extern int SetAttributeNative(int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetAttributeFor")]
	private static extern int SetAttributeNativeFor(uint instanceId, int id, string data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetAttributeLen")]
	private static extern int GetAttributeLenNative(int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetAttributeLenFor")]
	private static extern int GetAttributeLenNativeFor(uint instanceId, int id);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetAttribute")]
	private static extern int GetAttributeNative(int id, StringBuilder data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetAttributeFor")]
	private static extern int GetAttributeNativeFor(uint instanceId, int id, StringBuilder data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddTagParam")]
	private static extern int AddTagParamNative(int msg, int tagId, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddTagParamFor")]
	private static extern int AddTagParamNativeFor(uint instanceId, int msg, int tagId, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddTagParamWithFlag")]
	private static extern int AddTagParamWithFlagNative(int msg, int tagId, int emvTag, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_AddTagParamWithFlagFor")]
	private static extern int AddTagParamWithFlagNativeFor(uint instanceId, int msg, int tagId, int EmvTag, byte[] data, int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParam")]
	private static extern int GetTagParamNative(int msg, byte[] data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParamFor")]
	private static extern int GetTagParamNativeFor(uint instanceId, int msg, byte[] data, ref int len);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParamAndFlag")]
	private static extern int GetTagParamAndFlagNative(int msg, byte[] data, ref int len, ref int emvTag);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParamAndFlagFor")]
	private static extern int GetTagParamAndFlagNativeFor(uint instanceId, int msg, byte[] data, ref int len, ref int emvTag);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParamLen")]
	private static extern int GetTagParamLenNative(int msg);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetTagParamLenFor")]
	private static extern int GetTagParamLenNativeFor(uint instanceId, int msg);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ResetTagParam")]
	private static extern int ResetTagParamNative(int msg, uint tagId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ResetTagParamFor")]
	private static extern int ResetTagParamNativeFor(uint instanceId, int msg, uint tagId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ProcessMessage")]
	private static extern int ProcessMessageNative(int CommandID);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ProcessMessageFor")]
	private static extern int ProcessMessageNativeFor(uint instanceId, int CommandID);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetProcessMessageAsyncMode")]
	private static extern int SetProcessMessageAsyncModeNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetProcessMessageAsyncModeFor")]
	private static extern int SetProcessMessageAsyncModeNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetProcessMessageSyncMode")]
	private static extern int SetProcessMessageSyncModeNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetProcessMessageSyncModeFor")]
	private static extern int SetProcessMessageSyncModeNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetCommTimeouts")]
	private static extern int SetCommTimeoutsNative(SETTINGS_COMM_TIMEOUTS Timeouts);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetCommTimeoutsFor")]
	private static extern int SetCommTimeoutsNativeFor(uint instanceId, SETTINGS_COMM_TIMEOUTS Timeouts);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetCommTimeouts")]
	private static extern SETTINGS_COMM_TIMEOUTS GetCommTimeoutsNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetCommTimeoutsFor")]
	private static extern SETTINGS_COMM_TIMEOUTS GetCommTimeoutsNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_Connect")]
	private static extern int ConnectNative(SETTINGS_COMMUNICATION Settings);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ConnectFor")]
	private static extern int ConnectNativeFor(uint instanceId, SETTINGS_COMMUNICATION Settings);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_Reconnect")]
	private static extern int ReconnectNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_ReconnectFor")]
	private static extern int ReconnectNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetConnectionStatus")]
	private static extern int GetConnectionStatusNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_GetConnectionStatusFor")]
	private static extern int GetConnectionStatusNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_Disconnect")]
	private static extern int DisconnectNative();

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_DisconnectFor")]
	private static extern int DisconnectNativeFor(uint instanceId);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SendCustomMessage")]
	private static extern int SendCustomMessageNative(StringBuilder RawData, int DataLength, [MarshalAs(UnmanagedType.I1)] bool bWaitResponce);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SendCustomMessageFor")]
	private static extern int SendCustomMessageNativeFor(uint instanceId, StringBuilder RawData, int DataLength, [MarshalAs(UnmanagedType.I1)] bool bWaitResponce);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetMessageCallBack")]
	private static extern int SetMessageCallBackNative(PinPadMessageCallBack cbMessageHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetMessageCallBackWithInstanceId")]
	private unsafe static extern int SetMessageCallBackWithInstanceIdNative(uint instanceId, PinPadMessageCallBackWithInstanceId cbMessageHandler, void* userParam);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetNotifyRbaConnected")]
	private static extern int SetNotifyRbaConnectedNative(ConnectCallBack cbConnectHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetNotifyRbaConnectedFor")]
	private static extern int SetNotifyRbaConnectedForNative(uint instanceId, ConnectCallBackWithInstanceId cbConnectHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetNotifyRbaDisconnected")]
	private static extern int SetNotifyRbaDisconnectedNative(DisconnectCallBack cbConnectHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetNotifyRbaDisconnectedFor")]
	private static extern int SetNotifyRbaDisconnectedForNative(uint instanceId, DisconnectCallBackWithInstanceId cbConnectHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetLogCallBack")]
	private static extern void SetLogCallBackNative(LOUT cbFunction);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetDefaultLogLevel")]
	private static extern void SetDefaultLogLevelNative(LOG_LEVEL level);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetModuleLogLevel")]
	private static extern void SetModuleLogLevelNative([In][MarshalAs(UnmanagedType.LPStr)] string moduleName, LOG_LEVEL level);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetTraceOutputFormatOption")]
	private static extern void SetTraceOutputFormatOptionNative(LOG_OUTPUT_FORMAT_OPTIONS level);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_SetBatteryNotifyThreshold")]
	private static extern int SetBatteryNotifyThresholdNative(int threshold, BatteryLevelCallBack cbMessageHandler);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_LogOut")]
	private static extern int LogOutNative([In][MarshalAs(UnmanagedType.LPStr)] string module, LOG_LEVEL level, [In][MarshalAs(UnmanagedType.LPStr)] string line);

	[DllImport("RBA_SDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RBA_SDK_LogOutFor")]
	private static extern int LogOutNativeFor(uint instanceId, [In][MarshalAs(UnmanagedType.LPStr)] string module, LOG_LEVEL level, [In][MarshalAs(UnmanagedType.LPStr)] string line);

	object IRBA_API.GetMutableParam(PARAMETER_ID id)
	{
		throw new NotImplementedException();
	}
}
