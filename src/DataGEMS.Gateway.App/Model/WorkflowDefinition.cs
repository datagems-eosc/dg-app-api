
namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowDefinition
	{
		public String Id { get; set; }
		public String Name { get; set; }
		public Boolean? IsPaused { get; set; }
		public Boolean? IsStale { get; set; }
		public DateTime? LastParsedTime { get; set; }
		public DateTime? LastExpired { get; set; }
		public String BundleName { get; set; }
		public String BundleVersion { get; set; }
		public String RelativeFileLocation { get; set; }
		public String FileLocation { get; set; }
		public String FileToken { get; set; }
		public String Description { get; set; }
		public String TimetableSummary { get; set; }
		public String TimetableDescription { get; set; }
		public List<String> Tags { get; set; }
		public int? MaxActiveTasks { get; set; }
		public int? MaxActiveRuns { get; set; }
		public int? MaxConsecutiveFailedRuns { get; set; }
		public Boolean? HasTaskConcurrencyLimits { get; set; }
		public Boolean? HasImportErrors { get; set; }
		public DateTime? NextLogicalDate { get; set; }
		public DateTime? NextDataIntervalStart { get; set; }
		public DateTime? NextDataIntervalEnd { get; set; }
		public DateTime? NextRunAfter { get; set; }
		public List<String> Owners { get; set; }
	}
}
