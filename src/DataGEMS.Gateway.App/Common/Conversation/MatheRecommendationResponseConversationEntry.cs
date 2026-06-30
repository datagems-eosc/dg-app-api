namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class MatheRecommendationResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind
		{
			get { return ConversationMessageKind.MatheRecommendationResponse; }
		}

		public App.Service.DatasetRecommender.Model.MatheRecommendationResponse Payload { get; set; }
	}
}
