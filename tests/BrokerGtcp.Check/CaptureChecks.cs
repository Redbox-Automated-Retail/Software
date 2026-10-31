using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.BrokerServices.Proxy.Gtcp;
using Redbox.Remoting.Nrbf;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;
using Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces;

namespace BrokerGtcp.Check;

/// <summary>
/// Offline checks against messages captured between the real kiosk client (.NET Framework 4.8 + GenuineChannels
/// 2.5.9.5) and the broker (rb-remoting): Captures\*.bin hold one GTCP message each (frames joined: envelope +
/// NRBF content). "c2s" = written by the real client, "s2c" = written by the broker (Mono BinaryFormatter).
/// </summary>
internal static class CaptureChecks
{
	public static void Run()
	{
		Console.WriteLine("-- framing and envelope");
		Check.Run("framing", Framing);
		Check.Run("envelope round trip", EnvelopeRoundTrip);
		Console.WriteLine("-- NRBF reader: captured broker calls and replies");
		Check.Run("Reserve3 cart", Reserve3Cart);
		Check.Run("Ping and CancelReservation calls", PingAndCancelCalls);
		Check.Run("Register call (real client)", CapturedRegisterCall);
		Check.Run("Register reply and exception reply", RegisterAndExceptionReplies);
		Console.WriteLine("-- NRBF writer: replies byte-for-byte like the real client");
		Check.Run("reply bytes", ReplyBytes);
		Check.Run("dispatch of captured calls", DispatchCapturedCalls);
		Console.WriteLine("-- NRBF writer/reader round trips");
		Check.Run("Register call written by the GTCP client", WrittenRegisterCall);
		Check.Run("ReservationCart3 round trip", CartRoundTrip);
		Check.Run("exception return", ExceptionReturn);
	}

	private static byte[] Capture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Captures", name));

	private static byte[] Content(string name) => GtcpMessage.Decode(Capture(name)).Content;

	// ------------------------------------------------------------------ framing / envelope

	private static void Framing()
	{
		byte[] small = { 1, 2, 3 };
		Check.Bytes(new byte[] { 0x37, 3, 0, 0, 0, 1, 1, 2, 3 }, GtcpFraming.Encode(small), "framing: one frame");
		byte[] large = Enumerable.Range(0, 150000).Select(i => (byte)i).ToArray();
		byte[] frames = GtcpFraming.Encode(large);
		Check.Equal(150000 + 3 * GtcpFraming.HeaderSize, frames.Length, "framing: 150000 bytes need 3 frames");
		Check.Bytes(large, GtcpFraming.ReadMessage(new MemoryStream(frames)), "framing: multi-frame message reads back");
		// A sender whose content exactly fills its buffer ends with an empty last frame (section 2).
		byte[] withEmptyLast = { 0x37, 2, 0, 0, 0, 0, 9, 8, 0x37, 0, 0, 0, 0, 1 };
		Check.Bytes(new byte[] { 9, 8 }, GtcpFraming.ReadMessage(new MemoryStream(withEmptyLast)), "framing: empty final frame accepted");
		Check.Equal(0, GtcpFraming.ReadMessage(new MemoryStream(GtcpFraming.PingFrame)).Length, "framing: ping is an empty message");
	}

	private static void EnvelopeRoundTrip()
	{
		foreach (string name in new[] { "register-call.c2s.bin", "register-reply.s2c.bin", "ping-call.s2c.bin", "ping-reply.c2s.bin",
			"reserve3-call.s2c.bin", "reserve3-reply.c2s.bin", "cancel-call.s2c.bin", "cancel-reply.c2s.bin" })
		{
			byte[] original = Capture(name);
			GtcpMessage message = GtcpMessage.Decode(original);
			Check.Bytes(original, message.Encode(), "envelope: " + name + " decodes and re-encodes");
		}
		GtcpMessage call = GtcpMessage.Decode(Capture("register-call.c2s.bin"));
		Check.Equal(2, call.MessageId, "envelope: Register MessageId");
		Check.Equal("RegistrationBroker/Default.rem", call.RequestUri, "envelope: Register __RequestUri");
		GtcpMessage reply = GtcpMessage.Decode(Capture("register-reply.s2c.bin"));
		Check.Equal(2, reply.ReplyToId, "envelope: Register reply ReplyToId");
	}

