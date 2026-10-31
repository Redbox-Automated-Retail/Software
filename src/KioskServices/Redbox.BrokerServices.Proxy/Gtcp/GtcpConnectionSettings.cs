using System;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// Connection parameters. The defaults are the kiosk's GenuineChannels settings (BrokerServicesProxy.RegisterChannel in
/// the .NET Framework build) plus the GenuineChannels defaults the broker relies on (gtcp-protocol.md sections 6 and 7).
/// </summary>
internal sealed class GtcpConnectionSettings
{
	/// <summary>ConnectTimeout: TCP connect plus handshake.</summary>
	public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromMilliseconds(5000);

	/// <summary>InvocationTimeout: how long a call to the broker waits for its reply.</summary>
	public TimeSpan InvocationTimeout { get; init; } = TimeSpan.FromMilliseconds(30000);

	/// <summary>PersistentConnectionSendPingAfterInactivity: send an empty frame after this long without sending.</summary>
	public TimeSpan PingAfterInactivity { get; init; } = TimeSpan.FromMilliseconds(20000);

	/// <summary>
	/// ClosePersistentConnectionAfterInactivity (GenuineChannels default 100 s, both sides): a connection on which nothing
	/// arrives for this long is dead. The broker pings every 20 s, so this only fires on a silently broken link.
	/// </summary>
	public TimeSpan CloseAfterInactivity { get; init; } = TimeSpan.FromSeconds(100);

	/// <summary>SleepBetweenReconnections: wait before each reconnection attempt.</summary>
	public TimeSpan SleepBetweenReconnections { get; init; } = TimeSpan.FromMilliseconds(5000);

	/// <summary>Number of same-identity reconnection attempts (GenuineChannels ReconnectionTries 1 gives 2 attempts).</summary>
	public int ReconnectionAttempts { get; init; } = 2;

	/// <summary>MaxTimeSpanToReconnect: give up reconnecting after this long.</summary>
	public TimeSpan MaxTimeSpanToReconnect { get; init; } = TimeSpan.FromMilliseconds(30000);

	/// <summary>How long a received call is remembered to drop re-sent duplicates (GenuineChannels UniqueCallTracer).</summary>
	public TimeSpan DuplicateCallWindow { get; init; } = TimeSpan.FromSeconds(180);
}
