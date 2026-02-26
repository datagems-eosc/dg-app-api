ALTER TABLE public.dm_dataset_collection
DROP CONSTRAINT IF EXISTS dm_dataset_collection_dataset_id_fkey;
	
UPDATE version_info
SET 
  version = '01.06.000',
  released_at = '2026-02-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'dataset_collection.DropConstraint.Dataset'
WHERE key = 'DataGEMS.Gateway.db'
