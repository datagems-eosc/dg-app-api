using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.DataManagement.Data
{
    public class Dataset
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(50)]
		public String Code { get; set; }

		[Required]
		[MaxLength(250)]
		public String Name { get; set; }

		[InverseProperty(nameof(DatasetCollection.Dataset))]
		public List<DatasetCollection> Collections { get; set; }
	}

	public class DatasetEntityConfiguration : EntityTypeConfigurationBase<Dataset>
	{
		public DatasetEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<Dataset> builder)
		{
			builder.ToTable("dm_dataset");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Code).HasColumnName("code");
			builder.Property(x => x.Name).HasColumnName("name");
		}
	}
}
