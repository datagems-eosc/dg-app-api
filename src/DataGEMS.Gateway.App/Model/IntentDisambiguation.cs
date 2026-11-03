using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class IntentDisambiguation 
	{
		public static string ModelVersion = "V1";
		public IntentDisambiguationIntentType Intent { get; set; }
		public decimal Confidence { get; set; }
		public string SearchQuery { get; set; }
		public bool Degraded { get; set; }
		public string Model { get; set; }
		public string PromptVersion { get; set; }
	}
}
