create schema wolverine;

create table wolverine.wolverine_agent_restrictions
(
    id uuid not null,
    uri varchar not null,
    type varchar not null,
    node integer not null default 0,

    constraint pkey_wolverine_agent_restrictions_id primary key (id)
);

create table wolverine.wolverine_control_queue
(
    id uuid not null,
    message_type varchar not null,
    node_id uuid not null,
    body bytea not null,
    posted timestamptz not null default now(),
    expires timestamptz,

    constraint pkey_wolverine_control_queue_id primary key (id)
);

create table wolverine.wolverine_dead_letters
(
    id uuid not null,
    execution_time timestamptz,
    body bytea not null,
    message_type varchar not null,
    received_at varchar,
    source varchar,
    exception_type varchar,
    exception_message varchar,
    sent_at timestamptz,
    replayable boolean,

    constraint pkey_wolverine_dead_letters_id primary key (id)
);

create table wolverine.wolverine_incoming_envelopes
(
    id uuid not null,
    status varchar not null,
    owner_id integer not null,
    execution_time timestamptz,
    attempts integer default 0,
    body bytea not null,
    message_type varchar not null,
    received_at varchar,
    keep_until timestamptz,

    constraint pkey_wolverine_incoming_envelopes_id primary key (id)
);

create table wolverine.wolverine_nodes
(
    id uuid not null,
    node_number serial not null,
    description varchar not null,
    uri varchar not null,
    started timestamptz not null default now(),
    health_check timestamptz not null default now(),
    version varchar,
    capabilities text[],

    constraint pkey_wolverine_nodes_id primary key (id)
);

create table wolverine.wolverine_node_assignments
(
    id varchar not null,
    node_id uuid,
    started timestamptz not null default now(),

    constraint pkey_wolverine_node_assignments_id primary key (id),
    constraint fkey_wolverine_node_assignments_node_id
        foreign key (node_id)
        references wolverine.wolverine_nodes (id)
        on delete cascade
);

create table wolverine.wolverine_node_records
(
    id serial not null,
    node_number integer not null,
    event_name varchar not null,
    "timestamp" timestamptz not null default now(),
    description varchar,

    constraint pkey_wolverine_node_records_id primary key (id)
);

create table wolverine.wolverine_outgoing_envelopes
(
    id uuid not null,
    owner_id integer not null,
    destination varchar not null,
    deliver_by timestamptz,
    body bytea not null,
    attempts integer default 0,
    message_type varchar not null,

    constraint pkey_wolverine_outgoing_envelopes_id primary key (id)
);
