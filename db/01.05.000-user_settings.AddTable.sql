CREATE TABLE public.user_settings
(
    id uuid NOT NULL,
	key character varying(250) NOT NULL,
    user_id uuid NOT NULL,
    value text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT user_settings_pkey PRIMARY KEY (id),
    CONSTRAINT user_settings_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);
	
UPDATE version_info
SET 
  version = '01.05.000',
  released_at = '2025-10-15 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'user_settings.AddTable'
WHERE key = 'DataGEMS.Gateway.db'
