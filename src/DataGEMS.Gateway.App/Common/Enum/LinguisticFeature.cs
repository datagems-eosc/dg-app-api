using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common.Enum
{
	public enum LinguisticFeature: short
	{
		[Description("Term Frequency")]
		TermFrequency = 0,
		[Description("Sentiment Profile")]
		SentimentProfile = 1,
		[Description("Collocations")]
		Collocations = 2
	}
}
