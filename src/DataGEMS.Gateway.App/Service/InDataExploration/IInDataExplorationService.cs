using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public interface IInDataExplorationService
	{
		Task<App.Model.InDataExplore> ExploreAsync(Service.InDataExploration.ExploreInfo request, IFieldSet fieldSet);
	}

	public class ExploreInfo
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Question { get; set; }
		public List<Guid> DatasetIds { get; set; }
	}
}
