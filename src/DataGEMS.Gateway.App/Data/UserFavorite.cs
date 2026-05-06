using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.Data
{
	public class UserFavorite
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[Required]
		public Guid DatasetId { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[ForeignKey(nameof(UserFavorite.UserId))]
		public User User { get; set; }
	}

	public class UserFavoriteEntityConfiguration : EntityTypeConfigurationBase<UserFavorite>
	{
		public UserFavoriteEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<UserFavorite> builder)
		{
			builder.ToTable("user_favorite");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.DatasetId).HasColumnName("dataset_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
