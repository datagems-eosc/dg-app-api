using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Data
{
    public class UserSettings
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(250)]
		public String Key { get; set; }

		[Required]
		public Guid UserId { get; set; }

		public String Value { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[ForeignKey(nameof(UserSettings.UserId))]
		public User User { get; set; }
	}

	public class UserSettingsEntityConfiguration : EntityTypeConfigurationBase<UserSettings>
	{
		public UserSettingsEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<UserSettings> builder)
		{
			builder.ToTable("user_settings");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Key).HasColumnName("key");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.Value).HasColumnName("value");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
