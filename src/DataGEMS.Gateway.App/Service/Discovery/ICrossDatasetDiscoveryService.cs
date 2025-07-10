using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.Discovery
{
	public interface ICrossDatasetDiscoveryService
	{
		Task<List<CrossDatasetDiscovery>> DiscoverAsync(DiscoverInfo request, IFieldSet fieldSet);
	}

	public class DiscoverInfo
	{
		public String Query { get; set; }
		public int? ResultCount { get; set; }
	}
}
