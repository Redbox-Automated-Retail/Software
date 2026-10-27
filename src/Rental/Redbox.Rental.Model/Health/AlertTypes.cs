using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Health;

public static class AlertTypes
{
	public const string Qlm = "Qlm";

	public const string ReturnRejected = "ReturnRejected";

	public const string ApplicationCrash = "ApplicationCrash";

	public const string HardwareError = "HardwareError";

	public const string HeartbeatNotReceived = "HeartbeatNotReceived";

	public const string MachineNearingFull = "MachineNearingFull";

	public const string MachineFull = "MachineFull";

	public const string TitleEmpty = "TitleEmpty";

	private static readonly IDictionary<string, int> AlertMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{
		{ "Qlm", 1 },
		{ "ReturnRejected", 2 },
		{ "ApplicationCrash", 3 },
		{ "HardwareError", 4 },
		{ "HeartbeatNotReceived", 5 },
		{ "MachineNearingFull", 6 },
		{ "MachineFull", 7 },
		{ "TitleEmpty", 8 }
	};

	public static int ToAlertTypeNumeric(this string key)
	{
		if (!AlertMappings.ContainsKey(key))
		{
			return 0;
		}
		return AlertMappings[key];
	}
}
