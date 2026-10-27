using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal abstract class AbstractIPCResponse : IIPCResponse, IDisposable
{
	protected readonly bool LogDetails;

	protected readonly StringBuilder ReadBuilder = new StringBuilder();

	private bool Disposed;

	public bool IsComplete { get; protected set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			OnDispose(fromDispose: true);
			GC.SuppressFinalize(this);
		}
	}

	public bool Accumulate(byte[] rawResponse)
	{
		return Accumulate(rawResponse, 0, rawResponse.Length);
	}

	public bool Accumulate(byte[] bytes, int start, int length)
	{
		ReadBuilder.Append(Encoding.ASCII.GetString(bytes, start, length));
		return IsComplete = OnTestResponse();
	}

	public void Clear()
	{
		IsComplete = false;
		if (ReadBuilder.Length > 0)
		{
			ReadBuilder.Length = 0;
		}
		OnClear();
	}

	protected abstract void OnClear();

	protected virtual void OnDispose(bool fromDispose)
	{
	}

	protected abstract bool OnTestResponse();

	protected string GetNextBufferLine()
	{
		string text = ReadBuilder.ToString();
		int num = text.IndexOf(Environment.NewLine);
		if (-1 != num)
		{
			string text2 = text.Substring(0, num + Environment.NewLine.Length);
			ReadBuilder.Remove(0, num + Environment.NewLine.Length);
			return text2.Trim();
		}
		return string.Empty;
	}

	protected bool BufferHasMoreLines()
	{
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
	}

	protected AbstractIPCResponse()
		: this(logDetails: false)
	{
	}

	protected AbstractIPCResponse(bool logDetails)
	{
		LogDetails = logDetails;
	}
}
