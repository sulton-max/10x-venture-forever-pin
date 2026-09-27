CREATE TABLE inbox_messages (
    message_id  varchar(200) NOT NULL,
    seen_at_utc timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT pk_inbox_messages PRIMARY KEY (message_id)
);
ALTER TABLE subscriptions ADD COLUMN last_event_at timestamptz NULL;
