using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataSimpleExploreResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataSimpleExploreResponse; } }
		public List<DataGEMS.Gateway.App.Model.InDataSimpleExploreExploration> Payload { get; set; }
	}
}
