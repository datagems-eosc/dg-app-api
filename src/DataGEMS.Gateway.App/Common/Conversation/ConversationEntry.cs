
using Cite.Tools.Json.Inflater;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	[JsonConverter(typeof(SubTypeConverter))]
	[SubTypeConverterAnchor(nameof(ConversationEntry.Kind), typeof(ConversationMessageKind))]
	[SubTypeConverterMap(ConversationMessageKind.CrossDatasetQuery, typeof(CrossDatasetQueryConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.CrossDatasetResponse, typeof(CrossDatasetResponseConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataExploreQuery, typeof(InDataExploreQueryConversationEntry))]
	[SubTypeConverterMap(ConversationMessageKind.InDataExploreResponse, typeof(InDataSimpleExploreResponseConversationEntry))]
	public abstract class ConversationEntry
	{
		public abstract ConversationMessageKind Kind { get; }
		public String Version { get; set; }
	}
}
