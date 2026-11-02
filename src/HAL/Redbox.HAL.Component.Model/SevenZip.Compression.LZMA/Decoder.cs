using System;
using System.IO;
using System.Runtime.InteropServices;
using SevenZip.Compression.LZ;
using SevenZip.Compression.RangeCoder;

namespace SevenZip.Compression.LZMA;

[ComVisible(true)]
public class Decoder : ICoder, ISetDecoderProperties
{
	private class LenDecoder
	{
		private BitDecoder m_Choice = default(BitDecoder);

		private BitDecoder m_Choice2 = default(BitDecoder);

		private BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[16];

		private BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[16];

		private BitTreeDecoder m_HighCoder = new BitTreeDecoder(8);

		private uint m_NumPosStates;

		public void Create(uint numPosStates)
		{
			for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
			{
				m_LowCoder[posState] = new BitTreeDecoder(3);
				m_MidCoder[posState] = new BitTreeDecoder(3);
			}
			m_NumPosStates = numPosStates;
		}

		public void Init()
		{
			m_Choice.Init();
			for (uint posState = 0u; posState < m_NumPosStates; posState++)
			{
				m_LowCoder[posState].Init();
				m_MidCoder[posState].Init();
			}
			m_Choice2.Init();
			m_HighCoder.Init();
		}

		public uint Decode(SevenZip.Compression.RangeCoder.Decoder rangeDecoder, uint posState)
		{
			if (m_Choice.Decode(rangeDecoder) == 0)
			{
				return m_LowCoder[posState].Decode(rangeDecoder);
			}
			uint symbol = 8u;
			if (m_Choice2.Decode(rangeDecoder) == 0)
			{
				return symbol + m_MidCoder[posState].Decode(rangeDecoder);
			}
			symbol += 8;
			return symbol + m_HighCoder.Decode(rangeDecoder);
		}
	}

	private class LiteralDecoder
	{
		private struct Decoder2
		{
			private BitDecoder[] m_Decoders;

			public void Create()
			{
				m_Decoders = new BitDecoder[768];
			}

			public void Init()
			{
				for (int i = 0; i < 768; i++)
				{
					m_Decoders[i].Init();
				}
			}

			public byte DecodeNormal(SevenZip.Compression.RangeCoder.Decoder rangeDecoder)
			{
				uint symbol = 1u;
				do
				{
					symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
				}
				while (symbol < 256);
				return (byte)symbol;
			}

			public byte DecodeWithMatchByte(SevenZip.Compression.RangeCoder.Decoder rangeDecoder, byte matchByte)
			{
				uint symbol = 1u;
				do
				{
					uint matchBit = (uint)((matchByte >> 7) & 1);
					matchByte <<= 1;
					uint bit = m_Decoders[(1 + matchBit << 8) + symbol].Decode(rangeDecoder);
					symbol = (symbol << 1) | bit;
					if (matchBit != bit)
					{
						while (symbol < 256)
						{
							symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
						}
						break;
					}
				}
				while (symbol < 256);
				return (byte)symbol;
			}
		}

		// Set by Create.
		private Decoder2[] m_Coders = null!;

		private int m_NumPrevBits;

		private int m_NumPosBits;

		private uint m_PosMask;

		public void Create(int numPosBits, int numPrevBits)
		{
			if (m_Coders == null || m_NumPrevBits != numPrevBits || m_NumPosBits != numPosBits)
			{
				m_NumPosBits = numPosBits;
				m_PosMask = (uint)((1 << numPosBits) - 1);
				m_NumPrevBits = numPrevBits;
				uint numStates = (uint)(1 << m_NumPrevBits + m_NumPosBits);
				m_Coders = new Decoder2[numStates];
				for (uint i = 0u; i < numStates; i++)
				{
					m_Coders[i].Create();
				}
			}
		}

		public void Init()
		{
			uint numStates = (uint)(1 << m_NumPrevBits + m_NumPosBits);
			for (uint i = 0u; i < numStates; i++)
			{
				m_Coders[i].Init();
			}
		}

		private uint GetState(uint pos, byte prevByte)
		{
			return ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> 8 - m_NumPrevBits);
		}

		public byte DecodeNormal(SevenZip.Compression.RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
		{
			return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder);
		}

