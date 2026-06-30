namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class MatheRecommendationQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind
		{
			get { return ConversationMessageKind.MatheRecommendationQuery; }
		}

		public App.Service.DatasetRecommender.Model.MatheRecommendationRequest Payload { get; set; }
	}
}
