using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbox.Core;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

public class ControlListConverter : JsonConverter<ControlList>
{
	public override ControlList ReadJson(JsonReader reader, Type objectType, ControlList existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		try
		{
			ControlList controlList = existingValue;
			if (reader.TokenType == JsonToken.StartArray)
			{
				JArray jArray = JArray.Load(reader);
				if (jArray != null)
				{
					controlList = new ControlList();
					foreach (JToken item in jArray)
					{
						switch (item.GetCaseInsensitveKeyValue("ControlType").ToObject<ControlType>())
						{
						case ControlType.StartAsset:
							controlList.Add(new StartAssetControl
							{
								ControlId = item.GetCaseInsensitveKeyValue("ControlId").ToObject<long>(),
								DisplayDuration = item.GetCaseInsensitveKeyValue("DisplayDuration").ToObject<int>(),
								Asset = item.GetCaseInsensitveKeyValue("Asset").ToObject<Asset>(),
								Target = item.GetCaseInsensitveKeyValue("Target").ToObject<AssetTarget>(),
								TargetValue = item.GetCaseInsensitveKeyValue("TargetValue")?.ToObject<string>(),
								Order = item.GetCaseInsensitveKeyValue("Order").ToObject<short>(),
								IncludeIfNoInventory = item.GetCaseInsensitveKeyValue("IncludeIfNoInventory")?.ToObject<bool?>(),
								Validation = item.GetCaseInsensitveKeyValue("Validation")?.ToObject<StartScreenPromotionCodeValidationResponse>()
							});
							break;
						case ControlType.Carousel:
							controlList.Add(new CarouselControl
							{
								ControlId = item.GetCaseInsensitveKeyValue("ControlId").ToObject<long>(),
								DisplayDuration = item.GetCaseInsensitveKeyValue("DisplayDuration").ToObject<int>(),
								MaxTitles = item.GetCaseInsensitveKeyValue("MaxTitles").ToObject<int>()
							});
							break;
						}
					}
				}
			}
			return controlList;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KCS.Campaign.ControlListConverter", e);
			return existingValue;
		}
	}

	public override void WriteJson(JsonWriter writer, ControlList value, JsonSerializer serializer)
	{
		writer.WriteStartArray();
		value.ForEach(delegate(IControl item)
		{
			serializer.Serialize(writer, item);
		});
		writer.WriteEndArray();
	}
}
