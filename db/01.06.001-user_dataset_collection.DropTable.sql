DROP TABLE public.user_dataset_collection;
	
UPDATE version_info
SET 
  version = '01.06.001',
  released_at = '2026-02-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'user_dataset_collection.DropTable'
WHERE key = 'DataGEMS.Gateway.db'
