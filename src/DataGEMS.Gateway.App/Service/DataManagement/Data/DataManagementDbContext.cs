using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Service.DataManagement.Data
{
    public class DataManagementDbContext : DbContext
    {
        public DataManagementDbContext(DbContextOptions<DataManagementDbContext> options) : base(options) { }

        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<DatasetCollection> DatasetCollections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new DatasetEntityConfiguration().Configure(modelBuilder.Entity<Dataset>());
            new CollectionEntityConfiguration().Configure(modelBuilder.Entity<Collection>());
            new DatasetCollectionEntityConfiguration().Configure(modelBuilder.Entity<DatasetCollection>());
        }
    }
}
