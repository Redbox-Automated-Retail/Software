using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Compression.LZ;

[ComVisible(true)]
public class InWindow
{
	public byte[] _bufferBase;

	private Stream _stream;

	private uint _posLimit;

	private bool _streamEndWasReached;

	private uint _pointerToLastSafePosition;

	public uint _bufferOffset;

	public uint _blockSize;

	public uint _pos;

	private uint _keepSizeBefore;

	private uint _keepSizeAfter;

	public uint _streamPos;

	public void MoveBlock()
	{
		uint offset = _bufferOffset + _pos - _keepSizeBefore;
		if (offset != 0)
		{
			offset--;
		}
		uint numBytes = _bufferOffset + _streamPos - offset;
		for (uint i = 0u; i < numBytes; i++)
		{
			_bufferBase[i] = _bufferBase[offset + i];
		}
		_bufferOffset -= offset;
	}

	public virtual void ReadBlock()
	{
		if (_streamEndWasReached)
		{
			return;
		}
		while (true)
		{
			int size = (int)(0 - _bufferOffset + _blockSize - _streamPos);
			if (size == 0)
			{
				return;
			}
			int numReadBytes = _stream.Read(_bufferBase, (int)(_bufferOffset + _streamPos), size);
			if (numReadBytes == 0)
			{
				break;
			}
			_streamPos += (uint)numReadBytes;
			if (_streamPos >= _pos + _keepSizeAfter)
			{
				_posLimit = _streamPos - _keepSizeAfter;
			}
		}
		_posLimit = _streamPos;
		uint pointerToPosition = _bufferOffset + _posLimit;
		if (pointerToPosition > _pointerToLastSafePosition)
		{
			_posLimit = _pointerToLastSafePosition - _bufferOffset;
		}
		_streamEndWasReached = true;
	}

	private void Free()
	{
		_bufferBase = null;
	}

	public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
	{
		_keepSizeBefore = keepSizeBefore;
		_keepSizeAfter = keepSizeAfter;
		uint blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
		if (_bufferBase == null || _blockSize != blockSize)
		{
			Free();
			_blockSize = blockSize;
			_bufferBase = new byte[_blockSize];
		}
		_pointerToLastSafePosition = _blockSize - keepSizeAfter;
	}

	public void SetStream(Stream stream)
	{
		_stream = stream;
	}

	public void ReleaseStream()
	{
		_stream = null;
	}

	public void Init()
	{
		_bufferOffset = 0u;
		_pos = 0u;
		_streamPos = 0u;
		_streamEndWasReached = false;
		ReadBlock();
	}

	public void MovePos()
	{
		_pos++;
		if (_pos > _posLimit)
		{
			uint pointerToPosition = _bufferOffset + _pos;
			if (pointerToPosition > _pointerToLastSafePosition)
			{
				MoveBlock();
			}
			ReadBlock();
		}
	}

	public byte GetIndexByte(int index)
	{
		return _bufferBase[_bufferOffset + _pos + index];
	}

	public uint GetMatchLen(int index, uint distance, uint limit)
	{
		if (_streamEndWasReached && _pos + index + limit > _streamPos)
		{
			limit = _streamPos - (uint)(int)(_pos + index);
		}
		distance++;
		uint pby = _bufferOffset + _pos + (uint)index;
		uint i;
		for (i = 0u; i < limit && _bufferBase[pby + i] == _bufferBase[pby + i - distance]; i++)
		{
		}
		return i;
	}

	public uint GetNumAvailableBytes()
	{
		return _streamPos - _pos;
	}

	public void ReduceOffsets(int subValue)
	{
		_bufferOffset += (uint)subValue;
		_posLimit -= (uint)subValue;
		_pos -= (uint)subValue;
		_streamPos -= (uint)subValue;
	}
}
