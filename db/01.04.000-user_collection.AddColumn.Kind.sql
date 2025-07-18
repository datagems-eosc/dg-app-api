ALTER TABLE IF EXISTS public.user_collection
    ADD COLUMN kind smallint NOT NULL DEFAULT 0;
	
UPDATE version_info
SET 
  version = '01.04.000',
  released_at = '2025-07-18 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'user_collection.AddColumn.Kind'
WHERE key = 'DataGEMS.Gateway.db'
