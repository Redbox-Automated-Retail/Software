using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Redbox.Remoting.Nrbf;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// A GenuineChannels persistent GTCP client connection (gtcp-protocol.md sections 3, 4, 6 and 7):
/// <list type="bullet">
/// <item>handshake with a fixed identity (host URI, connection name, host number), reused on reconnects;</item>
/// <item>a reader thread that completes our pending calls and hands the broker's calls to a handler on the thread pool;</item>
/// <item>an empty ping frame after <see cref="GtcpConnectionSettings.PingAfterInactivity"/> without sending, and failure
/// when nothing has been received for <see cref="GtcpConnectionSettings.CloseAfterInactivity"/>;</item>
/// <item>on a broken socket, same-identity reconnection; unanswered calls are re-sent and calls the broker re-sends are
/// answered from a reply cache instead of being run twice;</item>
/// <item><see cref="Closed"/> when the connection is lost for good: reconnection failed, the broker restarted (its URI
/// changed) or the link was silent too long. The caller then has to register again.</item>
/// </list>
/// </summary>
internal sealed class GtcpConnection : IDisposable
{
	private enum State
	{
		New,
		Connected,
		Reconnecting,
		Closed
	}

	/// <summary>
	/// One host URI per process, like a GenuineChannels transport context. The broker identifies the kiosk by it, and
	/// drops messages whose (host URI, MessageId) it saw in the last 180 s, so MessageIds are also per process.
	/// </summary>
	private static readonly string ProcessHostUri = "_gtcp://" + Guid.NewGuid().ToString("N");

	private static int s_lastMessageId = 1; // GenuineChannels starts its counter at 1, so the first id is 2
	private static int s_lastHostNumber;

	private const int PingMessageReplyId = -3;
	private const byte ProtocolVersion = 3;
	private const byte PersistentConnection = 1;

	private readonly string _host;
	private readonly int _port;
	private readonly GtcpConnectionSettings _settings;
	private readonly Func<GtcpMessage, byte[]> _callHandler;
	private readonly Action<string, Exception?> _log;
	private readonly string _connectionName;
	private readonly int _hostNumber;
	private readonly object _stateLock = new object();
	private readonly object _writeLock = new object();
	private readonly ManualResetEventSlim _disposed = new ManualResetEventSlim(false);
	private readonly ConcurrentDictionary<int, PendingCall> _pendingCalls = new ConcurrentDictionary<int, PendingCall>();
	private readonly ConcurrentDictionary<int, ReceivedCall> _receivedCalls = new ConcurrentDictionary<int, ReceivedCall>();

	private State _state = State.New;
	private Socket? _socket;
	private NetworkStream? _stream;
	private int _generation;
	private long _lastSendTicks;
	private long _lastReceiveTicks;
	private Timer? _timer;

	/// <param name="host">Host name as written in the URL; it is also part of the connection name.</param>
	/// <param name="callHandler">Runs a call from the broker (on a thread-pool thread) and returns the reply content.</param>
	public GtcpConnection(string host, int port, GtcpConnectionSettings settings, Func<GtcpMessage, byte[]> callHandler, Action<string, Exception?> log)
	{
		_host = host;
		_port = port;
		_settings = settings;
		_callHandler = callHandler;
		_log = log ?? ((_, _) => { });
		_connectionName = "$/__GC/" + host;
		_hostNumber = Interlocked.Increment(ref s_lastHostNumber);
	}

	/// <summary>Raised once, on a thread-pool thread, when the connection is lost for good (not on Dispose).</summary>
	public event Action<string>? Closed;

	/// <summary>Our host URI ("_gtcp://" + 32 hex digits); the broker calls back to it.</summary>
	public string HostUri => ProcessHostUri;

	/// <summary>The broker's host URI from the handshake; it changes when the broker restarts.</summary>
	public string? ServerUri { get; private set; }

	public bool IsConnected
	{
		get
		{
			lock (_stateLock)
			{
				return _state == State.Connected;
			}
		}
	}

	public static int NextMessageId() => Interlocked.Increment(ref s_lastMessageId);

