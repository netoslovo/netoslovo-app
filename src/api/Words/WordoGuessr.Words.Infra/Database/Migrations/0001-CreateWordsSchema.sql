create schema words;

create table words.words_versions
(
    version integer not null primary key,    
    state integer not null,
    created_at timestamptz not null,
    activated_at timestamptz null,
    retired_at timestamptz null 
);

 create unique index ux_words_versions_single_active
    on words.words_versions (state)
    where state = 1;   

create table words.words_indexes
(
    version integer not null,
    word_id integer not null,
    word_text text not null,

    constraint pk_words_indexes
        primary key (version, word_id),

    constraint fk_words_indexes_words_versions_version
        foreign key(version)
        references words.words_versions(version)
) partition by list (version);

create table words.word_distance_maps
(
    version integer not null,
    word_id integer not null,
    words_ids_by_distance bytea not null,
    distances_by_words_ids bytea not null,

    constraint pk_word_distance_maps
        primary key (version, word_id),

    constraint fk_word_distance_maps_words_versions_version
        foreign key(version)
        references words.words_versions(version)
) partition by list (version);