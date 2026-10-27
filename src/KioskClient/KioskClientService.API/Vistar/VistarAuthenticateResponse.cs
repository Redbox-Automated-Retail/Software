using System.Collections.Generic;

namespace KioskClientService.API.Vistar;

public class VistarAuthenticateResponse
{
	public string username { get; set; }

	public bool is_buyer { get; set; }

	public Dictionary<string, List<string>> abilities { get; set; }

	public string public_id { get; set; }

	public string name { get; set; }

	public string locale { get; set; }

	public string country { get; set; }

	public decimal client_fee_bp { get; set; }

	public string id { get; set; }

	public string currency { get; set; }

	public bool is_admin { get; set; }

	public bool is_seller { get; set; }

	public string homepage { get; set; }

	public string email { get; set; }

	public bool screen_count_enabled { get; set; }

	public string account_email { get; set; }
}
