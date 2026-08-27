namespace WordoGuessr.Game.Dto;

public sealed record GuessDto(string Word, int Distance, double FillPercentage, GuessSourceDto Source);
