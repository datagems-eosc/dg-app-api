ALTER TABLE IF EXISTS public.dm_collection
    RENAME TO collection;
	
UPDATE version_info
SET 
  version = '01.06.004',
  released_at = '2026-02-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'collection.RenameTable'
WHERE key = 'DataGEMS.Gateway.db'
