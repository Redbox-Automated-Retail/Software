using System;
using System.IO;
using System.Runtime.InteropServices;
using SevenZip.Compression.LZ;
using SevenZip.Compression.RangeCoder;

namespace SevenZip.Compression.LZMA;

[ComVisible(true)]
public class Encoder : ICoder, ISetCoderProperties, IWriteCoderProperties
{
	private enum EMatchFinderType
	{
		BT2,
		BT4
	}

	private class LiteralEncoder
	{
		public struct Encoder2
		{
			private BitEncoder[] m_Encoders;

			public void Create()
			{
				m_Encoders = new BitEncoder[768];
			}

			public void Init()
			{
				for (int i = 0; i < 768; i++)
				{
					m_Encoders[i].Init();
				}
			}

			public void Encode(SevenZip.Compression.RangeCoder.Encoder rangeEncoder, byte symbol)
			{
				uint context = 1u;
				for (int i = 7; i >= 0; i--)
				{
					uint bit = (uint)((symbol >> i) & 1);
					m_Encoders[context].Encode(rangeEncoder, bit);
					context = (context << 1) | bit;
				}
			}

			public void EncodeMatched(SevenZip.Compression.RangeCoder.Encoder rangeEncoder, byte matchByte, byte symbol)
			{
				uint context = 1u;
				bool same = true;
				for (int i = 7; i >= 0; i--)
				{
					uint bit = (uint)((symbol >> i) & 1);
					uint state = context;
					if (same)
					{
						uint matchBit = (uint)((matchByte >> i) & 1);
						state += 1 + matchBit << 8;
						same = matchBit == bit;
					}
					m_Encoders[state].Encode(rangeEncoder, bit);
					context = (context << 1) | bit;
				}
			}

			public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
			{
				uint price = 0u;
				uint context = 1u;
				int i = 7;
				if (matchMode)
				{
					while (i >= 0)
					{
						uint matchBit = (uint)((matchByte >> i) & 1);
						uint bit = (uint)((symbol >> i) & 1);
						price += m_Encoders[(1 + matchBit << 8) + context].GetPrice(bit);
						context = (context << 1) | bit;
						if (matchBit != bit)
						{
							i--;
							break;
						}
						i--;
					}
				}
				while (i >= 0)
				{
					uint bit = (uint)((symbol >> i) & 1);
					price += m_Encoders[context].GetPrice(bit);
					context = (context << 1) | bit;
					i--;
				}
				return price;
			}
		}

		private Encoder2[] m_Coders;

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
				m_Coders = new Encoder2[numStates];
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

		public Encoder2 GetSubCoder(uint pos, byte prevByte)
		{
			return m_Coders[(int)((pos & m_PosMask) << m_NumPrevBits) + (prevByte >> 8 - m_NumPrevBits)];
		}
	}

	private class LenEncoder
	{
		private BitEncoder _choice = default(BitEncoder);

		private BitEncoder _choice2 = default(BitEncoder);

		private BitTreeEncoder[] _lowCoder = new BitTreeEncoder[16];

		private BitTreeEncoder[] _midCoder = new BitTreeEncoder[16];

		private BitTreeEncoder _highCoder = new BitTreeEncoder(8);

		public LenEncoder()
		{
			for (uint posState = 0u; posState < 16; posState++)
			{
				_lowCoder[posState] = new BitTreeEncoder(3);
				_midCoder[posState] = new BitTreeEncoder(3);
			}
		}

		public void Init(uint numPosStates)
		{
			_choice.Init();
			_choice2.Init();
			for (uint posState = 0u; posState < numPosStates; posState++)
			{
				_lowCoder[posState].Init();
				_midCoder[posState].Init();
			}
			_highCoder.Init();
		}

		public void Encode(SevenZip.Compression.RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
		{
			if (symbol < 8)
			{
				_choice.Encode(rangeEncoder, 0u);
				_lowCoder[posState].Encode(rangeEncoder, symbol);
				return;
			}
			symbol -= 8;
			_choice.Encode(rangeEncoder, 1u);
			if (symbol < 8)
			{
				_choice2.Encode(rangeEncoder, 0u);
				_midCoder[posState].Encode(rangeEncoder, symbol);
			}
			else
			{
				_choice2.Encode(rangeEncoder, 1u);
				_highCoder.Encode(rangeEncoder, symbol - 8);
			}
		}

		public void SetPrices(uint posState, uint numSymbols, uint[] prices, uint st)
		{
			uint a0 = _choice.GetPrice0();
			uint a1 = _choice.GetPrice1();
			uint b0 = a1 + _choice2.GetPrice0();
			uint b1 = a1 + _choice2.GetPrice1();
			uint i = 0u;
			for (i = 0u; i < 8; i++)
			{
				if (i >= numSymbols)
				{
					return;
				}
				prices[st + i] = a0 + _lowCoder[posState].GetPrice(i);
			}
			for (; i < 16; i++)
			{
				if (i >= numSymbols)
				{
					return;
				}
				prices[st + i] = b0 + _midCoder[posState].GetPrice(i - 8);
			}
			for (; i < numSymbols; i++)
			{
				prices[st + i] = b1 + _highCoder.GetPrice(i - 8 - 8);
			}
		}
	}

	private class LenPriceTableEncoder : LenEncoder
	{
		private uint[] _prices = new uint[4352];

		private uint _tableSize;

		private uint[] _counters = new uint[16];

		public void SetTableSize(uint tableSize)
		{
			_tableSize = tableSize;
		}

		public uint GetPrice(uint symbol, uint posState)
		{
			return _prices[posState * 272 + symbol];
		}

		private void UpdateTable(uint posState)
		{
			SetPrices(posState, _tableSize, _prices, posState * 272);
			_counters[posState] = _tableSize;
		}

