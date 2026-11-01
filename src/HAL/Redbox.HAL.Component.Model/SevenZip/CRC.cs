using System.Runtime.InteropServices;

namespace SevenZip;

[ComVisible(true)]
public class CRC
{
	public static readonly uint[] Table;

	private uint _value = uint.MaxValue;

	static CRC()
	{
		Table = new uint[256];
		for (uint i = 0u; i < 256; i++)
		{
			uint r = i;
			for (int j = 0; j < 8; j++)
			{
				r = (((r & 1) == 0) ? (r >> 1) : ((r >> 1) ^ 0xEDB88320u));
			}
			Table[i] = r;
		}
	}

	public void Init()
	{
		_value = uint.MaxValue;
	}

	public void UpdateByte(byte b)
	{
		_value = Table[(byte)_value ^ b] ^ (_value >> 8);
	}

	public void Update(byte[] data, uint offset, uint size)
	{
		for (uint i = 0u; i < size; i++)
		{
			_value = Table[(byte)_value ^ data[offset + i]] ^ (_value >> 8);
		}
	}

	public uint GetDigest()
	{
		return _value ^ 0xFFFFFFFFu;
	}

	private static uint CalculateDigest(byte[] data, uint offset, uint size)
	{
		CRC cRC = new CRC();
		cRC.Update(data, offset, size);
		return cRC.GetDigest();
	}

	private static bool VerifyDigest(uint digest, byte[] data, uint offset, uint size)
	{
		return CalculateDigest(data, offset, size) == digest;
	}
}
