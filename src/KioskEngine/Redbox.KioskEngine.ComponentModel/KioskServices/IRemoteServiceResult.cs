using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IRemoteServiceResult
{
	bool Success { get; }

	ErrorList Errors { get; }

	TimeSpan ExecutionTime { get; }

	IDictionary<string, object> Properties { get; }

	ReadOnlyCollection<IRemoteServiceProviderInstruction> Instructions { get; }

	T GetProperty<T>(string keyName);

	string ToObfuscatedString();
}
