using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.InDatasetDiscovery.Model;

namespace DataGEMS.Gateway.App.Service.InDatasetDiscovery
{
	public interface IInDatasetDiscoveryService
	{
		public Task<LanguagePilotResponse> LinguisticFeaturesAsync(LinguisticFeaturesRequest request);
	}
}
