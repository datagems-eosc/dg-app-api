CREATE TABLE public.conversation_message
(
    id uuid NOT NULL,
    conversation_id uuid NOT NULL,
    kind smallint NOT NULL,
    data text,
    created_at time with time zone NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (conversation_id)
        REFERENCES public.conversation (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);
	
INSERT INTO version_info(key, version, released_at, deployed_at, description)
VALUES ('DataGEMS.Gateway.db', '01.02.001', '"2025-06-26 00:00:00.00000+00"', now(), 'CreateTable.ConversationMessage');

