using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Data
{
    public class UserDatasetCollection
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid UserCollectionId { get; set; }

		[Required]
		public Guid DatasetId { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[ForeignKey(nameof(UserDatasetCollection.UserCollectionId))]
		public UserCollection UserCollection { get; set; }
	}

	public class UserDatasetCollectionEntityConfiguration : EntityTypeConfigurationBase<UserDatasetCollection>
	{
		public UserDatasetCollectionEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<UserDatasetCollection> builder)
		{
			builder.ToTable("user_dataset_collection");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.UserCollectionId).HasColumnName("user_collection_id");
			builder.Property(x => x.DatasetId).HasColumnName("dataset_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
