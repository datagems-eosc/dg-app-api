ALTER TABLE IF EXISTS public.ad_hoc_query_result
    DROP COLUMN IF EXISTS dataset_id;

UPDATE version_info 
SET 
  version = '01.07.002',
  released_at = '2026-05-19 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'ad_hoc_query_result.DeleteColumn.DatasetId'
WHERE key = 'DataGEMS.Gateway.db';