
namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowConfig
	{
		public String BaseUrl { get; set; }
		public String TokenEndpoint { get; set; }
		public String DagListEndpoint { get; set; }
		public String DagByIdEndpoint { get; set; }
		public String DagRunEndpoint {  get; set; }
		public String DagExecutionListEndpoint {  get; set; }
		public String DagExecutionByIdEndpoint { get; set; }
		public String TaskInstancesLogsEndpoint { get; set; }
		public String XcomEntriesEndpoint { get; set; }
		public String TaskListEndpoint { get; set; }
		public String TaskByIdEndpoint { get; set; }
		public String TaskInstanceListEndpoint { get; set; }
		public String TaskInstanceByIdEndpoint { get; set; }
		public String Username { get; set; }
		public String Password { get; set; }

	}
}
