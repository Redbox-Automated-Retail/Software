using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using Redbox.Core;

namespace Redbox.Installer.Client;

public abstract class RIXServiceCredentialsInstaller : System.Configuration.Install.Installer
{
	private const string ServiceNameSuffix = "service-name";

	private const string ServiceStartModeSuffix = "service-start-mode";

	private const string ServicesDependedOnSuffix = "services-depended-on";

	private const string ServiceAccountSuffix = "service-account";

	private const string ServiceUsernameSuffix = "service-username";

	private const string ServicePasswordSuffix = "service-password";

	private readonly ServiceInstaller _serviceInstaller = new ServiceInstaller();

	private readonly ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();

	private IContainer components;

	public abstract string PagePrefix { get; }

	public abstract string DefaultServiceNameValue { get; }

	public abstract ServiceStartMode DefaultServiceStartModeValue { get; }

	public abstract string DefaultServicesDependedOnValue { get; }

	public abstract ServiceAccount DefaultServiceAccountValue { get; }

	public abstract string DefaultServiceUsernameValue { get; }

	public abstract string DefaultServicePasswordValue { get; }

	public abstract string DisplayNameValue { get; }

	public abstract string DescriptionValue { get; }

	public override void Install(IDictionary stateSaver)
	{
		ConfigureServiceAndServiceProcessInstaller(stateSaver);
		WriteToLog("Install - _serviceInstaller.ServiceName: {0}", _serviceInstaller.ServiceName);
		WriteToLog("Install - _serviceInstaller.StartType: {0}", _serviceInstaller.StartType);
		WriteToLog("Install - _serviceProcessInstaller.Account: {0}", _serviceProcessInstaller.Account);
		WriteToLog("Install - _serviceProcessInstaller.Username: {0}", _serviceProcessInstaller.Username);
		try
		{
			base.Install(stateSaver);
		}
		catch (Exception ex)
		{
			WriteToLog("Install - exception calling base.Install! Exception is: {0}, {1}", ex.GetType(), ex.Message);
		}
	}

	public override void Uninstall(IDictionary savedState)
	{
		_serviceInstaller.ServiceName = savedState[PageVariableName("service-name")].ToString();
		_serviceInstaller.StartType = Enum<ServiceStartMode>.Parse(savedState[PageVariableName("service-start-mode")].ToString());
		_serviceProcessInstaller.Account = Enum<ServiceAccount>.Parse(savedState[PageVariableName("service-account")].ToString());
		WriteToLog("Uninstall - _serviceInstaller.ServiceName: {0}", _serviceInstaller.ServiceName);
		WriteToLog("Uninstall - _serviceInstaller.StartType: {0}", _serviceInstaller.StartType);
		WriteToLog("Uninstall - _serviceProcessInstaller.Account: {0}", _serviceProcessInstaller.Account);
		try
		{
			base.Uninstall(savedState);
		}
		catch (Exception ex)
		{
			WriteToLog("Uninstall - exception calling base.Uninstall! Exception is: {0}, {1}", ex.GetType(), ex.Message);
		}
	}

	protected RIXServiceCredentialsInstaller()
	{
		InitializeComponent();
		base.Installers.Add(_serviceInstaller);
		base.Installers.Add(_serviceProcessInstaller);
	}

	private string PageVariableName(string suffix)
	{
		return $"{PagePrefix}:{suffix}";
	}

	private KeyValuePair<string, string> DetermineStringValue(string suffix, string defaultvalue, IDictionary state)
	{
		string text = PageVariableName(suffix);
		WriteToLog("DetermineStringValue - key: {0}", text);
		string text2 = base.Context.Parameters[text];
		if (string.IsNullOrEmpty(text2))
		{
			WriteToLog("DetermineStringValue - parameter not found; using default value: '{0}'...", defaultvalue);
			text2 = defaultvalue;
		}
		else
		{
			text2 = text2.Trim('"');
		}
		WriteToLog("DetermineStringValue - value: '{0}'", text2);
		state[text] = text2;
		return new KeyValuePair<string, string>(text, text2);
	}

	private KeyValuePair<string, string[]> DetermineDelimitedStringValue(string suffix, string defaultvalue, IDictionary state)
	{
		KeyValuePair<string, string> keyValuePair = DetermineStringValue(suffix, defaultvalue, state);
		string[] parts = keyValuePair.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		WriteToLog("DetermineDelimitedStringValue - values: '{0}'", EnumerableExtensions.Join(parts, ","));
		state[keyValuePair.Key] = parts;
		return new KeyValuePair<string, string[]>(keyValuePair.Key, parts);
	}

	private KeyValuePair<string, T> DetermineEnumValue<T>(string suffix, T defaultvalue, IDictionary state)
	{
		KeyValuePair<string, string> keyValuePair = DetermineStringValue(suffix, defaultvalue.ToString(), state);
		T val;
		try
		{
			val = Enum<T>.Parse(keyValuePair.Value);
		}
		catch (Exception ex)
		{
			WriteToLog("DetermineEnumValue - in catch: " + ex.Message);
			WriteToLog("Unable to parse {0} value '{1}'!", typeof(T), keyValuePair);
			val = defaultvalue;
		}
		WriteToLog("DetermineEnumValue - determinedvalue: " + val);
		state[keyValuePair.Key] = val;
		return new KeyValuePair<string, T>(keyValuePair.Key, val);
	}

	private KeyValuePair<string, string> DetermineEncryptedValue(string suffix, string defaultvalue, IDictionary state)
	{
		KeyValuePair<string, string> keyValuePair = DetermineStringValue(suffix, defaultvalue, state);
		string value = PasswordHelper.Decrypt(keyValuePair.Value);
		state[keyValuePair.Key] = value;
		return new KeyValuePair<string, string>(keyValuePair.Key, value);
	}

	private void ConfigureServiceAndServiceProcessInstaller(IDictionary state)
	{
		_serviceInstaller.ServiceName = DetermineStringValue("service-name", DefaultServiceNameValue, state).Value;
		_serviceInstaller.StartType = DetermineEnumValue("service-start-mode", DefaultServiceStartModeValue, state).Value;
		_serviceInstaller.ServicesDependedOn = DetermineDelimitedStringValue("services-depended-on", DefaultServicesDependedOnValue, state).Value;
		_serviceProcessInstaller.Account = DetermineEnumValue("service-account", DefaultServiceAccountValue, state).Value;
		if (_serviceProcessInstaller.Account == ServiceAccount.User)
		{
			_serviceProcessInstaller.Username = DetermineStringValue("service-username", DefaultServiceUsernameValue, state).Value;
			if (!_serviceProcessInstaller.Username.Contains("\\"))
			{
				_serviceProcessInstaller.Username = Environment.MachineName + "\\" + _serviceProcessInstaller.Username;
			}
			base.Context.Parameters["username"] = _serviceProcessInstaller.Username;
			_serviceProcessInstaller.Password = DetermineEncryptedValue("service-password", DefaultServicePasswordValue, state).Value;
			base.Context.Parameters["password"] = _serviceProcessInstaller.Password;
		}
	}

	private void WriteToLog(string mask, params object[] parameters)
	{
		using StreamWriter streamWriter = new StreamWriter(base.Context.Parameters["logfile"], append: true);
		streamWriter.WriteLine(mask, parameters);
		streamWriter.Flush();
		streamWriter.Close();
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
		components = new Container();
	}
}
