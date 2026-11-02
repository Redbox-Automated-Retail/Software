using System;
using Redbox.Remoting.Nrbf;

namespace Redbox.BrokerServices.Proxy.Gtcp;

/// <summary>
/// The .NET Remoting pieces that have no type on .NET 10: the ObjRef that hands the broker our callback object, and
/// turning a BinaryMethodReturn into a result or an exception (gtcp-protocol.md 5.1).
/// </summary>
internal static class RemotingMessages
{
	/// <summary>
	/// A System.Runtime.Remoting.ObjRef for a callback object. The broker only needs (5.1):
	/// <list type="bullet">
	/// <item>uri: the object URI it will put in __RequestUri of every callback;</item>
	/// <item>typeInfo.interfacesImplemented: the interface it casts the proxy to (serverType can be any loadable type);</item>
	/// <item>channelInfo.channelData: a ChannelDataStore whose first URI is our host URI + "/", which tells the broker's
	/// GTCP channel to call back over the persistent connection we opened.</item>
	/// </list>
	/// This minimal form (no CrossAppDomainData, no call context) was accepted by the broker (spec probe).
	/// </summary>
	public static NrbfSyntheticObject ObjRef(string objectUri, string hostUri, Type interfaceType)
	{
		NrbfSyntheticObject typeInfo = new NrbfSyntheticObject("System.Runtime.Remoting.TypeInfo", null,
			("serverType", NrbfMemberType.String, "System.MarshalByRefObject, " + FrameworkTypeNames.Mscorlib),
			("serverHierarchy", NrbfMemberType.StringArray, null),
			("interfacesImplemented", NrbfMemberType.StringArray, new[] { FrameworkTypeNames.AssemblyQualifiedName(interfaceType) }));
		NrbfSyntheticObject channelDataStore = new NrbfSyntheticObject("System.Runtime.Remoting.Channels.ChannelDataStore", null,
			("_channelURIs", NrbfMemberType.StringArray, new[] { hostUri + "/" }),
			("_extraData", NrbfMemberType.SystemClass("System.Collections.DictionaryEntry[]"), null));
		NrbfSyntheticObject channelInfo = new NrbfSyntheticObject("System.Runtime.Remoting.ChannelInfo", null,
			("channelData", NrbfMemberType.ObjectArray, new object[] { channelDataStore }));
		return new NrbfSyntheticObject("System.Runtime.Remoting.ObjRef", null,
			("uri", NrbfMemberType.String, objectUri),
			("objrefFlags", NrbfMemberType.Of(NrbfPrimitiveType.Int32), 0),
			("typeInfo", NrbfMemberType.SystemClass("System.Runtime.Remoting.TypeInfo"), typeInfo),
			("envoyInfo", NrbfMemberType.SystemClass("System.Runtime.Remoting.IEnvoyInfo"), null),
			("channelInfo", NrbfMemberType.SystemClass("System.Runtime.Remoting.ChannelInfo"), channelInfo));
	}

	/// <summary>
	/// Reads the reply to one of our calls and returns its return value (still an NRBF graph), or throws
	/// <see cref="BrokerRemoteException"/> if the broker reported an exception, either as a BinaryMethodReturn with
	/// ExceptionInArray or as a bare serialized exception flagged by the __gensrlex header (section 4).
	/// </summary>
	public static object? ReadReturnValue(GtcpMessage reply, string methodName)
	{
		if (reply.Headers.ContainsKey(GtcpMessage.SerializedExceptionHeader))
		{
			throw BrokerRemoteException.From(methodName, NrbfReader.ReadGraph(reply.Content));
		}
		NrbfMethodReturn methodReturn = NrbfReader.ReadMethodReturn(reply.Content);
		if (methodReturn.IsException)
		{
			throw BrokerRemoteException.From(methodName, methodReturn.Exception);
		}
		return methodReturn.ReturnValue;
	}
}

/// <summary>The broker answered a call with an exception.</summary>
public sealed class BrokerRemoteException : Exception
{
	public BrokerRemoteException(string message, string remoteType, string? remoteStackTrace)
		: base(message)
	{
		RemoteType = remoteType;
		RemoteStackTrace = remoteStackTrace;
	}

	/// <summary>The exception's class name on the broker, e.g. System.InvalidCastException.</summary>
	public string RemoteType { get; }

	public string? RemoteStackTrace { get; }

	internal static BrokerRemoteException From(string methodName, object? serializedException)
	{
		NrbfObject? exception = serializedException as NrbfObject;
		string type = exception?["ClassName"] as string ?? exception?.TypeName ?? "unknown exception";
		string message = exception?["Message"] as string ?? "(no message)";
		return new BrokerRemoteException($"The broker failed {methodName}: {type}: {message}", type, exception?["StackTraceString"] as string);
	}
}