		public void UpdateTables(uint numPosStates)
		{
			for (uint posState = 0u; posState < numPosStates; posState++)
			{
				UpdateTable(posState);
			}
		}

		public new void Encode(SevenZip.Compression.RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
		{
			base.Encode(rangeEncoder, symbol, posState);
			if (--_counters[posState] == 0)
			{
				UpdateTable(posState);
			}
		}
	}

	private class Optimal
	{
		public Base.State State;

		public bool Prev1IsChar;

		public bool Prev2;

		public uint PosPrev2;

		public uint BackPrev2;

		public uint Price;

		public uint PosPrev;

		public uint BackPrev;

		public uint Backs0;

		public uint Backs1;

		public uint Backs2;

		public uint Backs3;

		public void MakeAsChar()
		{
			BackPrev = uint.MaxValue;
			Prev1IsChar = false;
		}

		public void MakeAsShortRep()
		{
			BackPrev = 0u;
			Prev1IsChar = false;
		}

		public bool IsShortRep()
		{
			return BackPrev == 0;
		}
	}

	private const uint kIfinityPrice = 268435455u;

	private const int kDefaultDictionaryLogSize = 22;

	private const uint kNumFastBytesDefault = 32u;

	private const uint kNumLenSpecSymbols = 16u;

	private const uint kNumOpts = 4096u;

	private const int kPropSize = 5;

	private static byte[] g_FastPos;

	private Base.State _state = default(Base.State);

	private byte _previousByte;

	private uint[] _repDistances = new uint[4];

	private Optimal[] _optimum = new Optimal[4096];

	private IMatchFinder _matchFinder;

	private SevenZip.Compression.RangeCoder.Encoder _rangeEncoder = new SevenZip.Compression.RangeCoder.Encoder();

	private BitEncoder[] _isMatch = new BitEncoder[192];

	private BitEncoder[] _isRep = new BitEncoder[12];

	private BitEncoder[] _isRepG0 = new BitEncoder[12];

	private BitEncoder[] _isRepG1 = new BitEncoder[12];

	private BitEncoder[] _isRepG2 = new BitEncoder[12];

	private BitEncoder[] _isRep0Long = new BitEncoder[192];

	private BitTreeEncoder[] _posSlotEncoder = new BitTreeEncoder[4];

	private BitEncoder[] _posEncoders = new BitEncoder[114];

	private BitTreeEncoder _posAlignEncoder = new BitTreeEncoder(4);

	private LenPriceTableEncoder _lenEncoder = new LenPriceTableEncoder();

	private LenPriceTableEncoder _repMatchLenEncoder = new LenPriceTableEncoder();

	private LiteralEncoder _literalEncoder = new LiteralEncoder();

	private uint[] _matchDistances = new uint[548];

	private uint _numFastBytes = 32u;

	private uint _longestMatchLength;

	private uint _numDistancePairs;

	private uint _additionalOffset;

	private uint _optimumEndIndex;

	private uint _optimumCurrentIndex;

	private bool _longestMatchWasFound;

	private uint[] _posSlotPrices = new uint[256];

	private uint[] _distancesPrices = new uint[512];

	private uint[] _alignPrices = new uint[16];

	private uint _alignPriceCount;

	private uint _distTableSize = 44u;

	private int _posStateBits = 2;

	private uint _posStateMask = 3u;

	private int _numLiteralPosStateBits;

	private int _numLiteralContextBits = 3;

	private uint _dictionarySize = 4194304u;

	private uint _dictionarySizePrev = uint.MaxValue;

	private uint _numFastBytesPrev = uint.MaxValue;

	private long nowPos64;

	private bool _finished;

	private Stream _inStream;

	private EMatchFinderType _matchFinderType = EMatchFinderType.BT4;

	private bool _writeEndMark;

	private bool _needReleaseMFStream;

	private uint[] reps = new uint[4];

	private uint[] repLens = new uint[4];

	private byte[] properties = new byte[5];

	private uint[] tempPrices = new uint[128];

	private uint _matchPriceCount;

	private static string[] kMatchFinderIDs;

	static Encoder()
	{
		g_FastPos = new byte[2048];
		kMatchFinderIDs = new string[2] { "BT2", "BT4" };
		int c = 2;
		g_FastPos[0] = 0;
		g_FastPos[1] = 1;
		for (byte slotFast = 2; slotFast < 22; slotFast++)
		{
			uint k = (uint)(1 << (slotFast >> 1) - 1);
			uint j = 0u;
			while (j < k)
			{
				g_FastPos[c] = slotFast;
				j++;
				c++;
			}
		}
	}

	private static uint GetPosSlot(uint pos)
	{
		if (pos < 2048)
		{
			return g_FastPos[pos];
		}
		if (pos < 2097152)
		{
			return (uint)(g_FastPos[pos >> 10] + 20);
		}
		return (uint)(g_FastPos[pos >> 20] + 40);
	}

	private static uint GetPosSlot2(uint pos)
	{
		if (pos < 131072)
		{
			return (uint)(g_FastPos[pos >> 6] + 12);
		}
		if (pos < 134217728)
		{
			return (uint)(g_FastPos[pos >> 16] + 32);
		}
		return (uint)(g_FastPos[pos >> 26] + 52);
	}

	private void BaseInit()
	{
		_state.Init();
		_previousByte = 0;
		for (uint i = 0u; i < 4; i++)
		{
			_repDistances[i] = 0u;
		}
	}

	private void Create()
	{
		if (_matchFinder == null)
		{
			BinTree binTree = new BinTree();
			int type = 4;
			if (_matchFinderType == EMatchFinderType.BT2)
			{
				type = 2;
			}
			binTree.SetType(type);
			_matchFinder = binTree;
		}
		_literalEncoder.Create(_numLiteralPosStateBits, _numLiteralContextBits);
		if (_dictionarySize != _dictionarySizePrev || _numFastBytesPrev != _numFastBytes)
		{
			_matchFinder.Create(_dictionarySize, 4096u, _numFastBytes, 274u);
			_dictionarySizePrev = _dictionarySize;
			_numFastBytesPrev = _numFastBytes;
		}
	}

