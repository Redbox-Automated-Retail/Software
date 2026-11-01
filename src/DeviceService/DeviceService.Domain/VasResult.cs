using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel;
using RBA_SDK_ComponentModel;

namespace DeviceService.Domain;

public class VasResult
{
	public static readonly char[] VasStringSeperator = new char[1] { '\u001c' };

	public VasCmd VasCommand { get; set; }

	public ERROR_ID CommandResult { get; set; } = ERROR_ID.RESULT_ERROR;

	public VasStatus Status { get; set; } = VasStatus.Unknown;

	public string VasDataResult { get; set; }

	public string AdditionalData { get; set; }

	public List<string> Data { get; set; }

	public bool Success
	{
		get
		{
			if (CommandResult == ERROR_ID.RESULT_SUCCESS)
			{
				if (VasCommand == VasCmd.GetData)
				{
					return Status == VasStatus.Successful;
				}
				return true;
			}
			return false;
		}
	}

	public string GetVasData(ReadCardContext context)
	{
		List<string> data = Data;
		if (data == null || !data.Any())
		{
			return null;
		}
		if (string.IsNullOrEmpty(VasDataResult))
		{
			return null;
		}
		switch (VasDataResult)
		{
		case "0":
			context.Wallet = WalletType.Apple;
			return ParseApple(context);
		case "1":
			context.Wallet = WalletType.Google;
			return ParseGoogle();
		default:
			return null;
		}
	}

	private string ParseApple(ReadCardContext context)
	{
		string result = null;
		foreach (string datum in Data)
		{
			string[] parts = datum.Split(VasStringSeperator);
			if (parts[0] != 0.ToString() || parts[1] != "0")
			{
				continue;
			}
			_ = parts[2];
			_ = parts[3];
			string text = parts[4];
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				continue;
			}
			string hexData = parts[5];
			if (!string.IsNullOrEmpty(hexData) && hexData != "0")
			{
				string decodedData = ParseTags.ConvertHexToString(hexData, context?.Errors);
				if (!string.IsNullOrEmpty(decodedData))
				{
					result = decodedData;
					break;
				}
			}
		}
		return result;
	}

	private string ParseGoogle()
	{
		string vasData = null;
		string[] headerParts = Data[0].Split(VasStringSeperator);
		string headerResult = headerParts[0];
		string headerStatus = headerParts[1];
		if (headerResult != 0.ToString() || headerStatus != 0.ToString())
		{
			return null;
		}
		_ = headerParts[2];
		_ = headerParts[3];
		_ = headerParts[4];
		foreach (string line in Data.Skip(1))
		{
			string[] parts = line.Split(VasStringSeperator);
			if (!(parts[0] != 0.ToString()) && !(parts[1] != 0.ToString()))
			{
				vasData = parts[2];
				if (!string.IsNullOrWhiteSpace(vasData))
				{
					Enum.TryParse<VasCardType>(parts[3], out var _);
				}
			}
		}
		return vasData;
	}

	public override string ToString()
	{
		return string.Format("Status: {0}-({1}) - AdditionalData: {2} - VasDataResult: {3} - Data: {4}", Status, Status.ToString(), AdditionalData, VasDataResult, string.Join(",", Data));
	}
}
