using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Data
{
    public class UserCollection
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(250)]
		public String Name { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public UserCollectionKind Kind { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[InverseProperty(nameof(UserDatasetCollection.UserCollection))]
		public List<UserDatasetCollection> UserDatasets { get; set; }

		[ForeignKey(nameof(UserCollection.UserId))]
		public User User { get; set; }
	}

	public class UserCollectionEntityConfiguration : EntityTypeConfigurationBase<UserCollection>
	{
		public UserCollectionEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<UserCollection> builder)
		{
			builder.ToTable("user_collection");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Name).HasColumnName("name");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.Kind).HasColumnName("kind");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
