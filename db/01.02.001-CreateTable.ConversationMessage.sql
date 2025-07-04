CREATE TABLE public.conversation_message
(
    id uuid NOT NULL,
    conversation_id uuid NOT NULL,
    kind smallint NOT NULL,
    data text,
    created_at timestamp with time zone NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (conversation_id)
        REFERENCES public.conversation (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);
	
	
UPDATE version_info 
SET 
  version = '01.02.001',
  released_at = '2025-06-26 00:00:00.00000+00', 
  deployed_at = now(),
  description = 'CreateTable.ConversationMessage'
WHERE key = 'DataGEMS.Gateway.db'

