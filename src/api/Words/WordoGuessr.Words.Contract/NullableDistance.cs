using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Words.Contract;

public sealed record NullableDistance(Word Word, ushort? Distance);