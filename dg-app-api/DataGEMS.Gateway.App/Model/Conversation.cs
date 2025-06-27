using DataGEMS.Gateway.App.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class Conversation
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public User User { get; set; }
		public List<ConversationDataset> ConversationDatasets { get; set; }
		public List<ConversationMessage> ConversationMessages { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}
}
