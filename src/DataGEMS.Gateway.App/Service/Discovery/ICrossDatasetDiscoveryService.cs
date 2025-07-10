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
		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Query { get; set; }
		public int? ResultCount { get; set; }
	}
}
