using DataGEMS.Gateway.App.Common.Data;
using DataGEMS.Gateway.App.Common.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace DataGEMS.Gateway.App.Data
{
	public class WorkflowProcessStep
	{
		[Key]
		[Required]
		public Guid Id { get; set; }
		[Required]
		public Guid ProcessId { get; set; }
		[Required]
		public Guid StepId { get; set; }
		public string WorkflowTaskInstanceDetails { get; set; }
		public Guid? UserId { get; set; }
		[Required]
		public WorkflowProcessStatus Status { get; set; }
		[Required]
		public DateTime CreatedAt { get; set; }
		[Required]
		public DateTime UpdatedAt { get; set; }
	}

	public class WorkflowProcessStepEntityConfiguration : EntityTypeConfigurationBase<WorkflowProcessStep>
	{
		public WorkflowProcessStepEntityConfiguration() : base() { }

		public override void Configure(EntityTypeBuilder<WorkflowProcessStep> builder)
		{
			builder.ToTable("workflow_process_step");
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.ProcessId).HasColumnName("process_id");
			builder.Property(x => x.StepId).HasColumnName("step_id");
			builder.Property(x => x.WorkflowTaskInstanceDetails).HasColumnName("workflow_task_instance_details");
			builder.Property(x => x.UserId).HasColumnName("user_id");
			builder.Property(x => x.Status).HasColumnName("status");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
		}
	}
}
