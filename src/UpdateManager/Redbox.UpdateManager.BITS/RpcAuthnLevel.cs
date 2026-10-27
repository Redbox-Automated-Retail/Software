using System;

namespace Redbox.UpdateManager.BITS;

[Flags]
public enum RpcAuthnLevel
{
	Default = 0,
	None = 1,
	Connect = 2,
	Call = None | Connect,
	Pkt = 4,
	PktIntegrity = None | Pkt,
	PktPrivacy = Connect | Pkt
}
