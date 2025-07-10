using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class ConversationMessage
	{
		public Guid? Id { get; set; }
		public Conversation Conversation { get; set; }
		public ConversationMessageKind? Kind { get; set; }
		public String Data { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
