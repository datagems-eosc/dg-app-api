using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.AccessToken
{
	public class ClientAccessToken
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }
	}
}
