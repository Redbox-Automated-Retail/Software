using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Compression.LZ;

[ComVisible(true)]
public class OutWindow
{
	private byte[] _buffer;

	private uint _pos;

	private uint _windowSize;

	private uint _streamPos;

	private Stream _stream;

	public void Create(uint windowSize)
	{
		if (_windowSize != windowSize)
		{
			_buffer = new byte[windowSize];
		}
		_windowSize = windowSize;
		_pos = 0u;
		_streamPos = 0u;
	}

	public void Init(Stream stream, bool solid)
	{
		ReleaseStream();
		_stream = stream;
		if (!solid)
		{
			_streamPos = 0u;
			_pos = 0u;
		}
	}

	public void Init(Stream stream)
	{
		Init(stream, solid: false);
	}

	public void ReleaseStream()
	{
		Flush();
		_stream = null;
	}

	public void Flush()
	{
		uint size = _pos - _streamPos;
		if (size != 0)
		{
			_stream.Write(_buffer, (int)_streamPos, (int)size);
			if (_pos >= _windowSize)
			{
				_pos = 0u;
			}
			_streamPos = _pos;
		}
	}

	public void CopyBlock(uint distance, uint len)
	{
		uint pos = _pos - distance - 1;
		if (pos >= _windowSize)
		{
			pos += _windowSize;
		}
		while (len != 0)
		{
			if (pos >= _windowSize)
			{
				pos = 0u;
			}
			_buffer[_pos++] = _buffer[pos++];
			if (_pos >= _windowSize)
			{
				Flush();
			}
			len--;
		}
	}

	public void PutByte(byte b)
	{
		_buffer[_pos++] = b;
		if (_pos >= _windowSize)
		{
			Flush();
		}
	}

	public byte GetByte(uint distance)
	{
		uint pos = _pos - distance - 1;
		if (pos >= _windowSize)
		{
			pos += _windowSize;
		}
		return _buffer[pos];
	}
}