	public void Open()
	{
		(Socket socket, NetworkStream stream, string serverUri) = ConnectAndHandshake();
		lock (_stateLock)
		{
			if (_state != State.New)
			{
				socket.Dispose();
				throw new InvalidOperationException("The connection was already opened.");
			}
			ServerUri = serverUri;
			Install(socket, stream);
		}
		_timer = new Timer(_ => OnTimer(), null, 1000, 1000);
		_log($"GTCP: connected to {_host}:{_port} as {HostUri} (broker {serverUri}).", null);
	}

	/// <summary>Sends a request and waits for its reply. Throws <see cref="TimeoutException"/> or <see cref="IOException"/>.</summary>
	public GtcpMessage Invoke(string objectUri, byte[] content)
	{
		int messageId = NextMessageId();
		PendingCall call = new PendingCall(GtcpMessage.Request(messageId, objectUri, content).Encode());
		_pendingCalls[messageId] = call;
		try
		{
			lock (_stateLock)
			{
				if (_state == State.Closed || _state == State.New)
				{
					throw new IOException("The connection to the broker is closed.");
				}
			}
			// If the socket breaks now, the request stays pending and is re-sent after reconnecting (section 7).
			TrySend(call.Encoded);
			if (!call.Reply.Task.Wait(_settings.InvocationTimeout))
			{
				throw new TimeoutException($"The broker did not answer within {_settings.InvocationTimeout.TotalSeconds:0} s.");
			}
			return call.Reply.Task.GetAwaiter().GetResult();
		}
		catch (AggregateException e) when (e.InnerException != null)
		{
			throw e.InnerException;
		}
		finally
		{
			_pendingCalls.TryRemove(messageId, out _);
		}
	}

	public void Dispose()
	{
		lock (_stateLock)
		{
			if (_disposed.IsSet)
			{
				return;
			}
			_disposed.Set();
			_state = State.Closed;
			CloseSocket();
		}
		_timer?.Dispose();
		FailPendingCalls(new ObjectDisposedException(nameof(GtcpConnection)));
	}

	// ------------------------------------------------------------------ connection and handshake (section 3)

	private (Socket Socket, NetworkStream Stream, string ServerUri) ConnectAndHandshake()
	{
		Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
		try
		{
			using (CancellationTokenSource timeout = new CancellationTokenSource(_settings.ConnectTimeout))
			{
				// GenuineChannels 2.5.9.5 is IPv4-only: try IPv4 addresses first, so "localhost" does not first wait
				// for a refused ::1.
				IPAddress[] addresses = Dns.GetHostAddressesAsync(_host, timeout.Token).GetAwaiter().GetResult()
					.OrderBy(a => a.AddressFamily == AddressFamily.InterNetwork ? 0 : 1)
					.ToArray();
				socket.ConnectAsync(addresses, _port, timeout.Token).AsTask().GetAwaiter().GetResult();
			}
			NetworkStream stream = new NetworkStream(socket, ownsSocket: false)
			{
				ReadTimeout = (int)_settings.ConnectTimeout.TotalMilliseconds
			};
			// Client → broker: the connection header, then our host information, in one write.
			using MemoryStream hello = new MemoryStream();
			hello.Write(GtcpFraming.Encode(ConnectionHeader()));
			hello.Write(GtcpFraming.Encode(HostInformation()));
			stream.Write(hello.GetBuffer(), 0, (int)hello.Length);
			// Broker → client: lps serverUri, int32 its number for our HostInformation.
			WireReader reply = new WireReader(GtcpFraming.ReadMessage(stream));
			string serverUri = reply.ReadString();
			reply.ReadInt32();
			stream.ReadTimeout = Timeout.Infinite;
			return (socket, stream, serverUri);
		}
		catch (OperationCanceledException)
		{
			socket.Dispose();
			throw new IOException($"Could not connect to the broker at {_host}:{_port} within {_settings.ConnectTimeout.TotalSeconds:0} s.");
		}
		catch
		{
			socket.Dispose();
			throw;
		}
	}

	// byte protocol version 3, byte connection type (1 = persistent), lps connection name "$/__GC/<host>".
	private byte[] ConnectionHeader()
	{
		using MemoryStream stream = new MemoryStream();
		stream.WriteByte(ProtocolVersion);
		stream.WriteByte(PersistentConnection);
		WireBinary.WriteString(stream, _connectionName);
		return stream.ToArray();
	}

