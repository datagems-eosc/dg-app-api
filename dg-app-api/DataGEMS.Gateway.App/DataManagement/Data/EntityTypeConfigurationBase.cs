using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.DataManagement.Data
{
	public abstract class EntityTypeConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
	{
		public EntityTypeConfigurationBase() { }

		public abstract void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> builder);
	}
}
