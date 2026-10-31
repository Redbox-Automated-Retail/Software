using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// The two primitive encodings shared by every GTCP layer: little-endian integers and the length-prefixed UTF-8 string
/// that .NET's BinaryWriter.Write(string) produces (a 7-bit encoded length followed by the bytes). MS-NRBF's
/// LengthPrefixedString uses the same encoding.
/// </summary>
internal static class WireBinary
{
	public static void WriteString(Stream stream, string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		Write7BitLength(stream, bytes.Length);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void Write7BitLength(Stream stream, int length)
	{
		uint value = (uint)length;
		while (value >= 0x80)
		{
			stream.WriteByte((byte)(value | 0x80));
			value >>= 7;
		}
		stream.WriteByte((byte)value);
	}

	public static void WriteInt16(Stream stream, short value)
	{
		Span<byte> buffer = stackalloc byte[2];
		BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
		stream.Write(buffer);
	}

	public static void WriteInt32(Stream stream, int value)
	{
		Span<byte> buffer = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
		stream.Write(buffer);
	}

	public static void WriteInt64(Stream stream, long value)
	{
		Span<byte> buffer = stackalloc byte[8];
		BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
		stream.Write(buffer);
	}
}

/// <summary>Sequential reader over a byte array; throws <see cref="InvalidDataException"/> when the data runs out.</summary>
internal sealed class WireReader
{
	private readonly byte[] _data;

	public WireReader(byte[] data, int position = 0)
	{
		_data = data;
		Position = position;
	}

	public int Position { get; private set; }

	public bool AtEnd => Position >= _data.Length;

	public byte[] Data => _data;

	public byte ReadByte()
	{
		Require(1);
		return _data[Position++];
	}

	public bool ReadBoolean() => ReadByte() != 0;

	public short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(Take(2));

	public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));

	public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(Take(4));

	public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(Take(4));

	public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(Take(8));

	public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(Take(8));

	public float ReadSingle() => BinaryPrimitives.ReadSingleLittleEndian(Take(4));

	public double ReadDouble() => BinaryPrimitives.ReadDoubleLittleEndian(Take(8));

	public string ReadString()
	{
		int length = 0;
		int shift = 0;
		byte b;
		do
		{
			if (shift > 28)
			{
				throw new InvalidDataException("Malformed 7-bit encoded string length.");
			}
			b = ReadByte();
			length |= (b & 0x7F) << shift;
			shift += 7;
		}
		while ((b & 0x80) != 0);
		return Encoding.UTF8.GetString(Take(length));
	}

	/// <summary>Reads one UTF-8 encoded character (MS-NRBF Char).</summary>
	public char ReadUtf8Char()
	{
		byte first = _data[Position];
		int length = first < 0x80 ? 1 : first < 0xE0 ? 2 : first < 0xF0 ? 3 : 4;
		return Encoding.UTF8.GetString(Take(length))[0];
	}

	public byte[] ReadRemaining() => Take(_data.Length - Position).ToArray();

	private ReadOnlySpan<byte> Take(int count)
	{
		Require(count);
		ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(_data, Position, count);
		Position += count;
		return span;
	}

	private void Require(int count)
	{
		if (count < 0 || Position + count > _data.Length)
		{
			throw new InvalidDataException($"Unexpected end of data at offset {Position} (needed {count} more bytes).");
		}
	}
}
