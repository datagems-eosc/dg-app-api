ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN size bigint;
	
UPDATE version_info
SET 
  version = '01.03.001',
  released_at = '2025-07-17 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'dm_dataset.AddColumn.DataType'
WHERE key = 'DataGEMS.Gateway.db'
