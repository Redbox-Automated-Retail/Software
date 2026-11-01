using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class DecodeCommand : AbstractCortexCommand
{
	internal readonly List<string> FoundCodes = new List<string>();

	private readonly List<int> DataPackets = new List<int>();

	private readonly byte[] m_rawCommand = new byte[4] { 36, 1, 5, 0 };

	private readonly char[] Separator = new char[1] { ' ' };

	protected override byte[] CoreCommand => m_rawCommand;

	protected override int ReadTimeout => BarcodeConfiguration.Instance.ScanTimeout;

	internal bool DecodeFailure { get; private set; }

	protected override bool CoreValidate()
	{
		int doneResponses = 0;
		int errorResponses = 0;
		int dataPacketCount = 0;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'd')
			{
				doneResponses++;
			}
			else if (packet.RawPacketType == 'r')
			{
				errorResponses++;
			}
			else if (packet.PacketType == PacketType.Data)
			{
				dataPacketCount++;
			}
		}
		if (doneResponses == 0)
		{
			return false;
		}
		if (errorResponses <= 0)
		{
			return dataPacketCount > 0;
		}
		return true;
	}

	protected override bool CoreProcess()
	{
		bool sawDone = false;
		if (LogDetails)
		{
			LogHelper.Instance.Log("[Decode Command] start.");
		}
		try
		{
			foreach (CortexPacket packet in Packets)
			{
				if (packet.PacketType == PacketType.Incomplete)
				{
					return false;
				}
				if (packet.PacketType == PacketType.Data)
				{
					string text = Encoding.ASCII.GetString(packet.PayloadData);
					if (LogDetails)
					{
						LogHelper.Instance.Log(" [DecodeCommand] Data packet: barcode data = {0} number = {1}", text, packet.PacketNumber);
					}
					if (DataPackets.FindAll((int each) => each == packet.PacketNumber).Count != 0)
					{
						continue;
					}
					DataPackets.Add(packet.PacketNumber);
					string[] parts = text.Split(Separator);
					if ((int)Convert.ChangeType(parts[2], typeof(int)) == 0)
					{
						DecodeFailure = true;
						continue;
					}
					for (int partIndex = 3; partIndex < parts.Length; partIndex++)
					{
						FoundCodes.Add(parts[partIndex]);
					}
				}
				else
				{
					if (LogDetails)
					{
						LogHelper.Instance.Log(" [DecodeCommand] Reponse packet '{0}'", packet.RawPacketType);
					}
					switch (packet.RawPacketType)
					{
					case 'e':
					case 'r':
						DecodeFailure = true;
						break;
					case 'd':
						sawDone = true;
						break;
					}
				}
			}
			if (!sawDone || DataPackets.Count == 0)
			{
				return false;
			}
			DumpPackets(Packets);
			if (LogDetails)
			{
				LogHelper.Instance.Log("  [DecodeCommand] Decode results:");
				FoundCodes.ForEach(delegate(string code)
				{
					LogHelper.Instance.Log("  Scanned code {0}", code);
				});
			}
			return DataPackets.Count > 0 || DecodeFailure;
		}
		finally
		{
			if (LogDetails)
			{
				LogHelper.Instance.Log("[Decode Command] End.");
			}
		}
	}

	protected override void OnDispose(bool disposing)
	{
		DataPackets.Clear();
	}

	internal DecodeCommand()
	{
	}
}
