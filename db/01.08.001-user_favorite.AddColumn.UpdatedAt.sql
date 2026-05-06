ALTER TABLE IF EXISTS public.user_favorite
    ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();
	
UPDATE version_info
SET 
  version = '01.08.001',
  released_at = '2025-05-06 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'user_favorite.AddColumn.UpdatedAt'
WHERE key = 'DataGEMS.Gateway.db'
