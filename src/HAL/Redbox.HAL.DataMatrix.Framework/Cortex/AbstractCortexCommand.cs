using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal abstract class AbstractCortexCommand : IDisposable
{
	protected readonly List<CortexPacket> Packets = new List<CortexPacket>();

	protected readonly bool LogDetails = BarcodeConfiguration.Instance.LogDetailedScan;

	private readonly byte[] Prefix = new byte[4] { 238, 238, 238, 238 };

	private byte[] m_preppedCommand;

	private bool Disposed;

	public byte[] Command
	{
		get
		{
			if (m_preppedCommand == null)
			{
				m_preppedCommand = PrepCommand(CoreCommand);
			}
			return m_preppedCommand;
		}
	}

	public CortexPacket[] ParsedPackets => Packets.ToArray();

	protected virtual int ReadTimeout => 2000;

	protected abstract byte[] CoreCommand { get; }

	internal bool PortError { get; private set; }

	internal TimeSpan ExecutionTime { get; private set; }

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void Send(ICommPort port)
	{
		port.ValidateResponse = OnTest;
		if (port == null || !port.IsOpen)
		{
			LogHelper.Instance.Log("[Cortex Service] The port is not open sending a command.");
			return;
		}
		using ExecutionTimer executionTimer = new ExecutionTimer();
		if (!port.SendRecv(Command, ReadTimeout).CommOk)
		{
			LogHelper.Instance.Log("[Cortex Service] Communication error with the device.");
			PortError = true;
		}
		else if (!OnValidateResponse())
		{
			PortError = true;
		}
		executionTimer.Stop();
		ExecutionTime = executionTimer.Elapsed;
	}

	protected virtual bool CoreValidate()
	{
		return true;
	}

	protected virtual bool CoreProcess()
	{
		foreach (CortexPacket packet in Packets)
		{
			if (packet.PacketType == PacketType.Incomplete)
			{
				return false;
			}
			if (packet.RawPacketType == 'd')
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void OnDispose(bool disposing)
	{
	}

	protected bool OnTest(IChannelResponse r)
	{
		return OnTest(r.RawResponse);
	}

	protected virtual bool OnTest(byte[] array)
	{
		Packets.Clear();
		List<CortexPacket> list = PacketFactory.Instance.Parse(array);
		if (list.Count == 0)
		{
			return false;
		}
		if (list.FindAll((CortexPacket each) => each.PacketType == PacketType.Incomplete).Count > 0)
		{
			return false;
		}
		Packets.AddRange(list);
		return CoreProcess();
	}

	protected virtual bool OnValidateResponse()
	{
		if (Packets.Count == 0)
		{
			LogHelper.Instance.Log("[Cortex Commands] Validate response found no response packets.");
			return false;
		}
		foreach (CortexPacket packet in Packets)
		{
			if (packet.PacketType == PacketType.Incomplete)
			{
				LogHelper.Instance.Log("[Cortex Commands] Validate response found an incomplete packet.");
				return false;
			}
		}
		return CoreValidate();
	}

	protected void DumpPackets(List<CortexPacket> packets)
	{
		if (LogDetails)
		{
			LogHelper.Instance.Log(" [AbstractCortexCommand] packet statistics ");
			LogHelper.Instance.Log("  Packet received count: {0}", packets.Count);
			packets.ForEach(delegate(CortexPacket p)
			{
				LogHelper.Instance.Log("  packet type {0} raw {1} size {2}", p.PacketType, p.RawPacketType, p.Length);
			});
		}
	}

	private bool Test(IChannelResponse r)
	{
		return OnTest(r);
	}

	private byte[] PrepCommand(byte[] baseCommand)
	{
		List<byte> list = new List<byte>();
		try
		{
			list.AddRange(Prefix);
			ushort num = 0;
			for (int i = 0; i < baseCommand.Length; i++)
			{
				num = CRC16.CRC(num, baseCommand[i]);
				list.Add(baseCommand[i]);
			}
			byte item = (byte)((num >> 8) & 0x7F);
			list.Add(item);
			byte item2 = (byte)(num & 0x7F);
			list.Add(item2);
			return list.ToArray();
		}
		finally
		{
			list.Clear();
		}
	}

	private void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Disposed = true;
			OnDispose(disposing);
			Packets.Clear();
		}
	}
}
