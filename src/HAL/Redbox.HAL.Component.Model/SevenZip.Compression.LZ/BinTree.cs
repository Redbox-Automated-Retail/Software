using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Compression.LZ;

[ComVisible(true)]
public class BinTree : InWindow, IMatchFinder, IInWindowStream
{
	private const uint kHash2Size = 1024u;

	private const uint kHash3Size = 65536u;

	private const uint kBT2HashSize = 65536u;

	private const uint kStartMaxLen = 1u;

	private const uint kHash3Offset = 1024u;

	private const uint kEmptyHashValue = 0u;

	private const uint kMaxValForNormalize = 2147483647u;

	private uint _cyclicBufferPos;

	private uint _cyclicBufferSize;

	private uint _matchMaxLen;

	// _son and _hash are set by Create.
	private uint[] _son = null!;

	private uint[] _hash = null!;

	private uint _cutValue = 255u;

	private uint _hashMask;

	private uint _hashSizeSum;

	private bool HASH_ARRAY = true;

	private uint kNumHashDirectBytes;

	private uint kMinMatchCheck = 4u;

	private uint kFixHashSize = 66560u;

	public void SetType(int numHashBytes)
	{
		HASH_ARRAY = numHashBytes > 2;
		if (HASH_ARRAY)
		{
			kNumHashDirectBytes = 0u;
			kMinMatchCheck = 4u;
			kFixHashSize = 66560u;
		}
		else
		{
			kNumHashDirectBytes = 2u;
			kMinMatchCheck = 3u;
			kFixHashSize = 0u;
		}
	}

	public new void SetStream(Stream stream)
	{
		base.SetStream(stream);
	}

	public new void ReleaseStream()
	{
		base.ReleaseStream();
	}

	public new void Init()
	{
		base.Init();
		for (uint i = 0u; i < _hashSizeSum; i++)
		{
			_hash[i] = 0u;
		}
		_cyclicBufferPos = 0u;
		ReduceOffsets(-1);
	}

	public new void MovePos()
	{
		if (++_cyclicBufferPos >= _cyclicBufferSize)
		{
			_cyclicBufferPos = 0u;
		}
		base.MovePos();
		if (_pos == int.MaxValue)
		{
			Normalize();
		}
	}

	public new byte GetIndexByte(int index)
	{
		return base.GetIndexByte(index);
	}

	public new uint GetMatchLen(int index, uint distance, uint limit)
	{
		return base.GetMatchLen(index, distance, limit);
	}

	public new uint GetNumAvailableBytes()
	{
		return base.GetNumAvailableBytes();
	}

