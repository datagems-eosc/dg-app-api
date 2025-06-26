ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN description text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN license text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN url character varying(300);

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN version character varying(50);

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN headline text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN keywords text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN field_of_science text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN language text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN country text;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN date_published timestamp with time zone;

ALTER TABLE IF EXISTS public.dm_dataset
    ADD COLUMN profile text;
	
INSERT INTO version_info(key, version, released_at, deployed_at, description)
VALUES ('DataGEMS.Gateway.db', '01.01.000', '"2025-06-26 00:00:00.00000+00"', now(), 'dm_dataset.AddColumn.ProfileFields');
