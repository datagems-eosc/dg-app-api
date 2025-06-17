
namespace DataGEMS.Gateway.App.AccessToken
{
	public interface IAccessTokenService
	{
		Task<String> GetClientAccessTokenAsync(String scope);
		Task<String> GetExchangeAccessTokenAsync(String requestAccessToken, String scope);
	}
}
