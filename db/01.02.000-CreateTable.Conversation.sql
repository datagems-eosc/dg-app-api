CREATE TABLE IF NOT EXISTS public.conversation
(
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(300) COLLATE pg_catalog."default" NOT NULL,
    is_active smallint NOT NULL,
    created_at time with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT conversation_pkey PRIMARY KEY (id),
    CONSTRAINT conversation_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
	
INSERT INTO version_info(key, version, released_at, deployed_at, description)
VALUES ('DataGEMS.Gateway.db', '01.02.000', '"2025-06-26 00:00:00.00000+00"', now(), 'CreateTable.Conversation');

