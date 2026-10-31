using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using Redbox.Remoting.Nrbf;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// One GTCP message after framing (gtcp-protocol.md section 4; GenuineChannels ConnectionManager.Send and
/// MessageCoder.Serialize/Deserialize):
/// <code>
///   lps   security session name   "D" (the built-in session that leaves the content unchanged)
///   byte  compressed              0 (1 = the rest is GZip; never used with the broker's settings)
///   byte  message type            1 = Ordinary
///   int32 MessageId               sender's per-process counter
///   int32 ReplyToId               0 = request, otherwise the MessageId being answered
///   byte  IsOneWay, byte has-destination-MBR, int16 security session attributes, lps remote transport user
///   { lps key, lps value }* lps "__"   transport headers
///   ...   content (MS-NRBF)
/// </code>
/// </summary>
internal sealed class GtcpMessage
{
	public const string RequestUriHeader = "__RequestUri";

	public const string ContentTypeHeader = "Content-Type";

	public const string BinaryContentType = "application/octet-stream";

	/// <summary>Set by GenuineChannels' server sink when the content is a bare serialized exception (section 4).</summary>
	public const string SerializedExceptionHeader = "__gensrlex";

	private const string SecuritySessionName = "D";

	private const byte OrdinaryMessageType = 1;

	private const string HeadersEnd = "__";

	public int MessageId { get; init; }

	public int ReplyToId { get; init; }

	public bool IsOneWay { get; init; }

	public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

	public byte[] Content { get; init; } = Array.Empty<byte>();

	public bool IsReply => ReplyToId > 0;

	public string RequestUri => Headers.TryGetValue(RequestUriHeader, out string uri) ? uri : null;

	public static GtcpMessage Request(int messageId, string objectUri, byte[] content) => new GtcpMessage
	{
		MessageId = messageId,
		Headers = new Dictionary<string, string>
		{
			[RequestUriHeader] = objectUri,
			[ContentTypeHeader] = BinaryContentType
		},
		Content = content
	};

	/// <summary>Replies carry no transport headers (section 4, captured from the real client).</summary>
	public static GtcpMessage Reply(int messageId, int replyToId, byte[] content) => new GtcpMessage
	{
		MessageId = messageId,
		ReplyToId = replyToId,
		Content = content
	};

	public byte[] Encode()
	{
		using MemoryStream stream = new MemoryStream(Content.Length + 128);
		WireBinary.WriteString(stream, SecuritySessionName);
		stream.WriteByte(0);
		stream.WriteByte(OrdinaryMessageType);
		WireBinary.WriteInt32(stream, MessageId);
		WireBinary.WriteInt32(stream, ReplyToId);
		stream.WriteByte(IsOneWay ? (byte)1 : (byte)0);
		stream.WriteByte(0);
		WireBinary.WriteInt16(stream, 0);
		WireBinary.WriteString(stream, string.Empty);
		// Header order matters only for byte-level comparison with the captures: __RequestUri, then Content-Type.
		foreach (string key in OrderedHeaderKeys())
		{
			WireBinary.WriteString(stream, key);
			WireBinary.WriteString(stream, Headers[key]);
		}
		WireBinary.WriteString(stream, HeadersEnd);
		stream.Write(Content, 0, Content.Length);
		return stream.ToArray();
	}

	public static GtcpMessage Decode(byte[] data)
	{
		WireReader reader = new WireReader(data);
		reader.ReadString(); // security session name; only "D" is configured on the broker
		if (reader.ReadBoolean())
		{
			reader = new WireReader(Decompress(data, reader.Position));
		}
		reader.ReadByte(); // message type
		int messageId = reader.ReadInt32();
		int replyToId = reader.ReadInt32();
		bool isOneWay = reader.ReadBoolean();
		if (reader.ReadBoolean())
		{
			throw new InvalidDataException("GTCP messages with a destination MarshalByRef (broadcast) are not supported.");
		}
		reader.ReadInt16(); // security session attributes
		reader.ReadString(); // remote transport user
		Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.Ordinal);
		while (true)
		{
			string key = reader.ReadString();
			if (key == HeadersEnd)
			{
				break;
			}
			headers[key] = reader.ReadString();
		}
		return new GtcpMessage
		{
			MessageId = messageId,
			ReplyToId = replyToId,
			IsOneWay = isOneWay,
			Headers = headers,
			Content = reader.ReadRemaining()
		};
	}

	private IEnumerable<string> OrderedHeaderKeys()
	{
		if (Headers.ContainsKey(RequestUriHeader))
		{
			yield return RequestUriHeader;
		}
		if (Headers.ContainsKey(ContentTypeHeader))
		{
			yield return ContentTypeHeader;
		}
		foreach (string key in Headers.Keys)
		{
			if (key != RequestUriHeader && key != ContentTypeHeader)
			{
				yield return key;
			}
		}
	}

	private static byte[] Decompress(byte[] data, int offset)
	{
		using GZipStream gzip = new GZipStream(new MemoryStream(data, offset, data.Length - offset), CompressionMode.Decompress);
		using MemoryStream output = new MemoryStream();
		gzip.CopyTo(output);
		return output.ToArray();
	}

	public override string ToString() => $"GTCP message {MessageId} (reply to {ReplyToId}, {Content.Length} bytes)";
}
