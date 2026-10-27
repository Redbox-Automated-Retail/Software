using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.ProductLookupCatalog;

public class MemoryArchive : IDisposable
{
	private readonly MemoryStream _stream;

	private const int RecordLength = 18;

	private const int HeaderLength = 312;

	private readonly byte[] m_header = Encoding.ASCII.GetBytes("<~look~>");

	private const string m_headerString = "<~look~>";

	private const byte Version = 0;

	public Inventory this[long i]
	{
		get
		{
			return ReadInventory(i);
		}
		set
		{
			WriteInventory(i, value);
		}
	}

	public long Count => GetNumberOfRecords();

	public byte[] Header => m_header;

	public static MemoryArchive Create()
	{
		MemoryArchive memoryArchive = new MemoryArchive();
		memoryArchive.WriteHeader(DateTime.UtcNow);
		return memoryArchive;
	}

	public static MemoryArchive Open(string name)
	{
		if (!File.Exists(name))
		{
			throw new ArgumentException($"{name} does not exists.", "name");
		}
		using FileStream stream = new FileStream(name, FileMode.Open, FileAccess.Read);
		return new MemoryArchive(stream);
	}

	public static MemoryArchive Open(Stream stream)
	{
		return new MemoryArchive(stream);
	}

	public static bool IsValid(string name)
	{
		using MemoryArchive memoryArchive = Open(name);
		return memoryArchive.IsValidArchive();
	}

	public static bool IsValid(Stream stream)
	{
		using MemoryArchive memoryArchive = Open(stream);
		return memoryArchive.IsValidArchive();
	}

	public byte[] ToArray()
	{
		return _stream.ToArray();
	}