	public Encoder()
	{
		for (int i = 0; (long)i < 4096L; i++)
		{
			_optimum[i] = new Optimal();
		}
		for (int j = 0; (long)j < 4L; j++)
		{
			_posSlotEncoder[j] = new BitTreeEncoder(6);
		}
	}

	private void SetWriteEndMarkerMode(bool writeEndMarker)
	{
		_writeEndMark = writeEndMarker;
	}

	private void Init()
	{
		BaseInit();
		_rangeEncoder.Init();
		for (uint i = 0u; i < 12; i++)
		{
			for (uint j = 0u; j <= _posStateMask; j++)
			{
				uint complexState = (i << 4) + j;
				_isMatch[complexState].Init();
				_isRep0Long[complexState].Init();
			}
			_isRep[i].Init();
			_isRepG0[i].Init();
			_isRepG1[i].Init();
			_isRepG2[i].Init();
		}
		_literalEncoder.Init();
		for (uint i = 0u; i < 4; i++)
		{
			_posSlotEncoder[i].Init();
		}
		for (uint i = 0u; i < 114; i++)
		{
			_posEncoders[i].Init();
		}
		_lenEncoder.Init((uint)(1 << _posStateBits));
		_repMatchLenEncoder.Init((uint)(1 << _posStateBits));
		_posAlignEncoder.Init();
		_longestMatchWasFound = false;
		_optimumEndIndex = 0u;
		_optimumCurrentIndex = 0u;
		_additionalOffset = 0u;
	}

	private void ReadMatchDistances(out uint lenRes, out uint numDistancePairs)
	{
		lenRes = 0u;
		numDistancePairs = _matchFinder.GetMatches(_matchDistances);
		if (numDistancePairs != 0)
		{
			lenRes = _matchDistances[numDistancePairs - 2];
			if (lenRes == _numFastBytes)
			{
				lenRes += _matchFinder.GetMatchLen((int)(lenRes - 1), _matchDistances[numDistancePairs - 1], 273 - lenRes);
			}
		}
		_additionalOffset++;
	}

	private void MovePos(uint num)
	{
		if (num != 0)
		{
			_matchFinder.Skip(num);
			_additionalOffset += num;
		}
	}

	private uint GetRepLen1Price(Base.State state, uint posState)
	{
		return _isRepG0[state.Index].GetPrice0() + _isRep0Long[(state.Index << 4) + posState].GetPrice0();
	}

	private uint GetPureRepPrice(uint repIndex, Base.State state, uint posState)
	{
		uint price;
		if (repIndex == 0)
		{
			price = _isRepG0[state.Index].GetPrice0();
			return price + _isRep0Long[(state.Index << 4) + posState].GetPrice1();
		}
		price = _isRepG0[state.Index].GetPrice1();
		if (repIndex == 1)
		{
			return price + _isRepG1[state.Index].GetPrice0();
		}
		price += _isRepG1[state.Index].GetPrice1();
		return price + _isRepG2[state.Index].GetPrice(repIndex - 2);
	}

	private uint GetRepPrice(uint repIndex, uint len, Base.State state, uint posState)
	{
		uint price = _repMatchLenEncoder.GetPrice(len - 2, posState);
		return price + GetPureRepPrice(repIndex, state, posState);
	}

	private uint GetPosLenPrice(uint pos, uint len, uint posState)
	{
		uint lenToPosState = Base.GetLenToPosState(len);
		uint price = ((pos >= 128) ? (_posSlotPrices[(lenToPosState << 6) + GetPosSlot2(pos)] + _alignPrices[pos & 0xF]) : _distancesPrices[lenToPosState * 128 + pos]);
		return price + _lenEncoder.GetPrice(len - 2, posState);
	}

	private uint Backward(out uint backRes, uint cur)
	{
		_optimumEndIndex = cur;
		uint posPrev = _optimum[cur].PosPrev;
		uint backPrev = _optimum[cur].BackPrev;
		do
		{
			if (_optimum[cur].Prev1IsChar)
			{
				_optimum[posPrev].MakeAsChar();
				_optimum[posPrev].PosPrev = posPrev - 1;
				if (_optimum[cur].Prev2)
				{
					_optimum[posPrev - 1].Prev1IsChar = false;
					_optimum[posPrev - 1].PosPrev = _optimum[cur].PosPrev2;
					_optimum[posPrev - 1].BackPrev = _optimum[cur].BackPrev2;
				}
			}
			uint prev = posPrev;
			uint backPrev2 = backPrev;
			backPrev = _optimum[prev].BackPrev;
			posPrev = _optimum[prev].PosPrev;
			_optimum[prev].BackPrev = backPrev2;
			_optimum[prev].PosPrev = cur;
			cur = prev;
		}
		while (cur != 0);
		backRes = _optimum[0].BackPrev;
		_optimumCurrentIndex = _optimum[0].PosPrev;
		return _optimumCurrentIndex;
	}

