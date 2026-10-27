using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

public class RemoteServiceResult : IRemoteServiceResult
{
	private readonly ErrorList m_errors = new ErrorList();

	private readonly IDictionary<string, object> m_properties = new Dictionary<string, object>();

	private readonly List<IRemoteServiceProviderInstruction> m_instructions = new List<IRemoteServiceProviderInstruction>();

	public bool Success { get; set; }

	public ErrorList Errors => m_errors;

	public TimeSpan ExecutionTime { get; set; }

	public IDictionary<string, object> Properties => m_properties;

	public ReadOnlyCollection<IRemoteServiceProviderInstruction> Instructions => InnerInstructions.AsReadOnly();

	public List<IRemoteServiceProviderInstruction> InnerInstructions => m_instructions;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(this.ToJson());
		return stringBuilder.ToString();
	}

	public string ToObfuscatedString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		stringBuilder.Append("\"Success\":" + Success.ToJson() + ",");
		stringBuilder.Append("\"Errors\":" + Errors?.ToJson() + ",");
		stringBuilder.Append("\"ExecutionTime\":" + ExecutionTime.ToJson() + ",");
		Dictionary<string, object> dictionary = new Dictionary<string, object>(Properties);
		ServiceLocator.Instance.GetService<IObfuscationService>()?.ObfuscateDictionary(dictionary);
		stringBuilder.Append("\"Properties\":" + dictionary.ToJson() + ",");
		stringBuilder.Append("\"Instructions\":" + Instructions?.ToJson());
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public void AddError(string code, string message, string details)
	{
		m_errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError(code, message, details));
	}

	public T GetProperty<T>(string keyName)
	{
		object obj = (m_properties.ContainsKey(keyName) ? m_properties[keyName] : null);
		if (typeof(T) == typeof(string))
		{
			obj = ((obj != null) ? obj.ToString() : string.Empty);
		}
		else if (typeof(T) == typeof(int))
		{
			obj = GetInt(obj);
		}
		if (obj != null)
		{
			return (T)obj;
		}
		return default(T);
	}

	private static int GetInt(object obj)
	{
		int result = 0;
		Type type = obj.GetType();
		if (type == typeof(int))
		{
			result = (int)obj;
		}
		else if (type == typeof(string))
		{
			int.TryParse((string)obj, out result);
		}
		return result;
	}
}
