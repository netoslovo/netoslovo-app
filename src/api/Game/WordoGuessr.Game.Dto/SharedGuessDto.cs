namespace WordoGuessr.Game.Dto;

public sealed record SharedGuessDto(
    string? Word,
    int Distance,
    int Order,
    double FillPercentage,
    GuessSourceDto Source);

public sealed record VisibleSharedGuessDto(
    string Word,
    int Distance,
    int Order,
    double FillPercentage,
    GuessSourceDto Source);

public sealed record HiddenSharedGuessDto(
    int Distance,
    int Order,
    double FillPercentage,
    GuessSourceDto Source);