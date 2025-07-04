CREATE TABLE IF NOT EXISTS public.conversation
(
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(300) COLLATE pg_catalog."default" NOT NULL,
    is_active smallint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT conversation_pkey PRIMARY KEY (id),
    CONSTRAINT conversation_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);


UPDATE version_info 
SET 
  version = '01.02.000',
  released_at = '2025-06-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'CreateTable.Conversation'
WHERE key = 'DataGEMS.Gateway.db'
