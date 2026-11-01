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
		MemoryStream inStream = new MemoryStream(inputBytes);
		MemoryStream outStream = new MemoryStream();
		Encoder encoder = new Encoder();
		encoder.SetCoderProperties(propIDs, properties);
		encoder.WriteCoderProperties(outStream);
		long length = inStream.Length;
		for (int i = 0; i < 8; i++)
		{
			outStream.WriteByte((byte)(length >> 8 * i));
		}
		encoder.Code(inStream, outStream, -1L, -1L, null);
		return outStream.ToArray();
	}

	public static byte[] Decompress(byte[] inputBytes)
	{
		MemoryStream inStream = new MemoryStream(inputBytes);
		Decoder decoder = new Decoder();
		inStream.Seek(0L, SeekOrigin.Begin);
		MemoryStream outStream = new MemoryStream();
		byte[] decoderProperties = new byte[5];
		if (inStream.Read(decoderProperties, 0, 5) != 5)
		{
			throw new Exception("input .lzma is too short");
		}
		long outSize = 0L;
		for (int i = 0; i < 8; i++)
		{
			int v = inStream.ReadByte();
			if (v < 0)
			{
				throw new Exception("Can't Read 1");
			}
			outSize |= (long)((ulong)(byte)v << 8 * i);
		}
		decoder.SetDecoderProperties(decoderProperties);
		long inSize = inStream.Length - inStream.Position;
		decoder.Code(inStream, outStream, inSize, outSize, null);
		return outStream.ToArray();
	}
}
