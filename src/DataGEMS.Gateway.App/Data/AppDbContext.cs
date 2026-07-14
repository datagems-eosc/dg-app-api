using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<AdHocQueryResult> AdHocQueryResults { get; set; }
		public DbSet<Collection> Collections { get; set; }
		public DbSet<Conversation> Conversations { get; set; }
		public DbSet<ConversationDataset> ConversationDatasets { get; set; }
		public DbSet<ConversationMessage> ConversationMessages { get; set; }
		public DbSet<DatasetCollection> DatasetCollections { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserFavorite> UserFavorites { get; set; }
		public DbSet<UserSettings> UserSettings { get; set; }
		public DbSet<VersionInfo> VersionInfos { get; set; }
		public DbSet<WorkflowProcess> WorkflowProcesses { get; set; }
		public DbSet<WorkflowProcessStep> WorkflowProcessSteps { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			new AdHocQueryResultEntityConfiguration().Configure(modelBuilder.Entity<AdHocQueryResult>());
			new CollectionEntityConfiguration().Configure(modelBuilder.Entity<Collection>());
			new ConversationEntityConfiguration().Configure(modelBuilder.Entity<Conversation>());
			new ConversationDatasetEntityConfiguration().Configure(modelBuilder.Entity<ConversationDataset>());
			new ConversationMessageEntityConfiguration().Configure(modelBuilder.Entity<ConversationMessage>());
			new DatasetCollectionEntityConfiguration().Configure(modelBuilder.Entity<DatasetCollection>());
			new UserEntityConfiguration().Configure(modelBuilder.Entity<User>());
			new UserFavoriteEntityConfiguration().Configure(modelBuilder.Entity<UserFavorite>());
			new UserSettingsEntityConfiguration().Configure(modelBuilder.Entity<UserSettings>());
			new VersionInfoEntityConfiguration().Configure(modelBuilder.Entity<VersionInfo>());
			new WorkflowProcessEntityConfiguration().Configure(modelBuilder.Entity<WorkflowProcess>());
			new WorkflowProcessStepEntityConfiguration().Configure(modelBuilder.Entity<WorkflowProcessStep>());
		}
	}
}
