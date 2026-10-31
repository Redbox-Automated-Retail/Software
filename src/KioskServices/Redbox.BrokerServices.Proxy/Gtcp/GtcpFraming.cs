using System;
using System.Buffers.Binary;
using System.IO;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// GTCP framing (gtcp-protocol.md section 2; GenuineChannels TcpConnectionManager.LowLevel_SendSync and
/// SyncSocketReadingStream). A message is sent as one or more frames:
/// <code>
///   byte  0x37        magic (MessageCoder.COMMAND_MAGIC_CODE)
///   int32 length      payload length of this frame, little-endian
///   byte  last        1 = last frame of the message
///   byte[length]      payload
/// </code>
/// An empty message (one frame, length 0, last 1) is a keep-alive ping.
/// </summary>
internal static class GtcpFraming
{
	public const byte Magic = 0x37;

	public const int HeaderSize = 6;

	/// <summary>GenuineChannels' TcpMaxSendSize default: the frame buffer, header included.</summary>
	public const int MaxFrameSize = 64000;

	public const int MaxPayloadPerFrame = MaxFrameSize - HeaderSize;

	/// <summary>
	/// Upper bound for one incoming message. GenuineChannels does not check sizes on receive (section 2); the kiosk's
	/// own channel limits were 500000 bytes, so anything far beyond that is treated as a broken stream.
	/// </summary>
	public const int MaxMessageSize = 4 * 1024 * 1024;

	public static readonly byte[] PingFrame = { Magic, 0, 0, 0, 0, 1 };

	/// <summary>Builds the frames of one message, ready to be written to the socket in one call.</summary>
	public static byte[] Encode(ReadOnlySpan<byte> message)
	{
		int frames = Math.Max(1, (message.Length + MaxPayloadPerFrame - 1) / MaxPayloadPerFrame);
		byte[] buffer = new byte[message.Length + frames * HeaderSize];
		int source = 0;
		int target = 0;
		for (int i = 0; i < frames; i++)
		{
			int length = Math.Min(MaxPayloadPerFrame, message.Length - source);
			buffer[target] = Magic;
			BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(target + 1), length);
			buffer[target + 5] = (byte)(i == frames - 1 ? 1 : 0);
			message.Slice(source, length).CopyTo(buffer.AsSpan(target + HeaderSize));
			source += length;
			target += HeaderSize + length;
		}
		return buffer;
	}

	/// <summary>
	/// Reads frames until one is marked last and returns the joined payload (empty for a ping). Zero-length
	/// continuation frames are legal (a sender whose content exactly fills its buffer emits one).
	/// </summary>
	public static byte[] ReadMessage(Stream stream)
	{
		byte[] header = new byte[HeaderSize];
		using MemoryStream message = new MemoryStream();
		while (true)
		{
			ReadExactly(stream, header, HeaderSize);
			if (header[0] != Magic)
			{
				throw new InvalidDataException($"GTCP frame starts with 0x{header[0]:x2} instead of 0x37.");
			}
			int length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(1));
			if (length < 0 || message.Length + length > MaxMessageSize)
			{
				throw new InvalidDataException($"GTCP frame length {length} is out of range.");
			}
			if (length > 0)
			{
				byte[] payload = new byte[length];
				ReadExactly(stream, payload, length);
				message.Write(payload, 0, length);
			}
			if (header[5] != 0)
			{
				return message.ToArray();
			}
		}
	}

	private static void ReadExactly(Stream stream, byte[] buffer, int count)
	{
		int offset = 0;
		while (offset < count)
		{
			int read = stream.Read(buffer, offset, count - offset);
			if (read <= 0)
			{
				throw new EndOfStreamException("The broker closed the connection.");
			}
			offset += read;
		}
	}
}
