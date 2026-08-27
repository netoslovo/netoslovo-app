using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Words.Contract;

public sealed record WordDistance(Word Word, ushort Distance);