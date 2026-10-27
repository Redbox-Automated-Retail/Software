using System.Text;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal abstract class AWSBaseMessageProcessorService<S, D> : BaseMessageProcessorService where S : class where D : class, new()
{
	private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
	{
		DateFormatHandling = DateFormatHandling.IsoDateFormat
	};

	public override BeforeMessageProcessingDelegate BeforeMessageProcessing => OnBeforeMessageProcessing;

	public override AfterMessageProcessedDelegate AfterMessageProcessed => OnAfterMessageProcessed;

	protected abstract void CopyFields(D destination, S source);

	private void OnBeforeMessageProcessing(IMessage message, out object clientData)
	{
		clientData = message.Data;
		S source = JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize<S>(Encoding.ASCII.GetString(message.Data));
		D val = new D();
		CopyFields(val, source);
		message.Data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(val, serializerSettings));
	}

	private static void OnAfterMessageProcessed(IMessage message, object clientData)
	{
		message.Data = clientData as byte[];
	}
}
