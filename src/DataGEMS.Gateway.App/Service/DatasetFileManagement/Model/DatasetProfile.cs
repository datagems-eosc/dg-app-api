using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement.Model
{
	public class Profile
	{
		public List<AnalyticalPatternEdge> Edges { get; set; }
		public List<AnalyticalPatternNode> Nodes { get; set; }
	}
}