	public void Create(uint historySize, uint keepAddBufferBefore, uint matchMaxLen, uint keepAddBufferAfter)
	{
		if (historySize > 2147483391)
		{
			throw new Exception();
		}
		_cutValue = 16 + (matchMaxLen >> 1);
		uint keepSizeReserv = (historySize + keepAddBufferBefore + matchMaxLen + keepAddBufferAfter) / 2 + 256;
		Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, keepSizeReserv);
		_matchMaxLen = matchMaxLen;
		uint cyclicBufferSize = historySize + 1;
		if (_cyclicBufferSize != cyclicBufferSize)
		{
			_son = new uint[(_cyclicBufferSize = cyclicBufferSize) * 2];
		}
		uint hs = 65536u;
		if (HASH_ARRAY)
		{
			hs = historySize - 1;
			hs |= hs >> 1;
			hs |= hs >> 2;
			hs |= hs >> 4;
			hs |= hs >> 8;
			hs >>= 1;
			hs |= 0xFFFF;
			if (hs > 16777216)
			{
				hs >>= 1;
			}
			_hashMask = hs;
			hs++;
			hs += kFixHashSize;
		}
		if (hs != _hashSizeSum)
		{
			_hash = new uint[_hashSizeSum = hs];
		}
	}

	public uint GetMatches(uint[] distances)
	{
		uint lenLimit;
		if (_pos + _matchMaxLen <= _streamPos)
		{
			lenLimit = _matchMaxLen;
		}
		else
		{
			lenLimit = _streamPos - _pos;
			if (lenLimit < kMinMatchCheck)
			{
				MovePos();
				return 0u;
			}
		}
		uint offset = 0u;
		uint matchMinPos = ((_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0u);
		uint cur = _bufferOffset + _pos;
		uint maxLen = 1u;
		uint hash2Value = 0u;
		uint hash3Value = 0u;
		uint hashValue;
		if (HASH_ARRAY)
		{
			uint temp = CRC.Table[_bufferBase![cur]] ^ _bufferBase[cur + 1];
			hash2Value = temp & 0x3FF;
			temp ^= (uint)(_bufferBase[cur + 2] << 8);
			hash3Value = temp & 0xFFFF;
			hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
		}
		else
		{
			hashValue = (uint)(_bufferBase![cur] ^ (_bufferBase[cur + 1] << 8));
		}
		uint curMatch = _hash[kFixHashSize + hashValue];
		if (HASH_ARRAY)
		{
			uint curMatch2 = _hash[hash2Value];
			uint curMatch3 = _hash[1024 + hash3Value];
			_hash[hash2Value] = _pos;
			_hash[1024 + hash3Value] = _pos;
			if (curMatch2 > matchMinPos && _bufferBase[_bufferOffset + curMatch2] == _bufferBase[cur])
			{
				maxLen = (distances[offset++] = 2u);
				distances[offset++] = _pos - curMatch2 - 1;
			}
			if (curMatch3 > matchMinPos && _bufferBase[_bufferOffset + curMatch3] == _bufferBase[cur])
			{
				if (curMatch3 == curMatch2)
				{
					offset -= 2;
				}
				maxLen = (distances[offset++] = 3u);
				distances[offset++] = _pos - curMatch3 - 1;
				curMatch2 = curMatch3;
			}
			if (offset != 0 && curMatch2 == curMatch)
			{
				offset -= 2;
				maxLen = 1u;
			}
		}
		_hash[kFixHashSize + hashValue] = _pos;
		uint ptr1 = (_cyclicBufferPos << 1) + 1;
		uint ptr0 = _cyclicBufferPos << 1;
		uint len1;
		uint len0 = (len1 = kNumHashDirectBytes);
		if (kNumHashDirectBytes != 0 && curMatch > matchMinPos && _bufferBase[_bufferOffset + curMatch + kNumHashDirectBytes] != _bufferBase[cur + kNumHashDirectBytes])
		{
			maxLen = (distances[offset++] = kNumHashDirectBytes);
			distances[offset++] = _pos - curMatch - 1;
		}
		uint count = _cutValue;
		while (true)
		{
			if (curMatch <= matchMinPos || count-- == 0)
			{
				_son[ptr1] = (_son[ptr0] = 0u);
				break;
			}
			uint delta = _pos - curMatch;
			uint cyclicPos = ((delta <= _cyclicBufferPos) ? (_cyclicBufferPos - delta) : (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;
			uint pby1 = _bufferOffset + curMatch;
			uint len = Math.Min(len0, len1);
			if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
			{
				while (++len != lenLimit && _bufferBase[pby1 + len] == _bufferBase[cur + len])
				{
				}
				if (maxLen < len)
				{
					maxLen = (distances[offset++] = len);
					distances[offset++] = delta - 1;
					if (len == lenLimit)
					{
						_son[ptr0] = _son[cyclicPos];
						_son[ptr1] = _son[cyclicPos + 1];
						break;
					}
				}
			}
			if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
			{
				_son[ptr0] = curMatch;
				ptr0 = cyclicPos + 1;
				curMatch = _son[ptr0];
				len1 = len;
			}
			else
			{
				_son[ptr1] = curMatch;
				ptr1 = cyclicPos;
				curMatch = _son[ptr1];
				len0 = len;
			}
		}
		MovePos();
		return offset;
	}

	public void Skip(uint num)
	{
		do
		{
			uint lenLimit;
			if (_pos + _matchMaxLen <= _streamPos)
			{
				lenLimit = _matchMaxLen;
			}
			else
			{
				lenLimit = _streamPos - _pos;
				if (lenLimit < kMinMatchCheck)
				{
					MovePos();
					continue;
				}
			}
			uint matchMinPos = ((_pos > _cyclicBufferSize) ? (_pos - _cyclicBufferSize) : 0u);
			uint cur = _bufferOffset + _pos;
			uint hashValue;
			if (HASH_ARRAY)
			{
				uint temp = CRC.Table[_bufferBase![cur]] ^ _bufferBase[cur + 1];
				uint hash2Value = temp & 0x3FF;
				_hash[hash2Value] = _pos;
				temp ^= (uint)(_bufferBase[cur + 2] << 8);
				uint hash3Value = temp & 0xFFFF;
				_hash[1024 + hash3Value] = _pos;
				hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
			}
			else
			{
				hashValue = (uint)(_bufferBase![cur] ^ (_bufferBase[cur + 1] << 8));
			}
			uint curMatch = _hash[kFixHashSize + hashValue];
			_hash[kFixHashSize + hashValue] = _pos;
			uint ptr1 = (_cyclicBufferPos << 1) + 1;
			uint ptr0 = _cyclicBufferPos << 1;
			uint len1;
			uint len0 = (len1 = kNumHashDirectBytes);
			uint count = _cutValue;
			while (true)
			{
				if (curMatch <= matchMinPos || count-- == 0)
				{
					_son[ptr1] = (_son[ptr0] = 0u);
					break;
				}
				uint delta = _pos - curMatch;
				uint cyclicPos = ((delta <= _cyclicBufferPos) ? (_cyclicBufferPos - delta) : (_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;
				uint pby1 = _bufferOffset + curMatch;
				uint len = Math.Min(len0, len1);
				if (_bufferBase[pby1 + len] == _bufferBase[cur + len])
				{
					while (++len != lenLimit && _bufferBase[pby1 + len] == _bufferBase[cur + len])
					{
					}
					if (len == lenLimit)
					{
						_son[ptr0] = _son[cyclicPos];
						_son[ptr1] = _son[cyclicPos + 1];
						break;
					}
				}
				if (_bufferBase[pby1 + len] < _bufferBase[cur + len])
				{
					_son[ptr0] = curMatch;
					ptr0 = cyclicPos + 1;
					curMatch = _son[ptr0];
					len1 = len;
				}
				else
				{
					_son[ptr1] = curMatch;
					ptr1 = cyclicPos;
					curMatch = _son[ptr1];
					len0 = len;
				}
			}
			MovePos();
		}
		while (--num != 0);
	}

	private void NormalizeLinks(uint[] items, uint numItems, uint subValue)
	{
		for (uint i = 0u; i < numItems; i++)
		{
			uint value = items[i];
			value = ((value > subValue) ? (value - subValue) : 0u);
			items[i] = value;
		}
	}

	private void Normalize()
	{
		uint subValue = _pos - _cyclicBufferSize;
		NormalizeLinks(_son, _cyclicBufferSize * 2, subValue);
		NormalizeLinks(_hash, _hashSizeSum, subValue);
		ReduceOffsets((int)subValue);
	}

	public void SetCutValue(uint cutValue)
	{
		_cutValue = cutValue;
	}
}
