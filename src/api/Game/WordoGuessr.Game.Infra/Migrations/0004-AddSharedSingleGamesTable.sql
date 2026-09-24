create table game.single_game_shares
(
    game_id uuid primary key,
    public_id uuid not null,
    show_guess_words boolean not null,
    created_at timestamptz not null,
    updated_at timestamptz not null,

    constraint fk_single_game_shares_single_games_game_id
        foreign key (game_id)
        references game.single_games (id)
);

create unique index ux_single_game_shares_public_id
    on game.single_game_shares (public_id);
