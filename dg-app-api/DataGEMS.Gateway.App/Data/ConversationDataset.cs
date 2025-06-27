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
	public class ConversationDataset
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid ConversationId { get; set; }

		[Required]
		public Guid DatasetId { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[ForeignKey(nameof(ConversationDataset.ConversationId))]
		public Conversation Conversation { get; set; }
	}

	public class ConversationDatasetEntityConfiguration : EntityTypeConfigurationBase<ConversationDataset>
	{
		public ConversationDatasetEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<ConversationDataset> builder)
		{
			builder.ToTable("conversation_dataset");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.ConversationId).HasColumnName("conversation_id");
			builder.Property(x => x.DatasetId).HasColumnName("dataset_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