	// ------------------------------------------------------------------ reader

	private static void Reserve3Cart()
	{
		NrbfMethodCall call = NrbfReader.ReadMethodCall(Content("reserve3-call.s2c.bin"));
		Check.Equal("Reserve3", call.MethodName, "Reserve3: method name");
		Check.Equal(FrameworkTypeNames.AssemblyQualifiedName(typeof(IKioskOperations)), call.TypeName, "Reserve3: type name matches our KioskShared build");
		ReservationCart3 cart = NrbfObjectMapper.Map<ReservationCart3>(call.Args.Single());
		// Expected values: captures\run1\conn1.decoded.txt, S->C message #3.
		Check.Equal(990010, cart.KioskID, "cart.KioskID");
		Check.Equal(123456789L, cart.ReferenceNumber, "cart.ReferenceNumber");
		Check.Equal("card-1", cart.CardID, "cart.CardID");
		Check.Equal(false, cart.IsGc, "cart.IsGc");
		Check.Equal(0, cart.GcIds.Count, "cart.GcIds is empty (capacity-0 list)");
		Check.Equal("cc1", string.Join(",", cart.CcIds), "cart.CcIds (_size 1 of capacity 4)");
		Check.Equal("a@b.test", cart.Email, "cart.Email");
		Check.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), cart.ReserveDate, "cart.ReserveDate");
		Check.Equal(DateTimeKind.Utc, cart.ReserveDate.Kind, "cart.ReserveDate kind bits");
		Check.Equal(1.99m, cart.DiscountedSubTotal, "cart.DiscountedSubTotal");
		Check.Equal(1.99m, cart.SubTotal, "cart.SubTotal");
		Check.Equal(0.15m, cart.Tax, "cart.Tax");
		Check.Equal(2.14m, cart.GrandTotal, "cart.GrandTotal");
		Check.Equal(0m, cart.HiveOnlinePromotionDiscount, "cart.HiveOnlinePromotionDiscount");
		Check.Equal(0, cart.NumberOfCreditsRemaining, "cart.NumberOfCreditsRemaining");
		Check.Equal(null, cart.PromoCode, "cart.PromoCode");
		Check.Equal("C1", cart.CustomerNumber, "cart.CustomerNumber");
		Check.Equal(false, cart.AppliedCredit || cart.AppliedHiveOnlinePromotion || cart.AppliedTitleMarketing, "cart.Applied* all false");
		Check.Equal("7,8", string.Join(",", cart.HistoryTitleIDs), "cart.HistoryTitleIDs (Int32 list)");
		Check.Equal("0.075", cart.TaxRate.ToString(System.Globalization.CultureInfo.InvariantCulture), "cart.TaxRate keeps its scale");
		Check.Equal("60601", cart.Zip, "cart.Zip");
		Check.Equal(false, cart.IsMnp || cart.IsMdv || cart.LoyaltyRedemption, "cart.IsMnp/IsMdv/LoyaltyRedemption false");
		Check.Equal(0m, cart.DefaultServiceFeeAmount, "cart.DefaultServiceFeeAmount");
		Check.Equal(0m, cart.ActualServiceFeeAmount, "cart.ActualServiceFeeAmount");
		Check.Equal(true, cart.AuthorizeAtPickup, "cart.AuthorizeAtPickup");
		Check.Equal("abc", cart.ReservationCardHash?.CardBinHash, "cart.ReservationCardHash.CardBinHash");
		Check.Equal("1234", cart.ReservationCardHash?.Last4, "cart.ReservationCardHash.Last4");
		Check.Equal(0, cart.AlternateCardHashes.Count, "cart.AlternateCardHashes is empty");
		Check.Equal(1, cart.Titles.Count, "cart.Titles count");
		ReservationTitle3 title = cart.Titles[0];
		Check.Equal(1, title.ItemID, "title.ItemID");
		Check.Equal(4711, title.TitleID, "title.TitleID");
		Check.Equal(ReservationType.Rental, title.ReservationType, "title.ReservationType (enum value__)");
		Check.Equal(1.99m, title.Price, "title.Price");
		Check.Equal(0m, title.Discount, "title.Discount");
		Check.Equal(DiscountType.None, title.DiscountType, "title.DiscountType (enum value__)");
		Check.Equal(1.99m, title.DiscountedPrice, "title.DiscountedPrice");
		Check.Equal(0, title.NumberOfCredits, "title.NumberOfCredits");
		Check.Equal(0m, title.LoyaltyPoints, "title.LoyaltyPoints");
	}

	private static void PingAndCancelCalls()
	{
		NrbfMethodCall ping = NrbfReader.ReadMethodCall(Content("ping-call.s2c.bin"));
		Check.Equal("Ping", ping.MethodName, "Ping: method name");
		Check.Equal(0, ping.Args.Length, "Ping: no arguments");
		NrbfMethodCall cancel = NrbfReader.ReadMethodCall(Content("cancel-call.s2c.bin"));
		Check.Equal("CancelReservation", cancel.MethodName, "CancelReservation: method name");
		Check.Equal((object)123456789L, cancel.Args.Single(), "CancelReservation: inline Int64 argument");
	}

	private static void CapturedRegisterCall()
	{
		NrbfMethodCall call = NrbfReader.ReadMethodCall(Content("register-call.c2s.bin"));
		Check.Equal("Register", call.MethodName, "Register (real client): method name");
		Check.Equal(NrbfMessageFlags.ArgsInArray | NrbfMessageFlags.ContextInArray, call.Flags, "Register (real client): flags");
		Check.Equal((object)990010, call.Args[0], "Register (real client): kioskID");
		NrbfObject objRef = call.Args[1] as NrbfObject;
		Check.Equal("System.Runtime.Remoting.ObjRef", objRef?.TypeName, "Register (real client): ObjRef");
		string channelUri = ((objRef?["channelInfo"] as NrbfObject)?["channelData"] as NrbfArray)?.Items
			.OfType<NrbfObject>().Select(o => (o["_channelURIs"] as NrbfArray)?.Items[0] as string).FirstOrDefault(u => u != null);
		Check.Equal("_gtcp://0f04560181d64d64ad3918bcf8016271/", channelUri, "Register (real client): channel URI in the ObjRef");
		Check.That((call.Args[2] as string)?.StartsWith("{\"Runtime\":\"Kiosk Engine\"") == true, "Register (real client): version JSON");
	}

	private static void RegisterAndExceptionReplies()
	{
		object value = RemotingMessages.ReadReturnValue(GtcpMessage.Decode(Capture("register-reply.s2c.bin")), "Register");
		Check.Equal(null, value, "Register reply (Mono, ArgsInline|ContextInArray|NoReturnValue) is a plain success");
		try
		{
			RemotingMessages.ReadReturnValue(GtcpMessage.Decode(Capture("exception-reply.s2c.bin")), "Register");
			Check.That(false, "exception reply throws");
		}
		catch (BrokerRemoteException e)
		{
			Check.Equal("System.InvalidCastException", e.RemoteType, "exception reply throws BrokerRemoteException with the remote type");
		}
	}

	// ------------------------------------------------------------------ writer

	private static ReservationResult CapturedReservationResult()
	{
		// What the real client answered in run1 (C->S message #4), built the same way ReservationCallbackEntry3 does.
		List<TitleResult> titles = new List<TitleResult>();
		titles.Add(new TitleResult { ItemID = 1, Reservable = true, ReservedBarcode = "H123456789-4711" });
		return new ReservationResult { ReservationSucceeded = true, ErrorMessage = null, TitleResults = titles };
	}

	private static void ReplyBytes()
	{
		Check.Bytes(Content("reserve3-reply.c2s.bin"), NrbfWriter.MethodReturn(CapturedReservationResult(), isVoid: false),
			"ReservationResult return equals the real client's");
		Check.Bytes(Content("cancel-reply.c2s.bin"), NrbfWriter.MethodReturn(new CancelReservationResult { CancellationSucceeded = true }, isVoid: false),
			"CancelReservationResult return equals the real client's");
		Check.Bytes(Content("ping-reply.c2s.bin"), NrbfWriter.MethodReturn(null, isVoid: true), "Ping void return equals the real client's");
		GtcpMessage capturedPingReply = GtcpMessage.Decode(Capture("ping-reply.c2s.bin"));
		Check.Bytes(Capture("ping-reply.c2s.bin"), GtcpMessage.Reply(capturedPingReply.MessageId, capturedPingReply.ReplyToId, NrbfWriter.MethodReturn(null, true)).Encode(),
			"Ping reply message (envelope + content) equals the real client's");
	}

	private sealed class RecordingKiosk : IKioskOperations
	{
		public ReservationCart3 Cart;
		public long CancelledReference;
		public int Pings;

		public ReservationResult Reserve(ReservationCart cart) => null;

		public ReservationResult Reserve2(ReservationCart2 cart) => null;

		public ReservationResult Reserve3(ReservationCart3 cart)
		{
			Cart = cart;
			return CapturedReservationResult();
		}

		public void Ping() => Pings++;

		public CancelReservationResult CancelReservation(long referenceNumber)
		{
			CancelledReference = referenceNumber;
			return new CancelReservationResult { CancellationSucceeded = true };
		}
	}

	private static void DispatchCapturedCalls()
	{
		RecordingKiosk kiosk = new RecordingKiosk();
		byte[] reply = GtcpBrokerClient.Dispatch(kiosk, NrbfReader.ReadMethodCall(Content("reserve3-call.s2c.bin")));
		Check.Equal("a@b.test", kiosk.Cart?.Email, "dispatch: Reserve3 reached the kiosk with the cart");
		Check.Bytes(Content("reserve3-reply.c2s.bin"), reply, "dispatch: Reserve3 reply");
		reply = GtcpBrokerClient.Dispatch(kiosk, NrbfReader.ReadMethodCall(Content("cancel-call.s2c.bin")));
		Check.Equal(123456789L, kiosk.CancelledReference, "dispatch: CancelReservation reached the kiosk");
		Check.Bytes(Content("cancel-reply.c2s.bin"), reply, "dispatch: CancelReservation reply");
		reply = GtcpBrokerClient.Dispatch(kiosk, NrbfReader.ReadMethodCall(Content("ping-call.s2c.bin")));
		Check.Equal(1, kiosk.Pings, "dispatch: Ping reached the kiosk");
		Check.Bytes(Content("ping-reply.c2s.bin"), reply, "dispatch: Ping reply");
	}

	private static void WrittenRegisterCall()
	{
		NrbfSyntheticObject objRef = RemotingMessages.ObjRef("/x/kiosk_operations.rem", "_gtcp://00112233445566778899aabbccddeeff", typeof(IKioskOperations));
		byte[] content = NrbfWriter.MethodCall("Register", FrameworkTypeNames.AssemblyQualifiedName(typeof(IKioskRegister)), new object[] { 990012, objRef, "{}" });
		NrbfMethodCall call = NrbfReader.ReadMethodCall(content);
		Check.Equal(NrbfMessageFlags.ArgsInArray | NrbfMessageFlags.NoContext, call.Flags, "written Register: ArgsInArray|NoContext (call array wraps args)");
		Check.Equal((object)990012, call.Args[0], "written Register: kioskID");
		NrbfObject written = (NrbfObject)call.Args[1];
		Check.Equal("/x/kiosk_operations.rem", written["uri"] as string, "written Register: ObjRef uri");
		string iface = ((written["typeInfo"] as NrbfObject)?["interfacesImplemented"] as NrbfArray)?.Items[0] as string;
		Check.Equal("Redbox.Services.KioskBrokerServices.KioskShared.ServiceInterfaces.IKioskOperations, Redbox.Services.KioskBrokerServices.KioskShared, Version=0.0.0.56, Culture=neutral, PublicKeyToken=null",
			iface, "written Register: interface name equals the captured 0.0.0.56 name");
		Check.Equal("{}", call.Args[2], "written Register: version");
		Check.Equal(FrameworkTypeNames.AssemblyQualifiedName(typeof(IKioskRegister)), call.TypeName, "written Register: type name");
	}

	private static void CartRoundTrip()
	{
		ReservationCart3 original = NrbfObjectMapper.Map<ReservationCart3>(NrbfReader.ReadMethodCall(Content("reserve3-call.s2c.bin")).Args.Single());
		original.ReserveDate = new DateTime(2026, 11, 1, 9, 30, 0, DateTimeKind.Local);
		original.Titles.Add(new ReservationTitle3 { ItemID = 2, TitleID = 99, ReservationType = ReservationType.Purchase, DiscountType = DiscountType.Loyalty, Price = 12.50m });
		byte[] written = NrbfWriter.MethodReturn(original, isVoid: false);
		ReservationCart3 copy = NrbfObjectMapper.Map<ReservationCart3>(NrbfReader.ReadMethodReturn(written).ReturnValue);
		Check.Equal(original.ToJsonForCompare(), copy.ToJsonForCompare(), "cart written and read back is equal (enums, lists, decimals)");
		Check.Equal(DateTimeKind.Local, copy.ReserveDate.Kind, "cart round trip keeps DateTime kind Local");
		Check.Equal(original.ReserveDate.Ticks, copy.ReserveDate.Ticks, "cart round trip keeps local ticks");
	}

	private static void ExceptionReturn()
	{
		byte[] content = NrbfWriter.ExceptionReturn(new InvalidOperationException("boom"));
		NrbfMethodReturn methodReturn = NrbfReader.ReadMethodReturn(content);
		Check.That(methodReturn.IsException, "exception return: ExceptionInArray");
		Check.Equal("System.InvalidOperationException: boom", (methodReturn.Exception as NrbfObject)?["Message"] as string, "exception return: message");
	}
}