	public bool IsValidArchive()
	{
		_stream.Seek(0L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(_stream, Encoding.ASCII);
		string text = Encoding.ASCII.GetString(binaryReader.ReadBytes(8));
		byte b = binaryReader.ReadByte();
		if (text == "<~look~>")
		{
			return b == 0;
		}
		return false;
	}

	public byte GetVersion()
	{
		_stream.Seek(8L, SeekOrigin.Begin);
		return new BinaryReader(_stream, Encoding.ASCII).ReadByte();
	}

	public string GetOriginMachine()
	{
		_stream.Seek(10L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(_stream, Encoding.ASCII);
		return Encoding.ASCII.GetString(binaryReader.ReadBytes(32));
	}

	public byte GetFlags()
	{
		_stream.Seek(9L, SeekOrigin.Begin);
		return new BinaryReader(_stream, Encoding.ASCII).ReadByte();
	}

	public long GetSizeInKB()
	{
		if (_stream != null)
		{
			return _stream.Length / 1024;
		}
		return 0L;
	}

	public string GetCreatedOn()
	{
		_stream.Seek(42L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(_stream, Encoding.ASCII);
		return Encoding.ASCII.GetString(binaryReader.ReadBytes(14));
	}

	public void Write(Inventory inventory)
	{
		BinaryWriter writer = new BinaryWriter(_stream, Encoding.ASCII);
		Encode(inventory, writer);
	}

	public long GetNumberOfRecords()
	{
		return (_stream.Length - 312) / 18;
	}

	public Inventory Find(string barcode)
	{
		long num = 0L;
		long num2 = GetNumberOfRecords() - 1;
		while (num <= num2)
		{
			long num3 = (num + num2) / 2;
			string text = ReadBarcode(num3);
			if (text.CompareTo(barcode) < 0)
			{
				num = num3 + 1;
				continue;
			}
			if (text.CompareTo(barcode) > 0)
			{
				num2 = num3 - 1;
				continue;
			}
			return ReadInventory(num3);
		}
		return null;
	}

	public long FindIndex(string barcode)
	{
		long num = 0L;
		long num2 = GetNumberOfRecords() - 1;
		while (num <= num2)
		{
			long num3 = (num + num2) / 2;
			string text = ReadBarcode(num3);
			if (text.CompareTo(barcode) < 0)
			{
				num = num3 + 1;
				continue;
			}
			if (text.CompareTo(barcode) > 0)
			{
				num2 = num3 - 1;
				continue;
			}
			return num3;
		}
		return -1L;
	}

	public void Dispose()
	{
		if (_stream != null)
		{
			_stream.Close();
			_stream.Dispose();
		}
	}

	private void WriteHeader(DateTime createdOn)
	{
		BinaryWriter binaryWriter = new BinaryWriter(_stream, Encoding.ASCII);
		binaryWriter.Write(Header);
		binaryWriter.Write((byte)0);
		binaryWriter.Write((byte)0);
		string machineName = Environment.MachineName;
		if (machineName.Length > 32)
		{
			machineName = machineName.Substring(0, 32);
			binaryWriter.Write(machineName);
		}
		else
		{
			binaryWriter.Write(Encoding.ASCII.GetBytes(new string(' ', 32 - machineName.Length)));
			binaryWriter.Write(Encoding.ASCII.GetBytes(machineName));
		}
		binaryWriter.Write(Encoding.ASCII.GetBytes(createdOn.ToString("yyyyMMddHHmmss")));
		byte[] array = new byte[256];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 0;
		}
		binaryWriter.Write(array);
	}

	private string ReadBarcode(long index)
	{
		_stream.Seek(312 + index * 18, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(_stream, Encoding.ASCII);
		byte prefix = binaryReader.ReadByte();
		ulong integerPart = binaryReader.ReadUInt64();
		return DecompressBarcode(prefix, integerPart);
	}

	private Inventory ReadInventory(long index)
	{
		_stream.Seek(312 + index * 18, SeekOrigin.Begin);
		return Decode(new BinaryReader(_stream, Encoding.ASCII));
	}

	private void WriteInventory(long index, Inventory i)
	{
		_stream.Seek(312 + index * 18, SeekOrigin.Begin);
		BinaryWriter writer = new BinaryWriter(_stream, Encoding.ASCII);
		Encode(i, writer);
	}

	private static Inventory Decode(BinaryReader reader)
	{
		Inventory inventory = new Inventory();
		byte prefix = reader.ReadByte();
		ulong integerPart = reader.ReadUInt64();
		inventory.Barcode = DecompressBarcode(prefix, integerPart);
		inventory.TitleId = reader.ReadUInt32();
		inventory.Code = (InventoryStatusCode)reader.ReadByte();
		inventory.TotalRentalCount = reader.ReadUInt32();
		return inventory;
	}

	private static void Encode(Inventory i, BinaryWriter writer)
	{
		KeyValuePair<byte, ulong> keyValuePair = CompressBarcode(i.Barcode);
		writer.Write(keyValuePair.Key);
		writer.Write(keyValuePair.Value);
		writer.Write(i.TitleId);
		writer.Write((byte)i.Code);
		writer.Write(i.TotalRentalCount);
	}

	private MemoryArchive()
	{
		_stream = new MemoryStream();
	}

	private MemoryArchive(Stream stream)
	{
		_stream = new MemoryStream();
		StreamCopy(stream, _stream, 4096);
	}

	private void StreamCopy(Stream source, Stream destination, int bufferSize)
	{
		byte[] array = new byte[bufferSize];
		int count;
		while ((count = source.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}

	private static string DecompressBarcode(byte prefix, ulong integerPart)
	{
		return $"{new string('0', prefix)}{integerPart}";
	}

	private static KeyValuePair<byte, ulong> CompressBarcode(string barcode)
	{
		if (!ulong.TryParse(barcode, out var _))
		{
			throw new FormatException($"{barcode} can not be encoded because it is not a natural number.");
		}
		byte b = 0;
		while (b < barcode.Length && barcode[b] == '0')
		{
			b++;
		}
		if (b == barcode.Length)
		{
			b--;
		}
		return new KeyValuePair<byte, ulong>(b, Convert.ToUInt64(barcode.Substring(b)));
	}
}
