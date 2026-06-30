using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class LinguisticFeaturesQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind
		{
			get { return ConversationMessageKind.LinguisticFeaturesQuery; }
		}

		public LanguagePilotRequest Payload { get; set; }
	}
}
