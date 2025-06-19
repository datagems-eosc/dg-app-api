using DataGEMS.Gateway.App.Common.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
    public class User
	{
		[Key]
		[Required]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(250)]
		public String IdpSubjectId { get; set; }

		[Required]
		[MaxLength(250)]
		public String Name { get; set; }

		[Required]
		[MaxLength(250)]
		public String Email { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime UpdatedAt { get; set; }

		[InverseProperty(nameof(UserCollection.User))]
		public List<UserCollection> UserCollections { get; set; }
	}

	public class UserEntityConfiguration : EntityTypeConfigurationBase<User>
	{
		public UserEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<User> builder)
		{
			builder.ToTable("user");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.IdpSubjectId).HasColumnName("idp_subject_id");
			builder.Property(x => x.Name).HasColumnName("name");
			builder.Property(x => x.Email).HasColumnName("email");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
