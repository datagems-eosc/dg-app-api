using DataGEMS.Gateway.App.Common.Data;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Data
{
	public class VersionInfo
	{
		[Key]
		[Required]
		[MaxLength(20)]
		public String Key { get; set; }

		[Required]
		[MaxLength(50)]
		public String Version { get; set; }

		[Required]
		public DateTime ReleasedAt { get; set; }

		[Required]
		public DateTime DeployedAt { get; set; }

		public String Description { get; set; }
	}

	public class VersionInfoEntityConfiguration : EntityTypeConfigurationBase<VersionInfo>
	{
		public VersionInfoEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<VersionInfo> builder)
		{
			builder.ToTable("version_info");
			builder.Property(x => x.Key).HasColumnName("key");
			builder.Property(x => x.Version).HasColumnName("version");
			builder.Property(x => x.ReleasedAt).HasColumnName("released_at");
			builder.Property(x => x.DeployedAt).HasColumnName("deployed_at");
			builder.Property(x => x.Description).HasColumnName("description");
		}
	}
}
