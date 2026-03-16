DROP TABLE public.dm_dataset;
	
UPDATE version_info
SET 
  version = '01.06.003',
  released_at = '2026-02-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'dataset.DropTable'
WHERE key = 'DataGEMS.Gateway.db'
