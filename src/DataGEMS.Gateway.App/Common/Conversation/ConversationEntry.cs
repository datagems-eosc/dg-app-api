
using Cite.Tools.Json.Inflater;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	[JsonConverter(typeof(SubTypeConverter))]
	[SubTypeConverterAnchor(nameof(ConversationEntry.Kind), typeof(ConversationMessageKind))]
	[SubTypeConverterMap(ConversationMessageKind.CrossDatasetQuery, typeof(CrossDatasetQueryConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.CrossDatasetResponse, typeof(CrossDatasetResponseConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataGeoQuery, typeof(InDataGeoQueryConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataGeoResponse, typeof(InDataGeoResponseConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataTextToSqlQuery, typeof(InDataTextToSqlQueryConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataTextToSqlResponse, typeof(InDataTextToSqlResponseConversationEntry))]
	public abstract class ConversationEntry
	{
		public abstract ConversationMessageKind Kind { get; }
		public String Version { get; set; }
	}
}
