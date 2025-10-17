using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.Service.DataManagement.Data
{
    public class DatasetCollection
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid DatasetId { get; set; }

        [Required]
        public Guid CollectionId { get; set; }

        [ForeignKey(nameof(DatasetId))]
        public Dataset Dataset { get; set; }

        [ForeignKey(nameof(CollectionId))]
        public Collection Collection { get; set; }
    }

    public class DatasetCollectionEntityConfiguration : EntityTypeConfigurationBase<DatasetCollection>
    {
        public DatasetCollectionEntityConfiguration() : base() { }

        public override void Configure(EntityTypeBuilder<DatasetCollection> builder)
        {
            builder.ToTable("dm_dataset_collection");
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.DatasetId).HasColumnName("dataset_id");
            builder.Property(x => x.CollectionId).HasColumnName("collection_id");
        }
    }
}
