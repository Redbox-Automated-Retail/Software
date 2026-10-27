using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.ProductLookupCatalog;

public class Archive : IDisposable
{
	private readonly FileStream m_file;

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

	public static Archive Create(string name)
	{
		return Create(name, DateTime.UtcNow);
	}

	public static Archive Open(string name)
	{
		if (!File.Exists(name))
		{
			throw new ArgumentException($"{name} does not exists.", "name");
		}
		return new Archive(name, readOnly: true);
	}

	public static Archive Open(string name, bool readOnly)
	{
		if (!File.Exists(name))
		{
			throw new ArgumentException($"{name} does not exists.", "name");
		}
		return new Archive(name, readOnly);
	}

	public static bool IsValid(string name)
	{
		using Archive archive = Open(name);
		return archive.IsValidArchive();
	}

	public bool IsValidArchive()
	{
		m_file.Seek(0L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(m_file, Encoding.ASCII);
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
		m_file.Seek(8L, SeekOrigin.Begin);
		return new BinaryReader(m_file, Encoding.ASCII).ReadByte();
	}

	public string GetOriginMachine()
	{
		m_file.Seek(10L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(m_file, Encoding.ASCII);
		return Encoding.ASCII.GetString(binaryReader.ReadBytes(32));
	}

	public byte GetFlags()
	{
		m_file.Seek(9L, SeekOrigin.Begin);
		return new BinaryReader(m_file, Encoding.ASCII).ReadByte();
	}

	public long GetSizeInKB()
	{
		if (m_file != null)
		{
			return m_file.Length / 1024;
		}
		return 0L;
	}

	public string GetCreatedOn()
	{
		m_file.Seek(42L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(m_file, Encoding.ASCII);
		return Encoding.ASCII.GetString(binaryReader.ReadBytes(14));
	}

	public void Write(Inventory inventory)
	{
		BinaryWriter writer = new BinaryWriter(m_file, Encoding.ASCII);
		Encode(inventory, writer);
	}

	public long GetNumberOfRecords()
	{
		return (m_file.Length - 312) / 18;
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
		if (m_file != null)
		{
			m_file.Close();
			m_file.Dispose();
		}
	}

	internal static Archive Create(string name, DateTime createdOn)
	{
		if (File.Exists(name))
		{
			throw new ArgumentException($"{name} already exists.", "name");
		}
		Archive archive = new Archive(name, readOnly: false);
		archive.WriteHeader(createdOn);
		return archive;
	}

	private void WriteHeader(DateTime createdOn)
	{
		BinaryWriter binaryWriter = new BinaryWriter(m_file, Encoding.ASCII);
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
		m_file.Seek(312 + index * 18, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(m_file, Encoding.ASCII);
		byte prefix = binaryReader.ReadByte();
		ulong integerPart = binaryReader.ReadUInt64();
		return DecompressBarcode(prefix, integerPart);
	}

	private Inventory ReadInventory(long index)
	{
		m_file.Seek(312 + index * 18, SeekOrigin.Begin);
		return Decode(new BinaryReader(m_file, Encoding.ASCII));
	}

	private void WriteInventory(long index, Inventory i)
	{
		m_file.Seek(312 + index * 18, SeekOrigin.Begin);
		BinaryWriter writer = new BinaryWriter(m_file, Encoding.ASCII);
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

	private Archive(string name, bool readOnly)
	{
		m_file = new FileStream(name, FileMode.OpenOrCreate, readOnly ? FileAccess.Read : FileAccess.ReadWrite);
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
