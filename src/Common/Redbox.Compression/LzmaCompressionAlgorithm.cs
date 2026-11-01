using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Redbox.Compression;

public class LzmaCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		using MemoryStream targetStream = new MemoryStream();
		using MemoryStream sourceStream = new MemoryStream(source);
		Compress(sourceStream, targetStream);
		return targetStream.ToArray();
	}

	public override byte[] Decompress(byte[] source)
	{
		using MemoryStream targetStream = new MemoryStream();
		using MemoryStream sourceStream = new MemoryStream(source);
		Decompress(sourceStream, targetStream);
		return targetStream.ToArray();
	}

	public override void Compress(Stream source, Stream target)
	{
		CoderPropID[] propIDs = new CoderPropID[8]
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
		object[] properties = new object[8] { 2097152, 2, 4, 0, 2, 273, "bt4", true };
		Encoder encoder = new Encoder();
		encoder.SetCoderProperties(propIDs, properties);
		encoder.WriteCoderProperties(target);
		long length = source.Length;
		for (int i = 0; i < 8; i++)
		{
			target.WriteByte((byte)(length >> 8 * i));
		}
		encoder.Code(source, target, -1L, -1L, null);
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] decoderProperties = new byte[5];
		if (source.Read(decoderProperties, 0, 5) != 5)
		{
			throw new Exception("input .lzma is too short");
		}
		Decoder decoder = new Decoder();
		decoder.SetDecoderProperties(decoderProperties);
		long outSize = 0L;
		for (int i = 0; i < 8; i++)
		{
			int v = source.ReadByte();
			if (v < 0)
			{
				throw new Exception("Can't Read 1");
			}
			outSize |= (long)((ulong)(byte)v << 8 * i);
		}
		long inSize = source.Length - source.Position;
		decoder.Code(source, target, inSize, outSize, null);
	}
}
