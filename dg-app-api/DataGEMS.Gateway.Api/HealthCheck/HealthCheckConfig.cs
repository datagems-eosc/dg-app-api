namespace DataGEMS.Gateway.Api.HealthCheck
{
	public class HealthCheckConfig
	{
		public FolderConfig Folder { get; set; }
		public MemoryConfig Memory { get; set; }
		public EndpointConfig Endpoint { get; set; }

		public class FolderConfig
		{
			public List<String> Paths { get; set; }
		}

		public class MemoryConfig
		{
			public long? MaxPrivateMemoryBytes { get; set; }
			public long? MaxProcessMemoryBytes { get; set; }
			public long? MaxVirtualMemoryBytes { get; set; }
		}

		public class EndpointConfig
		{
			public bool IsEnabled { get; set; }
			public string EndpointPath { get; set; }
			public string[] RequireHosts { get; set; }
			public string IncludeTag { get; set; }
			public bool AllowCaching { get; set; }
			public int HealthyStatusCode { get; set; }
			public int DegradedStatusCode { get; set; }
			public int UnhealthyStatusCode { get; set; }
			public bool VerboseResponse { get; set; }
		}
	}
}
