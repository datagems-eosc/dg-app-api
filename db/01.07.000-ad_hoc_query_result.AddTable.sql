CREATE TABLE if not exists public.ad_hoc_query_result (
    id UUID NOT NULL,
    user_id UUID NOT NULL,
    analytical_pattern TEXT NOT NULL,
    is_active SMALLINT NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT as_hoc_query_result_pkey PRIMARY KEY (id),
    CONSTRAINT fk_ad_hoc_query_result_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public."user" (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

UPDATE version_info 
SET 
  version = '01.07.000',
  released_at = '2026-05-05 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'CreateTable.AdHocQueryResult'
WHERE key = 'DataGEMS.Gateway.db'