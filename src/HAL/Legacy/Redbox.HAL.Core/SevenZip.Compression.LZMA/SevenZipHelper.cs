using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Compression.LZMA;

[ComVisible(true)]
public static class SevenZipHelper
{
	private static int dictionary = 8388608;

	private static bool eos = false;

	private static CoderPropID[] propIDs = new CoderPropID[8]
	{
		CoderPropID.DictionarySize,
		CoderPropID.PosStateBits,
		CoderPropID.LitContextBits,
		CoderPropID.LitPosBits,
		CoderPropID.Algorithm,
		CoderPropID.NumFastBytes,
		CoderPropID.MatchFinder,
		CoderPropID.EndMarker
	};

	private static object[] properties = new object[8] { dictionary, 2, 3, 0, 2, 128, "bt4", eos };

	public static byte[] Compress(byte[] inputBytes)
	{
		MemoryStream memoryStream = new MemoryStream(inputBytes);
		MemoryStream memoryStream2 = new MemoryStream();
		Encoder encoder = new Encoder();
		encoder.SetCoderProperties(propIDs, properties);
		encoder.WriteCoderProperties(memoryStream2);
		long length = memoryStream.Length;
		for (int i = 0; i < 8; i++)
		{
			memoryStream2.WriteByte((byte)(length >> 8 * i));
		}
		encoder.Code(memoryStream, memoryStream2, -1L, -1L, null);
		return memoryStream2.ToArray();
	}

	public static byte[] Decompress(byte[] inputBytes)
	{
		MemoryStream input = new MemoryStream(inputBytes);
		Decoder decoder = new Decoder();
		input.Seek(0L, SeekOrigin.Begin);
		MemoryStream output = new MemoryStream();
		byte[] properties = new byte[5];
		if (input.Read(properties, 0, 5) != 5)
		{
			throw new Exception("input .lzma is too short");
		}
		long outSize = 0L;
		for (int i = 0; i < 8; i++)
		{
			int sizeByte = input.ReadByte();
			if (sizeByte < 0)
			{
				throw new Exception("Can't Read 1");
			}
			outSize |= (long)((ulong)(byte)sizeByte << 8 * i);
		}
		decoder.SetDecoderProperties(properties);
		long inSize = input.Length - input.Position;
		decoder.Code(input, output, inSize, outSize, null);
		return output.ToArray();
	}
}
