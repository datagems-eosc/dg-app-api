CREATE TABLE public.conversation_dataset
(
    id uuid NOT NULL,
    conversation_id uuid NOT NULL,
    dataset_id uuid NOT NULL,
    is_active smallint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (conversation_id)
        REFERENCES public.conversation (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

UPDATE version_info 
SET 
  version = '01.02.002',
  released_at = '2025-06-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'CreateTable.ConversationDataset'
WHERE key = 'DataGEMS.Gateway.db'

