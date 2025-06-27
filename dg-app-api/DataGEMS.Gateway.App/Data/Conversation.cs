using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
	public class Conversation
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(300)]
		public String Name { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[InverseProperty(nameof(ConversationDataset.Conversation))]
		public List<ConversationDataset> ConversationDatasets { get; set; }

		[InverseProperty(nameof(ConversationMessage.Conversation))]
		public List<ConversationMessage> ConversationMessages { get; set; }

		[ForeignKey(nameof(Conversation.UserId))]
		public User User { get; set; }
	}

	public class ConversationEntityConfiguration : EntityTypeConfigurationBase<Conversation>
	{
		public ConversationEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<Conversation> builder)
		{
			builder.ToTable("conversation");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Name).HasColumnName("name");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
