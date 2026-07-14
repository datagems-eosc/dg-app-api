using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public class WorkflowProcessConfig
	{
		public List<WorkflowProcessConfigItem> Items { get; set; }

		public class WorkflowProcessConfigItem
		{
			public Guid Id { get; set; }
			public WorkflowDefinitionKind Kind { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public List<WorkflowProcessConfigItemStep> Steps { get; set; }

			public class WorkflowProcessConfigItemStep
			{
				public string Id { get; set; }
				public int Order { get; set; }
				public string TaskId { get; set; }
			}
		}
	}
}
