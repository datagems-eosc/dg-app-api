using DataGEMS.Gateway.App.Common.Enum;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DataManagement.Model
{
	public class DatasetQueryList
	{
		[JsonProperty("code")]
		public int Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("datasets")]
		public List<Dataset> Datasets { get; set; }

		public class Dataset
		{
			[JsonProperty("nodes")]
			public List<Node> Nodes { get; set; }

			public class Node
			{

				[JsonProperty("id")]
				public string Id { get; set; }

				[JsonProperty("properties")]
				public NodeProperties Properties { get; set; }

				public class NodeProperties
				{
					[JsonProperty("id")]
					public string Id { get; set; }
					[JsonProperty("dg:headline")]
					public string Headline { get; set; }
					[JsonProperty("dg:fieldOfScience")]
					public List<string> FieldsOfScience { get; set; }
					[JsonProperty("name")]
					public string Name { get; set; }
					[JsonProperty("conformsTo")]
					public string ConformsTo { get; set; }
					[JsonProperty("url")]
					public string Url { get; set; }
					[JsonProperty("datePublished")]
					public DateTime? DatePublished { get; set; }
					[JsonProperty("license")]
					public string License { get; set; }
					[JsonProperty("dg:keywords")]
					public List<string> Keywords { get; set; }
					[JsonProperty("description")]
					public string Description { get; set; }
					[JsonProperty("inLanguage")]
					public List<string> Languages { get; set; }
					[JsonProperty("version")]
					public string Version { get; set; }
					[JsonProperty("sc:archivedAt")]
					public string ArchivedAt { get; set; }
					[JsonProperty("citeAs")]
					public string CiteAs { get; set; }
					[JsonProperty("country")]
					public string Country { get; set; }

					[JsonProperty("dg:status")]
					public string Status { get; set; }
					//TODO: [JsonProperty("access")]
					//TODO: public string Access { get; set; }
					//TODO: [JsonProperty("uploadedBy")]
					//TODO: public string UploadedBy { get; set; }
					//TODO: [JsonProperty("distribution")]
					//TODO: public string Distribution { get; set; }
					//TODO: [JsonProperty("recordSet")]
					//TODO: public string RecordSet { get; set; }
				}
			}
		}
	}
}
