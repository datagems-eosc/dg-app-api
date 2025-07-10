using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
	public class ConversationMessage
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid ConversationId { get; set; }

		[Required]
		public ConversationMessageKind Kind { get; set; }

		public String Data { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[ForeignKey(nameof(ConversationMessage.ConversationId))]
		public Conversation Conversation { get; set; }
	}

	public class ConversationMessageEntityConfiguration : EntityTypeConfigurationBase<ConversationMessage>
	{
		public ConversationMessageEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<ConversationMessage> builder)
		{
			builder.ToTable("conversation_message");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.ConversationId).HasColumnName("conversation_id");
			builder.Property(x => x.Kind).HasColumnName("kind");
			builder.Property(x => x.Data).HasColumnName("data");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
		}
	}
}
