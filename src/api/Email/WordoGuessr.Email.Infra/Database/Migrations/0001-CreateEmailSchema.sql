create schema email;

create table email."messages"
(
    id uuid primary key,
    "to" text not null,
    subject text not null,
    body text not null,
    created_at timestamptz not null,
    updated_at timestamptz not null,
    expire_at timestamptz null,
    attempts integer not null default 0,
    state integer not null,
    source_id uuid not null
);

create index ix_messages_created_pending
    on email."messages" (created_at)
    where state = 0;

create index ix_messages_created_sending
    on email."messages" (created_at)
    where state = 1;

create index ix_messages_created_failed_retryable
    on email."messages" (created_at)
    where state = 3;

create index ix_messages_expire_at
    on email."messages" (expire_at);

create unique index ux_messages_source_id
    on email."messages" (source_id);

create table email."messages_deduplication"
(    
    source_id uuid primary key,
    created_at timestamptz not null
);

create index ix_messages_deduplication_created_at
    on email."messages_deduplication" (created_at);

create table email."messages_history"
(
    message_id uuid primary key,
    "to" text not null,
    subject text not null,
    created_at timestamptz not null,
    updated_at timestamptz not null,
    expire_at timestamptz null,
    attempts integer not null default 0,
    state integer not null,
    source_id uuid not null,
    archived_at timestamptz not null,
    archive_reason integer not null 
);
