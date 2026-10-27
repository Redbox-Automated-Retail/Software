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
			string[] array = datum.Split(VasStringSeperator);
			if (array[0] != 0.ToString() || array[1] != "0")
			{
				continue;
			}
			_ = array[2];
			_ = array[3];
			string text = array[4];
			if (string.IsNullOrEmpty(text) || text == "0")
			{
				continue;
			}
			string text2 = array[5];
			if (!string.IsNullOrEmpty(text2) && text2 != "0")
			{
				string text3 = ParseTags.ConvertHexToString(text2, context?.Errors);
				if (!string.IsNullOrEmpty(text3))
				{
					result = text3;
					break;
				}
			}
		}
		return result;
	}

	private string ParseGoogle()
	{
		string text = null;
		string[] array = Data[0].Split(VasStringSeperator);
		string text2 = array[0];
		string text3 = array[1];
		if (text2 != 0.ToString() || text3 != 0.ToString())
		{
			return null;
		}
		_ = array[2];
		_ = array[3];
		_ = array[4];
		foreach (string item in Data.Skip(1))
		{
			string[] array2 = item.Split(VasStringSeperator);
			if (!(array2[0] != 0.ToString()) && !(array2[1] != 0.ToString()))
			{
				text = array2[2];
				if (!string.IsNullOrWhiteSpace(text))
				{
					Enum.TryParse<VasCardType>(array2[3], out var _);
				}
			}
		}
		return text;
	}

	public override string ToString()
	{
		return string.Format("Status: {0}-({1}) - AdditionalData: {2} - VasDataResult: {3} - Data: {4}", Status, Status.ToString(), AdditionalData, VasDataResult, string.Join(",", Data));
	}
}
