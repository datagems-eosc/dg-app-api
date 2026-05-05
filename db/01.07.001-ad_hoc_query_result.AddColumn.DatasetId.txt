ALTER TABLE IF EXISTS public.ad_hoc_query_result
    ADD COLUMN dataset_id UUID;

UPDATE public.ad_hoc_query_result SET dataset_id = gen_random_uuid();

ALTER TABLE public.ad_hoc_query_result ALTER COLUMN dataset_id SET NOT NULL;

UPDATE version_info 
SET 
  version = '01.07.001',
  released_at = '2026-05-05 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'ad_hoc_query_result.AddColumn.DatasetId'
WHERE key = 'DataGEMS.Gateway.db'