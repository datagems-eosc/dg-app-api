using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.Service.DataManagement.Data
{
    public class Collection
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [InverseProperty(nameof(DatasetCollection.Collection))]
        public List<DatasetCollection> Datasets { get; set; }
    }

    public class CollectionEntityConfiguration : EntityTypeConfigurationBase<Collection>
    {
        public CollectionEntityConfiguration() : base() { }

        public override void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.ToTable("dm_collection");
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Name).HasColumnName("name");
        }
    }
}
