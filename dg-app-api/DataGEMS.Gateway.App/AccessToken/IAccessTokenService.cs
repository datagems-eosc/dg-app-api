using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.AccessToken
{
	public interface IAccessTokenService
	{
		Task<String> GetClientAccessTokenAsync(String scope);
		Task<String> GetExchangeAccessTokenAsync(String requestAccessToken, String scope);
	}
}
