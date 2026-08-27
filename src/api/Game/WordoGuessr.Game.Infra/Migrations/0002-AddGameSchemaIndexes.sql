create index ix_versioned_ggame_sources_difficulty_code
    on game.versioned_game_sources (difficulty_code);

create index ix_versioned_ggame_sources_predicted_difficulty
    on game.versioned_game_sources (predicted_difficulty);

create unique index ux_game_sources_word
    on game.game_sources (word);

create unique index ux_single_games_player_id_mode_game_source_id
    on game.single_games (player_id, mode, game_source_id);

create index ix_single_games_player_id_state
    on game.single_games (player_id, state);

create index ix_single_games_created_at
    on game.single_games (created_at);

create index ix_single_games_updated_at
    on game.single_games (updated_at);

create unique index ux_guesses_game_id_word
    on game.guesses (game_id, word);

create unique index ux_single_game_daily_schedule_day
    on game.single_game_daily_schedule(day);

create unique index ux_single_game_daily_schedule_game_source_id
    on game.single_game_daily_schedule(game_source_id);

create index ix_daily_game_source_reviews_created_at
    on game.daily_game_source_reviews(created_at);

create index ix_daily_game_source_reviews_updated_at
    on game.daily_game_source_reviews(updated_at);

create index ix_single_game_results_daily_guessed_day_player
    on game.single_game_results (day_of_daily_game, player_id)
    include (score, attempts, duration)
    where mode = 1 and state = 2;

create index ix_daily_game_streaks_info_current_active_top
    on game.daily_game_streaks_info (last_success_day, current_streak, player_id)
    where current_streak <> 0;

create index ix_daily_game_streaks_info_longest_top
    on game.daily_game_streaks_info (longest_streak, player_id)
    where longest_streak <> 0;

create index ix_arcade_game_stats_leaderboard
    on game.arcade_game_stats (difficulty_code, guessed_games desc, average_score);
