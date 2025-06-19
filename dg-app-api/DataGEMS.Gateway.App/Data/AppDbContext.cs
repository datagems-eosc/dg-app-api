using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<UserCollection> UserCollections { get; set; }
		public DbSet<UserDatasetCollection> UserDatasetCollections { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			new UserEntityConfiguration().Configure(modelBuilder.Entity<User>());
			new UserCollectionEntityConfiguration().Configure(modelBuilder.Entity<UserCollection>());
			new UserDatasetCollectionEntityConfiguration().Configure(modelBuilder.Entity<UserDatasetCollection>());
		}
	}
}
