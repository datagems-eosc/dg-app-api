CREATE TABLE IF NOT EXISTS public.workflow_process (
    id UUID NOT NULL,
    process_id UUID NOT NULL,
    user_id UUID NULL,
    status SMALLINT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL,

    CONSTRAINT workflow_process_pkey PRIMARY KEY (id),

    CONSTRAINT fk_workflow_process_user_id_fkey
        FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

UPDATE version_info
SET
    version = '01.09.000',
    released_at = '2026-07-22 00:00:00.00000+00',
    deployed_at = NOW(),
    description = 'CreateTable.WorkflowProcess'
WHERE key = 'DataGEMS.Gateway.db';