using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataGEMS.Gateway.App.Service.DataManagement.Data
{
    public class Dataset
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

        public string Description { get; set; }

        public string License { get; set; }

        [MaxLength(300)]
        public string Url { get; set; }

        [MaxLength(50)]
        public string Version { get; set; }

        [MaxLength(250)]
        public string MimeType { get; set; }

        public long? Size { get; set; }

        public string Headline { get; set; }

        public string Keywords { get; set; }

        public string FieldOfScience { get; set; }

        public string Language { get; set; }

        public string Country { get; set; }

        [NotMapped]
        public DateOnly? DatePublished
        {
            get { return DatePublishedRaw.HasValue ? DateOnly.FromDateTime(DatePublishedRaw.Value) : null; }
            set { DatePublishedRaw = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : null; }
        }
        public DateTime? DatePublishedRaw { get; set; }

        public string Profile { get; set; }

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
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.License).HasColumnName("license");
            builder.Property(x => x.MimeType).HasColumnName("mime_type");
            builder.Property(x => x.Size).HasColumnName("size");
            builder.Property(x => x.Url).HasColumnName("url");
            builder.Property(x => x.Version).HasColumnName("version");
            builder.Property(x => x.Headline).HasColumnName("headline");
            builder.Property(x => x.Keywords).HasColumnName("keywords");
            builder.Property(x => x.FieldOfScience).HasColumnName("field_of_science");
            builder.Property(x => x.Language).HasColumnName("language");
            builder.Property(x => x.Country).HasColumnName("country");
            builder.Property(x => x.DatePublishedRaw).HasColumnName("date_published");
            builder.Property(x => x.Profile).HasColumnName("profile");
        }
    }
}
