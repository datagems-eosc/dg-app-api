using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataSimpleExploreQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataSimpleExploreQuery; } }
		public Service.InDataExploration.ExploreSimpleExploreInfo Payload { get; set; }
	}
}
