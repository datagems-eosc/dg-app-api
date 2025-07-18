ALTER TABLE IF EXISTS public.user_collection
    ALTER COLUMN kind DROP DEFAULT;
	
UPDATE version_info
SET 
  version = '01.04.001',
  released_at = '2025-07-18 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'user_collection.EditColumn.Kind'
WHERE key = 'DataGEMS.Gateway.db'
