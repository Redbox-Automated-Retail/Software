using KioskClientService.API.App;

namespace KioskClientService.API.Models;

public static class KioskModels
{
	public class KioskAuthenticateRequest : IMessageScrub
	{
		public string Username { get; set; }

		public string Password { get; set; }

		public long KioskId { get; set; }

		public bool UseNtAuthentication { get; set; }

		public object Scrub()
		{
			return new
			{
				Username = ObfuscationHelper.ObfuscateStringData(Username, 1, 1),
				KioskId = KioskId,
				UseNtAuthentication = UseNtAuthentication
			}.ToJsonIndented();
		}
	}

	public class KioskAuthenticateResponse : BaseResponse
	{
		public KioskAuthenticateResponse()
			: base(200)
		{
		}

		public override object Scrub()
		{
			return this.ToJson();
		}
	}
}