	private uint GetOptimum(uint position, out uint backRes)
	{
		if (_optimumEndIndex != _optimumCurrentIndex)
		{
			uint lenRes = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
			backRes = _optimum[_optimumCurrentIndex].BackPrev;
			_optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
			return lenRes;
		}
		_optimumCurrentIndex = (_optimumEndIndex = 0u);
		uint lenMain;
		uint numDistancePairs;
		if (!_longestMatchWasFound)
		{
			ReadMatchDistances(out lenMain, out numDistancePairs);
		}
		else
		{
			lenMain = _longestMatchLength;
			numDistancePairs = _numDistancePairs;
			_longestMatchWasFound = false;
		}
		uint numAvailableBytes = _matchFinder.GetNumAvailableBytes() + 1;
		if (numAvailableBytes < 2)
		{
			backRes = uint.MaxValue;
			return 1u;
		}
		if (numAvailableBytes > 273)
		{
			numAvailableBytes = 273u;
		}
		uint repMaxIndex = 0u;
		for (uint i = 0u; i < 4; i++)
		{
			reps[i] = _repDistances[i];
			repLens[i] = _matchFinder.GetMatchLen(-1, reps[i], 273u);
			if (repLens[i] > repLens[repMaxIndex])
			{
				repMaxIndex = i;
			}
		}
		if (repLens[repMaxIndex] >= _numFastBytes)
		{
			backRes = repMaxIndex;
			uint lenRes = repLens[repMaxIndex];
			MovePos(lenRes - 1);
			return lenRes;
		}
		if (lenMain >= _numFastBytes)
		{
			backRes = _matchDistances[numDistancePairs - 1] + 4;
			MovePos(lenMain - 1);
			return lenMain;
		}
		byte currentByte = _matchFinder.GetIndexByte(-1);
		byte matchByte = _matchFinder.GetIndexByte((int)(0 - _repDistances[0] - 1 - 1));
		if (lenMain < 2 && currentByte != matchByte && repLens[repMaxIndex] < 2)
		{
			backRes = uint.MaxValue;
			return 1u;
		}
		_optimum[0].State = _state;
		uint posState = position & _posStateMask;
		_optimum[1].Price = _isMatch[(_state.Index << 4) + posState].GetPrice0() + _literalEncoder.GetSubCoder(position, _previousByte).GetPrice(!_state.IsCharState(), matchByte, currentByte);
		_optimum[1].MakeAsChar();
		uint matchPrice = _isMatch[(_state.Index << 4) + posState].GetPrice1();
		uint repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();
		if (matchByte == currentByte)
		{
			uint shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
			if (shortRepPrice < _optimum[1].Price)
			{
				_optimum[1].Price = shortRepPrice;
				_optimum[1].MakeAsShortRep();
			}
		}
		uint lenEnd = ((lenMain >= repLens[repMaxIndex]) ? lenMain : repLens[repMaxIndex]);
		if (lenEnd < 2)
		{
			backRes = _optimum[1].BackPrev;
			return 1u;
		}
		_optimum[1].PosPrev = 0u;
		_optimum[0].Backs0 = reps[0];
		_optimum[0].Backs1 = reps[1];
		_optimum[0].Backs2 = reps[2];
		_optimum[0].Backs3 = reps[3];
		uint len = lenEnd;
		do
		{
			_optimum[len--].Price = 268435455u;
		}
		while (len >= 2);
		for (uint i = 0u; i < 4; i++)
		{
			uint repLen = repLens[i];
			if (repLen < 2)
			{
				continue;
			}
			uint price = repMatchPrice + GetPureRepPrice(i, _state, posState);
			do
			{
				uint curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
				Optimal optimum = _optimum[repLen];
				if (curAndLenPrice < optimum.Price)
				{
					optimum.Price = curAndLenPrice;
					optimum.PosPrev = 0u;
					optimum.BackPrev = i;
					optimum.Prev1IsChar = false;
				}
			}
			while (--repLen >= 2);
		}
		uint normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();
		len = ((repLens[0] >= 2) ? (repLens[0] + 1) : 2u);
		if (len <= lenMain)
		{
			uint offs;
			for (offs = 0u; len > _matchDistances[offs]; offs += 2)
			{
			}
			while (true)
			{
				uint distance = _matchDistances[offs + 1];
				uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
				Optimal optimum = _optimum[len];
				if (curAndLenPrice < optimum.Price)
				{
					optimum.Price = curAndLenPrice;
					optimum.PosPrev = 0u;
					optimum.BackPrev = distance + 4;
					optimum.Prev1IsChar = false;
				}
				if (len == _matchDistances[offs])
				{
					offs += 2;
					if (offs == numDistancePairs)
					{
						break;
					}
				}
				len++;
			}
		}
		uint cur = 0u;
		uint newLen;
		while (true)
		{
			cur++;
			if (cur == lenEnd)
			{
				return Backward(out backRes, cur);
			}
			ReadMatchDistances(out newLen, out numDistancePairs);
			if (newLen >= _numFastBytes)
			{
				break;
			}
			position++;
			uint posPrev = _optimum[cur].PosPrev;
			Base.State state;
			if (_optimum[cur].Prev1IsChar)
			{
				posPrev--;
				if (_optimum[cur].Prev2)
				{
					state = _optimum[_optimum[cur].PosPrev2].State;
					if (_optimum[cur].BackPrev2 < 4)
					{
						state.UpdateRep();
					}
					else
					{
						state.UpdateMatch();
					}
				}
				else
				{
					state = _optimum[posPrev].State;
				}
				state.UpdateChar();
			}
			else
			{
				state = _optimum[posPrev].State;
			}
			if (posPrev == cur - 1)
			{
				if (_optimum[cur].IsShortRep())
				{
					state.UpdateShortRep();
				}
				else
				{
					state.UpdateChar();
				}
			}
			else
			{
				uint pos;
				if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
				{
					posPrev = _optimum[cur].PosPrev2;
					pos = _optimum[cur].BackPrev2;
					state.UpdateRep();
				}
				else
				{
					pos = _optimum[cur].BackPrev;
					if (pos < 4)
					{
						state.UpdateRep();
					}
					else
					{
						state.UpdateMatch();
					}
				}
				Optimal opt = _optimum[posPrev];
				switch (pos)
				{
				case 0u:
					reps[0] = opt.Backs0;
					reps[1] = opt.Backs1;
					reps[2] = opt.Backs2;
					reps[3] = opt.Backs3;
					break;
				case 1u:
					reps[0] = opt.Backs1;
					reps[1] = opt.Backs0;
					reps[2] = opt.Backs2;
					reps[3] = opt.Backs3;
					break;
				case 2u:
					reps[0] = opt.Backs2;
					reps[1] = opt.Backs0;
					reps[2] = opt.Backs1;
					reps[3] = opt.Backs3;
					break;
				case 3u:
					reps[0] = opt.Backs3;
					reps[1] = opt.Backs0;
					reps[2] = opt.Backs1;
					reps[3] = opt.Backs2;
					break;
				default:
					reps[0] = pos - 4;
					reps[1] = opt.Backs0;
					reps[2] = opt.Backs1;
					reps[3] = opt.Backs2;
					break;
				}
			}
			_optimum[cur].State = state;
			_optimum[cur].Backs0 = reps[0];
			_optimum[cur].Backs1 = reps[1];
			_optimum[cur].Backs2 = reps[2];
			_optimum[cur].Backs3 = reps[3];
			uint curPrice = _optimum[cur].Price;
			currentByte = _matchFinder.GetIndexByte(-1);
			matchByte = _matchFinder.GetIndexByte((int)(0 - reps[0] - 1 - 1));
			posState = position & _posStateMask;
			uint curAnd1Price = curPrice + _isMatch[(state.Index << 4) + posState].GetPrice0() + _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexByte(-2)).GetPrice(!state.IsCharState(), matchByte, currentByte);
			Optimal nextOptimum = _optimum[cur + 1];
			bool nextIsChar = false;
			if (curAnd1Price < nextOptimum.Price)
			{
				nextOptimum.Price = curAnd1Price;
				nextOptimum.PosPrev = cur;
				nextOptimum.MakeAsChar();
				nextIsChar = true;
			}
			matchPrice = curPrice + _isMatch[(state.Index << 4) + posState].GetPrice1();
			repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();
			if (matchByte == currentByte && (nextOptimum.PosPrev >= cur || nextOptimum.BackPrev != 0))
			{
				uint shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
				if (shortRepPrice <= nextOptimum.Price)
				{
					nextOptimum.Price = shortRepPrice;
					nextOptimum.PosPrev = cur;
					nextOptimum.MakeAsShortRep();
					nextIsChar = true;
				}
			}
			uint numAvailableBytesFull = _matchFinder.GetNumAvailableBytes() + 1;
			numAvailableBytesFull = Math.Min(4095 - cur, numAvailableBytesFull);
			numAvailableBytes = numAvailableBytesFull;
			if (numAvailableBytes < 2)
			{
				continue;
			}
			if (numAvailableBytes > _numFastBytes)
			{
				numAvailableBytes = _numFastBytes;
			}
			if (!nextIsChar && matchByte != currentByte)
			{
				uint t = Math.Min(numAvailableBytesFull - 1, _numFastBytes);
				uint lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
				if (lenTest2 >= 2)
				{
					Base.State state2 = state;
					state2.UpdateChar();
					uint posStateNext = (position + 1) & _posStateMask;
					uint nextRepMatchPrice = curAnd1Price + _isMatch[(state2.Index << 4) + posStateNext].GetPrice1() + _isRep[state2.Index].GetPrice1();
					uint offset = cur + 1 + lenTest2;
					while (lenEnd < offset)
					{
						_optimum[++lenEnd].Price = 268435455u;
					}
					uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(0u, lenTest2, state2, posStateNext);
					Optimal optimum = _optimum[offset];
					if (curAndLenPrice < optimum.Price)
					{
						optimum.Price = curAndLenPrice;
						optimum.PosPrev = cur + 1;
						optimum.BackPrev = 0u;
						optimum.Prev1IsChar = true;
						optimum.Prev2 = false;
					}
				}
			}
			uint startLen = 2u;
			for (uint repIndex = 0u; repIndex < 4; repIndex++)
			{
				uint repLenTest = _matchFinder.GetMatchLen(-1, reps[repIndex], numAvailableBytes);
				if (repLenTest < 2)
				{
					continue;
				}
				uint repLenTestTemp = repLenTest;
				while (true)
				{
					if (lenEnd < cur + repLenTest)
					{
						_optimum[++lenEnd].Price = 268435455u;
						continue;
					}
					uint curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, repLenTest, state, posState);
					Optimal optimum = _optimum[cur + repLenTest];
					if (curAndLenPrice < optimum.Price)
					{
						optimum.Price = curAndLenPrice;
						optimum.PosPrev = cur;
						optimum.BackPrev = repIndex;
						optimum.Prev1IsChar = false;
					}
					if (--repLenTest < 2)
					{
						break;
					}
				}
				repLenTest = repLenTestTemp;
				if (repIndex == 0)
				{
					startLen = repLenTest + 1;
				}
				if (repLenTest >= numAvailableBytesFull)
				{
					continue;
				}
				uint t = Math.Min(numAvailableBytesFull - 1 - repLenTest, _numFastBytes);
				uint lenTest2 = _matchFinder.GetMatchLen((int)repLenTest, reps[repIndex], t);
				if (lenTest2 >= 2)
				{
					Base.State state2 = state;
					state2.UpdateRep();
					uint posStateNext = (position + repLenTest) & _posStateMask;
					uint curAndLenCharPrice = repMatchPrice + GetRepPrice(repIndex, repLenTest, state, posState) + _isMatch[(state2.Index << 4) + posStateNext].GetPrice0() + _literalEncoder.GetSubCoder(position + repLenTest, _matchFinder.GetIndexByte((int)(repLenTest - 1 - 1))).GetPrice(matchMode: true, _matchFinder.GetIndexByte((int)(repLenTest - 1 - (reps[repIndex] + 1))), _matchFinder.GetIndexByte((int)(repLenTest - 1)));
					state2.UpdateChar();
					posStateNext = (position + repLenTest + 1) & _posStateMask;
					uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << 4) + posStateNext].GetPrice1();
					uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();
					uint offset = repLenTest + 1 + lenTest2;
					while (lenEnd < cur + offset)
					{
						_optimum[++lenEnd].Price = 268435455u;
					}
					uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(0u, lenTest2, state2, posStateNext);
					Optimal optimum = _optimum[cur + offset];
					if (curAndLenPrice < optimum.Price)
					{
						optimum.Price = curAndLenPrice;
						optimum.PosPrev = cur + repLenTest + 1;
						optimum.BackPrev = 0u;
						optimum.Prev1IsChar = true;
						optimum.Prev2 = true;
						optimum.PosPrev2 = cur;
						optimum.BackPrev2 = repIndex;
					}
				}
			}
			if (newLen > numAvailableBytes)
			{
				newLen = numAvailableBytes;
				for (numDistancePairs = 0u; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2)
				{
				}
				_matchDistances[numDistancePairs] = newLen;
				numDistancePairs += 2;
			}
			if (newLen < startLen)
			{
				continue;
			}
			normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
			while (lenEnd < cur + newLen)
			{
				_optimum[++lenEnd].Price = 268435455u;
			}
			uint offs;
			for (offs = 0u; startLen > _matchDistances[offs]; offs += 2)
			{
			}
			uint lenTest = startLen;
			while (true)
			{
				uint curBack = _matchDistances[offs + 1];
				uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
				Optimal optimum = _optimum[cur + lenTest];
				if (curAndLenPrice < optimum.Price)
				{
					optimum.Price = curAndLenPrice;
					optimum.PosPrev = cur;
					optimum.BackPrev = curBack + 4;
					optimum.Prev1IsChar = false;
				}
				if (lenTest == _matchDistances[offs])
				{
					if (lenTest < numAvailableBytesFull)
					{
						uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
						uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, curBack, t);
						if (lenTest2 >= 2)
						{
							Base.State state2 = state;
							state2.UpdateMatch();
							uint posStateNext = (position + lenTest) & _posStateMask;
							uint curAndLenCharPrice = curAndLenPrice + _isMatch[(state2.Index << 4) + posStateNext].GetPrice0() + _literalEncoder.GetSubCoder(position + lenTest, _matchFinder.GetIndexByte((int)(lenTest - 1 - 1))).GetPrice(matchMode: true, _matchFinder.GetIndexByte((int)(lenTest - (curBack + 1) - 1)), _matchFinder.GetIndexByte((int)(lenTest - 1)));
							state2.UpdateChar();
							posStateNext = (position + lenTest + 1) & _posStateMask;
							uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << 4) + posStateNext].GetPrice1();
							uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();
							uint offset = lenTest + 1 + lenTest2;
							while (lenEnd < cur + offset)
							{
								_optimum[++lenEnd].Price = 268435455u;
							}
							curAndLenPrice = nextRepMatchPrice + GetRepPrice(0u, lenTest2, state2, posStateNext);
							optimum = _optimum[cur + offset];
							if (curAndLenPrice < optimum.Price)
							{
								optimum.Price = curAndLenPrice;
								optimum.PosPrev = cur + lenTest + 1;
								optimum.BackPrev = 0u;
								optimum.Prev1IsChar = true;
								optimum.Prev2 = true;
								optimum.PosPrev2 = cur;
								optimum.BackPrev2 = curBack + 4;
							}
						}
					}
					offs += 2;
					if (offs == numDistancePairs)
					{
						break;
					}
				}
				lenTest++;
			}
		}
		_numDistancePairs = numDistancePairs;
		_longestMatchLength = newLen;
		_longestMatchWasFound = true;
		return Backward(out backRes, cur);
	}

	private bool ChangePair(uint smallDist, uint bigDist)
	{
		if (smallDist < 33554432)
		{
			return bigDist >= smallDist << 7;
		}
		return false;
	}

	private void WriteEndMarker(uint posState)
	{
		if (_writeEndMark)
		{
			_isMatch[(_state.Index << 4) + posState].Encode(_rangeEncoder, 1u);
			_isRep[_state.Index].Encode(_rangeEncoder, 0u);
			_state.UpdateMatch();
			uint len = 2u;
			_lenEncoder.Encode(_rangeEncoder, len - 2, posState);
			uint symbol = 63u;
			uint lenToPosState = Base.GetLenToPosState(len);
			_posSlotEncoder[lenToPosState].Encode(_rangeEncoder, symbol);
			int footerBits = 30;
			uint posReduced = (uint)((1 << footerBits) - 1);
			_rangeEncoder.EncodeDirectBits(posReduced >> 4, footerBits - 4);
			_posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & 0xF);
		}
	}

	private void Flush(uint nowPos)
	{
		ReleaseMFStream();
		WriteEndMarker(nowPos & _posStateMask);
		_rangeEncoder.FlushData();
		_rangeEncoder.FlushStream();
	}

	public void CodeOneBlock(out long inSize, out long outSize, out bool finished)
	{
		inSize = 0L;
		outSize = 0L;
		finished = true;
		if (_inStream != null)
		{
			_matchFinder.SetStream(_inStream);
			_matchFinder.Init();
			_needReleaseMFStream = true;
			_inStream = null;
		}
		if (_finished)
		{
			return;
		}
		_finished = true;
		long progressPosValuePrev = nowPos64;
		if (nowPos64 == 0)
		{
			if (_matchFinder.GetNumAvailableBytes() == 0)
			{
				Flush((uint)nowPos64);
				return;
			}
			ReadMatchDistances(out var _, out var _);
			uint posState = (uint)(int)nowPos64 & _posStateMask;
			_isMatch[(_state.Index << 4) + posState].Encode(_rangeEncoder, 0u);
			_state.UpdateChar();
			byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
			_literalEncoder.GetSubCoder((uint)nowPos64, _previousByte).Encode(_rangeEncoder, curByte);
			_previousByte = curByte;
			_additionalOffset--;
			nowPos64++;
		}
		if (_matchFinder.GetNumAvailableBytes() == 0)
		{
			Flush((uint)nowPos64);
			return;
		}
		while (true)
		{
			uint len = GetOptimum((uint)nowPos64, out var backRes);
			uint posState = (uint)(int)nowPos64 & _posStateMask;
			uint complexState = (_state.Index << 4) + posState;
			if (len == 1 && backRes == uint.MaxValue)
			{
				_isMatch[complexState].Encode(_rangeEncoder, 0u);
				byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
				LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((uint)nowPos64, _previousByte);
				if (!_state.IsCharState())
				{
					byte matchByte = _matchFinder.GetIndexByte((int)(0 - _repDistances[0] - 1 - _additionalOffset));
					subCoder.EncodeMatched(_rangeEncoder, matchByte, curByte);
				}
				else
				{
					subCoder.Encode(_rangeEncoder, curByte);
				}
				_previousByte = curByte;
				_state.UpdateChar();
			}
			else
			{
				_isMatch[complexState].Encode(_rangeEncoder, 1u);
				if (backRes < 4)
				{
					_isRep[_state.Index].Encode(_rangeEncoder, 1u);
					if (backRes == 0)
					{
						_isRepG0[_state.Index].Encode(_rangeEncoder, 0u);
						if (len == 1)
						{
							_isRep0Long[complexState].Encode(_rangeEncoder, 0u);
						}
						else
						{
							_isRep0Long[complexState].Encode(_rangeEncoder, 1u);
						}
					}
					else
					{
						_isRepG0[_state.Index].Encode(_rangeEncoder, 1u);
						if (backRes == 1)
						{
							_isRepG1[_state.Index].Encode(_rangeEncoder, 0u);
						}
						else
						{
							_isRepG1[_state.Index].Encode(_rangeEncoder, 1u);
							_isRepG2[_state.Index].Encode(_rangeEncoder, backRes - 2);
						}
					}
					if (len == 1)
					{
						_state.UpdateShortRep();
					}
					else
					{
						_repMatchLenEncoder.Encode(_rangeEncoder, len - 2, posState);
						_state.UpdateRep();
					}
					uint distance = _repDistances[backRes];
					if (backRes != 0)
					{
						for (uint i = backRes; i >= 1; i--)
						{
							_repDistances[i] = _repDistances[i - 1];
						}
						_repDistances[0] = distance;
					}
				}
				else
				{
					_isRep[_state.Index].Encode(_rangeEncoder, 0u);
					_state.UpdateMatch();
					_lenEncoder.Encode(_rangeEncoder, len - 2, posState);
					backRes -= 4;
					uint posSlot = GetPosSlot(backRes);
					uint lenToPosState = Base.GetLenToPosState(len);
					_posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);
					if (posSlot >= 4)
					{
						int footerBits = (int)((posSlot >> 1) - 1);
						uint baseVal = (2 | (posSlot & 1)) << footerBits;
						uint posReduced = backRes - baseVal;
						if (posSlot < 14)
						{
							BitTreeEncoder.ReverseEncode(_posEncoders, baseVal - posSlot - 1, _rangeEncoder, footerBits, posReduced);
						}
						else
						{
							_rangeEncoder.EncodeDirectBits(posReduced >> 4, footerBits - 4);
							_posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & 0xF);
							_alignPriceCount++;
						}
					}
					uint distance = backRes;
					for (uint i = 3u; i >= 1; i--)
					{
						_repDistances[i] = _repDistances[i - 1];
					}
					_repDistances[0] = distance;
					_matchPriceCount++;
				}
				_previousByte = _matchFinder.GetIndexByte((int)(len - 1 - _additionalOffset));
			}
			_additionalOffset -= len;
			nowPos64 += len;
			if (_additionalOffset == 0)
			{
				if (_matchPriceCount >= 128)
				{
					FillDistancesPrices();
				}
				if (_alignPriceCount >= 16)
				{
					FillAlignPrices();
				}
				inSize = nowPos64;
				outSize = _rangeEncoder.GetProcessedSizeAdd();
				if (_matchFinder.GetNumAvailableBytes() == 0)
				{
					Flush((uint)nowPos64);
					return;
				}
				if (nowPos64 - progressPosValuePrev >= 4096)
				{
					break;
				}
			}
		}
		_finished = false;
		finished = false;
	}

	private void ReleaseMFStream()
	{
		if (_matchFinder != null && _needReleaseMFStream)
		{
			_matchFinder.ReleaseStream();
			_needReleaseMFStream = false;
		}
	}

	private void SetOutStream(Stream outStream)
	{
		_rangeEncoder.SetStream(outStream);
	}

	private void ReleaseOutStream()
	{
		_rangeEncoder.ReleaseStream();
	}

	private void ReleaseStreams()
	{
		ReleaseMFStream();
		ReleaseOutStream();
	}

	private void SetStreams(Stream inStream, Stream outStream, long inSize, long outSize)
	{
		_inStream = inStream;
		_finished = false;
		Create();
		SetOutStream(outStream);
		Init();
		FillDistancesPrices();
		FillAlignPrices();
		_lenEncoder.SetTableSize(_numFastBytes + 1 - 2);
		_lenEncoder.UpdateTables((uint)(1 << _posStateBits));
		_repMatchLenEncoder.SetTableSize(_numFastBytes + 1 - 2);
		_repMatchLenEncoder.UpdateTables((uint)(1 << _posStateBits));
		nowPos64 = 0L;
	}

	public void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress progress)
	{
		_needReleaseMFStream = false;
		try
		{
			SetStreams(inStream, outStream, inSize, outSize);
			while (true)
			{
				CodeOneBlock(out var inSize2, out var outSize2, out var finished);
				if (finished)
				{
					break;
				}
				progress?.SetProgress(inSize2, outSize2);
			}
		}
		finally
		{
			ReleaseStreams();
		}
	}

	public void WriteCoderProperties(Stream outStream)
	{
		properties[0] = (byte)((_posStateBits * 5 + _numLiteralPosStateBits) * 9 + _numLiteralContextBits);
		for (int i = 0; i < 4; i++)
		{
			properties[1 + i] = (byte)(_dictionarySize >> 8 * i);
		}
		outStream.Write(properties, 0, 5);
	}

	private void FillDistancesPrices()
	{
		for (uint i = 4u; i < 128; i++)
		{
			uint posSlot = GetPosSlot(i);
			int footerBits = (int)((posSlot >> 1) - 1);
			uint baseVal = (2 | (posSlot & 1)) << footerBits;
			tempPrices[i] = BitTreeEncoder.ReverseGetPrice(_posEncoders, baseVal - posSlot - 1, footerBits, i - baseVal);
		}
		for (uint lenToPosState = 0u; lenToPosState < 4; lenToPosState++)
		{
			BitTreeEncoder encoder = _posSlotEncoder[lenToPosState];
			uint st = lenToPosState << 6;
			for (uint posSlot = 0u; posSlot < _distTableSize; posSlot++)
			{
				_posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
			}
			for (uint posSlot = 14u; posSlot < _distTableSize; posSlot++)
			{
				_posSlotPrices[st + posSlot] += (posSlot >> 1) - 1 - 4 << 6;
			}
			uint st2 = lenToPosState * 128;
			uint i;
			for (i = 0u; i < 4; i++)
			{
				_distancesPrices[st2 + i] = _posSlotPrices[st + i];
			}
			for (; i < 128; i++)
			{
				_distancesPrices[st2 + i] = _posSlotPrices[st + GetPosSlot(i)] + tempPrices[i];
			}
		}
		_matchPriceCount = 0u;
	}

	private void FillAlignPrices()
	{
		for (uint i = 0u; i < 16; i++)
		{
			_alignPrices[i] = _posAlignEncoder.ReverseGetPrice(i);
		}
		_alignPriceCount = 0u;
	}

	private static int FindMatchFinder(string s)
	{
		for (int i = 0; i < kMatchFinderIDs.Length; i++)
		{
			if (s == kMatchFinderIDs[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetCoderProperties(CoderPropID[] propIDs, object[] properties)
	{
		for (uint i = 0u; i < properties.Length; i++)
		{
			object prop = properties[i];
			switch (propIDs[i])
			{
			case CoderPropID.NumFastBytes:
				if (!(prop is int numFastBytes))
				{
					throw new InvalidParamException();
				}
				if (numFastBytes < 5 || (long)numFastBytes > 273L)
				{
					throw new InvalidParamException();
				}
				_numFastBytes = (uint)numFastBytes;
				break;
			case CoderPropID.MatchFinder:
			{
				if (!(prop is string))
				{
					throw new InvalidParamException();
				}
				EMatchFinderType matchFinderType = _matchFinderType;
				int matchFinderIndex = FindMatchFinder(((string)prop).ToUpper());
				if (matchFinderIndex < 0)
				{
					throw new InvalidParamException();
				}
				_matchFinderType = (EMatchFinderType)matchFinderIndex;
				if (_matchFinder != null && matchFinderType != _matchFinderType)
				{
					_dictionarySizePrev = uint.MaxValue;
					_matchFinder = null;
				}
				break;
			}
			case CoderPropID.DictionarySize:
			{
				if (!(prop is int dictionarySize))
				{
					throw new InvalidParamException();
				}
				if ((long)dictionarySize < 1L || (long)dictionarySize > 1073741824L)
				{
					throw new InvalidParamException();
				}
				_dictionarySize = (uint)dictionarySize;
				int dicLogSize;
				for (dicLogSize = 0; (long)dicLogSize < 30L && dictionarySize > (uint)(1 << dicLogSize); dicLogSize++)
				{
				}
				_distTableSize = (uint)(dicLogSize * 2);
				break;
			}
			case CoderPropID.PosStateBits:
				if (!(prop is int posStateBits))
				{
					throw new InvalidParamException();
				}
				if (posStateBits < 0 || (long)posStateBits > 4L)
				{
					throw new InvalidParamException();
				}
				_posStateBits = posStateBits;
				_posStateMask = (uint)((1 << _posStateBits) - 1);
				break;
			case CoderPropID.LitPosBits:
				if (!(prop is int litPosBits))
				{
					throw new InvalidParamException();
				}
				if (litPosBits < 0 || (long)litPosBits > 4L)
				{
					throw new InvalidParamException();
				}
				_numLiteralPosStateBits = litPosBits;
				break;
			case CoderPropID.LitContextBits:
				if (!(prop is int litContextBits))
				{
					throw new InvalidParamException();
				}
				if (litContextBits < 0 || (long)litContextBits > 8L)
				{
					throw new InvalidParamException();
				}
				_numLiteralContextBits = litContextBits;
				break;
			case CoderPropID.EndMarker:
				if (!(prop is bool))
				{
					throw new InvalidParamException();
				}
				SetWriteEndMarkerMode((bool)prop);
				break;
			default:
				throw new InvalidParamException();
			case CoderPropID.Algorithm:
				break;
			}
		}
	}
}
