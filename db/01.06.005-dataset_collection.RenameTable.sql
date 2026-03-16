ALTER TABLE IF EXISTS public.dm_dataset_collection
    RENAME TO dataset_collection;
	
UPDATE version_info
SET 
  version = '01.06.005',
  released_at = '2026-02-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'dataset_collection.RenameTable'
WHERE key = 'DataGEMS.Gateway.db'
