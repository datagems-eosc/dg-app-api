namespace DataGEMS.Gateway.Api.Model
{
	public class SearchResult<M>
	{
		public SearchResult() { }
		public SearchResult(Guid? conversationId, M items)
		{
			this.Result = items;
			this.ConversationId = conversationId;
		}

		public M Result { get; set; }
		public Guid? ConversationId { get; set; }
	}
}
