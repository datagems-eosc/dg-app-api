using DataGEMS.Gateway.App.Common.Data;
using DataGEMS.Gateway.App.Common.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace DataGEMS.Gateway.App.Data
{
	public class WorkflowProcess
	{
		[Key]
		[Required]
		public Guid Id { get; set; }
		[Required]
		public Guid ProcessId { get; set; }
		public string WorkflowRunDetails { get; set; }
		public Guid? UserId { get; set; }
		[Required]
		public WorkflowProcessStatus Status { get; set; }
		[Required]
		public DateTime CreatedAt { get; set; }
		[Required]
		public DateTime UpdatedAt { get; set; }
	}

	public class WorkflowProcessEntityConfiguration : EntityTypeConfigurationBase<WorkflowProcess>
	{
		public WorkflowProcessEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<WorkflowProcess> builder)
		{
			builder.ToTable("workflow_process");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.ProcessId).HasColumnName("process_id");
			builder.Property(x => x.WorkflowRunDetails).HasColumnName("workflow_run_details");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.Status).HasColumnName("status");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
