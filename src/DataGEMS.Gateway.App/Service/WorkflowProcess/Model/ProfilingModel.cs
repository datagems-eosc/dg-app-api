using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess.Model
{
	public class ProfilingModel
	{
		public Guid DatasetId { get; set; }
		public DataStoreKind Kind { get; set; }
		public string DatabaseName { get; set; }
	}
}
