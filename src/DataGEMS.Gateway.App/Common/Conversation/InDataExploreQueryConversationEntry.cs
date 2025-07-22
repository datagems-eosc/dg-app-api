using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataExploreQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataExploreQuery; } }
		public Service.InDataExploration.ExploreInfo Payload { get; set; }
	}
}
