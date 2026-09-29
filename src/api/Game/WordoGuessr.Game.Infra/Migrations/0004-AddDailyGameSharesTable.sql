create table game.daily_game_shares
(
    game_id uuid primary key,
    public_id uuid not null,
    created_at timestamptz not null,

    constraint fk_daily_game_shares_single_games_game_id
        foreign key (game_id)
        references game.single_games (id)
        on delete cascade
);

create unique index ux_daily_game_shares_public_id
    on game.daily_game_shares (public_id);
