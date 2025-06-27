using DataGEMS.Gateway.App.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class ConversationMessage
	{
		public Guid? Id { get; set; }
		public Conversation Conversation { get; set; }
		public Kind? Kind { get; set; }
		public String Data { get; set; }
		public DateTime? CreatedAt { get; set; }
		public String ETag { get; set; }
	}
}