		public byte DecodeWithMatchByte(SevenZip.Compression.RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
		{
			return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte);
		}
	}

	private OutWindow m_OutWindow = new OutWindow();

	private SevenZip.Compression.RangeCoder.Decoder m_RangeDecoder = new SevenZip.Compression.RangeCoder.Decoder();

	private BitDecoder[] m_IsMatchDecoders = new BitDecoder[192];

	private BitDecoder[] m_IsRepDecoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG0Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG1Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRepG2Decoders = new BitDecoder[12];

	private BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[192];

	private BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[4];

	private BitDecoder[] m_PosDecoders = new BitDecoder[114];

	private BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(4);

	private LenDecoder m_LenDecoder = new LenDecoder();

	private LenDecoder m_RepLenDecoder = new LenDecoder();

	private LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

	private uint m_DictionarySize;

	private uint m_DictionarySizeCheck;

	private uint m_PosStateMask;

	public Decoder()
	{
		m_DictionarySize = uint.MaxValue;
		for (int i = 0; (long)i < 4L; i++)
		{
			m_PosSlotDecoder[i] = new BitTreeDecoder(6);
		}
	}

	private void SetDictionarySize(uint dictionarySize)
	{
		if (m_DictionarySize != dictionarySize)
		{
			m_DictionarySize = dictionarySize;
			m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1u);
			uint windowSize = Math.Max(m_DictionarySizeCheck, 4096u);
			m_OutWindow.Create(windowSize);
		}
	}

	private void SetLiteralProperties(int lp, int lc)
	{
		if (lp > 8)
		{
			throw new InvalidParamException();
		}
		if (lc > 8)
		{
			throw new InvalidParamException();
		}
		m_LiteralDecoder.Create(lp, lc);
	}

	private void SetPosBitsProperties(int pb)
	{
		if (pb > 4)
		{
			throw new InvalidParamException();
		}
		uint numPosStates = (uint)(1 << pb);
		m_LenDecoder.Create(numPosStates);
		m_RepLenDecoder.Create(numPosStates);
		m_PosStateMask = numPosStates - 1;
	}

	private void Init(Stream inStream, Stream outStream)
	{
		m_RangeDecoder.Init(inStream);
		m_OutWindow.Init(outStream);
		for (uint i = 0u; i < 12; i++)
		{
			for (uint j = 0u; j <= m_PosStateMask; j++)
			{
				uint index = (i << 4) + j;
				m_IsMatchDecoders[index].Init();
				m_IsRep0LongDecoders[index].Init();
			}
			m_IsRepDecoders[i].Init();
			m_IsRepG0Decoders[i].Init();
			m_IsRepG1Decoders[i].Init();
			m_IsRepG2Decoders[i].Init();
		}
		m_LiteralDecoder.Init();
		for (uint i = 0u; i < 4; i++)
		{
			m_PosSlotDecoder[i].Init();
		}
		for (uint i = 0u; i < 114; i++)
		{
			m_PosDecoders[i].Init();
		}
		m_LenDecoder.Init();
		m_RepLenDecoder.Init();
		m_PosAlignDecoder.Init();
	}

	public void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress? progress)
	{
		Init(inStream, outStream);
		Base.State state = default(Base.State);
		state.Init();
		uint rep0 = 0u;
		uint rep1 = 0u;
		uint rep2 = 0u;
		uint rep3 = 0u;
		ulong nowPos64 = 0uL;
		if (nowPos64 < (ulong)outSize)
		{
			if (m_IsMatchDecoders[state.Index << 4].Decode(m_RangeDecoder) != 0)
			{
				throw new DataErrorException();
			}
			state.UpdateChar();
			byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0u, 0);
			m_OutWindow.PutByte(b);
			nowPos64++;
		}
		while (nowPos64 < (ulong)outSize)
		{
			uint posState = (uint)(int)nowPos64 & m_PosStateMask;
			if (m_IsMatchDecoders[(state.Index << 4) + posState].Decode(m_RangeDecoder) == 0)
			{
				byte prevByte = m_OutWindow.GetByte(0u);
				byte b = (state.IsCharState() ? m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)nowPos64, prevByte) : m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder, (uint)nowPos64, prevByte, m_OutWindow.GetByte(rep0)));
				m_OutWindow.PutByte(b);
				state.UpdateChar();
				nowPos64++;
				continue;
			}
			uint len;
			if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
			{
				if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
				{
					if (m_IsRep0LongDecoders[(state.Index << 4) + posState].Decode(m_RangeDecoder) == 0)
					{
						state.UpdateShortRep();
						m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
						nowPos64++;
						continue;
					}
				}
				else
				{
					uint rep;
					if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
					{
						rep = rep1;
					}
					else
					{
						if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
						{
							rep = rep2;
						}
						else
						{
							rep = rep3;
							rep3 = rep2;
						}
						rep2 = rep1;
					}
					rep1 = rep0;
					rep0 = rep;
				}
				len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + 2;
				state.UpdateRep();
			}
			else
			{
				rep3 = rep2;
				rep2 = rep1;
				rep1 = rep0;
				len = 2 + m_LenDecoder.Decode(m_RangeDecoder, posState);
				state.UpdateMatch();
				uint posSlot = m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(m_RangeDecoder);
				if (posSlot >= 4)
				{
					int numDirectBits = (int)((posSlot >> 1) - 1);
					rep0 = (2 | (posSlot & 1)) << numDirectBits;
					if (posSlot < 14)
					{
						rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders, rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
					}
					else
					{
						rep0 += m_RangeDecoder.DecodeDirectBits(numDirectBits - 4) << 4;
						rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
					}
				}
				else
				{
					rep0 = posSlot;
				}
			}
			if (rep0 >= nowPos64 || rep0 >= m_DictionarySizeCheck)
			{
				if (rep0 == uint.MaxValue)
				{
					break;
				}
				throw new DataErrorException();
			}
			m_OutWindow.CopyBlock(rep0, len);
			nowPos64 += len;
		}
		m_OutWindow.Flush();
		m_OutWindow.ReleaseStream();
		m_RangeDecoder.ReleaseStream();
	}

	public void SetDecoderProperties(byte[] properties)
	{
		if (properties.Length < 5)
		{
			throw new InvalidParamException();
		}
		int lc = properties[0] % 9;
		int remainder = properties[0] / 9;
		int lp = remainder % 5;
		int pb = remainder / 5;
		if (pb > 4)
		{
			throw new InvalidParamException();
		}
		uint dictionarySize = 0u;
		for (int i = 0; i < 4; i++)
		{
			dictionarySize += (uint)(properties[1 + i] << i * 8);
		}
		SetDictionarySize(dictionarySize);
		SetLiteralProperties(lp, lc);
		SetPosBitsProperties(pb);
	}
}
