using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.Data
{
	public class AdHocQueryResult
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[Required]
		public string AnalyticalPattern { get; set; }

		[Required]
		public string ResultFilePath { get; set; }

		[Required]
		public IsActive IsActive { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[ForeignKey(nameof(AdHocQueryResult.UserId))]
		public User User { get; set; }
	}

	public class AdHocQueryResultEntityConfiguration : EntityTypeConfigurationBase<AdHocQueryResult>
	{
		public AdHocQueryResultEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<AdHocQueryResult> builder)
		{
			builder.ToTable("ad_hoc_query_result");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.AnalyticalPattern).HasColumnName("analytical_pattern");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.IsActive).HasColumnName("is_active");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
