using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<UserCollection> UserCollections { get; set; }
		public DbSet<UserDatasetCollection> UserDatasetCollections { get; set; }
		public DbSet<VersionInfo> VersionInfos { get; set; }
		public DbSet<Conversation> Conversations { get; set; }
		public DbSet<ConversationDataset> ConversationDatasets { get; set; }
		public DbSet<ConversationMessage> ConversationMessages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			new UserEntityConfiguration().Configure(modelBuilder.Entity<User>());
			new UserCollectionEntityConfiguration().Configure(modelBuilder.Entity<UserCollection>());
			new UserDatasetCollectionEntityConfiguration().Configure(modelBuilder.Entity<UserDatasetCollection>());
			new VersionInfoEntityConfiguration().Configure(modelBuilder.Entity<VersionInfo>());

			new ConversationEntityConfiguration().Configure(modelBuilder.Entity<Conversation>());
			new ConversationDatasetEntityConfiguration().Configure(modelBuilder.Entity<ConversationDataset>());
			new ConversationMessageEntityConfiguration().Configure(modelBuilder.Entity<ConversationMessage>());
		}
	}
}
