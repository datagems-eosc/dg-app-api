ALTER TABLE IF EXISTS public.workflow_process
    ADD COLUMN dataset_id UUID NULL;
	
UPDATE version_info
SET 
  version = '01.09.002',
  released_at = '2026-08-11 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'workflow_process.AddColumn.DatasetId'
WHERE key = 'DataGEMS.Gateway.db'
