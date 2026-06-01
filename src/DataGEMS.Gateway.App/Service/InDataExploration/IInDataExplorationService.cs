using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Common.Enum;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.InDataExploration.Model;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public interface IInDataExplorationService
	{
		Task<App.Model.InDataExplore> ExploreAsync(Service.InDataExploration.ExploreInfo request, IFieldSet fieldSet);
		Task<LanguagePilotResponse> LinguisticFeaturesAsync(LinguisticFeaturesRequest request);
		List<String> MapLinguisticFeatureFlag(List<LinguisticFeature> features);
	}

	public class ExploreInfo
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Question { get; set; }
		public List<Guid> DatasetIds { get; set; }
	}
}