internal static class CartCompare
{
	/// <summary>A flat text form of a cart for equality checks.</summary>
	public static string ToJsonForCompare(this ReservationCart3 cart)
	{
		string titles = string.Join(";", cart.Titles.Select(t => $"{t.ItemID}/{t.TitleID}/{t.ReservationType}/{t.Price}/{t.Discount}/{t.DiscountType}/{t.DiscountedPrice}/{t.NumberOfCredits}/{t.LoyaltyPoints}"));
		return string.Join("|", cart.KioskID, cart.ReferenceNumber, cart.CardID, cart.IsGc, string.Join(",", cart.GcIds), string.Join(",", cart.CcIds),
			cart.Email, cart.ReserveDate.ToString("o"), titles, cart.DiscountedSubTotal, cart.SubTotal, cart.Tax, cart.GrandTotal,
			cart.HiveOnlinePromotionDiscount, cart.NumberOfCreditsRemaining, cart.PromoCode, cart.CustomerNumber, cart.AppliedCredit,
			cart.AppliedHiveOnlinePromotion, cart.AppliedTitleMarketing, string.Join(",", cart.HistoryTitleIDs), cart.TaxRate, cart.Zip,
			cart.IsMnp, cart.IsMdv, cart.LoyaltyRedemption, cart.DefaultServiceFeeAmount, cart.ActualServiceFeeAmount, cart.AuthorizeAtPickup,
			cart.ReservationCardHash?.CardBinHash, cart.ReservationCardHash?.Last4, cart.AlternateCardHashes.Count);
	}
}
