using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class LinguisticFeaturesResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind
		{
			get { return ConversationMessageKind.LinguisticFeaturesResponse; }
		}

		public LanguagePilotResponse Payload { get; set; }
	}
}
