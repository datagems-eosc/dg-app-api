CREATE TABLE IF NOT EXISTS public.workflow_process_step (
    id UUID NOT NULL,
    process_id UUID NOT NULL,
    step_id UUID NOT NULL,
    workflow_task_instance_details TEXT NULL,
    status INTEGER NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL,

    CONSTRAINT workflow_process_step_pkey PRIMARY KEY (id),

    CONSTRAINT fk_workflow_process_step_process_id_fkey
        FOREIGN KEY (process_id)
        REFERENCES public.workflow_process (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

UPDATE version_info
SET
    version = '01.09.001',
    released_at = '2026-07-22 00:00:00.00000+00',
    deployed_at = NOW(),
    description = 'CreateTable.WorkflowProcessStep'
WHERE key = 'DataGEMS.Gateway.db';