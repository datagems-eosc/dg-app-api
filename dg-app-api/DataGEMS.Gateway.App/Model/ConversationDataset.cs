using DataGEMS.Gateway.App.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class ConversationDataset
	{
		public Guid? Id { get; set; }
		public Dataset Dataset { get; set; }
		public Conversation Conversation { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}
}
