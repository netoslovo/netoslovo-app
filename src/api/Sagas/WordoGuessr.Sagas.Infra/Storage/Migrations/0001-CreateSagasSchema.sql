create schema sagas;

create table sagas.upload_words_version_sagas
(
    saga_id uuid primary key,
    words_version integer not null,
    state integer not null,
    current_step integer not null,
    created_at timestamptz not null,
    completed_at timestamptz null,
    completed_steps jsonb not null,
    version integer not null
);

create unique index ux_upload_words_version_sagas_state_active
    on sagas.upload_words_version_sagas(state)
    where state = 0;

create table sagas.upload_words_version_sagas_history
(
    saga_id uuid primary key,
    words_version integer not null,
    state integer not null,
    current_step integer not null,
    created_at timestamptz not null,
    completed_at timestamptz null,
    completed_steps jsonb not null,
    version integer not null
);

create index ix_upload_words_version_sagas_history_created_at_saga_id
    on sagas.upload_words_version_sagas_history(created_at, saga_id);

create index ix_upload_words_version_sagas_history_words_version_created_at_saga_id
    on sagas.upload_words_version_sagas_history(words_version, created_at, saga_id);