	// lps host URI, int32 port (-1: we do not listen), byte connection type, int32 our number for the broker's
	// HostInformation. The same values on a reconnect make the broker keep the session (and the registration).
	private byte[] HostInformation()
	{
		using MemoryStream stream = new MemoryStream();
		WireBinary.WriteString(stream, ProcessHostUri);
		WireBinary.WriteInt32(stream, -1);
		stream.WriteByte(PersistentConnection);
		WireBinary.WriteInt32(stream, _hostNumber);
		return stream.ToArray();
	}

	// Called with _stateLock held.
	private void Install(Socket socket, NetworkStream stream)
	{
		_socket = socket;
		_stream = stream;
		_state = State.Connected;
		int generation = ++_generation;
		long now = Environment.TickCount64;
		Interlocked.Exchange(ref _lastReceiveTicks, now);
		Interlocked.Exchange(ref _lastSendTicks, now);
		Thread reader = new Thread(() => ReadLoop(stream, generation))
		{
			IsBackground = true,
			Name = "GTCP reader " + generation
		};
		reader.Start();
	}

	// Called with _stateLock held.
	private void CloseSocket()
	{
		try
		{
			_socket?.Shutdown(SocketShutdown.Both);
		}
		catch (SocketException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
		_stream?.Dispose();
		_socket?.Dispose();
		_stream = null;
		_socket = null;
	}

	// ------------------------------------------------------------------ receiving

	private void ReadLoop(NetworkStream stream, int generation)
	{
		try
		{
			while (true)
			{
				byte[] data = GtcpFraming.ReadMessage(stream);
				Interlocked.Exchange(ref _lastReceiveTicks, Environment.TickCount64);
				if (data.Length == 0)
				{
					continue; // ping
				}
				GtcpMessage message = GtcpMessage.Decode(data);
				if (message.IsReply)
				{
					// Unknown ReplyToIds are expected: after a reconnect the broker re-sends its last reply (section 7).
					if (_pendingCalls.TryGetValue(message.ReplyToId, out PendingCall? call))
					{
						call.Reply.TrySetResult(message);
					}
				}
				else if (message.ReplyToId != PingMessageReplyId)
				{
					OnCallReceived(message);
				}
			}
		}
		catch (Exception e)
		{
			OnConnectionBroken(generation, e);
		}
	}

	private void OnCallReceived(GtcpMessage message)
	{
		ReceivedCall received = new ReceivedCall(Environment.TickCount64);
		ReceivedCall existing = _receivedCalls.GetOrAdd(message.MessageId, received);
		if (existing != received)
		{
			// The broker re-sent a call it had sent before the connection broke: answer it again from the cache,
			// or ignore it while the first one is still running (its reply goes out on the current socket).
			byte[]? cachedReply = existing.EncodedReply;
			if (cachedReply != null)
			{
				_log($"GTCP: re-sending the reply to broker call {message.MessageId}.", null);
				TrySend(cachedReply);
			}
			return;
		}
		ThreadPool.QueueUserWorkItem(_ =>
		{
			byte[] content = _callHandler(message);
			if (message.IsOneWay)
			{
				return;
			}
			byte[] reply = GtcpMessage.Reply(NextMessageId(), message.MessageId, content).Encode();
			received.EncodedReply = reply;
			TrySend(reply);
		});
	}

	// ------------------------------------------------------------------ sending

	/// <summary>Sends an encoded message; a write failure breaks the connection (and starts reconnecting).</summary>
	private bool TrySend(byte[] encodedMessage) => TrySendFrames(GtcpFraming.Encode(encodedMessage));

	private bool TrySendFrames(byte[] frames)
	{
		NetworkStream stream;
		int generation;
		lock (_stateLock)
		{
			if (_state != State.Connected)
			{
				return false;
			}
			stream = _stream!; // Connected: Install has set it
			generation = _generation;
		}
		try
		{
			// One message at a time: frames of different messages must never interleave (section 2).
			lock (_writeLock)
			{
				stream.Write(frames, 0, frames.Length);
				Interlocked.Exchange(ref _lastSendTicks, Environment.TickCount64);
			}
			return true;
		}
		catch (Exception e) when (e is IOException || e is SocketException || e is ObjectDisposedException)
		{
			OnConnectionBroken(generation, e);
			return false;
		}
	}

	// Once a second: keep-alive ping, inactivity check, forget old received calls (section 6).
	private void OnTimer()
	{
		try
		{
			long now = Environment.TickCount64;
			if (IsConnected)
			{
				if (now - Interlocked.Read(ref _lastSendTicks) >= (long)_settings.PingAfterInactivity.TotalMilliseconds)
				{
					TrySendFrames(GtcpFraming.PingFrame);
				}
				if (now - Interlocked.Read(ref _lastReceiveTicks) >= (long)_settings.CloseAfterInactivity.TotalMilliseconds)
				{
					// Like GenuineChannels' ConnectionClosedAfterTimeout this is critical: no reconnection, register again.
					CloseForGood($"nothing received from the broker for {_settings.CloseAfterInactivity.TotalSeconds:0} s");
				}
			}
			long window = (long)_settings.DuplicateCallWindow.TotalMilliseconds;
			foreach (var entry in _receivedCalls)
			{
				if (entry.Value.EncodedReply != null && now - entry.Value.ReceivedTicks > window)
				{
					_receivedCalls.TryRemove(entry.Key, out _);
				}
			}
		}
		catch (Exception e)
		{
			_log("GTCP: timer failed.", e);
		}
	}

	// ------------------------------------------------------------------ failure and reconnection (section 7)

	private void OnConnectionBroken(int generation, Exception reason)
	{
		lock (_stateLock)
		{
			if (_disposed.IsSet || generation != _generation || _state != State.Connected)
			{
				return;
			}
			_state = State.Reconnecting;
			CloseSocket();
		}
		_log($"GTCP: connection to the broker lost ({reason.Message}); reconnecting.", null);
		new Thread(Reconnect) { IsBackground = true, Name = "GTCP reconnect" }.Start();
	}

	private void Reconnect()
	{
		long deadline = Environment.TickCount64 + (long)_settings.MaxTimeSpanToReconnect.TotalMilliseconds;
		for (int attempt = 1; attempt <= _settings.ReconnectionAttempts; attempt++)
		{
			if (_disposed.Wait(_settings.SleepBetweenReconnections) || Environment.TickCount64 > deadline)
			{
				break;
			}
			try
			{
				(Socket socket, NetworkStream stream, string serverUri) = ConnectAndHandshake();
				if (serverUri != ServerUri)
				{
					socket.Dispose();
					CloseForGood($"the broker restarted (its URI changed from {ServerUri} to {serverUri})");
					return;
				}
				lock (_stateLock)
				{
					if (_disposed.IsSet || _state != State.Reconnecting)
					{
						socket.Dispose();
						return;
					}
					Install(socket, stream);
				}
				_log($"GTCP: reconnected to the broker (attempt {attempt}).", null);
				foreach (PendingCall call in _pendingCalls.Values)
				{
					TrySend(call.Encoded);
				}
				return;
			}
			catch (Exception e)
			{
				_log($"GTCP: reconnection attempt {attempt} failed: {e.Message}", null);
			}
		}
		CloseForGood("the connection could not be re-established");
	}

	private void CloseForGood(string reason)
	{
		lock (_stateLock)
		{
			if (_disposed.IsSet || _state == State.Closed)
			{
				return;
			}
			_state = State.Closed;
			CloseSocket();
		}
		_log($"GTCP: connection to the broker closed: {reason}.", null);
		FailPendingCalls(new IOException("The connection to the broker was closed: " + reason));
		Action<string>? closed = Closed;
		if (closed != null)
		{
			ThreadPool.QueueUserWorkItem(_ => closed(reason));
		}
	}

	private void FailPendingCalls(Exception reason)
	{
		foreach (PendingCall call in _pendingCalls.Values)
		{
			call.Reply.TrySetException(reason);
		}
	}

	private sealed class PendingCall
	{
		public PendingCall(byte[] encoded)
		{
			Encoded = encoded;
		}

		public byte[] Encoded { get; }

		public TaskCompletionSource<GtcpMessage> Reply { get; } = new TaskCompletionSource<GtcpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
	}

	private sealed class ReceivedCall
	{
		public ReceivedCall(long receivedTicks)
		{
			ReceivedTicks = receivedTicks;
		}

		public long ReceivedTicks { get; }

		public volatile byte[]? EncodedReply;
	}
}
