using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class Helpers
{
    public static HintsInfoDto BuildHintsInfo(SingleGame game)
    {
        return new HintsInfoDto
        (
            RevealLengthHintUsed: game.RevealLengthHintUsed,
            NeighbourHintsTotal: SingleGame.HalfwayWordHintsTotal,
            NeighbourHintsLeft: game.HalfwayWordHintsLeft,
            RevealLetterHintsTotal: game.RevealLetterHintsTotal,
            RevealLetterHintsLeft: game.RevealLetterHintsLeft,
            RevealLengthHintPenalty: SingleGame.RevealLengthHintPenalty,
            RevealLetterHintPenalties: game.RevealLetterHintPenalties,
            RevealHalfwayWordHintPenalties: game.RevevalHalfwayWordHintPenalties
        );
    }
}
