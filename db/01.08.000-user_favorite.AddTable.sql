CREATE TABLE if not exists public.user_favorite (
    id UUID NOT NULL,
    user_id UUID NOT NULL,
	dataset_id UUID NOT NULL,
    is_active SMALLINT NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT user_favorite_pkey PRIMARY KEY (id),
    CONSTRAINT fk_user_favorite_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

UPDATE version_info 
SET 
  version = '01.08.000',
  released_at = '2026-05-05 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'CreateTable.UserFavorite'
WHERE key = 'DataGEMS.Gateway.db'